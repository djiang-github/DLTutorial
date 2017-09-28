using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NanYUtilityLib
{
    public class BigramHMM
    {
        public BigramHMM()
        {
        }

        //public double[] Forward(double[] T, double[] E)
        //{
        //}

        //public double[] Backward(double[] T, double[] E)
        //{
        //}

        static public int[] Viterbi(double[,] T, double[][] E)
        {
            int TagCount = T.GetLength(0);
            int Length = E.Length;

            int[,] backL = new int[Length, TagCount];

            double[] SLast = new double[TagCount];
            double[] SNext = new double[TagCount];

            for (int i = 0; i < SLast.Length; ++i)
            {
                SLast[i] = 1.0;
            }

            int argmax = 0;

            for (int fid = 0; fid < Length; ++fid)
            {
                if (fid > 0)
                {
                    for (int tid = 0; tid < TagCount; ++tid)
                    {
                        SNext[tid] = SLast[0] * T[0, tid];
                    }
                }
                else
                {
                    for (int tid = 0; tid < TagCount; ++tid)
                    {
                        SNext[tid] = SLast[0];
                    }
                }

                if (fid > 0)
                {
                    for (int ptid = 1; ptid < TagCount; ++ptid)
                    {
                        for (int tid = 0; tid < TagCount; ++tid)
                        {
                            double s = SLast[ptid] * T[ptid, tid];
                            if (s > SNext[tid])
                            {
                                SNext[tid] = s;
                                backL[fid, tid] = ptid;
                            }
                        }
                    }
                }

                double max = 0;
                argmax = 0;
                for (int tid = 0; tid < TagCount; ++tid)
                {
                    SNext[tid] *= E[fid][tid];
                    if (tid == 0 || SNext[tid] > max)
                    {
                        max = SNext[tid];
                        argmax = tid;
                    }
                }

                for (int tid = 0; tid < TagCount; ++tid)
                {
                    SLast[tid] = SNext[tid] / max;
                }
            }

            int[] TagIds = new int[Length];

            TagIds[TagIds.Length - 1] = argmax;

            for (int i = TagIds.Length - 1; i > 0; --i)
            {
                TagIds[i - 1] = backL[i, TagIds[i]];
            }

            return TagIds;
        }

        //public int[] MaxPosterior(double[] alpha, double[] beta)
        //{
        //}

        //public bool IsLogged { get; set; }
    }
}
