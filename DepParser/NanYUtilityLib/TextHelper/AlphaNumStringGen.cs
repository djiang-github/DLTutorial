using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NanYUtilityLib
{
    public class AlphaNumStringGen
    {
        public AlphaNumStringGen()
            :this(true)
        {
            
        }

        public AlphaNumStringGen(bool caseSensitive)
        {
            IsCaseSensitive = caseSensitive;
            Reset();
        }

        public void Reset()
        {
            counter = 0;
        }

        public string Next()
        {
            long x = counter++;
            int totalCnt = IsCaseSensitive ? NumCnt + 2 * AlphaCnt : NumCnt + AlphaCnt;
            StringBuilder sb = new StringBuilder();
            do
            {
                int y = (int)(x % totalCnt);

                if (y < NumCnt)
                {
                    sb.Append((char)('0' + y));
                }
                else if (y < NumCnt + AlphaCnt)
                {
                    sb.Append((char)('a' + y - NumCnt));
                }
                else
                {
                    sb.Append((char)('A' + y - NumCnt - AlphaCnt));
                }
                x /= totalCnt;
            } while (x > 0);

            return sb.ToString();
        }

        long counter;
        bool IsCaseSensitive;
        const int NumCnt = 10;
        const int AlphaCnt = 26;
    }
}
