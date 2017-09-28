using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace NanYUtilityLib
{
    public class Counter<T> : IEnumerable<KeyValuePair<T, int>>
    {
        Dictionary<T, int> dict = new Dictionary<T, int>();

        public Counter() { }

        public int this[T key]
        {
            get
            {
                int c;
                if (dict.TryGetValue(key, out c))
                {
                    return c;
                }
                else
                {
                    return 0;
                }
            }
        }

        public int Add(T key, int count = 1)
        {
            int cnt;

            if (dict.TryGetValue(key, out cnt))
            {
                cnt += count;
            }
            else
            {
                cnt = count;
            }

            dict[key] = cnt;

            return cnt;
        }

        public IEnumerator<KeyValuePair<T, int>> GetEnumerator()
        {
            return dict.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
