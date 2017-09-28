using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinearFunction
{
    public struct FeatureFunc
    {
        public int tag;
        public float weight;
    }

    public class BasicLinearFunction : ILinearFunction
    {
        public void GetScores(LinearModelFeature feature, float[] scores)
        {
            Array.Clear(scores, 0, scores.Length);
            AddScores(feature, scores);         
        }

        public void AddScores(LinearModelFeature feature, float[] scores)
        {
            if (!feature.IsValid)
            {
                return;
            }
            FeatureFunc[] ffs;
            if (scoreDicts[feature.DictId].TryGetValue(feature, out ffs))
            {
                foreach (FeatureFunc ff in ffs)
                {
                    scores[ff.tag] += ff.weight;
                }
            }
        }

        public void GetScores(LinearModelFeature[] features, float[] scores)
        {
            Array.Clear(scores, 0, scores.Length);

            AddScores(features, scores);
        }

        public void AddScores(LinearModelFeature[] features, float[] scores)
        {
            foreach (LinearModelFeature f in features)
            {
                if (f.ElemArr[0] >= 0)
                {
                    AddScores(f, scores);
                }
                //AddScores(f, scores);
            }
        }

        public void AddScores(LinearModelFeature feature, float featurevalue, float[] scores)
        {
            FeatureFunc[] ffs;
            if (scoreDicts[feature.DictId].TryGetValue(feature, out ffs))
            {
                foreach (FeatureFunc ff in ffs)
                {
                    scores[ff.tag] += ff.weight * featurevalue;
                }
            }
        }

        public void GetScores(LinearModelFeature feature, float featurevalue, float[] scores)
        {
            Array.Clear(scores, 0, scores.Length);
            AddScores(feature, featurevalue, scores);
        }

        public void GetScores(LinearModelFeature[] features, float[] featurevalues, float[] scores)
        {
            Array.Clear(scores, 0, scores.Length);

            AddScores(features, featurevalues, scores);
        }

        public void AddScores(LinearModelFeature[] features, float[] featurevalues, float[] scores)
        {
            for (int i = 0; i < features.Length; ++i)
            {
                var f = features[i];
                var fv = featurevalues[i];
                if (f.ElemArr[0] >= 0)
                {
                    AddScores(f, fv, scores);
                }
            }
        }

        public BasicLinearFunction(Dictionary<LinearModelFeature, FeatureFunc[]>[] scoreDicts)
        {
            this.scoreDicts = scoreDicts;
        }

        public BasicLinearFunction(int FeatureDictCount, IEnumerable<LinearFeatureFuncPackage> featureFuncs)
        {
            scoreDicts = new Dictionary<LinearModelFeature, FeatureFunc[]>[FeatureDictCount];

            for (int i = 0; i < FeatureDictCount; ++i)
            {
                scoreDicts[i] = new Dictionary<LinearModelFeature, FeatureFunc[]>();
            }

            if (featureFuncs != null)
            {
                foreach (LinearFeatureFuncPackage lffp in featureFuncs)
                {
                    FeatureFunc[] ffs = new FeatureFunc[lffp.funcs.Length];
                    for (int i = 0; i < ffs.Length; ++i)
                    {
                        ffs[i] = new FeatureFunc { tag = lffp.funcs[i].tag, weight = lffp.funcs[i].weight };
                    }
                    scoreDicts[lffp.feature.DictId][lffp.feature] = ffs;
                }
            }
        }

        private Dictionary<LinearModelFeature, FeatureFunc[]>[] scoreDicts;
    }
}
