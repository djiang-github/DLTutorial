using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NanYUtilityLib
{
    public class HsiehHash
    {
        public static uint SuperFastHash(params uint[] data)
        {
            int len = data.Length;
            uint hash = (uint)len;
            uint tmp = 0;

            if (len <= 0)
            {
                return 0;
            }

            for (int i = 0; i < len; ++len)
            {
                hash += data[i] & 0xffffu;
                tmp = ((data[i] >> 16) << 11) ^ hash;
                hash = (hash << 16) ^ tmp;
                hash += hash >> 11;
            }

            hash ^= hash << 3;
            hash += hash >> 5;
            hash ^= hash << 4;
            hash += hash >> 17;
            hash ^= hash << 25;
            hash += hash >> 6;

            return hash;
        }

        public static uint SuperFastHash(params int[] data)
        {
            int len = data.Length;
            uint hash = (uint)len;
            uint tmp = 0;

            if (len <= 0)
            {
                return 0;
            }

            for (int i = 0; i < len; ++len)
            {
                hash += (uint)data[i] & 0xffffu;
                tmp = (((uint)data[i] >> 16) << 11) ^ hash;
                hash = (hash << 16) ^ tmp;
                hash += hash >> 11;
            }

            hash ^= hash << 3;
            hash += hash >> 5;
            hash ^= hash << 4;
            hash += hash >> 17;
            hash ^= hash << 25;
            hash += hash >> 6;

            return hash;
        }

        public static uint SuperFastHash(int data)
        {
            uint hash = 1u;
            uint tmp = 0;

            hash += (uint)data & 0xffffu;
            tmp = (((uint)data >> 16) << 11) ^ hash;
            hash = (hash << 16) ^ tmp;
            hash += hash >> 11;

            hash ^= hash << 3;
            hash += hash >> 5;
            hash ^= hash << 4;
            hash += hash >> 17;
            hash ^= hash << 25;
            hash += hash >> 6;

            return hash;
        }

        public static uint SuperFastHash(uint data)
        {
            uint hash = 1u;
            uint tmp = 0;

            hash += (uint)data & 0xffffu;
            tmp = (((uint)data >> 16) << 11) ^ hash;
            hash = (hash << 16) ^ tmp;
            hash += hash >> 11;

            hash ^= hash << 3;
            hash += hash >> 5;
            hash ^= hash << 4;
            hash += hash >> 17;
            hash ^= hash << 25;
            hash += hash >> 6;

            return hash;
        }

        public static uint SuperFastHash(ulong xdata)
        {
            uint hash = 1u;
            uint tmp = 0;

            uint data = (uint)xdata;

            hash += (uint)data & 0xffffu;
            tmp = (((uint)data >> 16) << 11) ^ hash;
            hash = (hash << 16) ^ tmp;
            hash += hash >> 11;

            data = (uint)(xdata >> 32);

            hash += (uint)data & 0xffffu;
            tmp = (((uint)data >> 16) << 11) ^ hash;
            hash = (hash << 16) ^ tmp;
            hash += hash >> 11;

            hash ^= hash << 3;
            hash += hash >> 5;
            hash ^= hash << 4;
            hash += hash >> 17;
            hash ^= hash << 25;
            hash += hash >> 6;

            return hash;
        }
    }
}
