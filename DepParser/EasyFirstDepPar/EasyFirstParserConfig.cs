using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LinearFunction;
using NanYUtilityLib;

namespace EasyFirstDepPar
{
    public class EasyFirstParserState
    {
        public int LeftID;
        public int RightID;
        public int[] state;
        public int BestCommand;
        public float BestScore;
    }

    public class EasyFirstParserConfig
    {
        public float score;

        public EasyFirstParserConfig(int[] tok, int[] pos, LinearModelCache cache, EasyFirstParserModelInfo parserModelInfo, ILinearFunction model)
        {
            this.parserModelInfo = parserModelInfo;
            this.model = model;

            this.tok = tok;
            this.pos = pos;
            this.cache = cache;
            cache.Clear();
            hid = new int[tok.Length];
            label = new int[tok.Length];
            score = 0;

            havechd = new int[tok.Length];
            lchdid = new int[tok.Length];
            rchdid = new int[tok.Length];
            isprep = new int[tok.Length];
            spanlen = new int[tok.Length];

            for (int i = 0; i < tok.Length; ++i)
            {
                hid[i] = -1;
                havechd[i] = 0;
                lchdid[i] = -1;
                rchdid[i] = -1;
                isprep[i] = pos[i] == parserModelInfo.PrepositionPoSId ? 1 : -1;
                spanlen[i] = 1;
            }

            scorebuffers = new float[tok.Length][];
            
            for(int i = 0;i < tok.Length; ++i)
            {
                scorebuffers[i] = new float[parserModelInfo.CommandCount];
            }

            commandbuffers = new int[tok.Length][];

            for (int i = 0; i < tok.Length; ++i)
            {
                commandbuffers[i] = new int[parserModelInfo.CommandCount];
            }

            states = new int[tok.Length][];

            for (int i = 0; i < tok.Length; ++i)
            {
                states[i] = new int[EasyFirstParserStateDescriptor.Count];
            }

            FragmentCount = tok.Length;

            if (FragmentCount <= 3)
            {
                Finish();
                return;
            }

            FragmentHeadIds = new int[tok.Length];
            
            bestCommand = new int[tok.Length];
            bestScore = new float[tok.Length];

            

            for (int i = 0; i < FragmentHeadIds.Length; ++i)
            {
                FragmentHeadIds[i] = i;
            }

            for (int i = 1; i < FragmentCount - 2; ++i)
            {
                Estimate(i);
            }
        }

        public bool IsEnd
        {
            get
            {
                return FragmentCount <= 3;
            }
        }

        public bool ApplyNextBest()
        {
            int bestFragmentID;
            EasyFirstParserAction maxpa;
            int maxlabel;

            if (!GetNextBestAction(out bestFragmentID, out maxpa, out maxlabel))
            {
                return false;
            }
            
            return ApplyCommand(maxpa, maxlabel, bestFragmentID);
        }

        public bool ApplyNextBest(out int cid, out int hid)
        {
            cid = -1;
            hid = -1;

            int bestFragmentID;
            EasyFirstParserAction maxpa;
            int maxlabel;

            if (!GetNextBestAction(out bestFragmentID, out maxpa, out maxlabel))
            {
                return false;
            }

            int lid = FragmentHeadIds[bestFragmentID];
            int rid = FragmentHeadIds[bestFragmentID + 1];

            if (maxpa == EasyFirstParserAction.LA)
            {
                hid = rid;
                cid = lid;
            }
            else
            {
                hid = lid;
                cid = rid;
            }

            return ApplyCommand(maxpa, maxlabel, bestFragmentID);
        }

        private bool GetNextBestAction(out int bestFragmentID, out EasyFirstParserAction maxpa, out int maxlabel)
        {
            if (IsEnd)
            {
                bestFragmentID = -1;
                maxpa = EasyFirstParserAction.NIL;
                maxlabel = -1;
                return false;
            }

            bestFragmentID = 1;
            int bestFragmentHeadId = FragmentHeadIds[bestFragmentID];
            float maxscore = bestScore[bestFragmentHeadId];
            for (int fid = 2; fid < FragmentCount - 2; ++fid)
            {
                int fhid = FragmentHeadIds[fid];
                float s = bestScore[fhid];
                if (s > maxscore)
                {
                    maxscore = s;
                    bestFragmentHeadId = fhid;
                    bestFragmentID = fid;
                }
            }

            int maxCommand = bestCommand[bestFragmentHeadId];

            maxpa = parserModelInfo.vocab.command2Action[maxCommand];
            maxlabel = parserModelInfo.vocab.command2Label[maxCommand];
            return true;
        }

        private bool GetNextBestConformingAction(
            int[] refHids, int[] refLabels, int[] refspanlen,
            out int bestFragmentID, out EasyFirstParserAction maxpa, out int maxlabel)
        {
            bestFragmentID = -1;
            maxpa = EasyFirstParserAction.NIL;
            maxlabel = -1;

            float maxscore = 0;

            if (IsEnd)
            {
                return false;
            }

            for (int fid = 1; fid < FragmentCount - 2; ++fid)
            {
                int fhid = FragmentHeadIds[fid];
                int[] cmdIds = commandbuffers[fhid];
                float[] cmdscores = scorebuffers[fhid];

                for (int i = 0; i < cmdIds.Length; ++i)
                {
                    EasyFirstParserAction pa = parserModelInfo.vocab.command2Action[cmdIds[i]];
                    int xlabel = parserModelInfo.vocab.command2Label[cmdIds[i]];
                    float s = cmdscores[i];

                    if (IsConformingAction(refHids, refLabels, refspanlen, fid, pa, xlabel))
                    {
                        if (bestFragmentID < 0 || maxscore < s)
                        {
                            bestFragmentID = fid;
                            maxpa = pa;
                            maxlabel = xlabel;
                            maxscore = s;
                        }
                    }
                }
            }

            return bestFragmentID >= 0;
        }

        public bool GetUpdate(
            int[] refHids, int[] refLabels, int[] refspanlen,
            out List<FeatureUpdatePackage> updateList)
        {
            updateList = new List<FeatureUpdatePackage>();
            int predictFid;
            EasyFirstParserAction predictpa;
            int predictlabel;
            int bestFid;
            EasyFirstParserAction bestpa;
            int bestlabel;

            if (!GetNextBestAction(out predictFid, out predictpa, out predictlabel)
                || !GetNextBestConformingAction(refHids, refLabels, refspanlen, out bestFid, out bestpa, out bestlabel))
            {
                return false;
            }

            if (predictFid == bestFid
                && predictlabel == bestlabel
                && predictpa == bestpa)
            {
                return true;
            }

            int predictCmd = parserModelInfo.vocab.GetCommand(predictpa, predictlabel);

            int bestCmd = parserModelInfo.vocab.GetCommand(bestpa, bestlabel);
            
                List<LinearModelFeature> trgfeatureList = new List<LinearModelFeature>();
                List<LinearModelFeature> reffeatureList = new List<LinearModelFeature>();
                cache.GetFeatures(states[FragmentHeadIds[predictFid]], trgfeatureList);
                cache.GetFeatures(states[FragmentHeadIds[bestFid]], reffeatureList);

                for (int j = 0; j < trgfeatureList.Count; ++j)
                {
                    bool nullFeat = false;
                    foreach (var x in trgfeatureList[j].ElemArr)
                    {
                        if (x < 0)
                        {
                            nullFeat = true;
                            break;
                        }
                    }

                    if (nullFeat)
                    {
                        continue;
                    }

                    FeatureUpdatePackage fup = new FeatureUpdatePackage(
                        trgfeatureList[j], predictCmd , -1);

                    updateList.Add(fup);
                }

                for (int j = 0; j < reffeatureList.Count; ++j)
                {
                    bool nullFeat = false;
                    foreach (var x in reffeatureList[j].ElemArr)
                    {
                        if (x < 0)
                        {
                            nullFeat = true;
                            break;
                        }
                    }

                    if (nullFeat)
                    {
                        continue;
                    }

                    FeatureUpdatePackage fup = new FeatureUpdatePackage(
                        reffeatureList[j], bestCmd, 1);
                    updateList.Add(fup);
                }

                

            if (updateList.Count == 0)
            {
                return true;
            }

            List<FeatureUpdatePackage> compactFUP = new List<FeatureUpdatePackage>();

            FeatureUpdatePackage tmpFUP = new FeatureUpdatePackage(updateList[0].feature, updateList[0].tag, updateList[0].delta);

            for (int i = 1; i < updateList.Count; ++i)
            {
                FeatureUpdatePackage thisFUP = updateList[i];

                if (thisFUP.IsMergeable(tmpFUP))
                {
                    tmpFUP.delta += thisFUP.delta;
                }
                else
                {
                    if (tmpFUP.delta != 0)
                    {
                        compactFUP.Add(tmpFUP);
                    }
                    tmpFUP = new FeatureUpdatePackage(thisFUP.feature, thisFUP.tag, thisFUP.delta);
                }
                if (i == updateList.Count - 1)
                {
                    if (tmpFUP.delta != 0)
                    {
                        compactFUP.Add(tmpFUP);
                    }
                }
            }

            updateList = compactFUP;

            return true;

        }

        public bool ApplyNextBest(int[] refHids, int[] refLabels, int[] refspanlen)
        {
            int bestFragmentID;
            EasyFirstParserAction maxpa;
            int maxlabel;

            if (!GetNextBestAction(out bestFragmentID, out maxpa, out maxlabel))
            {
                return false;
            }


            if (!IsConformingAction(refHids, refLabels, refspanlen, bestFragmentID, maxpa, maxlabel))
            {
                return false;
            }

            return ApplyCommand(maxpa, maxlabel, bestFragmentID);
        }

        private bool IsConformingAction(
            int[] refHids, int[] refLabels, int[] refspanlen, 
            int FragmentID, EasyFirstParserAction pa, int xlabel)
        {
            int bestFragmentHeadId = FragmentHeadIds[FragmentID];

            int leftfhid = bestFragmentHeadId;
            int rightfhid = FragmentHeadIds[FragmentID + 1];

            if (pa == EasyFirstParserAction.NIL)
            {
                return false;
            }
            else if (pa == EasyFirstParserAction.LA)
            {
                if (refHids[leftfhid] != rightfhid ||
                    spanlen[leftfhid] != refspanlen[leftfhid] ||
                    refLabels[leftfhid] != xlabel)
                {
                    return false;
                }
            }
            else if (pa == EasyFirstParserAction.RA)
            {
                if (refHids[rightfhid] != leftfhid ||
                    spanlen[rightfhid] != refspanlen[rightfhid] ||
                    refLabels[rightfhid] != xlabel)
                {
                    return false;
                }
            }

            return true;
        }

        public bool ApplyCommand(EasyFirstParserAction action, int xlabel, int FragmentId)
        {
            if (IsEnd)
            {
                return false;
            }

            if (FragmentId < 1 || FragmentId >= FragmentCount - 2)
            {
                return false;
            }

            int leftFhid = FragmentHeadIds[FragmentId];
            int rightFhid = FragmentHeadIds[FragmentId + 1];

            switch (action)
            {
                case EasyFirstParserAction.LA:
                    // right is head
                    hid[leftFhid] = rightFhid;
                    label[leftFhid] = xlabel;
                    havechd[rightFhid] = 1;
                    lchdid[rightFhid] = leftFhid;
                    spanlen[rightFhid] += spanlen[leftFhid];
                    FragmentCount--;

                    for (int i = FragmentId; i < FragmentCount; ++i)
                    {
                        FragmentHeadIds[i] = FragmentHeadIds[i + 1];
                    }

                    for (int i = FragmentId - 3; i < FragmentId + 2; ++i)
                    {
                        Estimate(i);
                    }

                    if (IsEnd)
                    {
                        Finish();
                    }

                    return true;
                case EasyFirstParserAction.RA:
                    // left is head;
                    hid[rightFhid] = leftFhid;
                    label[rightFhid] = xlabel;
                    havechd[leftFhid] = 1;
                    rchdid[leftFhid] = rightFhid;
                    spanlen[leftFhid] += spanlen[rightFhid];
                    FragmentCount--;

                    for (int i = FragmentId + 1; i < FragmentCount; ++i)
                    {
                        FragmentHeadIds[i] = FragmentHeadIds[i + 1];
                    }

                    for (int i = FragmentId - 2; i < FragmentId + 3; ++i)
                    {
                        Estimate(i);
                    }

                    if (IsEnd)
                    {
                        Finish();
                    }

                    return true;
                default:
                    return false;
            }

            
        }
        
        private void Estimate(int FragmentID)
        {
            if (FragmentID < 1 || FragmentID >= FragmentCount - 2)
            {
                return;
            }

            int FragmentHeadID = FragmentHeadIds[FragmentID];

            MakeState(states[FragmentHeadID], FragmentID);
            cache.GetScores(states[FragmentHeadID], scorebuffers[FragmentHeadID]);

            for (int i = 0; i < commandbuffers[FragmentHeadID].Length; ++i)
            {
                commandbuffers[FragmentHeadID][i] = i;
            }

            UtilFunc.SortAgainstScore<int>(commandbuffers[FragmentHeadID], scorebuffers[FragmentHeadID]);

            bestCommand[FragmentHeadID] = commandbuffers[FragmentHeadID][0];
            bestScore[FragmentHeadID] = scorebuffers[FragmentHeadID][0];
        }

        private bool IsRealTokenRange(int id)
        {
            return id >= 1 && id < tok.Length - 1;
        }

        private bool IsValidRange(int id)
        {
            return id >= 0 && id < tok.Length;
        }

        private int HaveChild(int id)
        {
            return IsRealTokenRange(id) ? havechd[id] : -1;
        }

        private int SpanLength(int id)
        {
            return IsRealTokenRange(id) ? QuantDistance(spanlen[id]) : -1;
        }

        private int HeadDist(int lid, int rid)
        {
            return (IsRealTokenRange(lid) && IsRealTokenRange(rid)) ? QuantDistance(rid - lid) : -1;
        }

        private int IsPrep(int id)
        {
            return IsRealTokenRange(id) ? isprep[id] : -1;
        }

        private int POS(int id)
        {
            return IsValidRange(id) ? pos[id] : -1;
        }

        private int Token(int id)
        {
            return IsValidRange(id) ? tok[id] : -1;
        }

        private int LChdId(int id)
        {
            return IsRealTokenRange(id) ? (IsRealTokenRange(lchdid[id]) ? lchdid[id] : -1) : -1;
        }

        private int RChdId(int id)
        {
            return IsRealTokenRange(id) ? (IsRealTokenRange(rchdid[id]) ? rchdid[id] : -1) : -1;
        }

        private int LChdPOS(int id)
        {
            int lid = LChdId(id);
            return IsRealTokenRange(lid) ? pos[lid] : -1;
        }

        private int LChdToken(int id)
        {
            int lid = LChdId(id);
            return IsRealTokenRange(lid) ? tok[lid] : -1;
        }

        private int RChdPOS(int id)
        {
            int rid = RChdId(id);
            return IsRealTokenRange(rid) ? pos[rid] : -1;
        }

        private int RChdToken(int id)
        {
            int rid = RChdId(id);
            return IsRealTokenRange(rid) ? tok[rid] : -1;
        }

        private void MakeState(int[] xstate, int FragmentID)
        {
            int l0id = FragmentHeadIds[FragmentID];
            int l1id = FragmentID - 1 >= 0 ? FragmentHeadIds[FragmentID - 1] : -1;
            int l2id = FragmentID - 2 >= 0 ? FragmentHeadIds[FragmentID - 2] : -1;
            int r0id = FragmentHeadIds[FragmentID + 1];
            int r1id = FragmentID + 1 < FragmentCount ? FragmentHeadIds[FragmentID + 1] : -1;
            int r2id = FragmentID + 2 < FragmentCount ? FragmentHeadIds[FragmentID + 2] : -1;

            xstate[EasyFirstParserStateDescriptor.l0id] = l0id;//0;
        xstate[EasyFirstParserStateDescriptor.l1id] = l1id;//1;
        xstate[EasyFirstParserStateDescriptor.l2id] = l2id;//2;
        xstate[EasyFirstParserStateDescriptor.r0id] = r0id;//3;
        xstate[EasyFirstParserStateDescriptor.r1id] = r1id;//4;
        xstate[EasyFirstParserStateDescriptor.r2id] = r2id;//5;
        xstate[EasyFirstParserStateDescriptor.l0len] = SpanLength(l0id);//6;
        xstate[EasyFirstParserStateDescriptor.l1len] = SpanLength(l1id);//7;
        xstate[EasyFirstParserStateDescriptor.l2len] = SpanLength(l2id);//8;
        xstate[EasyFirstParserStateDescriptor.r0len] = SpanLength(r0id);//9;
        xstate[EasyFirstParserStateDescriptor.r1len] = SpanLength(r1id);//10;
        xstate[EasyFirstParserStateDescriptor.r2len] = SpanLength(r2id);//11;
        xstate[EasyFirstParserStateDescriptor.l0hc] = HaveChild(l0id);//12;
        xstate[EasyFirstParserStateDescriptor.l1hc] = HaveChild(l1id);//13;
        xstate[EasyFirstParserStateDescriptor.l2hc] = HaveChild(l2id);//14;
        xstate[EasyFirstParserStateDescriptor.r0hc] = HaveChild(r0id);//15;
        xstate[EasyFirstParserStateDescriptor.r1hc] = HaveChild(r1id);//16;
        xstate[EasyFirstParserStateDescriptor.r2hc] = HaveChild(r2id);//17;
        xstate[EasyFirstParserStateDescriptor.r3hc] = -1;//18;
        xstate[EasyFirstParserStateDescriptor.l2l1dst] = HeadDist(l2id, l1id);//19;
        xstate[EasyFirstParserStateDescriptor.l1l0dst] = HeadDist(l1id, l0id);//20;
        xstate[EasyFirstParserStateDescriptor.l0r0dst] = HeadDist(l0id, r0id);//21;
        xstate[EasyFirstParserStateDescriptor.r0r1dst] = HeadDist(r0id, r1id);//22;
        xstate[EasyFirstParserStateDescriptor.r1r2dst] = HeadDist(r1id, r2id);//23;
        xstate[EasyFirstParserStateDescriptor.l0t] = POS(l0id);//24;
        xstate[EasyFirstParserStateDescriptor.l1t] = POS(l1id);//25;
        xstate[EasyFirstParserStateDescriptor.l2t] = POS(l2id);//26;
        xstate[EasyFirstParserStateDescriptor.r0t] = POS(r0id);//27;
        xstate[EasyFirstParserStateDescriptor.r1t] = POS(r1id);//28;
        xstate[EasyFirstParserStateDescriptor.r2t] = POS(r2id);//29;
        xstate[EasyFirstParserStateDescriptor.l0w] = Token(l0id);//30;
        xstate[EasyFirstParserStateDescriptor.l1w] = Token(l1id);//31;
        xstate[EasyFirstParserStateDescriptor.l2w] = Token(l2id);//32;
        xstate[EasyFirstParserStateDescriptor.r0w] = Token(r0id);//33;
        xstate[EasyFirstParserStateDescriptor.r1w] = Token(r1id);//34;
        xstate[EasyFirstParserStateDescriptor.r2w] = Token(r2id);//35;
        xstate[EasyFirstParserStateDescriptor.l0lct] = LChdPOS(l0id);//36;
        xstate[EasyFirstParserStateDescriptor.l1lct] = LChdPOS(l1id);//37;
        xstate[EasyFirstParserStateDescriptor.l2lct] = LChdPOS(l2id);
        xstate[EasyFirstParserStateDescriptor.r0lct] = LChdPOS(r0id);
        xstate[EasyFirstParserStateDescriptor.r1lct] = LChdPOS(r1id);
        xstate[EasyFirstParserStateDescriptor.r2lct] = LChdPOS(r2id);
        xstate[EasyFirstParserStateDescriptor.l0rct] = RChdPOS(l0id);
        xstate[EasyFirstParserStateDescriptor.l1rct] = RChdPOS(l1id);
        xstate[EasyFirstParserStateDescriptor.l2rct] = RChdPOS(l2id);
        xstate[EasyFirstParserStateDescriptor.r0rct] = RChdPOS(r0id);
        xstate[EasyFirstParserStateDescriptor.r1rct] = RChdPOS(r1id);
        xstate[EasyFirstParserStateDescriptor.r2rct] = RChdPOS(r2id);

        xstate[EasyFirstParserStateDescriptor.l0lcw] = LChdToken(l0id);
        xstate[EasyFirstParserStateDescriptor.l1lcw] = LChdToken(l1id);
        xstate[EasyFirstParserStateDescriptor.l2lcw] = LChdToken(l2id);
        xstate[EasyFirstParserStateDescriptor.r0lcw] = LChdToken(r0id);
        xstate[EasyFirstParserStateDescriptor.r1lcw] = LChdToken(r1id);
        xstate[EasyFirstParserStateDescriptor.r2lcw] = LChdToken(r2id);
        

        xstate[EasyFirstParserStateDescriptor.l0rcw] = RChdToken(l0id);
        xstate[EasyFirstParserStateDescriptor.l1rcw] = RChdToken(l1id);
        xstate[EasyFirstParserStateDescriptor.l2rcw] = RChdToken(l2id);
        xstate[EasyFirstParserStateDescriptor.r0rcw] = RChdToken(r0id);
        xstate[EasyFirstParserStateDescriptor.r1rcw] = RChdToken(r1id);
        xstate[EasyFirstParserStateDescriptor.r2rcw] = RChdToken(r2id);

        xstate[EasyFirstParserStateDescriptor.l0prep] = IsPrep(l0id);//60;
        xstate[EasyFirstParserStateDescriptor.l1prep] = IsPrep(l1id);
        xstate[EasyFirstParserStateDescriptor.l2prep] = IsPrep(l2id);
        xstate[EasyFirstParserStateDescriptor.r0prep] = IsPrep(r0id);
        xstate[EasyFirstParserStateDescriptor.r1prep] = IsPrep(r1id);
        xstate[EasyFirstParserStateDescriptor.r2prep] = IsPrep(r2id);

            xstate[EasyFirstParserStateDescriptor.l0larc] = LChdLabel(l0id);//60;
        xstate[EasyFirstParserStateDescriptor.l1larc] = LChdLabel(l1id);
        xstate[EasyFirstParserStateDescriptor.l2larc] = LChdLabel(l2id);
        xstate[EasyFirstParserStateDescriptor.r0larc] = LChdLabel(r0id);
        xstate[EasyFirstParserStateDescriptor.r1larc] = LChdLabel(r1id);
        xstate[EasyFirstParserStateDescriptor.r2larc] = LChdLabel(r2id);

            xstate[EasyFirstParserStateDescriptor.l0rarc] = RChdLabel(l0id);//60;
        xstate[EasyFirstParserStateDescriptor.l1rarc] = RChdLabel(l1id);
        xstate[EasyFirstParserStateDescriptor.l2rarc] = RChdLabel(l2id);
        xstate[EasyFirstParserStateDescriptor.r0rarc] = RChdLabel(r0id);
        xstate[EasyFirstParserStateDescriptor.r1rarc] = RChdLabel(r1id);
        xstate[EasyFirstParserStateDescriptor.r2rarc] = RChdLabel(r2id);

            

        }

        private int QuantDistance(int d)
        {
            if (d < 4)
            {
                return d;
            }
            else if (d < 5)
            {
                return 4;
            }
            else if (d < 10)
            {
                return 5;
            }
            else
            {
                return 6;
            }
        }

        private int LChdLabel(int id)
        {
            int lc = LChdId(id);
            return IsRealTokenRange(lc) ? label[lc] : -1;
        }

        private int RChdLabel(int id)
        {
            int rc = RChdId(id);
            return IsRealTokenRange(rc) ? label[rc] : -1;
        }

        private void Finish()
        {
            for (int i = 1; i < hid.Length - 1; ++i)
            {
                if (hid[i] <= 0)
                {
                    hid[i] = 0;
                }
            }
        }


        public int[] tok;
        public int[] pos;
        public int[] hid;
        public int[] label;

        private int[] havechd;
        private int[] lchdid;
        private int[] rchdid;
        private int[] isprep;

        private int[] spanlen;

        private int FragmentCount;
        private int[] FragmentHeadIds;
        
        private int[] bestCommand;
        private float[] bestScore;

        private int[][] states;
        private float[][] scorebuffers;
        private int[][] commandbuffers;

        
        LinearModelCache cache;
        EasyFirstParserModelInfo parserModelInfo;
        ILinearFunction model;
    }
}
