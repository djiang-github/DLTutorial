using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NanYUtilityLib;
using LinearFunction;

namespace LinearFunction
{
    public interface ILinearChainDecoder
    {
        bool Run(string[][] Observ, string[] forcedTag, out string[] Tags);

        bool RunMultiTag(string[][] Observ, List<string>[] forcedTags, out string[] Tags);

        bool RunNBest(string[][] Observ, string[] forcedTag, out List<string[]> Tags, out List<float> Scores);

        bool RunMultiTagNBest(string[][] Observ, List<string>[] forcedTags, int N, out List<string[]> Tags, out List<float> Scores);
    }

    public class LinearChainNode
    {
        public float Score;
        public int[] Tags;
    }
    
    public class LinearChainDecoder : ILinearChainDecoder
    {
        public LinearChainDecoder(LinearChainModelInfo modelInfo, ILinearFunction model, int BeamSize)
        {
            this.model = model;
            
            this.BeamSize = BeamSize;

            this.modelInfo = modelInfo;

            this.TagCount = modelInfo.TagCount;

            tagBuffer = new int[TagCount];
            scoreBuffer = new float[TagCount];

            deltaScoreBuffer = new float[BeamSize * TagCount];

            scoreHeap = new SingleScoredHeapWithQueryKey<int, ulong>(BeamSize);

            modelCache = new LinearChainModelCache(model, modelInfo.TagCount, modelInfo.Templates);
        }

        public bool Run(string[][] Observ, string[] forcedTag, out string[] Tags)
        {
            Initialize(Observ);
            int[] forcedTagId = new int[Observ.Length];
            for (int i = 0; i < Observ.Length; ++i)
            {
                forcedTagId[i] = forcedTag == null ? -1 : modelInfo.TagVocab.TagId(forcedTag[i]);
            }

            while (!IsEnd)
            {
                if (!TagNext(forcedTagId[Next - 1]))
                {
                    Tags = null;
                    return false;
                }
            }

            Tags = new string[TotalLength - 2];

            for (int i = 0; i < Tags.Length; ++i)
            {
                Tags[i] = modelInfo.TagVocab.TagString(Beam[0].Tags[i + 1]);
            }

            return true;
        }

        public bool RunMultiTag(string[][] Observ, List<string>[] forcedTags, out string[] Tags)
        {
            Initialize(Observ);
            if (forcedTags == null)
            {
                forcedTags = new List<string>[Observ.Length];
            }
            HashSet<int>[] BinarizedForcedTag = new HashSet<int>[forcedTags.Length];

            if (forcedTags != null)
            {
                for (int i = 0; i < forcedTags.Length; ++i)
                {
                    if (forcedTags[i] == null || forcedTags[i].Count == 0)
                    {
                        continue;
                    }
                    BinarizedForcedTag[i] = new HashSet<int>();
                    foreach (string tag in forcedTags[i])
                    {
                        int id = modelInfo.TagVocab.TagId(tag);
                        if (id >= 0 && !BinarizedForcedTag[i].Contains(id))
                        {
                            BinarizedForcedTag[i].Add(id);
                        }
                    }

                    if (BinarizedForcedTag[i].Count == 0)
                    {
                        BinarizedForcedTag[i] = null;
                    }
                }
            }

            while (!IsEnd)
            {
                if (!TagNext(BinarizedForcedTag[Next - 1]))
                {
                    Tags = null;
                    return false;
                }
            }

            Tags = new string[TotalLength - 2];

            for (int i = 0; i < Tags.Length; ++i)
            {
                Tags[i] = modelInfo.TagVocab.TagString(Beam[0].Tags[i + 1]);
            }

            return true;

        }

        public bool RunNBest(string[][] Observ, string[] forcedTag, out List<string[]> Tags, out List<float> Scores)
        {
            throw new Exception("Not supported yet");
        }

        public bool RunMultiTagNBest(string[][] Observ, List<string>[] forcedTags, int N, out List<string[]> Tags, out List<float> Scores)
        {
            throw new Exception("Not supported yet");
        }

        protected void Initialize(string[][] Observ)
        {
            this.Observations = modelInfo.ModelVocab.ConvertToBinary(Observ);
            TotalLength = Observ.Length + 2;
            Beam = new LinearChainNode[BeamSize];
            Buffer = new LinearChainNode[BeamSize];
            for (int i = 0; i < BeamSize; ++i)
            {
                Beam[i] = CreateDummy(TotalLength);
                Buffer[i] = CreateDummy(TotalLength);
            }
            CreateStartNode(Beam[0]);
            Next = 1;
            NodeInBeam = 1;
            modelCache.StartNextInstance(TotalLength);
        }

        protected LinearChainNode CreateDummy(int Length)
        {
            LinearChainNode node = new LinearChainNode();
            node.Tags = new int[Length];
            return node;
        }

        protected void CreateStartNode(LinearChainNode node)
        {
            node.Tags[0] = TagCount;
        }

        protected bool IsEnd
        {
            get { return Next >= TotalLength - 1; }
        }

        protected int GetStateKey(int thisTag, int prevTag)
        {
            return thisTag * (TagCount + 1) + prevTag;
        }

        public bool TagNext(int ForcedTag)
        {
            if (NodeInBeam == 0)
            {
                return false;
            }
            scoreHeap.Clear();

            for (int i = 0; i < NodeInBeam; ++i)
            {
                LinearChainNode prevNode = Beam[i];
                int count = ScoreNextTag(prevNode, i);
                for (int j = 0; j < count; ++j)
                {
                    if (ForcedTag >= 0 && tagBuffer[j] != ForcedTag)
                    {
                        continue;
                    }
                    float s = scoreBuffer[j] + prevNode.Score;
                    if (!scoreHeap.IsAcceptableScore(s))
                    {
                        break;
                    }
                    UInt64 info = (((UInt64)i) << 32) | (UInt32)tagBuffer[j];
                    int key = GetStateKey(tagBuffer[j], prevNode.Tags[Next - 1]);
                    scoreHeap.Insert(info, key, s);
                }
            }

            if (scoreHeap.Count == 0)
            {
                return false;
            }

            int nextBeamCount = scoreHeap.Count;
            UInt64[] infos;
            float[] scores;
            scoreHeap.GetSortedArrayWithScores(out infos, out scores);

            int firstLatticeId = TagCount * (Next - 1);

            for (int i = 0; i < nextBeamCount; ++i)
            {
                Buffer[i].Score = (float)scores[i];
                int prevId = (int)(infos[i] >> 32);
                int tag = (int)(uint)infos[i];
                Beam[prevId].Tags.CopyTo(Buffer[i].Tags, 0);
                Buffer[i].Tags[Next] = tag;

                //TagLattice[i + firstLatticeId] = tag;
                int ptag = Buffer[i].Tags[Next - 1];
                for (int j = 0; j < NodeInBeam; ++j)
                {
                    if (Beam[j].Tags[Next - 1] == prevId)
                    {
                        int traceId = GetTraceNodeId(Next - 1, i, j);
                        //TagTrace[traceId].IsValid = true;
                        //TagTrace[traceId].score = deltaScoreBuffer[j * BeamSize + tag];
                    }
                }
            }
            LinearChainNode[] tmp = Buffer;
            Buffer = Beam;
            Beam = tmp;
            
            
            NodeInBeam = nextBeamCount;
            //NodeCounts[Next - 1] = NodeInBeam;
            Next++;
            return true;
        }

        private int GetTraceNodeId(int frameId, int thisBId, int prevBId)
        {
            return (frameId) * BeamSize * BeamSize + prevBId * BeamSize + thisBId;
        }

        public bool TagNext(HashSet<int> ForcedTag)
        {
            if (NodeInBeam == 0)
            {
                return false;
            }
            scoreHeap.Clear();

            for (int i = 0; i < NodeInBeam; ++i)
            {
                LinearChainNode prevNode = Beam[i];
                int count = ScoreNextTag(prevNode, i);
                for (int j = 0; j < count; ++j)
                {
                    if (ForcedTag != null && !ForcedTag.Contains(tagBuffer[j]))
                    {
                        continue;
                    }
                    float s = scoreBuffer[j] + prevNode.Score;
                    if (!scoreHeap.IsAcceptableScore(s))
                    {
                        break;
                    }
                    UInt64 info = (((UInt64)i) << 32) | (UInt32)tagBuffer[j];
                    int key = GetStateKey(tagBuffer[j], prevNode.Tags[Next - 1]);
                    scoreHeap.Insert(info, key, s);
                }
            }

            if (scoreHeap.Count == 0)
            {
                return false;
            }

            int nextBeamCount = scoreHeap.Count;
            UInt64[] infos;
            float[] scores;
            scoreHeap.GetSortedArrayWithScores(out infos, out scores);

            int firstLatticeId = TagCount * (Next - 1);

            for (int i = 0; i < nextBeamCount; ++i)
            {
                Buffer[i].Score = (float)scores[i];
                int prevId = (int)(infos[i] >> 32);
                int tag = (int)(uint)infos[i];
                Beam[prevId].Tags.CopyTo(Buffer[i].Tags, 0);
                Buffer[i].Tags[Next] = tag;

                //TagLattice[i + firstLatticeId] = tag;
                int ptag = Buffer[i].Tags[Next - 1];
                for (int j = 0; j < NodeInBeam; ++j)
                {
                    if (Beam[j].Tags[Next - 1] == prevId)
                    {
                        int traceId = GetTraceNodeId(Next - 1, i, j);
                        //TagTrace[traceId].IsValid = true;
                        //TagTrace[traceId].score = deltaScoreBuffer[j * BeamSize + tag];
                    }
                }
            }
            LinearChainNode[] tmp = Buffer;
            Buffer = Beam;
            Beam = tmp;
            NodeInBeam = nextBeamCount;
            //NodeCounts[Next - 1] = NodeInBeam;
            Next++;
            return true;
        }

        protected int ScoreNextTag(LinearChainNode node, int beamId)
        {
            // static feature score;
            modelCache.GetScore(Observations, node.Tags, Next, scoreBuffer);

            

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

            scoreBuffer.CopyTo(deltaScoreBuffer, TagCount * beamId);

            // sort scores;
            for (int i = 0; i < tagBuffer.Length; ++i)
            {
                tagBuffer[i] = i;
            }

            

            UtilFunc.SortAgainstScore<int>(tagBuffer, scoreBuffer);
            return tagBuffer.Length;
        }

        protected SingleScoredHeapWithQueryKey<int, UInt64> scoreHeap;

        protected int TotalLength;

        protected LinearChainNode[] Beam;
        protected LinearChainNode[] Buffer;
        protected int BeamSize;
        protected int Next;
        protected int NodeInBeam;
        protected int TagCount;

        protected int[] tagBuffer;

        protected float[] scoreBuffer;

        protected float[] deltaScoreBuffer;

        protected int[][] Observations;

        protected ILinearFunction model;

        protected LinearChainModelInfo modelInfo;

        protected LinearChainModelCache modelCache;

        public bool IsME = false;
    }
}
