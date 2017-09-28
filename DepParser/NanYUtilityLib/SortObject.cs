using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NanYUtilityLib
{
    public class SortObject<T> : IComparable<SortObject<T>>
    {
        public T Item { get; set; }
        public double Score { get; set; }
        public SortObject()
        { }

        public SortObject(T Item, double Score)
        {
            this.Item = Item;
            this.Score = Score;
        }

        int IComparable<SortObject<T>>.CompareTo(SortObject<T> other)
        {
            if (other == null)
            {
                return 1;
            }

            return other.Score.CompareTo(Score);
        }
    }
}
