using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NanYUtilityLib;
using LinearFunction;

namespace AveragePerceptron
{
    public class LinearChainTrainDecoder : LinearChainDecoder
    {
        public LinearChainTrainDecoder(LinearChainModelInfo modelInfo, ILinearFunction model, int BeamSize)
            : base(modelInfo, model, BeamSize)
        {
        }

        private int GetRefSequenceId(int[] refSequence)
        {
            if (NodeInBeam == 0)
            {
                return -1;
            }
            for (int i = 0; i < NodeInBeam; ++i)
            {
                int bp = BranchPointFromRef(refSequence, Beam[i].Tags);
                if (bp == Next)
                {
                    return i;
                }
            }

            return -1;
        }

        private int BranchPointFromRef(int[] refSequence, int[] tagSequence)
        {
            for (int i = 1; i < Next; ++i)
            {
                if (refSequence[i - 1] != -1 && tagSequence[i] != refSequence[i - 1])
                {
                    return i;
                }
            }
            return Next;
        }


        public List<FeatureUpdatePackage> RunTrain(string[][] Observ, string[] refTag)
        {
            Initialize(Observ);
            int[] refTagId = new int[refTag.Length];
            for (int i = 0; i < refTagId.Length; ++i)
            {
                refTagId[i] = modelInfo.TagVocab.TagId(refTag[i]);
                if (refTagId[i] < 0)
                {
                    return null;
                }
            }

            int refSeqId = 0;

            while (!IsEnd)
            {
                if (!TagNext(-1))
                {
                    return null;
                }

                refSeqId = GetRefSequenceId(refTagId);
                if (refSeqId < 0)
                {
                    // reference tags already fall out of beam;
                    break;
                }
            }

            // get updates
            if (refSeqId != 0)
            {
                int bp = BranchPointFromRef(refTagId, Beam[0].Tags);

                List<FeatureUpdatePackage> fup = new List<FeatureUpdatePackage>();

                LinearChainNode refNode = CreateDummy(TotalLength);
                CreateStartNode(refNode);

                for (int i = 1; i < Next; ++i)
                {
                    refNode.Tags[i] = refTagId[i - 1];
                }

                for (int i = bp; i < Next; ++i)
                {
                    GetUpdate(refNode, i, 1, fup);
                    GetUpdate(Beam[0], i, -1, fup);
                }

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

            return null;
        }

        private void GetUpdate(LinearChainNode node, int id, float delta, List<FeatureUpdatePackage> fup)
        {
            List<LinearModelFeature> upfeature = modelCache.GetAllFeatures(Observations, node.Tags, id);
            foreach (LinearModelFeature f in upfeature)
            {
                fup.Add(new FeatureUpdatePackage(f, node.Tags[id], delta));
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

    }
}