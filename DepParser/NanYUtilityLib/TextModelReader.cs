using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

namespace NanYUtilityLib
{
    public class TextModelReader : IDisposable
    {
        private bool disposed = false;
        private bool ownStream = false;
        private StreamReader sr;
        private int nestLevel = 0;

        private const UInt64 SIG = 0x2333233323332333UL;
        private const UInt64 VER = 0UL;
        public int NestLevel {
            get
            {
                return nestLevel;
            }
        }
        public TextModelReader(string file)
            :this(new StreamReader(file), true)
        {
        }

        public TextModelReader(StreamReader sr, bool ownStream = false)
        {
            this.ownStream = ownStream;
            this.sr = sr;

            var xsig = ReadOptionUInt64("SIG");
            var xver = ReadOptionUInt64("VER");

            if (xsig != SIG || xver != VER)
            {
                throw new Exception("Signiture or Version number does not match!");
            }
        }

        public UInt64 ReadOptionUInt64(string key)
        {
            string xkey;
            string xvalue;
            GetKVPair(out xkey, out xvalue);

            if (key != xkey)
            {
                throw new Exception("option key does not match!");
            }

            return UInt64.Parse(xvalue);
        }

        public int ReadOptionInt(string key)
        {
            string xkey;
            string xvalue;
            GetKVPair(out xkey, out xvalue);

            if (key != xkey)
            {
                throw new Exception("option key does not match!");
            }

            return int.Parse(xvalue);
        }

        public string ReadOptionString(string key)
        {
            string xkey;
            string xvalue;
            GetKVPair(out xkey, out xvalue);

            if (key != xkey)
            {
                throw new Exception("option key does not match!");
            }

            return xvalue;
        }

        public double ReadOptionDouble(string key)
        {
            string xkey;
            string xvalue;
            GetKVPair(out xkey, out xvalue);

            if (key != xkey)
            {
                throw new Exception("option key does not match!");
            }

            return double.Parse(xvalue);
        }

        public string Read()
        {
            string line;
            if (!GetNextLine(out line))
            {
                throw new Exception("No more lines to read");
            }

            return deEscape(line, out nestLevel);
        }

        private bool GetNextLine(out string line)
        {
            bool find = false;
            line = null;
            while (!sr.EndOfStream)
            {
                line = sr.ReadLine();

                if (string.IsNullOrEmpty(line) || line.StartsWith("//"))
                {
                    line = null;
                    continue;
                }
                else
                {
                    find = true;
                    break;
                }
            }
            return find;
        }

        private void GetKVPair(out string key, out string value)
        {
            string line;
            if (!GetNextLine(out line))
            {
                throw new Exception("No more lines to read");
            }
            line = deEscape(line, out nestLevel);
            int pos = line.IndexOf('\t');
            if (pos < 0 || pos == 0 || pos == line.Length - 1)
            {
                throw new Exception("error reading options");
            }
            key = line.Substring(0, pos);
            value = line.Substring(pos + 1);
        }
        public void Close()
        {
            sr.Close();
        }
        private static string deEscape(string line, out int nestLvl)
        {
            nestLvl = 0;
            if (line == null)
            {
                return null;
            }
            int i = 0;
            for (i = 0; i < line.Length; ++i)
            {
                if (line[i] == '\t')
                {
                    nestLvl += 1;
                }
                else
                {
                    break;
                }
            }

            if (i >= line.Length)
            {
                return null;
            }

            var sb = new StringBuilder();

            for (; i < line.Length; ++i)
            {
                char c = line[i];

                if (c == '\\')
                {
                    if (i == line.Length - 1)
                    {
                        throw new Exception("error in escape sequence");
                    }

                    char nc = line[i + 1];

                    switch (nc)
                    {
                        case 'r':
                            sb.Append('\r');
                            break;
                        case 'n':
                            sb.Append('\n');
                            break;
                        case '\\':
                            sb.Append('\\');
                            break;
                        case 't':
                            if (sb.Length == 0)
                            {
                                sb.Append('\t');
                            }
                            else
                            {
                                throw new Exception("error in escape sequence");
                            }
                            break;
                        default:
                            throw new Exception("error in escape sequence");
                    }

                    i += 1;
                }
                else
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called. 
            if(!this.disposed)
            {
                // If disposing equals true, dispose all managed 
                // and unmanaged resources. 
                if(disposing)
                {
                    // Dispose managed resources.
                    //component.Dispose();
                    if(ownStream)
                    {
                        sr.Dispose();
                    }
                }

                // Call the appropriate methods to clean up 
                // unmanaged resources here. 
                // If disposing is false, 
                // only the following code is executed.
                if(ownStream)
                {
                    sr.Close();
                }

                // Note disposing has been done.
                disposed = true;
            }
        }
    }
}
