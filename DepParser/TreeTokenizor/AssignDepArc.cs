using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace ConvertTreeTokenization
{
    public class DepArcConverter
    {
        public bool MapDepArc(
            string[] oriPoS,
            int[] oriHId,
            string[] oriArcType,
            string[] wbToken,
            RecoverWBAlignment.AlignSpan align,
            out int[] wbHId,
            out string[] wbArcType)
        {
            // check if input is valid;
            if (!ValidInput(oriPoS, oriHId, oriArcType, wbToken, align))
            {
                wbHId = null;
                wbArcType = null;
                return false;
            }

            wbHId = new int[wbToken.Length];
            wbArcType = new string[wbToken.Length];

            // foreach tokenized fragment, guess its head word and arc label of non-head word from the fragment itself.
            // foreach original fragment, guess its head word from original dep-arcs.
            // foreach original fragment, guess dep-arc and arc label of its head word from original dep-arcs.
            // foreach tokenized fragment, guess dep-arc and arc label of its head word from its corresponding original fragment.

            int[] wbFragInternalHead;
            int[] oriFragInternalHead;
            int[] oriFragHead;
            string[] oriFragArcType;

            return AssignWBFragmentInternalArcs(wbToken, wbHId, wbArcType, align, out wbFragInternalHead)
                && AssignOriFragmentInternalArcs(oriPoS, oriHId, align, out oriFragInternalHead)
                && AssignOriFragmentArcs(oriHId, oriArcType, oriFragInternalHead, out oriFragHead, out oriFragArcType)
                && AssignWBFragmentArcs(wbFragInternalHead, oriFragHead, oriFragArcType, align, wbHId, wbArcType);
        }

        private bool ValidInput(
            string[] oriPoS,
            int[] oriHId,
            string[] oriArcType,
            string[] wbToken,
            RecoverWBAlignment.AlignSpan align
            )
        {
            return oriPoS != null
                && oriHId != null
                && oriArcType != null
                && wbToken != null
                && align != null
                && oriPoS.Length == oriHId.Length
                && oriPoS.Length == oriArcType.Length
                && oriPoS.Length == align.SrcLen
                && wbToken.Length == align.TrgLen;
        }

        private bool AssignWBFragmentInternalArcs(
            string[] wbToken,
            int[] wbHId,
            string[] wbArcType,
            RecoverWBAlignment.AlignSpan align,
            out int[] wbFragInternalHead)
        {
            wbFragInternalHead = new int[wbToken.Length];
            for (int i = 0; i < wbFragInternalHead.Length; ++i)
            {
                wbFragInternalHead[i] = invalid_HId;
            }

            for (int wbBeg = 0; wbBeg < wbFragInternalHead.Length; ++wbBeg)
            {
                if (wbFragInternalHead[wbBeg] != invalid_HId)
                {
                    // already assigned head;
                    continue;
                }
                if (align.UnanlignedTrg(wbBeg))
                {
                    // tokenized words must align to some word..
                    Debug.WriteLine("Some tokenized word is not aligned to any raw words!");
                    return false;
                }
                int xbeg, spanLen;
                align.TrgSpanAlignToTrg(wbBeg, out xbeg, out spanLen);
                if (spanLen == 0 || xbeg != wbBeg)
                {
                    Debug.WriteLine("AlignSpan contains errors!");
                    return false;
                }

                int internalHeadId = FindWBInternalHead(wbToken, wbBeg, spanLen);

                Debug.Assert(internalHeadId >= wbBeg && internalHeadId < wbBeg + spanLen,
                    "wb Fragment has head word outside itself!");

                for (int wbId = wbBeg; wbId < wbBeg + spanLen; ++wbId)
                {
                    wbFragInternalHead[wbId] = internalHeadId;
                    wbHId[wbId] = ArrId2TreeId(internalHeadId);
                    wbArcType[wbId] = this.defaultArcType;
                }
            }
            return true;
        }

        private bool AssignOriFragmentInternalArcs(
            string[] oriPoS,
            int[] oriHId,
            RecoverWBAlignment.AlignSpan align,
            out int[] oriFragInternalHead)
        {
            oriFragInternalHead = new int[oriPoS.Length];
            for (int i = 0; i < oriFragInternalHead.Length; ++i)
            {
                oriFragInternalHead[i] = invalid_HId;
            }

            for (int oriBeg = 0; oriBeg < oriFragInternalHead.Length; ++oriBeg)
            {
                if (oriFragInternalHead[oriBeg] != invalid_HId || align.UnalignedSrc(oriBeg))
                {
                    continue;
                }

                int xbeg, spanLen;
                align.SrcSpanAlignToSrc(oriBeg, out xbeg, out spanLen);

                if (xbeg != oriBeg || spanLen < 1)
                {
                    Debug.WriteLine("Error in AlignSpan");
                    return false;
                }

                int head = FindOriInternalHead(oriPoS, oriHId, oriBeg, spanLen);

                Debug.Assert(head >= oriBeg && head < oriBeg + spanLen,
                    "Error in find fragment head!");

                for (int oriId = oriBeg; oriId < oriBeg + spanLen; ++oriId)
                {
                    oriFragInternalHead[oriId] = head;
                }
            }
            return true;
        }

        private bool AssignOriFragmentArcs(
            //string[] oriPoS,
            int[] oriHId,
            string[] oriArcType,
            int[] oriFragInternalHead,
            //RecoverWBAlignment.AlignSpan align,
            out int[] oriFragHead,
            out string[] oriFragArcType)
        {
            oriFragHead = new int[oriHId.Length];
            oriFragArcType = new string[oriHId.Length];

            for (int i = 0; i < oriFragHead.Length; ++i)
            {
                int fragInternalHId = oriFragInternalHead[i];
                if (fragInternalHId == invalid_HId)
                {
                    oriFragHead[i] = invalid_HId;
                    oriFragArcType[i] = nullArcType;
                }
                else
                {
                    Debug.Assert(fragInternalHId >= 0);
                    int treeH = oriHId[fragInternalHId];
                    int arrH = TreeId2ArrId(treeH);
                    oriFragHead[i] = arrH;
                    oriFragArcType[i] = oriArcType[fragInternalHId];
                }
            }

            return true;
        }

        private bool AssignWBFragmentArcs(
            int[] wbFragInternalHead,
            int[] oriFragHead,
            string[] oriFragArcType,
            RecoverWBAlignment.AlignSpan align,
            int[] wbHId,
            string[] wbArcType)
        {
            for (int i = 0; i < wbHId.Length; ++i)
            {
                if (wbFragInternalHead[i] == i)
                {
                    // it is the head of the wb fragment,
                    // we need to assign the head id and head arc;
                    Debug.Assert(!align.UnanlignedTrg(i));
                    int oriBeg, oriSpan;
                    align.SrcSpanAlignToTrg(i, out oriBeg, out oriSpan);
                    Debug.Assert(oriSpan > 0);
                    int oHead = oriFragHead[oriBeg];
                    string oArcType = oriFragArcType[oriBeg];

                    if(oHead == this.InternalRootId)
                    {
                        wbHId[i] = this.TreeRootId;
                        wbArcType[i] = this.rootArcType;
                        continue;
                    }

                    if (oHead == invalid_HId || align.UnalignedSrc(oHead))
                    {
                        return false;
                    }
                    
                    int wbFragBeg, wbSpan;
                    align.TrgSpanAlignToSrc(oHead, out wbFragBeg, out wbSpan);
                    Debug.Assert(wbSpan > 0);
                    int wbHead = wbFragInternalHead[wbFragBeg];
                    wbHId[i] = ArrId2TreeId(wbHead);
                    wbArcType[i] = oArcType;
                }
            }
            CheckTreeStructure(wbHId, wbArcType);
            return true;
        }

        [System.Diagnostics.Conditional("DEBUG")]
        private void CheckTreeStructure(int[] wbHId, string[] wbArcType)
        {
            if (wbHId == null || wbArcType == null)
            {
                Console.Error.WriteLine("head id or arctype array is null!");
                return;
            }
            if (wbHId.Length != wbArcType.Length)
            {
                Console.Error.WriteLine("Length of head id array does not match length of arctype array");
                return;
            }
            bool bHaveRoot = false;
            for (int i = 0; i < wbHId.Length; ++i)
            {
                if (wbHId[i] == this.TreeRootId)
                {
                    bHaveRoot = true;
                }
                if ((wbHId[i] == this.TreeRootId) ^ (wbArcType[i] == this.rootArcType))
                {
                    Console.Error.WriteLine("Root node does not have root arc type!");
                }
                if (wbHId[i] < 0 || wbHId[i] > wbHId.Length)
                {
                    Console.Error.WriteLine("head id exceed the tree terminal boundary!");
                }
                if (string.IsNullOrEmpty(wbArcType[i]))
                {
                    Console.Error.WriteLine("arc label is empty!");
                }
            }
            if (!bHaveRoot)
            {
                Console.Error.WriteLine("Tree does not have root!!!!");
            }
        }

        private int FindWBInternalHead(string[] wbToken, int wbBeg, int spanLen)
        {
            Debug.Assert(wbToken != null && spanLen > 0 && wbBeg >= 0 && wbBeg + spanLen <= wbToken.Length);
            bool bHaveNonPunc = false;
            int head = wbBeg;
            for (int i = wbBeg; i < wbBeg + spanLen; ++i)
            {
                if (!IsPuncToken(wbToken[i]))
                {
                    bHaveNonPunc = true;
                    head = i;
                }
                else if(!bHaveNonPunc)
                {
                    head = i;
                }
            }
            return head;
        }

        private int FindOriInternalHead(string[] oriPoS, int[] oriHId, int oriBeg, int spanLen)
        {
            Debug.Assert(oriPoS != null && spanLen > 0 && oriBeg >= 0 && oriBeg + spanLen <= oriPoS.Length);
            bool bHaveNonPunc = false;
            bool bHaveOutsideHead = false;
            bool bHaveRootHead = false;
            int head = oriBeg;
            for (int i = oriBeg; i < oriBeg + spanLen; ++i)
            {
                if (!IsPuncPoS(oriPoS[i]))
                {
                    bHaveNonPunc = true;
                    int treeH = oriHId[i];
                    if (treeH == TreeRootId)
                    {
                        bHaveRootHead = true;
                        bHaveOutsideHead = true;
                        head = i;
                    }else if(!bHaveRootHead
                        && (TreeId2ArrId(treeH) < oriBeg || TreeId2ArrId(treeH) >= oriBeg + spanLen))
                    {
                        bHaveOutsideHead = true;
                        head = i;
                    }else if(!bHaveOutsideHead)
                    {
                        head = i;
                    }
                }
                else if (!bHaveNonPunc)
                {
                    head = i;
                }
            }
            return head;
        }

        private int ArrId2TreeId(int arrId)
        {
            return arrId + 1;
        }

        private int TreeId2ArrId(int treeId)
        {
            return treeId - 1;
        }

        private string defaultArcType
        {
            get { return "dep"; }
        }

        private string rootArcType
        {
            get { return "root"; }
        }

        private string nullArcType
        {
            get { return "null"; }
        }

        private int TreeRootId
        {
            get { return 0; }
        }

        private int InternalRootId
        {
            get { return -1; }
        }

        private bool IsPuncToken(string token)
        {
            return PuncAndFactoidMatcher.IsPuncToken(token);
        }

        private bool IsPuncPoS(string pos)
        {
            return PuncAndFactoidMatcher.IsPuncPoS(pos);
        }

        private const int invalid_HId = -2; // must be less than -1; -1 indicate root;
    }
}
