using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NanYUtilityLib.Optimizor
{
    public class MIRAOptimizor
    {
        public double C { get; set; }

        public MIRAOptimizor()
        {
        }

        public MIRAOptimizor(double C)
        {
            this.C = C;
        }

        /// <summary>
        /// compute coefficient for updating.
        /// </summary>
        /// <param name="loss">loss[i]: loss for hypothesis i</param>
        /// <param name="scoreDiff">hypoScoreDiff[i]: score(hypo[i]) - score(ref)</param>
        /// <param name="featDistance">featDiffSimMtx[i, j] = (f(ref) - f(hypo[i])) * (f(ref) - f(hypo[j]))</param>
        /// <returns></returns>
        public double[] ComputeAlpha(double[] loss, double[] scoreDiff, double[,] featDistance)
        {
            int K = loss.Length;
            double[] marginLoss = new double[K];

            for (int i = 0; i < K; ++i)
            {
                marginLoss[i] = Math.Max(0, scoreDiff[i] + loss[i]);
            }

            double[] alpha = hildreth(featDistance, marginLoss);

            for (int i = 0; i < alpha.Length; ++i)
            {
                alpha[i] = Math.Min(alpha[i], C);
            }

            return alpha;
        }

        /// <summary>
        /// Heidreth's procedure for solve the following quadratic problem:
        /// argmin(x) = x' A x + b'x, x >= 0
        /// </summary>
        /// <param name="A">A</param>
        /// <param name="b">b</param>
        /// <returns>x</returns>
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

                diff_alpha = A[max_kkt_i, max_kkt_i] <= zero ? 0.0 : F[max_kkt_i] / A[max_kkt_i, max_kkt_i];
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
    }
}
