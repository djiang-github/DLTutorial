using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NanYUtilityLib
{
    public class ObjectShuffle
    {
        static public void Swap<T>(T[] ArrayToShuffle, int idxA, int idxB)
        {
            if (idxA == idxB)
            {
                return;
            }
            T tmp = ArrayToShuffle[idxA];
            ArrayToShuffle[idxA] = ArrayToShuffle[idxB];
            ArrayToShuffle[idxB] = tmp;
        }

        static public void Swap<T>(List<T> ListToShuffle, int idxA, int idxB)
        {
            if (idxA == idxB)
            {
                return;
            }
            T tmp = ListToShuffle[idxA];
            ListToShuffle[idxA] = ListToShuffle[idxB];
            ListToShuffle[idxB] = tmp;
        }

        static public void ShuffleArr<T>(T[] ArrayToShuffle, Random r)
        {
            for (int i = 0; i < ArrayToShuffle.Length; ++i)
            {
                int randpos = i + r.Next(ArrayToShuffle.Length - 1 - i);
                Swap<T>(ArrayToShuffle, i, randpos);
            }
        }

        static public void ShuffleArr<T>(List<T> ListToShuffle, Random r)
        {
            if (ListToShuffle == null || ListToShuffle.Count <= 0)
            {
                return;
            }

            for (int i = 0; i < ListToShuffle.Count; ++i)
            {
                int randpos = i + r.Next(ListToShuffle.Count - 1 - i);
                Swap<T>(ListToShuffle, i, randpos);
            }
        }
    }
}
