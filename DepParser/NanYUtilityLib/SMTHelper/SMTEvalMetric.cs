using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NanYUtilityLib.SMT
{
    public class RIBESCalculator
    {
        public RIBESCalculator()
            : this(DefaultAlpha, DefaultBeta)
        {
        }

        public RIBESCalculator(double alpha, double beta)
        {
            this.alpha = alpha;
            this.beta = beta;
        }

        public double Calc(string refTran, string trans)
        {
            if (string.IsNullOrWhiteSpace(refTran) || string.IsNullOrWhiteSpace(trans))
            {
                return 0;
            }

            string[] reftok = refTran.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            string[] transtok = trans.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            if (reftok.Length == 0 || transtok.Length == 0)
            {
                return 0;
            }

            if (reftok.Length == 1 && transtok.Length == 1)
            {
                return reftok[0] == transtok[0] ? 1 : 0;
            }

            if (reftok.Length == 1)
            {
                // there is no way to calculate kendal's tau if reference only have one word.
                int refmatch = Array.FindIndex<string>(transtok, (x) => (x == reftok[0]));
                if (refmatch < 0)
                {
                    return 0;
                }
                else
                {
                    double p = 1.0 / (double)transtok.Length;
                    double r = 1.0;
                    double f_b = (1.0 + beta * beta) * p * r / (beta * beta * p + r);
                    return Math.Pow(f_b, alpha);
                }
            }

            if (transtok.Length == 1)
            {
                int refmatch = Array.FindIndex<string>(reftok, (x) => (x == transtok[0]));
                if (refmatch < 0)
                {
                    return 0;
                }
                else
                {
                    double p = 1.0;
                    double r = 1.0 / reftok.Length;
                    double f_b = (1.0 + beta * beta) * p * r / (beta * beta * p + r);
                    return Math.Pow(f_b, alpha);
                }
            }

            // now both reference and token have more than more word;
            List<int> worder = GetWorder(transtok, reftok);

            if (worder.Count <= 1)
            {
                return 0;
            }

            double NKT = ComputeNormalizedKendalTau(worder);

            double precision = worder.Count / (double)transtok.Length;
            double recall = worder.Count / (double)reftok.Length;
            double fscore = (1.0 + beta * beta) * precision * recall / (beta * beta * precision + recall);

            return NKT * Math.Pow(fscore, alpha);

        }

        public double Calc(List<string> refTranList, string trans)
        {
            if (refTranList.Count <= 0)
            {
                return 0;
            }

            double ribes = 0;

            foreach (var refTran in refTranList)
            {
                double sntribes = Calc(refTran, trans);
                ribes = Math.Max(sntribes, ribes);
            }

            return ribes;
        }

        private double ComputeNormalizedKendalTau(List<int> worder)
        {
            if (worder == null || worder.Count <= 1)
            {
                return 0;
            }

            int incrP = 0;
            int N = worder.Count;
            for (int i = 0; i < N - 1; ++i)
            {
                for (int j = i + 1; j < N; ++j)
                {
                    if (worder[i] < worder[j])
                    {
                        incrP++;
                    }
                }
            }

            double kt = 4.0 * incrP / (double)(N * (N - 1)) - 1.0;

            return (1.0 + kt) / 2;
        }

        // what is Worder???
        // this is from NTT's paper
        // Worder stand for word order, haha!
        private List<int> GetWorder(string[] trantok, string[] reftok)
        {
            System.Diagnostics.Debug.Assert(trantok.Length > 1 && reftok.Length > 1);

            string[] refbigram = ConvertToBigram(reftok);
            string[] tranbigram = ConvertToBigram(trantok);

            var tranUnigramPosition = GetPositionDict(trantok);
            var refUnigramPosition = GetPositionDict(reftok);
            var tranBigramPosition = GetPositionDict(tranbigram);
            var refBigramPosition = GetPositionDict(refbigram);

            var worder = new List<int>();

            for (int i = 0; i < trantok.Length; ++i)
            {
                int uniRefP = GetPosition(trantok[i], refUnigramPosition);
                int uniTranP = GetPosition(trantok[i], tranUnigramPosition);

                if (uniRefP >= 0 && uniTranP >= 0)
                {
                    worder.Add(uniRefP);
                    continue;
                }

                if (i < trantok.Length - 1)
                {
                    int biref = GetPosition(tranbigram[i], refBigramPosition);
                    int bitran = GetPosition(tranbigram[i], tranBigramPosition);
                    if (biref >= 0 && bitran >= 0)
                    {
                        worder.Add(biref);
                        continue;
                    }
                }

                if (i > 0)
                {
                    int pbiref = GetPosition(tranbigram[i - 1], refBigramPosition);
                    int pbitran = GetPosition(tranbigram[i - 1], tranBigramPosition);
                    if (pbiref >= 0 && pbitran >= 0)
                    {
                        worder.Add(pbiref + 1);
                        continue;
                    }
                }

            }

            return worder;
        }

        private int GetPosition(string ngram, Dictionary<string, int> positionDict)
        {
            int p;
            if (!positionDict.TryGetValue(ngram, out p))
            {
                return -1;
            }
            return p;
        }

        private string[] ConvertToBigram(string[] tok)
        {
            string[] bigram = new string[tok.Length - 1];

            for (int i = 0; i < bigram.Length; ++i)
            {
                bigram[i] = tok[i] + " " + tok[i + 1];
            }

            return bigram;
        }

        private Dictionary<string, int> GetPositionDict(string[] tok)
        {
            var pdict = new Dictionary<string, int>();

            for (int i = 0; i < tok.Length; ++i)
            {
                string t = tok[i];
                if (pdict.ContainsKey(t))
                {
                    pdict[t] = -1;
                }
                else
                {
                    pdict[t] = i;
                }
            }

            return pdict;
        }

        const double DefaultAlpha = 0.25;
        const double DefaultBeta = 0.10;

        private double alpha { get; set; }
        private double beta { get; set; }
    }
}
