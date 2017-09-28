using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace NanYUtilityLib.MiscHelper
{
    static public class ArrayHelper
    {
        static public T[][] AllocateArray<T>(int m, int n, T v)
        {
            T[][] r = new T[m][];
            for (int i = 0; i < m; ++i)
            {
                r[i] = new T[n];

                for (int j = 0; j < n; ++j)
                {
                    r[i][j] = v;
                }
            }

            return r;
        }

        static public T[][] AllocateArray<T>(int m, int n)
        {
            T[][] r = new T[m][];
            for (int i = 0; i < m; ++i)
            {
                r[i] = new T[n];
            }

            return r;
        }

        static public T[][][] AllocateArray<T>(int m, int n, int k)
        {
            T[][][] r = new T[m][][];

            for (int i = 0; i < m; ++i)
            {
                r[i] = AllocateArray<T>(n, k);
            }

            return r;
        }

        static public void SetAll<T>(T[][] dest, T v)
        {
            for (int i = 0; i < dest.Length; ++i)
            {
                for (int j = 0; j < dest[i].Length; ++j)
                {
                    dest[i][j] = v;
                }
            }
        }

        static public T[][][] Clone<T>(T[][][] source)
        {
            if (source == null)
            {
                return null;
            }

            var dest = new T[source.Length][][];

            for (int i = 0; i < source.Length; ++i)
            {
                dest[i] = Clone(source[i]);
            }

            return dest;
        }

        static public T[][] Clone<T>(T[][] source)
        {
            if (source == null)
            {
                return null;
            }

            var dest = new T[source.Length][];
            for (int i = 0; i < source.Length; ++i)
            {
                if (source[i] == null)
                {
                    continue;
                }

                dest[i] = (T[])source[i].Clone();
            }

            return dest;
        }

        static public void Clear<T>(T[][][] x)
        {
            if (x == null)
            {
                return;
            }

            for (int i = 0; i < x.Length; ++i)
            {
                Clear(x[i]);
            }
        }

        static public void Clear<T>(T[][] x)
        {
            if (x == null)
            {
                return;
            }

            for (int i = 0; i < x.Length; ++i)
            {
                if (x[i] == null)
                {
                    continue;
                }

                Array.Clear(x[i], 0, x[i].Length);
            }
        }

        static public void Fill<T>(T[] x, T v)
        {
            if (x == null)
            {
                return;
            }

            for (int i = 0; i < x.Length; ++i)
            {
                x[i] = v;
            }
        }

        static public void Fill<T>(T[][] x, T v)
        {
            if (x == null)
            {
                return;
            }

            for (int i = 0; i < x.Length; ++i)
            {
                Fill<T>(x[i], v);
            }
        }

        static public void Fill<T>(T[][][] x, T v)
        {
            if (x == null)
            {
                return;
            }

            for (int i = 0; i < x.Length; ++i)
            {
                Fill<T>(x[i], v);
            }
        }

        static public void CopyTo<T>(T[] src, T[] dest)
        {
            if (src == null)
            {
                return;
            }

            Array.Copy(src, dest, src.Length);
        }

        static public void CopyTo<T>(T[][] src, T[][] dest)
        {
            if (src == null)
            {
                return;
            }

            for (int i = 0; i < src.Length; ++i)
            {
                CopyTo<T>(src[i], dest[i]);
            }
        }

        static public void CopyTo<T>(T[][][] src, T[][][] dest)
        {
            if (src == null)
            {
                return;
            }

            for (int i = 0; i < src.Length; ++i)
            {
                CopyTo<T>(src[i], dest[i]);
            }
        }

    }
}
