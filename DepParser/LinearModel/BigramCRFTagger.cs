using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinearModel
{
    /// <summary>
    /// Bigram Chain CRF decoder;
    /// </summary>
    public class BigramCRFTagger
    {
        /// <summary>
        /// M[i][j,k] = exp(w * f(y[i - 1] = j, y[i] = k)
        /// Assuming M[0] and M[M.Length - 1] is special matrices
        /// representing starting/ending transitions.
        /// </summary>
        /// <param name="M"></param>
        public BigramCRFTagger(double[][,] M)
        {
            this.M = M;
            AllocateMemory(M.Length, M[0].GetLength(0));
        }

        public void Run()
        {
            Clear();

            ForwardBackward();

            // compute normalizing factor
            Z = 0;
            for (int i = 0; i < TagCount; ++i)
            {
                Z += alpha[Length - 1][i];
            }
        }

        public double TranstionProb(int i, int j, int k)
        {
            return alpha[i - 1][j] * M[i][j, k] * beta[i][k] / Z;
        }

        public double MarginalProb(int i, int j)
        {
            return alpha[i][j] * beta[i][j] / Z;
        }

        /// <summary>
        /// state score matrix
        /// M[i][j,k] = exp(w * f(y[i - 1] = j, y[i] = k)
        /// </summary>
        public double[][,] M;

        private void AllocateMemory(int Length, int TagCount)
        {
            alpha = new double[Length][];
            for (int i = 0; i < Length; ++i)
            {
                alpha[i] = new double[TagCount];
            }
            
            beta = new double[Length][];
            for (int i = 0; i < Length; ++i)
            {
                beta[i] = new double[TagCount];
            }
        }

        private void Clear()
        {
            foreach (var a in alpha)
            {
                Array.Clear(a, 0, a.Length);
            }
            foreach (var b in beta)
            {
                Array.Clear(b, 0, b.Length);
            }
            Z = 0;
        }

        private void ForwardBackward()
        {
            Forward();
            Backward();
        }

        private void Forward()
        {
            for (int i = 0; i < TagCount; ++i)
            {
                alpha[0][i] = 1.0;
            }

            for (int i = 1; i < Length; ++i)
            {
                for (int j = 0; j < TagCount; ++j)
                {
                    for (int k = 0; k < TagCount; ++k)
                    {
                        alpha[i][k] += alpha[i - 1][j] * M[i][j, k];
                    }
                }
            }
        }

        private void Backward()
        {
            for (int i = 0; i < TagCount; ++i)
            {
                beta[Length - 1][i] = 1.0;
            }

            for (int i = Length - 2; i >= 0; --i)
            {
                for (int j = 0; j < TagCount; ++j)
                {
                    for (int k = 0; k < TagCount; ++k)
                    {
                        beta[i][j] += beta[i][k] * M[i + 1][j, k];
                    }
                }
            }
        }

        /// <summary>
        /// Forward state-cost;
        /// alpha[i] = alpha[i-1] * M[i]
        /// </summary>
        double[][] alpha;

        /// <summary>
        /// Backward state-cost;
        /// beta[i] = beta[i + 1] * M[i + 1]
        /// </summary>
        double[][] beta;

        /// <summary>
        /// Normalizing factor.
        /// Z = sum_of_y (exp(w * f(y, x))
        /// </summary>
        double Z { get; set; }

        int TagCount { get { return alpha[0].Length; } }

        int Length { get { return alpha.Length; } }

        
    }
}
