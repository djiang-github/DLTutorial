using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinearFunction
{
    public class LinearFeatureFuncPackage
    {
        public FeatureFunc[] funcs;
        public LinearModelFeature feature;

        public string GetStringDescription()
        {
            string featureIdString = string.Join(" ", feature.ElemArr);
            List<string> funcstring = new List<string>();
            foreach (FeatureFunc f in funcs)
            {
                funcstring.Add(f.tag.ToString());
                funcstring.Add(f.weight.ToString());
            }

            return string.Format("{0}\t{1}\t{2}",
                feature.DictId, featureIdString, string.Join(" ", funcstring));
        }
    }

    public class FeatureUpdatePackage : IComparable<FeatureUpdatePackage>
    {
        public LinearModelFeature feature;
        public int tag { get; private set; }
        public float delta;

        public static List<FeatureUpdatePackage> SortAndMerge(List<FeatureUpdatePackage> fup)
        {
            if (fup == null || fup.Count == 0)
            {
                return fup;
            }

            fup.Sort();

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

        public int DictId { get { return feature.DictId; } }

        public FeatureUpdatePackage(LinearModelFeature feature, int tag, float delta)
        {
            this.feature = feature;

            this.tag = tag;

            this.delta = delta;
        }

        public bool IsMergeable(FeatureUpdatePackage other)
        {
            if (other == null)
            {
                return false;
            }

            return tag == other.tag && feature.Equals(other.feature);
        }

        public int CompareTo(FeatureUpdatePackage other)
        {
            if (other == null)
            {
                return -1;
            }
            int r = feature.DictId.CompareTo(other.feature.DictId);
            if (r != 0)
            {
                return r;
            }
            r = tag.CompareTo(other.tag);
            if (r != 0)
            {
                return r;
            }
            for (int i = 0; i < feature.Length; ++i)
            {
                r = feature.ElemArr[i].CompareTo(feature.ElemArr[i]);
                if (r != 0)
                {
                    return r;
                }
            }
            return delta.CompareTo(other.delta);
        }
    }

    public class LinearFunctionHypothesis
    {
        public List<FeatureUpdatePackage> featureList { get; private set; }
        public double loss { get; private set; }

        public LinearFunctionHypothesis(List<FeatureUpdatePackage> fl, double loss)
        {
            this.loss = loss;
            featureList = FeatureUpdatePackage.SortAndMerge(fl);
        }
    }

    //public class MIRAOptimizor
    //{
    //    static public List<FeatureUpdatePackage> GetMIRAUpdate(
    //        List<LinearFunctionHypothesis> KBest,
    //        LinearFunctionHypothesis Gold)
    //    {
    //    }
    //}
}
