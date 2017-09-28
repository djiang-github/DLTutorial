using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NanYUtilityLib;
using NanYUtilityLib.Optimizor;

namespace LinearFunction
{
    struct BigramState
    {
        public int tag;
    }

    public class BigramChainDecoder : ILinearChainDecoder
    {
        public BigramChainDecoder(LinearChainModelInfo modelInfo, ILinearFunction model, int BeamSize)
        {
            this.model = model;

            this.BeamSize = BeamSize;

            this.modelInfo = modelInfo;

            this.TagCount = modelInfo.TagCount;

            tagBuffer = new int[TagCount];

            scoreBuffer = new float[TagCount];

            scoreHeap = new SingleScoredHeapWithQueryKey<int, BigramState>(BeamSize);

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

            //PosteriorDecoding();

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

                //if (RefBeamId < 0)
                //{
                //    // reference tags already fall out of beam.
                //    // do early update
                //    // early update is important
                //    // it is a special case of violation update
                //    break;
                //}
            }

            //if (RefBeamId == 0)
            //{
                // 1-best result conforms to gold tags.
                // no need to update
             //   updates = null;
             //   return true;
            //}

            updates = GetUpdates();

            return true;
        }

        private List<FeatureUpdatePackage> GetUpdatesMIRA()
        {

            int[] bestH = BackTrackBestPartialHypothesis();
            if (bestH == null)
            {
                return null;
            }

            int errocnt = 0;
            for (int i = 1; i < TotalLength - 1; ++i)
            {
                if (reftags[i] != bestH[i])
                {
                    errocnt += 1;
                }
            }

            if (errocnt == 0)
            {
                return new List<FeatureUpdatePackage>();
            }

            //var reffeature = GetFeaturesAndScore(reftags, out refs);

            //double preds;

            //var pfeature = GetFeaturesAndScore(bestH, out preds);

            ////int errocnt = GetErrorCount(reftags, bestH, Next);

            ////if (errocnt == 0)
            ////{
            ////    return new List<FeatureUpdatePackage>();
            ////}

            //var fupl = GetDiff(reffeature, pfeature);

            //if (fupl.Count == 0)
            //{
            //    return fupl;
            //}

            //double scorediff = 0;

            //scorediff = preds - refs;

            double scorediff;

            var fupl = GetMaxViolationUpdate(bestH, out scorediff, out errocnt);

            if (scorediff < 0)
            {
                //throw new Exception("search error!");
                Console.Error.WriteLine("Search Error!");
                return new List<FeatureUpdatePackage>();
            }

            double[] sd = { scorediff };

            double[] loss = { (double)errocnt };

            double[,] A = new double[1, 1];

            foreach (var f in fupl)
            {
                A[0, 0] += f.delta * f.delta;
            }

            MIRAOptimizor mira = new MIRAOptimizor(0.001);

            double[] alpha = mira.ComputeAlpha(loss, sd, A);

            foreach (var f in fupl)
            {
                f.delta *= (float)alpha[0];
            }

            return fupl;
        }

        
        private List<FeatureUpdatePackage> GetUpdates()
        {
            //return GetUpdatesMIRA();
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

            //return GetUpdatePerceptron(bestH, bpoint);//GetUpdatePassiveAggressive(bestH, bpoint);
            double sdiff;
            int errorcnt;
            return GetMaxViolationUpdate(bestH, out sdiff, out errorcnt);
        }


        //private List<FeatureUpdatePackage> GetUpdateMIRA()
        //{
        //    var hypoList = BackTrackAllPartialHypothesis();

        //    if (hypoList == null || hypoList.Count <= 0)
        //    {
        //        return null;
        //    }

        //    var hypoLoss = new List<double>();

        //    var features = new List<List<FeatureUpdatePackage>>();

        //    var featDist = new List<List<FeatureUpdatePackage>>();

        //    double refscore;

        //    var reffeature = GetFeaturesAndScore(reftags, out refscore);

        //    foreach (var hypo in hypoList)
        //    {
        //        int errorCount = GetErrorCount(reftags, hypo, Next);

        //        double s;
        //        var f = GetFeaturesAndScore(hypo, out s);
        //        var diff = GetDiff(reffeature, f);

        //        features.Add(f);
        //        featDist.Add(diff);
        //        double loss = Math.Max(0, s - refscore + Math.Sqrt(errorCount));

        //        hypoLoss.Add(loss);
        //    }

        //    double[] alpha = hildreth(featDist, hypoLoss);
        //    for (int i = 0; i < alpha.Length; ++i)
        //    {
        //        alpha[i] = Math.Min(alpha[i], PA_C);
        //    }

        //    var fup = new List<FeatureUpdatePackage>();

        //    for (int i = 0; i < featDist.Count; ++i)
        //    {
        //        var fl = featDist[i];
        //        foreach (var f in fl)
        //        {
        //            fup.Add(new FeatureUpdatePackage(f.feature, f.tag, f.delta * (float)alpha[i]));
        //        }
        //    }

        //    fup.Sort();

        //    if (fup.Count <= 1)
        //    {
        //        return fup;
        //    }

        //    var compactFUP = new List<FeatureUpdatePackage>();

        //    var tmpFUP = new FeatureUpdatePackage(fup[0].feature, fup[0].tag, fup[0].delta);

        //    for (int i = 1; i < fup.Count; ++i)
        //    {
        //        var thisFUP = fup[i];

        //        if (thisFUP.IsMergeable(tmpFUP))
        //        {
        //            tmpFUP.delta += thisFUP.delta;
        //        }
        //        else
        //        {
        //            if (tmpFUP.delta != 0)
        //            {
        //                compactFUP.Add(tmpFUP);
        //            }
        //            tmpFUP = new FeatureUpdatePackage(thisFUP.feature, thisFUP.tag, thisFUP.delta);
        //        }
        //        if (i == fup.Count - 1)
        //        {
        //            if (tmpFUP.delta != 0)
        //            {
        //                compactFUP.Add(tmpFUP);
        //            }
        //        }
        //    }

        //    return compactFUP;
        //}
        private double FeatureInnerProduct(List<FeatureUpdatePackage> a, List<FeatureUpdatePackage> b)
        {
            double p = 0;

            int bi = 0;

            int ai = 0;
            while(ai < a.Count && bi < b.Count)
            {
                //int r = 
                int r = a[ai].CompareTo(b[bi]);

                if (r == 0)
                {
                    p += a[ai].delta * b[bi].delta;
                    ai++;
                    bi++;
                }
                else if (r < 0)
                {
                    ai++;
                }
                else
                {
                    bi++;
                }
            }

            return p;
        }

        /// <summary>
        /// Heidreth's procedure for solve the following quadratic problem:
        /// argmin(x) = x' A x + b'x, x >= 0
        /// </summary>
        /// <param name="A"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private static double[] hildreth(double[,] A, double[] b)
        {
            int max_iter = 10000;

            double eps = 0.00000001;
            double zero = 0.000000000001;

            double[] alpha = new double[b.Length];

            double[] F = new double[b.Length];
            double[] kkt = new double[b.Length];
            double max_kkt = double.NegativeInfinity;

            int K = b.Length;

            int max_kkt_i = -1;

            for (int i = 0; i < F.Length; i++)
            {
                F[i] = b[i];
                kkt[i] = F[i];
                if (kkt[i] > max_kkt) { max_kkt = kkt[i]; max_kkt_i = i; }
            }

            int iter = 0;
            double diff_alpha;
            double try_alpha;
            double add_alpha;

            while (max_kkt >= eps && iter < max_iter)
            {

                diff_alpha = A[max_kkt_i,max_kkt_i] <= zero ? 0.0 : F[max_kkt_i] / A[max_kkt_i, max_kkt_i];
                try_alpha = alpha[max_kkt_i] + diff_alpha;
                add_alpha = 0.0;

                if (try_alpha < 0.0)
                    add_alpha = -1.0 * alpha[max_kkt_i];
                else
                    add_alpha = diff_alpha;

                alpha[max_kkt_i] = alpha[max_kkt_i] + add_alpha;

                for (int i = 0; i < F.Length; i++)
                {
                    F[i] -= add_alpha * A[i, max_kkt_i];
                    kkt[i] = F[i];
                    if (alpha[i] > zero)
                        kkt[i] = Math.Abs(F[i]);
                }

                max_kkt = double.NegativeInfinity;
                max_kkt_i = -1;
                for (int i = 0; i < F.Length; i++)
                    if (kkt[i] > max_kkt) { max_kkt = kkt[i]; max_kkt_i = i; }

                iter++;
            }

            return alpha;
        }

        private double[] hildreth(List<List<FeatureUpdatePackage>> a, List<double> b)
        {
            int max_iter = 10000;

            double eps = 0.00000001;
            double zero = 0.000000000001;

            double[] alpha = new double[b.Count];

            double[] F = new double[b.Count];
            double[] kkt = new double[b.Count];
            double max_kkt = double.NegativeInfinity;

            int K = a.Count;

            double[][] A = new double[K][];

            for (int i = 0; i < K; ++i)
            {
                A[i] = new double[K];
            }
            bool[] is_computed = new bool[K];
            for (int i = 0; i < K; i++)
            {
                A[i][i] = FeatureInnerProduct(a[i], a[i]);
                is_computed[i] = false;
            }

            int max_kkt_i = -1;


            for (int i = 0; i < F.Length; i++)
            {
                F[i] = b[i];
                kkt[i] = F[i];
                if (kkt[i] > max_kkt) { max_kkt = kkt[i]; max_kkt_i = i; }
            }

            int iter = 0;
            double diff_alpha;
            double try_alpha;
            double add_alpha;

            while (max_kkt >= eps && iter < max_iter)
            {

                diff_alpha = A[max_kkt_i][max_kkt_i] <= zero ? 0.0 : F[max_kkt_i] / A[max_kkt_i][max_kkt_i];
                try_alpha = alpha[max_kkt_i] + diff_alpha;
                add_alpha = 0.0;

                if (try_alpha < 0.0)
                    add_alpha = -1.0 * alpha[max_kkt_i];
                else
                    add_alpha = diff_alpha;

                alpha[max_kkt_i] = alpha[max_kkt_i] + add_alpha;

                if (!is_computed[max_kkt_i])
                {
                    for (int i = 0; i < K; i++)
                    {
                        A[i][max_kkt_i] = FeatureInnerProduct(a[i], a[max_kkt_i]); // for version 1
                        is_computed[max_kkt_i] = true;
                    }
                }

                for (int i = 0; i < F.Length; i++)
                {
                    F[i] -= add_alpha * A[i][max_kkt_i];
                    kkt[i] = F[i];
                    if (alpha[i] > zero)
                        kkt[i] = Math.Abs(F[i]);
                }

                max_kkt = double.NegativeInfinity;
                max_kkt_i = -1;
                for (int i = 0; i < F.Length; i++)
                    if (kkt[i] > max_kkt) { max_kkt = kkt[i]; max_kkt_i = i; }

                iter++;
            }

            return alpha;
        }

        private int GetErrorCount(int[] a, int[] b, int next)
        {
            int c = 0;
            for (int i = 1; i < next && i < TotalLength - 1; ++i)
            {
                if (a[i] != b[i])
                {
                    c++;
                }
            }
            return c;
        }

        private List<FeatureUpdatePackage> GetDiff(List<FeatureUpdatePackage> a, List<FeatureUpdatePackage> b)
        {
            var fup = new List<FeatureUpdatePackage>();

            foreach (var fa in a)
            {
                fup.Add(new FeatureUpdatePackage(fa.feature, fa.tag, fa.delta));
            }

            foreach (var fb in b)
            {
                fup.Add(new FeatureUpdatePackage(fb.feature, fb.tag, -fb.delta));
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

        private List<FeatureUpdatePackage> GetMaxViolationUpdate(int[] bestH, out double scoreDiff, out int errorcnt)
        {
            int argmax = -1;
            float maxViolate = 0;

            float refs = 0;
            float preds = 0;

            for (int up = 1; up < TotalLength - 1; ++up)
            {
                refs += ScoreTag(reftags, up);
                preds += ScoreTag(bestH, up);

                if (argmax == -1 || maxViolate <= preds - refs)
                {
                    argmax = up;
                    maxViolate = preds - refs;
                }
            }

            if (maxViolate < 0 || argmax < 0)
            {
                throw new Exception("Search error(maxv)!");
            }

            scoreDiff = maxViolate;

            var fup = new List<FeatureUpdatePackage>();
            errorcnt = 0;
            for (int up = 1; up <= argmax; ++up)
            {
                GetUpdate(reftags, up, 1.0f, fup);
                GetUpdate(bestH, up, -1.0f, fup);

                if (bestH[up] != reftags[up])
                {
                    errorcnt += 1;
                }
            }

            if (errorcnt == 0)
            {
                throw new Exception("Search error(errorcnt == 0)!");
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

        private List<FeatureUpdatePackage> GetFeaturesAndScore(int[] hypo, out double score)
        {
            var fup = new List<FeatureUpdatePackage>();
            score = 0;
            for (int up = 1; up < Next && up < TotalLength - 1; ++up)
            {
                GetUpdate(hypo, up, 1, fup);
                score += ScoreTag(hypo, up);
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

        private List<FeatureUpdatePackage> GetUpdatePerceptron(int[] bestH, int bpoint)
        {
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

        private List<FeatureUpdatePackage> GetUpdatePassiveAggressive(int[] bestH, int bpoint)
        {
            double loss = 0;

            for (int up = bpoint; up < Next && up < TotalLength - 1; ++up)
            {
                if (bestH[up] != reftags[up])
                {
                    loss++;
                }
            }

            loss = Math.Sqrt(loss);

            var fup = new List<FeatureUpdatePackage>();
            double refscore = 0;
            double predictscore = 0;

            for (int up = bpoint; up < Next && up < TotalLength - 1; ++up)
            {
                GetUpdate(reftags, up, 1, fup);
                refscore += ScoreTag(reftags, up);
                GetUpdate(bestH, up, -1, fup);
                predictscore += ScoreTag(bestH, up);
            }

            loss += predictscore - refscore;

            fup.Sort();

            double diff = GetFeatureDifference(fup);

            float tau = (float)Math.Min(loss / diff, PA_C);

            if (fup.Count <= 1)
            {
                fup[0].delta *= tau;
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
                        tmpFUP.delta *= tau;
                        compactFUP.Add(tmpFUP);
                    }
                    tmpFUP = new FeatureUpdatePackage(thisFUP.feature, thisFUP.tag, thisFUP.delta);
                }
                if (i == fup.Count - 1)
                {
                    if (tmpFUP.delta != 0)
                    {
                        tmpFUP.delta *= tau;
                        compactFUP.Add(tmpFUP);
                    }
                }
            }

            return compactFUP;
        }

        private static double GetFeatureDifference(List<FeatureUpdatePackage> fup)
        {
            double diff;
            if (fup.Count <= 1)
            {
                diff = 1;
            }
            else
            {
                diff = 0;
                var xtmpFUP = new FeatureUpdatePackage(fup[0].feature, fup[0].tag, 1);

                for (int i = 1; i < fup.Count; ++i)
                {
                    var thisFUP = fup[i];
                    if (thisFUP.IsMergeable(xtmpFUP))
                    {
                        xtmpFUP.delta += Math.Abs(thisFUP.delta);
                    }
                    else
                    {
                        diff += xtmpFUP.delta * xtmpFUP.delta;
                        xtmpFUP = new FeatureUpdatePackage(thisFUP.feature, thisFUP.tag, 1);
                    }
                    if (i == fup.Count - 1)
                    {
                        diff += xtmpFUP.delta * xtmpFUP.delta;
                    }
                }

            }
            return diff;
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

        private List<int[]> BackTrackAllPartialHypothesis()
        {
            if (frames[Next - 1].lnodes.Count <= 0)
            {
                return null;
            }

            int N = BeamSize;

            List<int[]> result = new List<int[]>();

            var frontierTrace = new List<TraceNode>();

            for (int i = 0; i < frames[Next - 1].lnodes.Count; ++i)
            {
                var sinkNode = new TraceNode
                {
                    beta = 0,
                    alpha = frames[Next - 1].lnodes[i].alpha,
                    lNode = frames[Next - 1].lnodes[i],
                    prevTraceNode = null,

                };

                frontierTrace.Add(sinkNode);
            }


            for (int frameId = Next - 2; frameId >= 0; --frameId)
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

            for (int i = 0; i < frontierTrace.Count; ++i)
            {
                int[] tags = new int[TotalLength];

                int next = 0;
                var trace = frontierTrace[i];

                // run forward
                while (trace != null)
                {
                    tags[next++] = trace.lNode.tag;
                    trace = trace.prevTraceNode;
                }
                //bool notRef = false;

                //for (int j = 1; j < Next && j < TotalLength - 1; ++j)
                //{
                //    if (tags[j] != reftags[j])
                //    {
                //        notRef = true;
                //    }
                //}
                //if (notRef)
                //{
                    result.Add(tags);
                //    break;
                //}
            }

            return result;
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
            return thisTag;
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
            BigramState[] states;
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
            

            var preSinkNodes = frames[TotalLength - 2].lnodes;

            var SinkNode = new LatticeNode
            {
                alpha = preSinkNodes[0].alpha,
                beta = 0,
                tag = TagCount,
                ptag = -1,
                backwardLinks = new List<TransitionNode>(),
                forwardLinks = new List<TransitionNode>(),
            };

            frames[TotalLength - 1].lnodes.Add(SinkNode);

            foreach (var pnode in preSinkNodes)
            {
                var transition = new TransitionNode
                {
                    startNode = pnode,
                    endNode = SinkNode,
                    alpha = pnode.alpha,
                    beta = 0,
                    score = 0
                };

                pnode.forwardLinks.Add(transition);
                SinkNode.backwardLinks.Add(transition);
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

        private void FillInNextFrame(BigramState[] states, float[] scores)
        {
            for (int bid = 0; bid < states.Length; ++bid)
            {
                var state = states[bid];
                var tag = state.tag;

                var newLNode = new LatticeNode
                {
                    tag = tag,
                    ptag = -1,
                    alpha = scores[bid],
                    beta = 0,
                    backwardLinks = new List<TransitionNode>(),
                    forwardLinks = new List<TransitionNode>(),
                    deltaScoreBuffer = new float[TagCount],
                };

                frames[Next].lnodes.Add(newLNode);

                foreach (var pnode in frames[Next - 1].lnodes)
                {
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

        private void GenNextTag(int[] ForcedTag, LatticeNode lnode, SingleScoredHeapWithQueryKey<int, BigramState> scoreHeap)
        {
            int count = ScoreNextTag(lnode, ForcedTag);

            for (int j = 0; j < count; ++j)
            {
                float s = scoreBuffer[j] + lnode.alpha;

                int key = GetStateKey(tagBuffer[j], lnode.tag);

                if (scoreHeap.IsAcceptableScore(s))
                {
                    var state = new BigramState { tag = tagBuffer[j] };

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

        private double[][,] GetTransitionMatrix()
        {
            var MatrixArray = new double[TotalLength - 1][,];

            for (int i = 0; i < TotalLength - 1; ++i)
            {
                var thisFrame = frames[i];
                var nextFrame = frames[i + 1];
                MatrixArray[i] = new double[thisFrame.lnodes.Count, nextFrame.lnodes.Count];

                for (int thisNodeId = 0; thisNodeId < thisFrame.lnodes.Count; ++thisNodeId)
                {
                    var thisNode = thisFrame.lnodes[thisNodeId];

                    for (int nextNodeId = 0; nextNodeId < nextFrame.lnodes.Count; ++nextNodeId)
                    {
                        var nextNode = nextFrame.lnodes[nextNodeId];
                        MatrixArray[i][thisNodeId, nextNodeId]
                            = i == TotalLength - 2 ? 1.0 :
                            Math.Exp(thisNode.deltaScoreBuffer[nextNode.tag] / 100.0f);
                    }
                }
            }

            return MatrixArray;
        }

        private double[][] GetForwardMatrix(double[][,] TransitionMatrix)
        {
            var Alphas = new double[TotalLength][];

            Alphas[0] = new double[1];
            Alphas[0][0] = 1.0;

            for (int i = 1; i < TotalLength - 1; ++i)
            {
                var pFrame = frames[i - 1];
                var cFrame = frames[i];

                var pNodes = pFrame.lnodes;
                var cNodes = cFrame.lnodes;

                Alphas[i] = new double[cNodes.Count];

                for (int cId = 0; cId < cNodes.Count; ++cId)
                {
                    for (int pId = 0; pId < pNodes.Count; ++pId)
                    {
                        Alphas[i][cId] += Alphas[i - 1][pId]
                            * TransitionMatrix[i - 1][pId, cId];
                    }
                }
            }

            return Alphas;
        }

        private double[][] GetBackwardMatrix(double[][,] TransitionMatrix)
        {
            var Betas = new double[TotalLength][];
            Betas[TotalLength - 1] = new double[1];
            Betas[TotalLength - 1][0] = 1.0;

            for (int i = TotalLength - 2; i >= 0; --i)
            {
                var nFrame = frames[i + 1];
                var cFrame = frames[i];

                var nNodes = nFrame.lnodes;
                var cNodes = cFrame.lnodes;

                Betas[i] = new double[cNodes.Count];

                for (int cId = 0; cId < cNodes.Count; ++cId)
                {
                    for (int nId = 0; nId < nNodes.Count; ++nId)
                    {
                        Betas[i][cId] += Betas[i + 1][nId]
                            * TransitionMatrix[i][cId, nId];
                    }
                }
            }

            return Betas;
        }

        private void PosteriorDecoding()
        {
            var TransitionMatrix = GetTransitionMatrix();

            var Alphas = GetForwardMatrix(TransitionMatrix);
            var Betas = GetBackwardMatrix(TransitionMatrix);
            var Marginals = new double[TotalLength][];

            for (int i = 1; i < TotalLength - 1; ++i)
            {
                Marginals[i] = new double[Alphas[i].Length];

                double sum = 0;

                for (int j = 0; j < Alphas[i].Length; ++j)
                {
                    Marginals[i][j] = Alphas[i][j] * Betas[i][j];
                    sum += Marginals[i][j];
                }

                for (int j = 0; j < Alphas[i].Length; ++j)
                {
                    Marginals[i][j] /= sum;
                }
            }
        }

        private float ScoreTag(int[] tags, int nextId)
        {
            modelCache.GetScore(Observations, tags, nextId, scoreBuffer);
            return scoreBuffer[tags[nextId]];
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

        private SingleScoredHeapWithQueryKey<int, BigramState> scoreHeap;

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
        public double PA_C = 1.0;
        public bool IsME = false;
    }
}
