using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NanYUtilityLib
{
    /// <summary>
    /// Efficent categorical distribution sampler.
    /// Algorithm is alias method.
    /// Setup complexity is O(K), where K is the number of categories.
    /// Sampling complexity is O(1).
    /// </summary>
    public class CategoricalDistribution
    {
        private struct BernoulliDist
        {
            public int a;
            public int b;
            public double pa;
        }

        public CategoricalDistribution(double[] probs, Random r)
        {
            p = probs;
            this.r = r;
            BuildAlias();
        }

        public CategoricalDistribution(double[] probs)
            : this(probs, new Random())
        {
        }

        public int Sample()
        {
            int k = r.Next(p.Length);
            double x = r.NextDouble();
            return x < BD[k].pa ? BD[k].a : BD[k].b;
        }

        public double Prob(int n)
        {
            return p[n];
        }

        private void BuildAlias()
        {
            int K = p.Length;

            BD = new BernoulliDist[K];

            for (int i = 0; i < K; ++i)
            {
                BD[i].a = i;
            }

            List<int> S = new List<int>();
            List<int> L = new List<int>();

            for (int i = 0; i < K; ++i)
            {
                BD[i].pa = K * p[i];

                if (BD[i].pa < 1.0)
                {
                    S.Add(i);
                }
                else
                {
                    L.Add(i);
                }
            }

            int nextS = S.Count - 1;
            int nextL = L.Count - 1;

            while (nextS >= 0 && nextL >= 0)
            {
                int l = L[nextL];
                int s = S[nextS];
                BD[s].b = l;
                BD[l].pa -= 1.0 - BD[s].pa;
                nextS--;
                if (BD[l].pa < 1.0)
                {
                    nextL--;
                    S[++nextS] = l;
                }
            }
        }

        private BernoulliDist[] BD;
        private double[] p;
        private Random r;
    }
}
