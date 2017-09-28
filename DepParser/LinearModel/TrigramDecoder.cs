using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NanYUtilityLib;

namespace LinearFunction
{
    class LatticeFrame
    {
        public List<LatticeNode> lnodes = new List<LatticeNode>();
    }

    class LatticeNode
    {
        public List<TransitionNode> forwardLinks = new List<TransitionNode>();
        public List<TransitionNode> backwardLinks = new List<TransitionNode>();
        public float alpha;
        public float beta;
        public int tag;
        public int ptag;
        public float[] deltaScoreBuffer;
    }

    class TraceNode
    {
        public float beta;
        public float alpha;
        public TraceNode prevTraceNode;
        public LatticeNode lNode;
    }

    class TransitionNode
    {
        public LatticeNode startNode;
        public LatticeNode endNode;
        public float score;
        public float alpha;
        public float beta;
    }

    struct TrigramState
    {
        public int tag;
        public int ptag;
    }

    public class TrigramChainDecoder : ILinearChainDecoder
    {
        public TrigramChainDecoder(LinearChainModelInfo modelInfo, ILinearFunction model, int BeamSize)
        {
            this.model = model;

            this.BeamSize = BeamSize;

            this.modelInfo = modelInfo;

            this.TagCount = modelInfo.TagCount;

            tagBuffer = new int[TagCount];

            scoreBuffer = new float[TagCount];

            scoreHeap = new SingleScoredHeapWithQueryKey<int, TrigramState>(BeamSize);

            modelCache = new LinearChainModelCache(model, modelInfo.TagCount, modelInfo.Templates);
        }

        public bool Run(string[][] Observ, string[] forcedTag, out string[] Tags)
        {
            if (forcedTag == null)
            {
                return RunMultiTag(Observ, null, out Tags);
            }
            else
            {
                List<string>[] forcedTags = new List<string>[forcedTag.Length];
                for (int i = 0; i < forcedTags.Length; ++i)
                {
                    if (forcedTag[i] != null)
                    {
                        forcedTags[i] = new List<string>();
                        forcedTags[i].Add(forcedTag[i]);
                    }
                }

                return RunMultiTag(Observ, null, out Tags);
            }
        }

        public bool RunMultiTag(string[][] Observ, List<string>[] forcedTags, out string[] Tags)
        {
            Initialize(Observ);
            if (forcedTags == null)
            {
                forcedTags = new List<string>[Observ.Length];
            }

            var BinarizedForcedTag = GetTagConstraints(forcedTags);

            while (!IsEnd)
            {
                if (!TagNext(BinarizedForcedTag[Next - 1]))
                {
                    Tags = null;
                    return false;
                }
            }

            int[] btags;
            if (!GetBest(out btags))
            {
                Tags = null;
                return false;
            }

            Tags = new string[TotalLength - 2];

            for (int i = 0; i < Tags.Length; ++i)
            {
                Tags[i] = modelInfo.TagVocab.TagString(btags[i + 1]);
            }

            return true;

        }

        private int[][] GetTagConstraints(List<string>[] forcedTags)
        {
            int[][] BinarizedForcedTag = new int[forcedTags.Length][];

            if (forcedTags != null)
            {
                for (int i = 0; i < forcedTags.Length; ++i)
                {
                    if (forcedTags[i] == null || forcedTags[i].Count == 0)
                    {
                        continue;
                    }

                    var ftlist = new List<int>();

                    foreach (string tag in forcedTags[i])
                    {
                        int id = modelInfo.TagVocab.TagId(tag);
                        if (id >= 0 && !ftlist.Contains(id))
                        {
                            ftlist.Add(id);
                        }
                    }

                    if (ftlist.Count > 0)
                    {
                        BinarizedForcedTag[i] = ftlist.ToArray();
                        Array.Sort(BinarizedForcedTag[i]);
                    }
                }
            }
            return BinarizedForcedTag;
        }

        public bool RunNBest(string[][] Observ, string[] forcedTag, out List<string[]> Tags, out List<float> Scores)
        {
            throw new Exception("Not supported yet");
        }

        public bool RunMultiTagNBest(string[][] Observ, List<string>[] forcedTags, int N, out List<string[]> TagList, out List<float> ScoreList)
        {
            Initialize(Observ);
            if (forcedTags == null)
            {
                forcedTags = new List<string>[Observ.Length];
            }
            var BinarizedForcedTag = GetTagConstraints(forcedTags);

            while (!IsEnd)
            {
                if (!TagNext(BinarizedForcedTag[Next - 1]))
                {
                    TagList = null;
                    ScoreList = null;
                    return false;
                }
            }

            List<int[]> btagsList;

            if (!GetNBest(N, out btagsList, out ScoreList))
            {
                TagList = null;
                ScoreList = null;
                return false;
            }

            TagList = new List<string[]>();

            for (int nbest = 0; nbest < btagsList.Count; ++nbest)
            {
                string[] Tags = new string[TotalLength - 2];

                for (int i = 0; i < Tags.Length; ++i)
                {
                    Tags[i] = modelInfo.TagVocab.TagString(btagsList[nbest][i + 1]);
                }
                TagList.Add(Tags);
            }
            return true;
        }


        public bool TrainMultiTag(string[][] Observ, List<string>[] forcedTags, string[] goldTags, out List<FeatureUpdatePackage> updates)
        {
            updates = null;

            Initialize(Observ);

            InitializeReference(goldTags);

            if (forcedTags == null)
            {
                forcedTags = new List<string>[Observ.Length];
            }

            var BinarizedForcedTag = GetTagConstraints(forcedTags);

            while (!IsEnd)
            {
                if (!TagNext(BinarizedForcedTag[Next - 1]))
                {
                    return false;
                }

                if (RefBeamId < 0)
                {
                    // reference tags already fall out of beam.
                    // do early update
                    break;
                }
            }

            if (RefBeamId == 0)
            {
                // 1-best result conforms to gold tags.
                // no need to update
                updates = null;
                return true;
            }

            updates = GetUpdates();

            return true;
        }

        private List<FeatureUpdatePackage> GetUpdates()
        {
            // back track best partial hypothesis first
            int[] bestH = BackTrackBestPartialHypothesis();
            if (bestH == null)
            {
                return null;
            }

            int bpoint = BranchPointFromRef(reftags, bestH);

            if (bpoint == Next)
            {
                return null;
            }

            var fup = new List<FeatureUpdatePackage>();

            for (int up = bpoint; up < Next && up < TotalLength - 1; ++up)
            {
                GetUpdate(reftags, up, 1, fup);
                GetUpdate(bestH, up, -1, fup);
            }

            fup.Sort();

            if (fup.Count <= 1)
            {
                return fup;
            }

            var compactFUP = new List<FeatureUpdatePackage>();

            var tmpFUP = new FeatureUpdatePackage(fup[0].feature, fup[0].tag, fup[0].delta);

            for (int i = 1; i < fup.Count; ++i)
            {
                var thisFUP = fup[i];

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
                if (i == fup.Count - 1)
                {
                    if (tmpFUP.delta != 0)
                    {
                        compactFUP.Add(tmpFUP);
                    }
                }
            }

            return compactFUP;
        }


        private void GetUpdate(int[] tags, int id, float delta, List<FeatureUpdatePackage> fup)
        {
            var upfeature = modelCache.GetAllFeatures(Observations, tags, id);
            foreach (var f in upfeature)
            {
                fup.Add(new FeatureUpdatePackage(f, tags[id], delta));
            }
        }


        private List<FeatureUpdatePackage> MergeFeatures(List<FeatureUpdatePackage> fup)
        {
            fup.Sort();

            if (fup.Count <= 1)
            {
                return fup;
            }

            List<FeatureUpdatePackage> compactFUP = new List<FeatureUpdatePackage>();

            FeatureUpdatePackage tmpFUP = new FeatureUpdatePackage(fup[0].feature, fup[0].tag, fup[0].delta);

            for (int i = 1; i < fup.Count; ++i)
            {
                FeatureUpdatePackage thisFUP = fup[i];

                if (thisFUP.IsMergeable(tmpFUP))
                {
                    tmpFUP.delta += thisFUP.delta;
                }
                else
                {
                    if (Math.Abs(tmpFUP.delta) >= 0.00000000001f)
                    {
                        compactFUP.Add(tmpFUP);
                    }
                    tmpFUP = new FeatureUpdatePackage(thisFUP.feature, thisFUP.tag, thisFUP.delta);
                }
                if (i == fup.Count - 1)
                {
                    if (Math.Abs(tmpFUP.delta) >= 0.00000000001f)
                    {
                        compactFUP.Add(tmpFUP);
                    }
                }
            }

            return compactFUP;
        }


        private int BranchPointFromRef(int[] refSequence, int[] tagSequence)
        {
            for (int i = 1; i < Next; ++i)
            {
                if (refSequence[i] != -1 && tagSequence[i] != refSequence[i])
                {
                    return i;
                }
            }
            return Next;
        }


        private int[] BackTrackBestPartialHypothesis()
        {
            int[] partial = new int[TotalLength];

            partial[0] = TagCount;

            if (!BackTracking(Next - 1, frames[Next - 1].lnodes[0], ref partial))
            {
                return null;
            }
            else
            {
                return partial;
            }
        }

        private bool BackTracking(int StartFrameId, LatticeNode node, ref int[] partialHyp)
        {
            if (StartFrameId == 0)
            {
                return true;
            }

            if (node.backwardLinks.Count == 0)
            {
                return false;
            }

            partialHyp[StartFrameId] = node.tag;


            var pnode = node.backwardLinks[0].startNode;

            return BackTracking(StartFrameId - 1, pnode, ref partialHyp);
        }

        private void Initialize(string[][] Observ)
        {
            this.Observations = modelInfo.ModelVocab.ConvertToBinary(Observ);
            TotalLength = Observ.Length + 2;

            Next = 1;
            NodeInBeam = 1;
            modelCache.StartNextInstance(TotalLength);


            frames = new LatticeFrame[TotalLength];

            for (int i = 0; i < frames.Length; ++i)
            {
                frames[i] = new LatticeFrame();
            }


            frames[0].lnodes.Add(
                new LatticeNode
                {
                    alpha = 0,
                    beta = 0,
                    backwardLinks = new List<TransitionNode>(),
                    forwardLinks = new List<TransitionNode>(),
                    deltaScoreBuffer = new float[TagCount],
                    tag = TagCount,
                    ptag = -1
                });

            FakeTagHistory = new int[TotalLength];

            reftags = null;

            RefBeamId = -1;
        }

        private void InitializeReference(string[] referenceTags)
        {
            reftags = new int[TotalLength];

            reftags[0] = TagCount;
            for (int i = 0; i < referenceTags.Length; ++i)
            {
                reftags[i + 1] = modelInfo.TagVocab.TagId(referenceTags[i]);
            }

            RefBeamId = 0;
        }

        private bool IsEnd
        {
            get { return Next >= TotalLength - 1; }
        }

        private int GetStateKey(int thisTag, int prevTag)
        {
            return thisTag * (TagCount + 1) + prevTag;
        }

        public bool TagNext(int ForcedTag)
        {
            if (ForcedTag < 0)
            {
                return TagNext(null);
            }
            else
            {
                int[] tagset = { ForcedTag };

                return TagNext(tagset);
            }
        }

        private int GetTraceNodeId(int frameId, int thisBId, int prevBId)
        {
            return (frameId) * BeamSize * BeamSize + prevBId * BeamSize + thisBId;
        }

        public bool TagNext(int[] ForcedTag)
        {
            if (NodeInBeam == 0)
            {
                return false;
            }
            scoreHeap.Clear();

            foreach (var lnode in frames[Next - 1].lnodes)
            {
                GenNextTag(ForcedTag, lnode, scoreHeap);
            }

            if (scoreHeap.Count == 0)
            {
                return false;
            }

            int nextBeamCount = scoreHeap.Count;
            TrigramState[] states;
            float[] scores;
            scoreHeap.GetSortedArrayWithScores(out states, out scores);


            FillInNextFrame(states, scores);

            if (reftags != null)
            {
                // for training, we track which partial hypothesis conforms to reference, if any.
                TrackingGoldHypothesis();
            }

            NodeInBeam = nextBeamCount;

            Next++;

            if (Next == TotalLength - 1)
            {
                // finish off
                FinishOffLastFrame();
            }

            return true;
        }

        private void FinishOffLastFrame()
        {
            foreach (var pnode in frames[TotalLength - 2].lnodes)
            {
                var newNode = new LatticeNode
                {
                    alpha = pnode.alpha,
                    beta = 0,
                    tag = TagCount,
                    ptag = pnode.tag,
                    backwardLinks = new List<TransitionNode>(),
                    forwardLinks = new List<TransitionNode>()
                };

                frames[TotalLength - 1].lnodes.Add(newNode);
                var transition = new TransitionNode
                {
                    startNode = pnode,
                    endNode = newNode,
                    alpha = pnode.alpha,
                    beta = 0,
                    score = 0
                };

                pnode.forwardLinks.Add(transition);
                newNode.backwardLinks.Add(transition);
            }
        }

        private void TrackingGoldHypothesis()
        {
            if (RefBeamId >= 0)
            {
                var pnode = frames[Next - 1].lnodes[RefBeamId];

                if (pnode.forwardLinks.Count == 0)
                {
                    RefBeamId = -1;
                    return;
                }

                for (int i = 0; i < pnode.forwardLinks.Count; ++i)
                {
                    var transition = pnode.forwardLinks[i];
                    var enode = transition.endNode;
                    if (enode.tag == reftags[Next])
                    {
                        if (enode.backwardLinks[0].startNode == pnode)
                        {
                            RefBeamId = frames[Next].lnodes.FindIndex((x) => { return x == enode; });
                            return;
                        }
                        else
                        {
                            RefBeamId = -1;
                            return;
                        }
                    }
                }

                RefBeamId = -1;
                return;
            }
        }

        private void FillInNextFrame(TrigramState[] states, float[] scores)
        {
            for (int bid = 0; bid < states.Length; ++bid)
            {
                var state = states[bid];
                var ptag = state.ptag;
                var tag = state.tag;

                var newLNode = new LatticeNode
                {
                    tag = tag,
                    ptag = ptag,
                    alpha = scores[bid],
                    beta = 0,
                    backwardLinks = new List<TransitionNode>(),
                    forwardLinks = new List<TransitionNode>(),
                    deltaScoreBuffer = new float[TagCount],
                };

                frames[Next].lnodes.Add(newLNode);

                foreach (var pnode in frames[Next - 1].lnodes)
                {
                    if (pnode.tag != newLNode.ptag)
                    {
                        continue;
                    }

                    var transition = new TransitionNode
                    {
                        startNode = pnode,
                        endNode = newLNode,
                        score = pnode.deltaScoreBuffer[tag],
                        alpha = pnode.alpha + pnode.deltaScoreBuffer[tag],
                        beta = 0
                    };
                    pnode.forwardLinks.Add(transition);
                    newLNode.backwardLinks.Add(transition);
                }
            }

            // sort the transition links
            foreach (var node in frames[Next].lnodes)
            {
                node.backwardLinks.Sort(
                    (lhs, rhs) => { return rhs.alpha.CompareTo(lhs.alpha); });
            }

            foreach (var node in frames[Next - 1].lnodes)
            {
                node.forwardLinks.Sort(
                    (lhs, rhs) => { return rhs.alpha.CompareTo(lhs.alpha); });
            }


        }

        private void GenNextTag(int[] ForcedTag, LatticeNode lnode, SingleScoredHeapWithQueryKey<int, TrigramState> scoreHeap)
        {
            int count = ScoreNextTag(lnode, ForcedTag);

            for (int j = 0; j < count; ++j)
            {
                float s = scoreBuffer[j] + lnode.alpha;

                int key = GetStateKey(tagBuffer[j], lnode.tag);

                if (scoreHeap.IsAcceptableScore(s))
                {
                    var state = new TrigramState { ptag = lnode.tag, tag = tagBuffer[j] };

                    scoreHeap.Insert(state, key, s);

                }
                else
                {
                    break;
                }

            }
        }

        private bool GetBest(out int[] btags)
        {
            btags = new int[TotalLength];
            btags[0] = TagCount;

            return BackTracking(TotalLength - 1, frames[TotalLength - 1].lnodes[0], ref btags);
        }

        private bool GetNBest(int N, out List<int[]> btagList, out List<float> scoreList)
        {
            btagList = null;
            scoreList = null;

            var frontierTrace = new List<TraceNode>();

            for (int i = 0; i < frames[TotalLength - 1].lnodes.Count; ++i)
            {
                var sinkNode = new TraceNode
                {
                    beta = 0,
                    alpha = frames[TotalLength - 1].lnodes[i].alpha,
                    lNode = frames[TotalLength - 1].lnodes[i],
                    prevTraceNode = null,

                };

                frontierTrace.Add(sinkNode);
            }


            for (int frameId = TotalLength - 2; frameId >= 0; --frameId)
            {
                var newFrontier = new List<TraceNode>();

                foreach (var pnode in frontierTrace)
                {
                    foreach (var transition in pnode.lNode.backwardLinks)
                    {
                        var newTrace = new TraceNode
                        {
                            lNode = transition.startNode,
                            prevTraceNode = pnode,
                            alpha = transition.startNode.alpha,
                            beta = pnode.beta + transition.score
                        };

                        newFrontier.Add(newTrace);
                    }
                }

                newFrontier.Sort((lhs, rhs) =>
                { return (rhs.alpha + rhs.beta).CompareTo(lhs.alpha + lhs.beta); });

                frontierTrace.Clear();

                for (int i = 0; i < N && i < newFrontier.Count; ++i)
                {
                    frontierTrace.Add(newFrontier[i]);
                }
            }
            scoreList = new List<float>();
            btagList = new List<int[]>();

            for (int i = 0; i < frontierTrace.Count; ++i)
            {
                int[] tags = new int[TotalLength];

                int next = 0;
                var trace = frontierTrace[i];

                scoreList.Add(trace.beta);
                // run forward
                while (trace != null)
                {
                    tags[next++] = trace.lNode.tag;
                    trace = trace.prevTraceNode;
                }

                btagList.Add(tags);
            }

            return true;
        }

        private int ScoreNextTag(LatticeNode node, int[] ForcedTags)
        {
            FakeTagHistory[Next - 1] = node.tag;

            if (Next > 1)
            {
                FakeTagHistory[Next - 2] = node.ptag;
            }

            modelCache.GetScore(Observations, FakeTagHistory, Next, scoreBuffer);

            if (IsME)
            {
                double sum = 0;
                for (int i = 0; i < tagBuffer.Length; ++i)
                {
                    sum += Math.Exp(scoreBuffer[i]);
                }

                sum = Math.Log(sum);
                for (int i = 0; i < tagBuffer.Length; ++i)
                {
                    scoreBuffer[i] -= (float)sum;
                }
            }

            scoreBuffer.CopyTo(node.deltaScoreBuffer, 0);

            // sort scores;
            if (ForcedTags == null)
            {
                for (int i = 0; i < tagBuffer.Length; ++i)
                {
                    tagBuffer[i] = i;
                }

                UtilFunc.SortAgainstScore<int>(tagBuffer, scoreBuffer);
                return tagBuffer.Length;
            }
            else
            {
                int count = ForcedTags.Length;

                int next = 0;
                foreach (int t in ForcedTags)
                {
                    scoreBuffer[next] = scoreBuffer[t];
                    tagBuffer[next] = t;
                    next++;
                }

                UtilFunc.SortAgainstScore<int>(tagBuffer, scoreBuffer, count);



                return count;
            }
        }

        private SingleScoredHeapWithQueryKey<int, TrigramState> scoreHeap;

        private LatticeFrame[] frames;

        

        private int TotalLength;
        private int BeamSize;
        private int Next;
        private int NodeInBeam;
        private int TagCount;

        private int[] reftags;

        private int RefBeamId;

        private int[] FakeTagHistory;

        private int[] tagBuffer;

        private float[] scoreBuffer;

        private int[][] Observations;

        private ILinearFunction model;

        private LinearChainModelInfo modelInfo;

        private LinearChainModelCache modelCache;

        public bool IsME = false;
    }

}
