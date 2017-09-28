using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NanYUtilityLib;
using LinearFunction;

namespace LinearDepParser
{
    public class ParserDecoder
    {
        public ParserDecoder(ParserModelInfo parserModelInfo, ILinearFunction model, int beamsize)
        {
            scoreHeap = new HeapWithSingleScore<ulong>(beamsize);
            this.beamsize = beamsize;
            modelCache = new LinearModelCache(model, parserModelInfo.lmInfo.TemplateSet);
            //ParserModelCache(featureGroups, model);
            Beam = new ParserConfig[beamsize];
            Buffer = new ParserConfig[beamsize];
            this.vocab = parserModelInfo.vocab;
            scorebuffer = new float[parserModelInfo.CommandCount];
            cmdbuffer = new int[parserModelInfo.CommandCount];
            PMInfo = parserModelInfo;
            NBestScore = new float[beamsize];
            NBestCmd = new int[beamsize];
            NBestPrevId = new int[beamsize];
        
        }

        public void SetBeamSize(int beamsize)
        {
            this.beamsize = beamsize;
            scoreHeap = new HeapWithSingleScore<ulong>(beamsize);
            Beam = new ParserConfig[beamsize];
            Buffer = new ParserConfig[beamsize];
        }

        public bool Run(string[] toks, string[] pos, out int[] hid, out string[] labels)
        {
            return Run(toks, pos, null, out hid, out labels);
        }

        public bool Run(string[] toks, string[] pos, out int[][] hid, out string[][] labels)
        {
            return Run(toks, pos, null, out hid, out labels);
        }

        public bool Evaluate(string[] toks, string[] pos, int[] hid, string[] labels, out float score)
        {
            int[] btok = vocab.BinarizeWithPadding(toks);
            int[] bpos = vocab.BinarizeWithPadding(pos);
            int[] blabels = vocab.BinarizeLabel(hid, labels);
            score = 0;

            for (int i = 0; i < blabels.Length; ++i)
            {
                if (hid[i] != 0 && blabels[i] < 0)
                {
                    return false;
                }
            }

            string[] wcstr = GetWordCluster(toks);

            int[] bwcluster = null;

            if (wcstr != null)
            {
                bwcluster = vocab.BinarizeWithPadding(wcstr);
            }

            Initialize(btok, bpos, bwcluster);

            InitializeRef(hid, blabels);


            for (int i = 0; i < RefCommands.Length && !IsEnd; ++i)
            {
                GetScores(Beam[0].state, scorebuffer);
                if (!TryApplyCommand(RefCommands[i], Beam[0]))
                {
                    score = 0;
                    return false;
                }
                score += scorebuffer[RefCommands[i]];
            }

            return true;
        }

        public bool Run(string[] toks, string[] pos, ParserForcedConstraints constraints, out int[] hid, out string[] labels)
        {
            hid = null;
            labels = null;
            int[] btok = vocab.BinarizeWithPadding(toks);
            int[] bpos = vocab.BinarizeWithPadding(pos);

            string[] wcstr = GetWordCluster(toks);

            int[] bwcluster = null;

            if (wcstr != null)
            {
                bwcluster = vocab.BinarizeWithPadding(wcstr);
            }

            Initialize(btok, bpos, bwcluster);

            while (!IsEnd)
            {
                if (!ParseNext(constraints))
                {
                    return false;
                }
            }

            hid = Beam[0].HeadIDs();
            int[] blabls = Beam[0].ArcLabels();

            labels = new string[toks.Length];

            for (int i = 0; i < blabls.Length; ++i)
            {
                if (blabls[i] < 0)
                {
                    labels[i] = "root";
                }
                else
                {
                    labels[i] = vocab.LabelId2Name[blabls[i]];
                }
            }

            return true;
        }

        public bool Run(string[] toks, string[] pos, ParserForcedConstraints constraints, out int[][] hidArr, out string[][] labelsArr)
        {
            hidArr = null;
            labelsArr = null;
            int[] btok = vocab.BinarizeWithPadding(toks);
            int[] bpos = vocab.BinarizeWithPadding(pos);

            string[] wcstr = GetWordCluster(toks);

            int[] bwcluster = null;

            if (wcstr != null)
            {
                bwcluster = vocab.BinarizeWithPadding(wcstr);
            }

            Initialize(btok, bpos, bwcluster);

            while (!IsEnd)
            {
                if (!ParseNext(constraints))
                {
                    return false;
                }
            }
            List<int[]> hidList = new List<int[]>();
            List<string[]> lablList = new List<string[]>();

            for (int i = 0; i < nodeInBeam; ++i)
            {
                int[] hid = Beam[i].HeadIDs();
                int[] blabls = Beam[i].ArcLabels();

                string[] labels = new string[toks.Length];

                for (int j = 0; j < blabls.Length; ++j)
                {
                    if (blabls[j] < 0)
                    {
                        labels[j] = "root";
                    }
                    else
                    {
                        labels[j] = vocab.LabelId2Name[blabls[j]];
                    }
                }
                hidList.Add(hid);
                lablList.Add(labels);
            }

            hidArr = hidList.ToArray();
            labelsArr = lablList.ToArray();

            return true;
        }

        public bool Train(string[] toks, string[] pos, int[] hid, string[] labls, out List<FeatureUpdatePackage> updates)
        {
            int[] btok = vocab.BinarizeWithPadding(toks);
            int[] bpos = vocab.BinarizeWithPadding(pos);
            int[] blabels = vocab.BinarizeLabel(hid, labls);

            string[] wcstr = GetWordCluster(toks);

            int[] bwcluster = null;

            if (wcstr != null)
            {
                bwcluster = vocab.BinarizeWithPadding(wcstr);
            }

            for (int i = 0; i < blabels.Length; ++i)
            {
                if (hid[i] != 0 && blabels[i] < 0)
                {
                    updates = null;
                    return false;
                }
            }

            Initialize(btok, bpos, bwcluster);
            InitializeRef(hid, blabels);

            int Round = 0;
            while (!IsEnd)
            {
                Round++;
                if (!ParseNext())
                {
                    updates = null;
                    return false;
                }
                
                if (HaveErrorInRef)
                {
                    updates = null;
                    return false;
                }

                

                if (refId < 0)
                {
                    break;
                }
            }

            //CheckCommandTrace();

            updates = GetUpdates(btok, bpos, bwcluster, Round);

            return true;
        }
        //private List<FeatureUpdatePackage> GetUpdate(int[] tok, int[] pos, int Round)
        //{
        //}

        private List<FeatureUpdatePackage> GetUpdates(int[] tok, int[] pos, int[] wcluster, int Round)
        {
            int pbid = 0;
            if (refId == 0)
            {
                if (beamsize == 1 || nodeInBeam <= 1)
                {
                    return null;
                }

                if (Beam[0].score - Beam[1].score > 1.0)
                {
                    return null;
                }

                pbid = 1;
            }
            List<FeatureUpdatePackage> fupl = new List<FeatureUpdatePackage>();
            int[] xhid = Beam[pbid].HeadIDs();
            int[] xlabls = Beam[pbid].ArcLabels();

            int[] trghid = Padding(xhid);
            int[] trglabl = Padding(xlabls);

            ParserConfig refConfig = new ParserConfig(tok, pos, wcluster);

            int i = 0;

            for (; i < Round; ++i)
            {
                if (Beam[pbid].CommandTrace[i] == RefCommands[i])
                {
                    ApplyCommand(RefCommands[i], refConfig);
                }
                else
                {
                    break;
                }
            }

            ParserConfig trgConfig = refConfig.Clone();

            for (; i < Round; ++i)
            {
                List<LinearModelFeature> trgfeatureList = new List<LinearModelFeature>();
                List<LinearModelFeature> reffeatureList = new List<LinearModelFeature>();
                modelCache.GetFeatures(trgConfig.state, trgfeatureList);
                modelCache.GetFeatures(refConfig.state, reffeatureList);

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
                        trgfeatureList[j], Beam[pbid].CommandTrace[i], -1);

                    fupl.Add(fup);
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
                        reffeatureList[j], RefCommands[i], 1);
                    fupl.Add(fup);
                }

                //break;
                ApplyCommand(RefCommands[i], refConfig);
                ApplyCommand(Beam[pbid].CommandTrace[i], trgConfig);
                //ApplyCommand(refCommandTrace[i], refConfig);
                //ApplyCommand(commandTrace[0][i], trgConfig);
            }

            if (fupl.Count == 0)
            {
                return fupl;
            }

            List<FeatureUpdatePackage> compactFUP = new List<FeatureUpdatePackage>();

            FeatureUpdatePackage tmpFUP = new FeatureUpdatePackage(fupl[0].feature, fupl[0].tag, fupl[0].delta);

            for (i = 1; i < fupl.Count; ++i)
            {
                FeatureUpdatePackage thisFUP = fupl[i];

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
                if (i == fupl.Count - 1)
                {
                    if (tmpFUP.delta != 0)
                    {
                        compactFUP.Add(tmpFUP);
                    }
                }
            }

            return compactFUP;
        }

        public bool CollectFeatures(string[] toks, string[] pos, int[] hid, string[] labls, out List<FeatureUpdatePackage> updates)
        {
            int[] btok = vocab.BinarizeWithPadding(toks);
            int[] bpos = vocab.BinarizeWithPadding(pos);
            int[] blabels = vocab.BinarizeLabel(hid, labls);

            string[] wcstr = GetWordCluster(toks);

            int[] bwcluster = null;

            if (wcstr != null)
            {
                bwcluster = vocab.BinarizeWithPadding(wcstr);
            }

            for (int i = 0; i < blabels.Length; ++i)
            {
                if (hid[i] != 0 && blabels[i] < 0)
                {
                    updates = null;
                    return false;
                }
            }

            Initialize(btok, bpos, bwcluster);

            InitializeRef(hid, blabels);

            List<FeatureUpdatePackage> fupl = new List<FeatureUpdatePackage>();

            while (!IsEnd)
            {
                ParserAction pa;
                int xlabl;
                Beam[0].InferAction(refHid, refLabels, refChdCount, out pa, out xlabl);

                List<LinearModelFeature> reffeatures = new List<LinearModelFeature>();
                modelCache.GetFeatures(Beam[0].state, reffeatures);
                if (reffeatures.Count > 0)
                {
                    for (int i = 0; i < reffeatures.Count; ++i)
                    {
                        fupl.Add(new FeatureUpdatePackage(reffeatures[i], 0, 1));
                    }
                }
                if (!Beam[0].ApplyParsingAction(pa, xlabl))
                {
                    updates = null;
                    return false;
                }
            }

            if (fupl.Count == 0)
            {
                updates = null;
                return false;
            }

            List<FeatureUpdatePackage> compactFUP = new List<FeatureUpdatePackage>();

            FeatureUpdatePackage tmpFUP = new FeatureUpdatePackage(fupl[0].feature, fupl[0].tag, fupl[0].delta);

            for (int i = 1; i < fupl.Count; ++i)
            {
                FeatureUpdatePackage thisFUP = fupl[i];

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
                if (i == fupl.Count - 1)
                {
                    if (tmpFUP.delta != 0)
                    {
                        compactFUP.Add(tmpFUP);
                    }
                }
            }

            updates = compactFUP;

            return true;
        }

        public bool CollectMETrainingInstance(string[] toks, string[] pos, int[] hid, string[] labls, out List<List<FeatureUpdatePackage>> trainingInstance)
        {
            trainingInstance = null;

            int[] btok = vocab.BinarizeWithPadding(toks);
            int[] bpos = vocab.BinarizeWithPadding(pos);
            int[] blabels = vocab.BinarizeLabel(hid, labls);

            for (int i = 0; i < blabels.Length; ++i)
            {
                if (hid[i] != 0 && blabels[i] < 0)
                {
                    return false;
                }
            }

            string[] wcstr = GetWordCluster(toks);

            int[] bwcluster = null;

            if (wcstr != null)
            {
                bwcluster = vocab.BinarizeWithPadding(wcstr);
            }

            Initialize(btok, bpos, bwcluster);

            InitializeRef(hid, blabels);


            trainingInstance = new List<List<FeatureUpdatePackage>>();
            while (!IsEnd)
            {
                ParserAction pa;
                int xlabl;
                Beam[0].InferAction(refHid, refLabels, refChdCount, out pa, out xlabl);

                var fupl = new List<FeatureUpdatePackage>();

                List<LinearModelFeature> reffeatures = new List<LinearModelFeature>();
                modelCache.GetFeatures(Beam[0].state, reffeatures);
                int cmd = vocab.GetCommand(pa, xlabl);
                if (reffeatures.Count > 0)
                {
                    for (int i = 0; i < reffeatures.Count; ++i)
                    {
                        if (reffeatures[i].IsValid)
                        {
                            
                            fupl.Add(new FeatureUpdatePackage(reffeatures[i], cmd, 1));
                        }
                    }
                }
                if (!Beam[0].ApplyParsingAction(pa, xlabl))
                {
                    return false;
                }

                if (fupl.Count > 0)
                {
                    trainingInstance.Add(fupl);
                }
            }

            return true;
        }

        public bool IsME { get; set; }

        private void GetScores(int[] state, float[] score)
        {
            modelCache.GetScores(state, score);

            if (IsME)
            {
                double sum = 0;
                foreach (float s in score)
                {
                    sum += Math.Exp((double)s);
                }
                float logsum = (float)Math.Log(sum);

                for (int i = 0; i < score.Length; ++i)
                {
                    score[i] -= logsum;
                }
            }
        }

        private bool ParseNext()
        {
            return ParseNext(null);
        }

        private bool ParseNext(ParserForcedConstraints constraints)
        {
            if (nodeInBeam <= 0 || IsEnd)
            {
                return false;
            }

            int nodeCount = FillScoreHeapWithCandidates(constraints);

            if (nodeCount <= 0)
            {
                return false;
            }

            GenerateNextBeam(nodeCount);

            if (refId >= 0)
            {
                KeepTrackOfReference(nodeCount);
            }

            ParserConfig[] tmp = Beam;
            Beam = Buffer;
            Buffer = tmp;
            nodeInBeam = nodeCount;//hashs.Length;
            return true;
        }

        private void GenerateNextBeam(int nodeCount)
        {
            for (int i = 0; i < nodeCount; ++i)
            {
                int argmax = i;
                float maxscore = NBestScore[i];
                for (int j = i + 1; j < nodeCount; ++j)
                {
                    if (NBestScore[j] > maxscore)
                    {
                        maxscore = NBestScore[j];
                        argmax = j;
                    }
                }
                ObjectShuffle.Swap<int>(NBestCmd, i, argmax);
                ObjectShuffle.Swap<int>(NBestPrevId, i, argmax);
                ObjectShuffle.Swap<float>(NBestScore, i, argmax);

                int prevId = NBestPrevId[i];
                int cmd = NBestCmd[i];
                float s = NBestScore[i];
                ParserAction pa = vocab.command2Action[cmd];
                int labl = vocab.command2Label[cmd];
                Beam[prevId].CopyTo(Buffer[i]);
                Buffer[i].ApplyParsingAction(pa, labl);
                Buffer[i].CommandTrace.Add(cmd);
                Buffer[i].score = s;
            }
        }

        private void KeepTrackOfReference(int nodeCount)
        {
            int newRef = -1;

            for (int i = 0; i < nodeCount; ++i)
            {
                bool notRef = false;
                for (int j = 0; j < Buffer[i].CommandTrace.Count; ++j)
                {
                    if (Buffer[i].CommandTrace[j] != RefCommands[j])
                    {
                        notRef = true;
                        break;
                    }
                }
                if (!notRef)
                {
                    newRef = i;
                    break;
                }
            }
            refId = newRef;
        }

        private int FillScoreHeapWithCandidates(ParserForcedConstraints constraints)
        {
            if (constraints == null)
            {
                return FillScoreHeapWithCandidates();
            }

            scoreHeap.Clear();

            int nodeCount = 0;

            for (int i = 0; i < nodeInBeam; ++i)
            {
                GetScores(Beam[i].state, scorebuffer);

                bool IsLAApplicable =
                    Beam[i].IsApplicableAction(ParserAction.LA);
                bool IsRAApplicable =
                    Beam[i].IsApplicableAction(ParserAction.RA);

                for (int j = 0; j < scorebuffer.Length; ++j)
                {
                    int cmd = j;

                    ParserAction pa = vocab.command2Action[cmd];
                    if ((pa == ParserAction.LA && !IsLAApplicable)
                        || (pa == ParserAction.RA && !IsRAApplicable))
                    {
                        continue;
                    }


                    int labl = vocab.command2Label[cmd];
                    if (Beam[i].IsApplicableAction(pa, labl, constraints))
                    {
                        float s = Beam[i].score + scorebuffer[cmd];
                        int replaceId = -1;
                        if (nodeCount < beamsize)
                        {
                            replaceId = nodeCount++;
                        }
                        else
                        {
                            float min = NBestScore[0];
                            int argmin = 0;
                            for (int k = 1; k < beamsize; ++k)
                            {
                                if (NBestScore[k] < min)
                                {
                                    min = NBestScore[k];
                                    argmin = k;
                                }
                            }
                            if (s > min)
                            {
                                replaceId = argmin;
                            }
                        }

                        if (replaceId < 0)
                        {
                            continue;
                        }

                        NBestScore[replaceId] = s;
                        NBestCmd[replaceId] = cmd;
                        NBestPrevId[replaceId] = i;

                    }
                }
            }
            return nodeCount;
        }

        private int FillScoreHeapWithCandidates()
        {
            scoreHeap.Clear();

            int nodeCount = 0;

            for (int i = 0; i < nodeInBeam; ++i)
            {
                GetScores(Beam[i].state, scorebuffer);

                if (Beam[i].IsApplicableAction(ParserAction.SHIFT))
                {
                    nodeCount = InsertCandidate(nodeCount, i, vocab.ShiftCMD);
                }

                if (Beam[i].IsApplicableAction(ParserAction.REDUCE))
                {
                    nodeCount = InsertCandidate(nodeCount, i, vocab.ReduceCMD);
                }

                if (Beam[i].IsApplicableAction(ParserAction.LA))
                {
                    foreach (int LACmd in vocab.LACommands)
                    {
                        nodeCount = InsertCandidate(nodeCount, i, LACmd);
                    }
                }

                if (Beam[i].IsApplicableAction(ParserAction.RA))
                {
                    foreach (int RACmd in vocab.RACommands)
                    {
                        nodeCount = InsertCandidate(nodeCount, i, RACmd);
                    }
                }

            }
            return nodeCount;
        }

        private string[] GetWordCluster(string[] toks)
        {
            if (PMInfo.lowerCaseWordClusterDict == null)
            {
                return null;
            }

            string[] wc = new string[toks.Length];

            for (int i = 0; i < toks.Length; ++i)
            {
                if (!PMInfo.lowerCaseWordClusterDict.TryGetValue(toks[i].ToLower(), out wc[i]))
                {
                    wc[i] = null;
                }
            }

            return wc;
        }

        private int InsertCandidate(int nodeCount, int BeamId, int cmd)
        {
            ParserAction pa = vocab.command2Action[cmd];

            int labl = vocab.command2Label[cmd];

            float s = Beam[BeamId].score + scorebuffer[cmd];
            int replaceId = -1;
            if (nodeCount < beamsize)
            {
                replaceId = nodeCount++;
            }
            else
            {
                float min = NBestScore[0];
                int argmin = 0;
                for (int k = 1; k < beamsize; ++k)
                {
                    if (NBestScore[k] < min)
                    {
                        min = NBestScore[k];
                        argmin = k;
                    }
                }
                if (s > min)
                {
                    replaceId = argmin;
                }
            }

            if (replaceId >= 0)
            {
                NBestScore[replaceId] = s;
                NBestCmd[replaceId] = cmd;
                NBestPrevId[replaceId] = BeamId;
            }
            return nodeCount;
        }


        private float[] NBestScore; //= new float[beamsize];
        private int[] NBestCmd; //= new int[beamsize];
        private int[] NBestPrevId; //= new int[beamsize];
            

        private int[] Padding(int[] raw)
        {
            int[] padded;
            padded = new int[raw.Length + 2];

            for (int i = 0; i < raw.Length; ++i)
            {
                padded[i + 1] = raw[i];
            }
            padded[0] = -1;
            padded[padded.Length - 1] = -1;
            return padded;
        }

        private void ApplyCommand(int cmd, ParserConfig cfg)
        {
            ParserAction pa = vocab.command2Action[cmd];
            int labl = vocab.command2Label[cmd];
            cfg.ApplyParsingAction(pa, labl);
        }

        private bool TryApplyCommand(int cmd, ParserConfig cfg)
        {
            ParserAction pa = vocab.command2Action[cmd];
            int labl = vocab.command2Label[cmd];
            return cfg.ApplyParsingAction(pa, labl);
        }

        private float[] scorebuffer;
        private int[] cmdbuffer;

        private void Initialize(int[] tok, int[] pos, int[] wcluster)
        {
            scoreHeap.Clear();
            modelCache.Clear();
            Beam[0] = new ParserConfig(tok, pos, wcluster);
            for (int i = 1; i < beamsize; ++i)
            {
                Beam[i] = Beam[0].Clone();
            }
            for (int i = 0; i < beamsize; ++i)
            {
                Buffer[i] = Beam[0].Clone();
            }
            nodeInBeam = 1;
            refId = -1;
        }

        private void InitializeRef(int[] hid, int[] label)
        {
            refHid = new int[hid.Length + 2];
            refLabels = new int[label.Length + 2];
            refChdCount = new int[label.Length + 2];
            for (int i = 0; i < hid.Length; ++i)
            {
                refHid[i + 1] = hid[i];
                refLabels[i + 1] = label[i];
                refChdCount[hid[i]]++;
            }
            refHid[0] = -1;
            refHid[refHid.Length - 1] = -1;
            refLabels[0] = -1;
            refLabels[refLabels.Length - 1] = -1;
            HaveErrorInRef = false;
            commandTrace = new List<int>[beamsize];
            commandTrace[0] = new List<int>();
            refCommandTrace = new List<int>();

            ParserAction[] actions;

            int[] labelIdx;

            ParserConfig.InferAction(hid, out actions, out labelIdx);

            RefCommands = new int[actions.Length];

            for (int i = 0; i < RefCommands.Length; ++i)
            {
                RefCommands[i] = vocab.GetCommand(actions[i], labelIdx[i] >= 0 ? label[labelIdx[i]] : -1);
            }

            refId = 0;
        }

        
        private bool IsEnd
        {
            get { return nodeInBeam <= 0 || Beam[0].IsEnd; }
        }

        private void CheckCommandTrace()
        {
            for (int i = 0; i < nodeInBeam; ++i)
            {
                int[] fakeTok = new int[refHid.Length];
                int[] fakePos = new int[refHid.Length];

                ParserConfig temp = new ParserConfig(fakeTok, fakePos, null);

                for (int j = 0; j < commandTrace[i].Count; ++j)
                {
                    int labl;
                    ParserAction action;
                    action = vocab.command2Action[commandTrace[i][j]];
                    labl = vocab.command2Label[commandTrace[i][j]];
                    if (!temp.ApplyParsingAction(action, labl))
                    {
                        throw new Exception();
                    }
                }

                if (!temp.Compare(Beam[i]))
                {
                    throw new Exception();
                }
            }
        }

        HeapWithSingleScore<ulong> scoreHeap;
        int beamsize;
        LinearModelCache modelCache;
        ParserVocab vocab;
        ParserConfig[] Beam;
        ParserConfig[] Buffer;
        int nodeInBeam;

        ParserModelInfo PMInfo;

        LinearModelInfo LMInfo { get { return PMInfo.lmInfo; } }

        ILinearFunction Model { get { return modelCache.Model; } }

        bool HaveErrorInRef;
        int refId;
        int[] refHid;
        int[] refLabels;
        int[] refChdCount;

        int[] RefCommands;



        List<int>[] commandTrace;
        List<int> refCommandTrace;
    }

    public class GreedyParDecoder
    {
        public GreedyParDecoder(ParserModelInfo parserModelInfo, ILinearFunction model, int beamsize)
        {
            modelCache = new LinearModelCache(model, parserModelInfo.lmInfo.TemplateSet);
            this.vocab = parserModelInfo.vocab;
            scorebuffer = new float[parserModelInfo.CommandCount];
            cmdbuffer = new int[parserModelInfo.CommandCount];
            PMInfo = parserModelInfo;
        }

        public bool Run(string[] toks, string[] pos, out int[] hid, out string[] labels)
        {
            return Run(toks, pos, null, out hid, out labels);
        }

        public delegate void UpdateDelegate(List<FeatureUpdatePackage> update);

        public bool Run(string[] toks, string[] pos, ParserForcedConstraints constraints, out int[] hid, out string[] labels)
        {
            hid = null;
            labels = null;
            int[] btok = vocab.BinarizeWithPadding(toks);
            int[] bpos = vocab.BinarizeWithPadding(pos);

            string[] wcstr = GetWordCluster(toks);

            int[] bwcluster = null;

            if (wcstr != null)
            {
                bwcluster = vocab.BinarizeWithPadding(wcstr);
            }

            var config = Initialize(btok, bpos, bwcluster);

            while (!config.IsEnd)
            {
                if (!ParseNext(config, constraints))
                {
                    return false;
                }
            }

            hid = config.HeadIDs();
            int[] blabls = config.ArcLabels();

            labels = new string[toks.Length];

            for (int i = 0; i < blabls.Length; ++i)
            {
                if (blabls[i] < 0)
                {
                    labels[i] = "root";
                }
                else
                {
                    labels[i] = vocab.LabelId2Name[blabls[i]];
                }
            }

            return true;
        }

        public bool Train(Random RNG, string[] toks, string[] pos, int[] hid, string[] labls, double exploreProb, UpdateDelegate updator)
        {
            int[] btok = vocab.BinarizeWithPadding(toks);
            int[] bpos = vocab.BinarizeWithPadding(pos);
            int[] blabels = vocab.BinarizeLabel(hid, labls);

            int[] refhid = new int[hid.Length + 1];
            int[] reflabel = new int[labls.Length + 1];

            refhid[0] = -1;
            hid.CopyTo(refhid, 1);
            blabels.CopyTo(reflabel, 1);
            reflabel[0] = -1;

            string[] wcstr = GetWordCluster(toks);

            int[] bwcluster = null;

            if (wcstr != null)
            {
                bwcluster = vocab.BinarizeWithPadding(wcstr);
            }

            for (int i = 0; i < blabels.Length; ++i)
            {
                if (hid[i] != 0 && blabels[i] < 0)
                {
                    return false;
                }
            }

            var config = Initialize(btok, bpos, bwcluster);

            while (!config.IsEnd)
            {
                ParserAction bestpa;

                int bestlabl;
                GetNextBestAction(config, null, out bestpa, out bestlabl);

                //if (bestlabl < 0)
                //{
                //    return false;
                //}

                if (bestpa == ParserAction.NIL)
                {
                    return false;
                }

                if (config.IsZeroCostAction(bestpa, bestlabl, refhid, reflabel))
                {
                    config.ApplyParsingAction(bestpa, bestlabl);
                    continue;
                }

                List<ParserAction> gactions;
                List<int> glabels;

                GetZeroCostActions(config, refhid, reflabel, out gactions, out glabels);

                if (gactions.Count == 0 || glabels.Count == 0)
                {
                    throw new Exception("Fail to find zero cost transition!");
                }


                int xid = RNG.Next(gactions.Count);

                List<FeatureUpdatePackage> fupl = GetUpdate(config, bestpa, bestlabl, gactions, glabels, xid);

                updator(fupl);

                modelCache.Clear();

                if (RNG.NextDouble() < exploreProb)
                {
                    config.ApplyParsingAction(bestpa, bestlabl);
                }
                else
                {
                    return true;
                    config.ApplyParsingAction(gactions[xid], glabels[xid]);
                }
            }


            return true;
        }

        private List<FeatureUpdatePackage> GetUpdate(GParserConfig config, ParserAction bestpa, int bestlabl, List<ParserAction> gactions, List<int> glabels, int xid)
        {
            List<LinearModelFeature> trgfeatureList = new List<LinearModelFeature>();
            List<LinearModelFeature> reffeatureList = new List<LinearModelFeature>();

            modelCache.GetFeatures(config.state, trgfeatureList);
            modelCache.GetFeatures(config.state, reffeatureList);

            List<FeatureUpdatePackage> fupl = new List<FeatureUpdatePackage>();

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

                var fup =
                    new FeatureUpdatePackage(trgfeatureList[j], vocab.GetCommand(bestpa, bestlabl), -1.0f);

                fupl.Add(fup);
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

                var xfup =
                    new FeatureUpdatePackage(reffeatureList[j], vocab.GetCommand(gactions[xid], glabels[xid]), 1.0f);

                fupl.Add(xfup);
            }

            fupl.Sort();

            List<FeatureUpdatePackage> compactFUP = new List<FeatureUpdatePackage>();

            FeatureUpdatePackage tmpFUP = new FeatureUpdatePackage(fupl[0].feature, fupl[0].tag, fupl[0].delta);

            for (int i = 1; i < fupl.Count; ++i)
            {
                FeatureUpdatePackage thisFUP = fupl[i];

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
                if (i == fupl.Count - 1)
                {
                    if (tmpFUP.delta != 0)
                    {
                        compactFUP.Add(tmpFUP);
                    }
                }
            }

            return compactFUP;
        }
        
        

        private void GetScores(int[] state, float[] score)
        {
            modelCache.GetScores(state, score);
            
            //modelCache.GetScoresNoCache(state, score);
        }

        private bool ParseNext(GParserConfig config)
        {
            return ParseNext(config, null);
        }

        private bool ParseNext(GParserConfig config, ParserForcedConstraints constraints)
        {
            if (config.IsEnd)
            {
                return false;
            }

            ParserAction bestpa;

            int bestlabl;
            GetNextBestAction(config, constraints, out bestpa, out bestlabl);

            if (bestpa != ParserAction.NIL)
            {
                config.ApplyParsingAction(bestpa, bestlabl);
                return true;
            }
            else
            {
                return false;
            }

        }

        private void GetNextBestAction(GParserConfig config, ParserForcedConstraints constraints, out ParserAction bestpa, out int bestlabl)
        {
            GetScores(config.state, scorebuffer);

            bool IsLAApplicable =
                config.IsApplicableAction(ParserAction.LA);
            bool IsRAApplicable =
                config.IsApplicableAction(ParserAction.RA);

            bestpa = ParserAction.NIL;
            bestlabl = -1;
            float maxS = 0;

            for (int j = 0; j < scorebuffer.Length; ++j)
            {
                int cmd = j;

                ParserAction pa = vocab.command2Action[cmd];
                if ((pa == ParserAction.LA && !IsLAApplicable)
                    || (pa == ParserAction.RA && !IsRAApplicable))
                {
                    continue;
                }

                int labl = vocab.command2Label[cmd];
                if (config.IsApplicableAction(pa, labl, constraints))
                {
                    float s = config.score + scorebuffer[cmd];
                    if (bestpa == ParserAction.NIL || s > maxS)
                    {
                        bestlabl = labl;
                        bestpa = pa;
                        maxS = s;
                    }
                }
            }
        }

        private void GetZeroCostActions(GParserConfig config, int[] refhid, int[] reflabl, out List<ParserAction> actions, out List<int> labels)
        {
            actions = new List<ParserAction>();
            labels = new List<int>();
            GetScores(config.state, scorebuffer);

            bool IsLAApplicable =
                config.IsApplicableAction(ParserAction.LA);
            bool IsRAApplicable =
                config.IsApplicableAction(ParserAction.RA);

            for (int j = 0; j < scorebuffer.Length; ++j)
            {
                int cmd = j;

                ParserAction pa = vocab.command2Action[cmd];
                if ((pa == ParserAction.LA && !IsLAApplicable)
                    || (pa == ParserAction.RA && !IsRAApplicable))
                {
                    continue;
                }

                int labl = vocab.command2Label[cmd];

                if (config.IsZeroCostAction(pa, labl, refhid, reflabl))
                {
                    labels.Add(labl);
                    actions.Add(pa);
                }
            }
        }


        private string[] GetWordCluster(string[] toks)
        {
            if (PMInfo.lowerCaseWordClusterDict == null)
            {
                return null;
            }

            string[] wc = new string[toks.Length];

            for (int i = 0; i < toks.Length; ++i)
            {
                if (!PMInfo.lowerCaseWordClusterDict.TryGetValue(toks[i].ToLower(), out wc[i]))
                {
                    wc[i] = null;
                }
            }

            return wc;
        }

        private int[] Padding(int[] raw)
        {
            int[] padded;
            padded = new int[raw.Length + 2];

            for (int i = 0; i < raw.Length; ++i)
            {
                padded[i + 1] = raw[i];
            }
            padded[0] = -1;
            padded[padded.Length - 1] = -1;
            return padded;
        }

        private void ApplyCommand(int cmd, GParserConfig cfg)
        {
            ParserAction pa = vocab.command2Action[cmd];
            int labl = vocab.command2Label[cmd];
            cfg.ApplyParsingAction(pa, labl);
        }

        private bool TryApplyCommand(int cmd, GParserConfig cfg)
        {
            ParserAction pa = vocab.command2Action[cmd];
            int labl = vocab.command2Label[cmd];
            return cfg.ApplyParsingAction(pa, labl);
        }

        private float[] scorebuffer;
        private int[] cmdbuffer;

        private GParserConfig Initialize(int[] tok, int[] pos, int[] wcluster)
        {
            modelCache.Clear();
            var config = new GParserConfig(tok, pos, wcluster);
            return config;
        }

        LinearModelCache modelCache;
        ParserVocab vocab;
        ParserModelInfo PMInfo;
        LinearModelInfo LMInfo { get { return PMInfo.lmInfo; } }
        ILinearFunction Model { get { return modelCache.Model; } }
    }
}
