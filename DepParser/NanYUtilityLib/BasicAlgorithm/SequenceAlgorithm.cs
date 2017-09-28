using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NanYUtilityLib.BasicAlgorithm
{
    public static class SequenceAlgorithm
    {
        struct LCSCell
        {
            public int length;
            public int mask;

            public const int aMask = 1;
            public const int bMask = 2;
            public const int abMask = 4;
        }

        static public void LongestCommonSubSequence<T>(T[] a, T[] b, out List<int> atrace, out List<int> btrace)
        {
            LCSCell[,] cells = new LCSCell[a.Length, b.Length];

            LongestCommonSubSequence(a, b, a.Length - 1, b.Length - 1, cells);

            atrace = new List<int>();
            btrace = new List<int>();

            int apos = a.Length - 1;
            int bpos = b.Length - 1;

            while (apos >= 0 && bpos >= 0)
            {
                if ((cells[apos, bpos].mask & LCSCell.abMask) != 0)
                {
                    atrace.Add(apos);
                    btrace.Add(bpos);
                    apos -= 1;
                    bpos -= 1;
                }
                else if ((cells[apos, bpos].mask & LCSCell.aMask) != 0)
                {
                    apos -= 1;
                }
                else if ((cells[apos, bpos].mask & LCSCell.bMask) != 0)
                {
                    bpos -= 1;
                }
                else
                {
                    throw new Exception("Infinite LOOP in LCS!");
                }
            }

            atrace.Reverse();
            btrace.Reverse();
        }

        static void LongestCommonSubSequence<T>(T[] a, T[] b, int apos, int bpos, LCSCell[,] cells)
        {
            if (cells[apos, bpos].mask != 0)
            {
                return;
            }

            if (a[apos].Equals(b[bpos]))
            {
                if (apos == 0 || bpos == 0)
                {
                    var c = cells[apos, bpos];
                    c.length = 1;
                    c.mask = LCSCell.abMask;
                    cells[apos, bpos] = c;
                }
                else
                {
                    LongestCommonSubSequence(a, b, apos - 1, bpos - 1, cells);
                    var c = cells[apos, bpos];
                    c.length = 1 + cells[apos - 1, bpos - 1].length;
                    c.mask = LCSCell.abMask;
                    cells[apos, bpos] = c;
                }
            }
            else
            {
                int llen = 0;
                if (apos > 0)
                {
                    LongestCommonSubSequence(a, b, apos - 1, bpos, cells);
                    llen = cells[apos - 1, bpos].length;
                }

                int rlen = 0;
                if (bpos > 0)
                {
                    LongestCommonSubSequence(a, b, apos, bpos - 1, cells);
                    rlen = cells[apos, bpos - 1].length;
                }

                var c = cells[apos, bpos];
                if (llen >= rlen)
                {
                    c.mask |= LCSCell.aMask;
                    c.length = llen;
                }
                if (rlen >= llen)
                {
                    c.mask |= LCSCell.bMask;
                    c.length = rlen;
                }

                cells[apos, bpos] = c;
            }
        }
    }
}
