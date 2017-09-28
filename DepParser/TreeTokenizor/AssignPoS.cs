using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RecoverWBAlignment;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace ConvertTreeTokenization
{
    public class PoSConverter
    {
        public bool MapPoS(string[] oriPosArr, string[] wbTokens, AlignSpan align, out string[] wbPosArr)
        {
            if (oriPosArr == null || wbTokens == null || align == null
                || oriPosArr.Length != align.SrcLen || wbTokens.Length != align.TrgLen)
            {
                wbPosArr = null;
                return false;
            }

            wbPosArr = new string[wbTokens.Length];

            int wbId = 0;
            while (wbId < wbPosArr.Length)
            {
                int wbBeg, wbLen;
                int oriBeg, oriLen;
                align.SpanAlignToTrg(wbId, out oriBeg, out oriLen, out wbBeg, out wbLen);

                if (oriLen == 0 || wbLen == 0)
                {
                    wbPosArr = null;
                    return false;
                }

                if (oriLen == 1 && wbLen == 1)
                {
                    // trival case; assign the original PoS;
                    wbPosArr[wbBeg] = oriPosArr[oriBeg];
                }
                else
                {
                    string oriPoS = GetOriPoS(oriPosArr, oriBeg, oriLen);
                    for (int wid = wbBeg; wid < wbBeg + wbLen; ++wid)
                    {
                        if (!TryAssignPoSByRule(wbTokens[wid], out wbPosArr[wid]))
                        {
                            wbPosArr[wid] = oriPoS;
                        }
                    }
                }

                wbId = wbBeg + wbLen;
            }

            foreach (string pos in wbPosArr)
            {
                if (pos == null)
                {
                    wbPosArr = null;
                    return false;
                }
            }

            return true;
        }

        private string GetOriPoS(string[] oriPosArr, int oriBeg, int oriLen)
        {
            Debug.Assert(oriPosArr != null && oriLen >= 1 && oriBeg >= 0 && oriBeg + oriLen <= oriPosArr.Length);

            if (oriLen == 1)
            {
                return oriPosArr[oriBeg];
            }

            bool bHaveNonPunc = false;
            string PoS = null;
            for (int i = oriBeg; i < oriBeg + oriLen; ++i)
            {
                string thisPoS = oriPosArr[i];
                if (!IsPuncPoS(thisPoS))
                {
                    bHaveNonPunc = true;
                    PoS = thisPoS;
                }
                else if (!bHaveNonPunc)
                {
                    PoS = thisPoS;
                }
            }

            Debug.Assert(!string.IsNullOrWhiteSpace(PoS));

            return PoS;
        }

        private bool TryAssignPoSByRule(string token, out string PoS)
        {
            if (TryMatchFactoid(token, out PoS))
            {
                return true;
            }
            else if (TryMatchPunc(token, out PoS))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool TryMatchFactoid(string token, out string PoS)
        {
            return PuncAndFactoidMatcher.TryMatchFactoid(token, out PoS);
        }

        private bool TryMatchPunc(string token, out string PoS)
        {
            return PuncAndFactoidMatcher.TryMatchPunc(token, out PoS);
        }

        private bool IsPuncPoS(string PoS)
        {
            return PuncAndFactoidMatcher.IsPuncPoS(PoS);
        }
    }

}
