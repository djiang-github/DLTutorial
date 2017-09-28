using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NanYUtilityLib
{
    public class ItemCounter<X> : IComparable<ItemCounter<X>>
    {
        public X item;
        public Int64 cnt;
        public ItemCounter(X item)
            : this(item, 0)
        {
        }
        public ItemCounter(X item, int cnt)
        {
            this.item = item;
            this.cnt = cnt;
        }

        int IComparable<ItemCounter<X>>.CompareTo(ItemCounter<X> other)
        {
            if (other == null)
            {
                return -1;
            }
            else
            {
                return other.cnt.CompareTo(this.cnt);
            }
        }
    }

    public class ItemCounterDict<T>
    {
        private Dictionary<T, ItemCounter<T>> dict;

        public ItemCounterDict()
        {
            dict = new Dictionary<T, ItemCounter<T>>();
            uniqCnt = 0;
            totalCnt = 0;
        }

        public void Add(T item)
        {
            Add(item, 1);
        }

        public void Add(T item, Int64 cnt)
        {
            ItemCounter<T> tmpCnter;
            if (!dict.TryGetValue(item, out tmpCnter))
            {
                tmpCnter = new ItemCounter<T>(item);
                dict[item] = tmpCnter;

            }
            tmpCnter.cnt += cnt;
        }

        public void Clear()
        {
            dict.Clear();
            uniqCnt = 0;
            totalCnt = 0;
        }

        public long this[T item]
        {
            get
            {
                ItemCounter<T> cnter;
                if (!dict.TryGetValue(item, out cnter))
                {
                    return 0;
                }

                return cnter.cnt;
            }
        }

        public IEnumerable<T> Keys
        {
            get
            {
                return dict.Keys;
            }
        }

        public List<ItemCounter<T>> GetSortedList()
        {
            List<ItemCounter<T>> list = new List<ItemCounter<T>>();
            foreach (ItemCounter<T> cnter in dict.Values)
            {
                list.Add(cnter);
            }
            list.Sort();
            return list;
        }

        public List<ItemCounter<T>> GetSortedList(int cutoff)
        {
            List<ItemCounter<T>> list = new List<ItemCounter<T>>();
            foreach (ItemCounter<T> cnter in dict.Values)
            {
                if (cnter.cnt > cutoff)
                {
                    list.Add(cnter);
                }
            }
            list.Sort();
            return list;
        }

        public T[] GetSortedItemArray()
        {
            List<ItemCounter<T>> cnterList = GetSortedList();
            if (cnterList == null || cnterList.Count == 0)
            {
                return null;
            }
            T[] itemArr = new T[cnterList.Count];
            int id = 0;
            foreach (ItemCounter<T> cnter in cnterList)
            {
                itemArr[id++] = cnter.item;
            }
            return itemArr;
        }

        public T[] GetSortedItemArray(int cutoff)
        {
            List<ItemCounter<T>> cnterList = GetSortedList();
            if (cnterList == null || cnterList.Count == 0)
            {
                return null;
            }
            //T[] itemArr = new T[cnterList.Count];
            List<T> itemList = new List<T>();
            //int id = 0;
            foreach (ItemCounter<T> cnter in cnterList)
            {
                if (cnter.cnt > cutoff)
                {
                    itemList.Add(cnter.item);
                }
                //itemArr[id++] = cnter.item;
            }
            //return itemArr;
            return itemList.ToArray();
        }

        public Int64 ToTalItem
        {
            get { return totalCnt; }
        }

        public Int64 UniqueItem
        {
            get { return uniqCnt; }
        }

        private Int64 totalCnt;
        private Int64 uniqCnt;
    }

    
    public static class UnixStringCompare
    {
        /// <summary>
        /// Compare strings as unix sort: upper case letter precedes lowercase letter;
        /// </summary>
        public static int Compare(string A, string B)
        {
            int comp = string.Compare(A, B);
            if (comp == 0 || A == null || B == null)
            {
                return comp;
            }
            char[] achar = A.ToCharArray();
            char[] bchar = B.ToCharArray();
            for (int i = 0; i < achar.Length && i < bchar.Length; ++i)
            {
                if ((int)achar[i] < (int)bchar[i])
                {
                    return -1;
                }
                else if ((int)achar[i] > (int)bchar[i])
                {
                    return 1;
                }
            }
            return achar.Length.CompareTo(bchar.Length);
        }
    }


}
