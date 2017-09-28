using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NanYUtilityLib
{
    public class LRUCache<T> where T: IEquatable<T>
    {
        private T[] Pool;
        private int[] Count;

        public int Length { get { return Pool.Length; } }

        public LRUCache(int Length)
        {
            Pool = new T[Length];
            Count = new int[Length];
        }

        public void Insert(T item, out T lastUsed, out int lastCount)
        {
            int xhash = item.GetHashCode();

            uint hash = rehash(xhash) % (uint)Length;

            if (Count[hash] == 0 || Pool[hash].Equals(item))
            {
                lastCount = -1;
                lastUsed = default(T);
                Count[hash]++;
            }
            else
            {
                lastCount = Count[hash];
                lastUsed = Pool[hash];
                Pool[hash] = item;
                Count[hash] = 1;
            }
        }

        public void Insert(T item, uint hash, out T lastUsed, out int lastCount)
        {
            hash = hash % (uint)Length;

            if (Count[hash] == 0 || Pool[hash].Equals(item))
            {
                lastCount = -1;
                lastUsed = default(T);
                Pool[hash] = item;
                Count[hash]++;
            }
            else
            {
                lastCount = Count[hash];
                lastUsed = Pool[hash];
                Pool[hash] = item;
                Count[hash] = 1;
            }
        }

        private uint rehash(int key)
        {
            unchecked
            {
                uint xkey = (uint)key;

                uint hash = 2166136261u;

                const uint xprime = 16777619u;

                hash ^= xkey & 0xffu;
                hash *= xprime;
                xkey >>= 8;

                hash ^= xkey & 0xffu;
                hash *= xprime;
                xkey >>= 8;

                hash ^= xkey & 0xffu;
                hash *= xprime;
                xkey >>= 8;

                hash ^= xkey & 0xffu;
                hash *= xprime;

                return hash;
            }
        }

        public IEnumerable<KeyValuePair<T, int>>
            Items
        {
            get
            {
                for (int i = 0; i < Count.Length; ++i)
                {
                    if (Count[i] > 0)
                    {
                        yield return new KeyValuePair<T, int>(Pool[i], Count[i]);
                    }
                }
            }
        }
    }
}
