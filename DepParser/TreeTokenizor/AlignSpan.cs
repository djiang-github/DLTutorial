using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RecoverWBAlignment
{
    public class AlignSpan
    {
        public AlignSpan(int srcLen, int trgLen, string alignStr)
        {
            this._srcLen = srcLen;
            this._trgLen = trgLen;
            this.srcMapping = new int[srcLen];
            this.trgMapping = new int[trgLen];
            ClearMapping();

            List<string> alignList = GetSortedAlignPairList(alignStr);

            foreach (string alignPair in alignList)
            {
                int srcId;
                int trgId;
                if (GetAlignmentFromStr(alignPair, out srcId, out trgId))
                {
                    if (!SrcHaveAlign(srcId) && !TrgHaveAlign(trgId))
                    {
                        srcMapping[srcId] = trgId;
                        trgMapping[trgId] = srcId;
                    }
                    else if (!SrcHaveAlign(srcId))
                    {
                        srcMapping[srcId] = srcMapping[trgMapping[trgId]];
                    }
                    else if (!TrgHaveAlign(trgId))
                    {
                        trgMapping[trgId] = trgMapping[srcMapping[srcId]];
                    }
                }
            }
        }

        public bool IsCorrectAlignSpan
        {
            get
            {
                // check for correctness.
                return CheckSrcSide() && CheckTrgSide();
            }
        }

        public bool IsAligned(int srcId, int trgId)
        {
            if (!ValidSrcId(srcId) || !ValidTrgId(trgId))
            {
                return false;
            }
            if (!SrcHaveAlign(srcId) || !TrgHaveAlign(trgId))
            {
                return false;
            }
            return InSameTrgSpan(TrgGroupAlignedToSrc(srcId), trgId);
        }

        public bool this[int srcId, int trgId]
        {
            get
            {
                return IsAligned(srcId, trgId);
            }
        }

        public bool UnalignedSrc(int srcId)
        {
            if (!ValidSrcId(srcId))
            {
                return false;
            }

            return !SrcHaveAlign(srcId);
        }

        public bool UnanlignedTrg(int trgId)
        {
            if (!ValidTrgId(trgId))
            {
                return false;
            }

            return !TrgHaveAlign(trgId);
        }

        public void SpanAlignToSrc(int srcId, out int srcBeg, out int srcSpanLen, out int trgBeg, out int trgSpanLen)
        {
            SrcSpanAlignToSrc(srcId, out srcBeg, out srcSpanLen);
            TrgSpanAlignToSrc(srcId, out trgBeg, out trgSpanLen);
        }

        public void SpanAlignToTrg(int trgId, out int srcBeg, out int srcSpanLen, out int trgBeg, out int trgSpanLen)
        {
            SrcSpanAlignToTrg(trgId, out srcBeg, out srcSpanLen);
            TrgSpanAlignToTrg(trgId, out trgBeg, out trgSpanLen);
        }

        public void SrcSpanAlignToSrc(int srcId, out int srcBeg, out int spanLen)
        {
            if (!ValidSrcId(srcId))
            {
                srcBeg = 0;
                spanLen = 0;
                return;
            }
            if (!SrcHaveAlign(srcId))
            {
                srcBeg = srcId;
                spanLen = 1;
                return;
            }
            //int group = TrgGroupAlignedToSrc(srcId);
            srcBeg = srcId;
            while (srcBeg > 0 && InSameSrcSpan(srcBeg - 1, srcId))
            {
                srcBeg--;
            }
            int srcEnd = srcId + 1;
            while (srcEnd < this._srcLen && InSameSrcSpan(srcEnd, srcId))
            {
                srcEnd++;
            }
            spanLen = srcEnd - srcBeg;
        }

        public void TrgSpanAlignToSrc(int srcId, out int trgBeg, out int spanLen)
        {
            
            if (!ValidSrcId(srcId) || !SrcHaveAlign(srcId))
            {
                trgBeg = -1;
                spanLen = 0;
                return;
            }
            
            int trgId = TrgGroupAlignedToSrc(srcId);

            TrgSpanAlignToTrg(trgId, out trgBeg, out spanLen);
        }

        public void SrcSpanAlignToTrg(int trgId, out int srcBeg, out int spanLen)
        {
            if (!ValidTrgId(trgId) || !TrgHaveAlign(trgId))
            {
                srcBeg = -1;
                spanLen = 0;
                return;
            }

            int srcId = SrcGroupAlignedToTrg(trgId);

            SrcSpanAlignToSrc(srcId, out srcBeg, out spanLen);
        }

        public void TrgSpanAlignToTrg(int trgId, out int trgBeg, out int spanLen)
        {
            if (!ValidTrgId(trgId))
            {
                trgBeg = -1;
                spanLen = 0;
                return;
            }
            if (!TrgHaveAlign(trgId))
            {
                trgBeg = trgId;
                spanLen = 1;
                return;
            }
            //int group = SrcGroupAlignedToTrg(trgId);
            trgBeg = trgId;
            while (trgBeg > 0 && InSameTrgSpan(trgBeg - 1, trgId))
            {
                trgBeg--;
            }
            int trgEnd = trgId + 1;
            while (trgEnd < this._trgLen && InSameTrgSpan(trgEnd, trgId))
            {
                trgEnd++;
            }
            spanLen = trgEnd - trgBeg;
        }

        public int SrcLen
        {
            get
            {
                return this._srcLen;
            }
        }

        public int TrgLen
        {
            get
            {
                return this._trgLen;
            }
        }

        private bool SrcHaveAlign(int srcId)
        {
            return srcMapping[srcId] != INVALID_MAPPING_ID;
        }

        private bool TrgHaveAlign(int trgId)
        {
            return trgMapping[trgId] != INVALID_MAPPING_ID;
        }

        private int TrgGroupAlignedToSrc(int srcId)
        {
            return srcMapping[srcId];
        }

        private int SrcGroupAlignedToTrg(int trgId)
        {
            return trgMapping[trgId];
        }

        private bool ValidSrcId(int srcId)
        {
            return srcId >= 0 && srcId < this._srcLen;
        }

        private bool ValidTrgId(int trgId)
        {
            return trgId >= 0 && trgId < this._trgLen;
        }

        private bool InSameSrcSpan(int srcIdA, int srcIdB)
        {
            return (srcIdA == srcIdB) ||
                (SrcHaveAlign(srcIdA) && SrcHaveAlign(srcIdB)
                && TrgGroupAlignedToSrc(srcIdA) == TrgGroupAlignedToSrc(srcIdB));
        }

        private bool InSameTrgSpan(int trgIdA, int trgIdB)
        {
            return (trgIdA == trgIdB) ||
                (TrgHaveAlign(trgIdA) && TrgHaveAlign(trgIdB)
                && SrcGroupAlignedToTrg(trgIdA) == SrcGroupAlignedToTrg(trgIdB));
        }

        private List<string> GetSortedAlignPairList(string alignStr)
        {
            string[] alignPairArr = alignStr.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            List<string> alignList = new List<string>(alignPairArr);
            alignList.Sort();
            return alignList;
        }

        private bool GetAlignmentFromStr(string alignPair, out int srcId, out int trgId)
        {
            srcId = 0;
            trgId = 0;
            if (string.IsNullOrWhiteSpace(alignPair))
            {
                return false;
            }
            string[] parts = alignPair.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
            if (parts == null || parts.Length != 2)
            {
                return false;
            }
            if (!int.TryParse(parts[0], out srcId) || !int.TryParse(parts[1], out trgId))
            {
                return false;
            }
            if (!ValidSrcId(srcId) || !ValidTrgId(trgId))
            {
                return false;
            }
            return true;
        }

        private void ClearMapping()
        {
            for (int i = 0; i < srcMapping.Length; ++i)
            {
                srcMapping[i] = INVALID_MAPPING_ID;
            }
            for (int i = 0; i < trgMapping.Length; ++i)
            {
                trgMapping[i] = INVALID_MAPPING_ID;
            }
        }

        private bool CheckTrgSide()
        {
            int trgBeg = 0;
            HashSet<int> alignedSrcGroup = new HashSet<int>();
            while (trgBeg < _trgLen)
            {
                if (!TrgHaveAlign(trgBeg))
                {
                    trgBeg++;
                    continue;
                }
                int srcGroup = SrcGroupAlignedToTrg(trgBeg);
                if (!ValidSrcId(srcGroup) || alignedSrcGroup.Contains(srcGroup))
                {
                    return false;
                }
                alignedSrcGroup.Add(srcGroup);
                int trgEnd = trgBeg + 1;
                while (trgEnd < _trgLen && InSameTrgSpan(trgEnd, trgBeg))
                {
                    trgEnd++;
                }
                int trgGroup = TrgGroupAlignedToSrc(srcGroup);
                if (trgGroup < trgBeg || trgGroup >= trgEnd)
                {
                    return false;
                }
                trgBeg = trgEnd;
            }
            return true;
        }

        private bool CheckSrcSide()
        {
            int srcBeg = 0;
            HashSet<int> alignedTrgGroup = new HashSet<int>();
            while (srcBeg < _srcLen)
            {
                if (!SrcHaveAlign(srcBeg))
                {
                    srcBeg++;
                    continue;
                }
                int trgGroup = TrgGroupAlignedToSrc(srcBeg);
                if (!ValidTrgId(trgGroup) || alignedTrgGroup.Contains(trgGroup))
                {
                    // discontinuous span.
                    return false;
                }
                alignedTrgGroup.Add(trgGroup);
                int srcEnd = srcBeg + 1;
                while (srcEnd < _srcLen && InSameSrcSpan(srcBeg, srcEnd))
                {
                    srcEnd++;
                }
                int srcGroup = SrcGroupAlignedToTrg(trgGroup);
                if (srcGroup < srcBeg || srcGroup >= srcEnd)
                {
                    return false;
                }
                srcBeg = srcEnd;
            }

            return true;
        }


        private readonly int _srcLen;
        private readonly int _trgLen;
        private readonly int[] srcMapping;
        private readonly int[] trgMapping;
        private const int INVALID_MAPPING_ID = -1;
    }
}
