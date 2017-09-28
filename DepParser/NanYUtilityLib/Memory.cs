using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace NanYUtilityLib
{
    unsafe public class Memory
    {
        // Handle for the process heap. This handle is used in all calls to the HeapXXX APIs in the methods below.
        static int ph = GetProcessHeap();
        // Private instance constructor to prevent instantiation.
        private Memory() { }
        // Allocates a memory block of the given size. The allocated memory is automatically initialized to zero.
        public static void* Alloc(int size)
        {
            void* result = HeapAlloc(ph, HEAP_ZERO_MEMORY, size);
            if (result == null) throw new OutOfMemoryException();
            return result;
        }
        // Copies count bytes from src to dst. The source and destination blocks are permitted to overlap.
        //public static void Copy(void* src, void* dst, int count)
        //{
        //    byte* ps = (byte*)src;
        //    byte* pd = (byte*)dst;
        //    if (ps > pd)
        //    {
        //        for (; count != 0; count--) *pd = *ps;
        //    }
        //    else if (ps < pd)
        //    {
        //        for (ps = count, pd = count; count != 0; count--) *--pd = *--ps;
        //    }
        //}
        // Frees a memory block.
        public static void Free(void* block)
        {
            if (!HeapFree(ph, 0, block)) throw new InvalidOperationException();
        }
        // Re-allocates a memory block. If the reallocation request is for a larger size, the additional region of memory is automatically
        // initialized to zero.
        public static void* ReAlloc(void* block, int size)
        {
            void* result = HeapReAlloc(ph, HEAP_ZERO_MEMORY, block, size);
            if (result == null) throw new OutOfMemoryException();
            return result;
        }
        // Returns the size of a memory block.        
        public static int SizeOf(void* block)
        {
            int result = HeapSize(ph, 0, block);
            if (result == -1) throw new InvalidOperationException();
            return result;
        }
        // Heap API flags
        const int HEAP_ZERO_MEMORY = 0x00000008;
        // Heap API functions
        [DllImport("kernel32")]
        static extern int GetProcessHeap();
        [DllImport("kernel32")]
        static extern void* HeapAlloc(int hHeap, int flags, int size);
        [DllImport("kernel32")]
        static extern bool HeapFree(int hHeap, int flags, void* block);
        [DllImport("kernel32")]
        static extern void* HeapReAlloc(int hHeap, int flags, void* block, int size);
        [DllImport("kernel32")]
        static extern int HeapSize(int hHeap, int flags, void* block);
    }

    internal class Win32API
    {
        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr CreateFile(
           String lpFileName, int dwDesiredAccess, int dwShareMode,
           IntPtr lpSecurityAttributes, int dwCreationDisposition,
           int dwFlagsAndAttributes, IntPtr hTemplateFile);

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr CreateFileMapping(
           IntPtr hFile, IntPtr lpAttributes, int flProtect,
           int dwMaximumSizeLow, int dwMaximumSizeHigh,
           String lpName);

        [DllImport("kernel32", SetLastError = true)]
        public static extern bool FlushViewOfFile(
           IntPtr lpBaseAddress, int dwNumBytesToFlush);

        [DllImport("kernel32", SetLastError = true)]
        public static extern IntPtr MapViewOfFile(
           IntPtr hFileMappingObject, int dwDesiredAccess, int dwFileOffsetHigh,
           int dwFileOffsetLow, int dwNumBytesToMap);

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr OpenFileMapping(
           int dwDesiredAccess, bool bInheritHandle, String lpName);

        [DllImport("kernel32", SetLastError = true)]
        public static extern bool UnmapViewOfFile(IntPtr lpBaseAddress);

        [DllImport("kernel32", SetLastError = true)]
        public static extern bool CloseHandle(IntPtr handle);

        [DllImport("kernel32", SetLastError = true)]
        public static extern bool SetEndOfFile(IntPtr handle);

        [DllImport("kernel32", SetLastError = true)]
        public static extern bool SetFilePointer(IntPtr hFile, UInt32 dwFileOffset,
            IntPtr lpDistanceToMoveHigh, int dwMoveMethod);

        unsafe public static bool SetFilePointer64(IntPtr hFile, Int64 offset)
        {
            int* pHighOffset = stackalloc int[1];
            *pHighOffset = (int)(offset >> 32);
            uint lowOffset = (uint)(offset & 0xFFFFFFFF);
            return SetFilePointer(hFile, lowOffset, (IntPtr)pHighOffset, 0);
        }
    }

    public class FileMapIOException : IOException
    {
        // properties
        private int m_win32Error = 0;
        public int Win32ErrorCode
        {
            get { return m_win32Error; }
        }
        public override string Message
        {
            get
            {
                if (Win32ErrorCode != 0)
                    return base.Message + " (" + Win32ErrorCode + ")";
                else
                    return base.Message;
            }
        }

        // construction
        public FileMapIOException(int error)
            : base()
        {
            m_win32Error = error;
        }
        public FileMapIOException(string message)
            : base(message)
        {
        }
        public FileMapIOException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

    }

    public class MemoryMappedFile : IDisposable
    {
        private const int GENERIC_READ = unchecked((int)0x80000000);
        private const int GENERIC_WRITE = unchecked((int)0x40000000);
        private const int CREATE_ALWAYS = 2;
        private const int OPEN_EXISTING = 3;
        private const int OPEN_ALWAYS = 4;
        private const int FILE_SHARE_READ = 1;
        private const int FILE_SHARE_WRITE = 2;
        private const int PAGE_READONLY = 2;
        private const int PAGE_READWRITE = 4;
        private const int FILE_MAP_WRITE = 2;
        private const int FILE_MAP_READ = 4;

        private static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
        private static readonly IntPtr NULL_HANDLE = IntPtr.Zero;

        private IntPtr _hFile = INVALID_HANDLE_VALUE;
        private IntPtr _hMap = IntPtr.Zero;
        private IntPtr _base = IntPtr.Zero;
        //private int _size = 0;

        ~MemoryMappedFile()
        {
            Dispose(false);
        }

        public IntPtr GetDataPtr()
        {
            return _base;
        }

        // open an MMF for read
        public MemoryMappedFile(string fileName)
            : this(fileName, 0)
        {
        }

        // open an MMF
        public MemoryMappedFile(string fileName, Int64 size)
        {
            _hFile = INVALID_HANDLE_VALUE;

            if (fileName == null)
                throw new FileMapIOException("Invalid file name.");

            if (size > 0)
            {
                _hFile = Win32API.CreateFile(fileName, GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ,
                         IntPtr.Zero, CREATE_ALWAYS, 0, IntPtr.Zero);
                if (!Win32API.SetFilePointer64(_hFile, size) || !Win32API.SetEndOfFile(_hFile))
                {
                    Win32API.CloseHandle(_hFile);
                    _hFile = INVALID_HANDLE_VALUE;
                    throw new FileMapIOException(Marshal.GetHRForLastWin32Error());
                }
            }
            else
            {
                _hFile = Win32API.CreateFile(fileName, GENERIC_READ, FILE_SHARE_READ,
                    IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);
            }

            if (_hFile == INVALID_HANDLE_VALUE)
            {
                throw new FileMapIOException(Marshal.GetHRForLastWin32Error());
            }

            _hMap = Win32API.CreateFileMapping(
                        _hFile, IntPtr.Zero,
                        size > 0 ? PAGE_READWRITE : PAGE_READONLY,
                        0, 0, null);
            if (_hMap == NULL_HANDLE)
            {
                Win32API.CloseHandle(_hFile);
                _hFile = INVALID_HANDLE_VALUE;
                throw new FileMapIOException(Marshal.GetHRForLastWin32Error());
            }

            _base = Win32API.MapViewOfFile(_hMap, size > 0 ? FILE_MAP_WRITE : FILE_MAP_READ, 0, 0, 0);
            if (_base == IntPtr.Zero)
            {
                Win32API.CloseHandle(_hFile);
                Win32API.CloseHandle(_hMap);
                _hFile = INVALID_HANDLE_VALUE;
                _hMap = IntPtr.Zero;
                throw new FileMapIOException(Marshal.GetHRForLastWin32Error());
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_hFile != INVALID_HANDLE_VALUE)
            {
                Win32API.UnmapViewOfFile(_base);
                Win32API.CloseHandle(_hMap);
                Win32API.CloseHandle(_hFile);
                _hFile = INVALID_HANDLE_VALUE;
                _hMap = NULL_HANDLE;
                _base = IntPtr.Zero;
            }

            if (disposing)
                GC.SuppressFinalize(this);
        }

    }

}