using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinearFunction
{
    public class APFunc
    {
        public int tag;

        private float _v;

        private double _x;

        public int UpdateCount { get { return Math.Abs(updateC); } }

        public float weight
        {
            get { return _v; }
            private set { _v = value; }
        }

        public APFunc(int tag, float value, long time, int UpdateThreshold)
        {
            this.tag = tag;

            if (value > 0)
            {
                updateC = 1;
            }
            else if (value < 0)
            {
                updateC = -1;
            }

            if (UpdateCount > UpdateThreshold)
            {
                _v = value;
                _x = time * (double)value;
            }
        }

        public void Update(float incr, long time, int UpdateThreshold)
        {
            if (UpdateCount > UpdateThreshold)
            {
                _v += incr;
                _x += (double)incr * time;
            }
            else
            {
                if (incr > 0)
                {
                    updateC++;
                }
                else if (incr < 0)
                {
                    updateC--;
                }
                if (UpdateCount > UpdateThreshold)
                {
                    _v = incr;
                    _x = time * (double)incr;
                }
            }
        }

        public void Burn()
        {
            _x = 0;
        }
        public APFunc Clone()
        {
            var cl = new APFunc();
            cl.tag = tag;
            cl._v = _v;
            cl._x = _x;
            cl.updateC = updateC;
            return cl;
        }

        private int updateC = 0;

        public float Avg(long time)
        {
            float x = _v - (float)(_x / time);

            //return Math.Abs(x) < MinValue ? 0 : x;
            return x;
        }

        static public APFunc Merge(APFunc a, APFunc b, float w_a)
        {
            if (a == null && b == null)
            {
                return null;
            }

            if (a == null)
            {
                return Weighting(b, 1.0f - w_a);
            }

            if (b == null)
            {
                return Weighting(a, w_a);
            }

            if (a.tag != b.tag)
            {
                throw new Exception("Cannot merge two incompatable APFunc");
            }

            var m = new APFunc();

            m.tag = a.tag;

            m._v = w_a * a._v + (1.0f - w_a) * b._v;
            m._x = w_a * a._x + (1.0f - w_a) * b._x;

            m.updateC = a.UpdateCount > b.UpdateCount ?
                a.updateC : b.updateC;

            return m;
        }

        static public APFunc Weighting(APFunc a, float w)
        {
            if (a == null)
            {
                return a;
            }

            var m = new APFunc();

            m.tag = a.tag;
            m._v = a._v * w;
            m._x = a._x * w;
            m.updateC = a.updateC;
            return m;
        }

        public void Merge(float otherWeight, APFunc other)
        {
            _v = otherWeight * other._v + (1.0f - otherWeight) * _v;
            _x = otherWeight * other._x + (1.0f - otherWeight) * _x;

            updateC = UpdateCount > other.UpdateCount ? updateC : other.updateC;
        }

        public void Merge(float otherWeight)
        {
            _v *= 1.0f - otherWeight;
            _x *= 1.0f - otherWeight;
        }

        private APFunc()
        { }

        public const float MinValue = 0.00000001f;
    }

    public class AveragePerceptronModel : ILinearFunction
    {
        public void GetScores(LinearModelFeature feature, float[] scores)
        {
            Array.Clear(scores, 0, scores.Length);
            AddScores(feature, scores);
        }

        public void AddScores(LinearModelFeature feature, float[] scores)
        {
            APFunc[] ffs;
            if (scoreDicts[feature.DictId].TryGetValue(feature, out ffs))
            {
                foreach (APFunc ff in ffs)
                {
                    scores[ff.tag] += IsTraining ? ff.weight : ff.Avg(Time);
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
            }
        }

        public void GetScores(LinearModelFeature feature, float featurevalue, float[] scores)
        {
            Array.Clear(scores, 0, scores.Length);
            AddScores(feature, featurevalue, scores);
        }

        public void AddScores(LinearModelFeature feature, float featurevalue, float[] scores)
        {
            APFunc[] ffs;
            if (scoreDicts[feature.DictId].TryGetValue(feature, out ffs))
            {
                foreach (APFunc ff in ffs)
                {
                    scores[ff.tag] += (IsTraining ? ff.weight : ff.Avg(Time)) * featurevalue;
                }
            }
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

        public void Update(IEnumerable<FeatureUpdatePackage> updates)
        {
            if (updates != null)
            {
                foreach (FeatureUpdatePackage upd in updates)
                {
                    Dictionary<LinearModelFeature, APFunc[]> dict = scoreDicts[upd.DictId];

                    APFunc[] funcs;

                    if (!upd.feature.IsValid)
                    {
                        continue;
                    }

                    if (dict.TryGetValue(upd.feature, out funcs))
                    {
                        bool found = false;
                        for (int i = 0; i < funcs.Length; ++i)
                        {
                            if (funcs[i].tag == upd.tag)
                            {
                                funcs[i].Update(upd.delta, Time, UpdateThreshold);
                                found = true;
                                break;
                            }
                        }
                        if (!found)
                        {
                            APFunc[] newFuncs = new APFunc[funcs.Length + 1];
                            funcs.CopyTo(newFuncs, 0);
                            newFuncs[newFuncs.Length - 1] = new APFunc(upd.tag, (float)upd.delta, Time, UpdateThreshold);
              
                            dict[upd.feature] = newFuncs;
                        }
                    }
                    else
                    {
                        funcs = new APFunc[1];
                        funcs[0] = new APFunc(upd.tag, (float)upd.delta, Time, UpdateThreshold);
                        
                        dict[upd.feature] = funcs;
                    }
                }
            }
            Time++;
        }

        public void UpdateNoTimer(IEnumerable<FeatureUpdatePackage> updates)
        {
            if (updates != null)
            {
                foreach (FeatureUpdatePackage upd in updates)
                {
                    Dictionary<LinearModelFeature, APFunc[]> dict = scoreDicts[upd.DictId];

                    APFunc[] funcs;

                    if (!upd.feature.IsValid)
                    {
                        continue;
                    }

                    if (dict.TryGetValue(upd.feature, out funcs))
                    {
                        bool found = false;
                        for (int i = 0; i < funcs.Length; ++i)
                        {
                            if (funcs[i].tag == upd.tag)
                            {
                                funcs[i].Update(upd.delta, Time, UpdateThreshold);
                                found = true;
                                break;
                            }
                        }
                        if (!found)
                        {
                            APFunc[] newFuncs = new APFunc[funcs.Length + 1];
                            funcs.CopyTo(newFuncs, 0);
                            newFuncs[newFuncs.Length - 1] = new APFunc(upd.tag, (float)upd.delta, Time, UpdateThreshold);

                            dict[upd.feature] = newFuncs;
                        }
                    }
                    else
                    {
                        funcs = new APFunc[1];
                        funcs[0] = new APFunc(upd.tag, (float)upd.delta, Time, UpdateThreshold);

                        dict[upd.feature] = funcs;
                    }
                }
            }
        }
        public void Update()
        {
            Time++;
        }

        public void Merge(float otherWeight, AveragePerceptronModel other)
        {
            if (other == null)
            {
                return;
            }

            if (other.scoreDicts.Length != scoreDicts.Length)
            {
                throw new System.ArgumentException(
                    "Cannot merge incompatable models.");
            }

            for (int i = 0; i < scoreDicts.Length; ++i)
            {
                if (scoreDicts[i] == null || other.scoreDicts[i] == null)
                {
                    continue;
                }

                var thisdict = scoreDicts[i];
                var otherdict = other.scoreDicts[i];
                var newDict = new Dictionary<LinearModelFeature, APFunc[]>();

                foreach (var feature in thisdict.Keys)
                {
                    APFunc[] thisFunc = thisdict[feature];
                    APFunc[] otherFunc;
                    if(!otherdict.TryGetValue(feature, out otherFunc))
                    {
                        otherFunc = null;
                    }
                    newDict[feature] = MergeArray(thisFunc, otherFunc, 1.0f - otherWeight);
                }

                foreach (var feature in otherdict.Keys)
                {
                    if (newDict.ContainsKey(feature))
                    {
                        continue;
                    }
                    APFunc[] otherFunc = otherdict[feature];
                    newDict[feature] = MergeArray(otherFunc, null, otherWeight);
                }

                scoreDicts[i] = newDict;
            }
        }

        public AveragePerceptronModel(int DictCount, int UpdateThreshold)
        {
            scoreDicts = new Dictionary<LinearModelFeature, APFunc[]> [DictCount];
            
            for (int i = 0; i < scoreDicts.Length; ++i)
            {
                scoreDicts[i] = new Dictionary<LinearModelFeature, APFunc[]>();
            }

            IsTraining = false;

            this.UpdateThreshold = UpdateThreshold;
        }

        public AveragePerceptronModel Clone()
        {
            var cl = new AveragePerceptronModel();
            cl.Time = Time;
            cl.UpdateThreshold = UpdateThreshold;
            cl.IsTraining = IsTraining;
            cl.scoreDicts = new Dictionary<LinearModelFeature, APFunc[]>[scoreDicts.Length];

            for (int i = 0; i < scoreDicts.Length; ++i)
            {
                var thisDict = scoreDicts[i];
                if (thisDict == null)
                {
                    continue;
                }

                var clDict = new Dictionary<LinearModelFeature, APFunc[]>();
                cl.scoreDicts[i] = clDict;

                foreach (var feature in thisDict.Keys)
                {
                    var funcs = thisDict[feature];
                    var clfuncs = new APFunc[funcs.Length];

                    for (int j = 0; j < funcs.Length; ++j)
                    {
                        clfuncs[j] = funcs[j].Clone();
                    }

                    clDict[feature] = clfuncs;
                }
            }

            return cl;
        }

        public List<LinearFeatureFuncPackage> GetAllFeatures()
        {
            List<LinearFeatureFuncPackage> lffpList = new List<LinearFeatureFuncPackage>();

            for (int i = 0; i < scoreDicts.Length; ++i)
            {
                Dictionary<LinearModelFeature, APFunc[]> scoreDict = scoreDicts[i];

                foreach (LinearModelFeature feat in scoreDict.Keys)
                {
                    bool nullFeat = false;
                    foreach (var x in feat.ElemArr)
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
                    APFunc[] funcs = scoreDict[feat];

                    List<FeatureFunc> ffl = new List<FeatureFunc>();

                    foreach (APFunc f in funcs)
                    {
                        if (f.UpdateCount > UpdateThreshold && f.Avg(Time) != 0)
                        {
                            ffl.Add(new FeatureFunc { tag = f.tag, weight = f.Avg(Time) });
                        }
                    }

                    if (ffl.Count == 0)
                    {
                        continue;
                    }

                    LinearFeatureFuncPackage lffp = new LinearFeatureFuncPackage();
                    lffp.funcs = ffl.ToArray();
                    lffp.feature = feat;
                    lffpList.Add(lffp);
                }
            }

            return lffpList;
        }

        public List<LinearFeatureFuncPackage> GetAllFeaturesNoAverage()
        {
            List<LinearFeatureFuncPackage> lffpList = new List<LinearFeatureFuncPackage>();

            for (int i = 0; i < scoreDicts.Length; ++i)
            {
                Dictionary<LinearModelFeature, APFunc[]> scoreDict = scoreDicts[i];

                foreach (LinearModelFeature feat in scoreDict.Keys)
                {
                    bool nullFeat = false;
                    foreach (var x in feat.ElemArr)
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
                    APFunc[] funcs = scoreDict[feat];

                    List<FeatureFunc> ffl = new List<FeatureFunc>();

                    foreach (APFunc f in funcs)
                    {
                        if (f.UpdateCount > UpdateThreshold && f.weight != 0)
                        {
                            ffl.Add(new FeatureFunc { tag = f.tag, weight = f.weight });
                        }
                    }

                    if (ffl.Count == 0)
                    {
                        continue;
                    }

                    LinearFeatureFuncPackage lffp = new LinearFeatureFuncPackage();
                    lffp.funcs = ffl.ToArray();
                    lffp.feature = feat;
                    lffpList.Add(lffp);
                }
            }

            return lffpList;
        }

        public void Burn()
        {
            foreach (var dict in scoreDicts)
            {
                if (dict != null)
                {
                    foreach (var fs in dict.Values)
                    {
                        foreach (var f in fs)
                        {
                            f.Burn();
                        }
                    }
                }
            }
            Time = 0;
        }
        public long Time { get; private set; }

        public int UpdateThreshold { get; private set; }

        public bool IsTraining { get; set; }

        private AveragePerceptronModel()
        {
        }

        private APFunc[] MergeArray(APFunc[] a, APFunc[] b, float w_a)
        {
            if (a == null && b == null)
            {
                return null;
            }

            if (a == null)
            {
                return WeightFuncArr(b, 1.0f - w_a);
            }

            if (b == null)
            {
                return WeightFuncArr(a, w_a);
            }

            var funcList = new List<APFunc>();

            Array.Sort<APFunc>(a, (lhs, rhs) => { return lhs.tag.CompareTo(rhs.tag); });
            Array.Sort<APFunc>(b, (lhs, rhs) => { return lhs.tag.CompareTo(rhs.tag); });

            int nextA = 0;
            int nextB = 0;
            float w_b = 1.0f - w_a;
            while (nextA < a.Length && nextB < b.Length)
            {
                if (a[nextA].tag == b[nextB].tag)
                {
                    funcList.Add(APFunc.Merge(a[nextA++], b[nextB++], w_a));
                }
                else if (a[nextA].tag < b[nextB].tag)
                {
                    funcList.Add(APFunc.Weighting(a[nextA++], w_a));
                }
                else
                {
                    funcList.Add(APFunc.Weighting(b[nextB++], w_b));
                }
            }

            while (nextA < a.Length)
            {
                funcList.Add(APFunc.Weighting(a[nextA++], w_a));
            }

            while (nextB < b.Length)
            {
                funcList.Add(APFunc.Weighting(b[nextB++], w_b));
            }

            return funcList.ToArray();
        }

        private APFunc[] WeightFuncArr(APFunc[] a, float w)
        {
            if (a == null)
            {
                return null;
            }

            APFunc[] m = new APFunc[a.Length];

            for (int i = 0; i < a.Length; ++i)
            {
                m[i] = APFunc.Weighting(a[i], w);
            }

            return m;
        }

        Dictionary<LinearModelFeature, APFunc[]>[] scoreDicts;

        static public AveragePerceptronModel Merge(IEnumerable<AveragePerceptronModel> Models)
        {
            long Time = 0;
            int UpdateThreshold = 0;
            int DictCount = 0;
            foreach (var model in Models)
            {
                Time += model.Time;
                UpdateThreshold = Math.Max(UpdateThreshold, model.UpdateThreshold);
                DictCount = Math.Max(DictCount, model.scoreDicts.Length);
            }

            AveragePerceptronModel MModel = new AveragePerceptronModel(DictCount, UpdateThreshold);
            //long AccTime = 0;

            foreach (var model in Models)
            {
                for (int i = 0; i < model.scoreDicts.Length; ++i)
                {
                    var sourceDict = model.scoreDicts[i];
                    var targetDict = MModel.scoreDicts[i];

                }
            }

            return null;
        }
    }
}
