using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NanYUtilityLib
{
    public class SetDictionary<Key, Value> : Dictionary<Key, HashSet<Value>>
    {
        public SetDictionary()
        {
        }

        public void Add(Key k, Value v)
        {
            HashSet<Value> s;

            if (!base.TryGetValue(k, out s))
            {
                s = new HashSet<Value>();

                base.Add(k, s);
            }

            if (!s.Contains(v))
            {
                s.Add(v);
            }
        }
    }
}
