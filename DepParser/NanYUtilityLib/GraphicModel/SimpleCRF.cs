using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NanYUtilityLib
{
    /// <summary>
    /// All score are log value
    /// </summary>
    public partial class SimpleCRF
    {
        /// <summary>
        /// Transition Score Matrix;
        /// TMtx[i][j][k] is the transition score
        /// from tag j to tag k ending at frame i;
        /// TMtx[0] is not used
        /// </summary>
        public double[][][] TMtx { get; private set; }
        /// <summary>
        /// Emit Score Vector;
        /// Emit[i][j] is the emit score
        /// for tag j at frame i 
        /// </summary>
        public double[][] Emit { get; private set; }
        /// <summary>
        /// Specify whether a tag is legal for given frame
        /// Legal[i][j] indicates whether tag j is legal for frame i
        /// </summary>
        public bool[][] Legal { get; private set; }
        /// <summary>
        /// Specify whether a tag is gold for given frame
        /// Gold[i][j] indicates whether tag j is gold from frame i 
        /// </summary>
        public bool[][] Gold { get; private set; }
        /// <summary>
        /// Get the viterbi sequence
        /// </summary>
        /// <returns>
        /// Return viterbi path if at least one path exist;
        /// Otherwise return null.
        /// </returns>
        public int[] Viterbi()
        {
            double[] ps = new double[TagCount];
            double[] s = new double[TagCount];
            int[,] backptr = new int[SequenceLength, TagCount];

            for (int t = 0; t < TagCount; ++t)
            {
                ps[t] = Emit[0][t];
            }

            for (int fid = 1; fid < SequenceLength; ++fid)
            {
                for (int t = 0; t < TagCount; ++t)
                {
                    backptr[fid, t] = -1;
                }

                for (int pt = 0; pt < TagCount; ++pt)
                {
                    if (Legal[fid - 1][pt])
                    {
                        for (int t = 0; t < TagCount; ++t)
                        {
                            if (Legal[fid][t])
                            {
                                double xs = ps[pt] + TMtx[fid][pt][t];
                                if (backptr[fid, t] < 0 || xs > s[t])
                                {
                                    s[t] = xs;
                                    backptr[fid, t] = pt;
                                }
                            }
                        }
                    }
                }

                for (int t = 0; t < TagCount; ++t)
                {
                    ps[t] = s[t] + Emit[fid][t];
                }
            }

            int[] tt = new int[SequenceLength];

            tt[SequenceLength - 1] = MathHelper.argmax(ps, Legal[SequenceLength - 1]);

            for (int fid = SequenceLength - 2; fid >= 0; --fid)
            {
                tt[fid] = backptr[fid + 1, tt[fid + 1]];
            }

            return tt;
        }
        /// <summary>
        /// Get Max Posterior sequence
        /// </summary>
        /// <returns>
        /// Return max posterior path if at least one path exist;
        /// Otherwise return null.
        /// </returns>
        public int[] MaxPosterior()
        {
            ForwardZ();
            BackwardZ();

            int[] tt = new int[SequenceLength];

            double[] logA = new double[TagCount];

            for (int fid = 0; fid < tt.Length; ++fid)
            {
                for (int t = 0; t < Legal[fid].Length; ++t)
                {
                    if (Legal[fid][t])
                    {
                        logA[t] = ZAlpha[fid][t] + ZBeta[fid][t] - Emit[fid][t];
                    }
                }

                tt[fid] = MathHelper.argmax(logA, Legal[fid]);
            }

            return tt;
        }

        public void GradientNegLogProb(out double[][][] GT, out double[][] GEmit)
        {
            ForwardZ();
            BackwardZ();
            ForwardGZ();
            BackwardGZ();

            double logz = LogZ();
            double loggz = LogGZ();

            GT = new double[SequenceLength][][];
            for (int i = 0; i < GT.Length; ++i)
            {
                GT[i] = new double[TagCount][];
                for (int j = 0; j < GT[i].Length; ++j)
                {
                    GT[i][j] = new double[TagCount];
                }
            }

            for (int fid = 1; fid < SequenceLength; ++fid)
            {
                for (int pt = 0; pt < TagCount; ++pt)
                {
                    if (Legal[fid - 1][pt])
                    {
                        for (int t = 0; t < TagCount; ++t)
                        {
                            if (Legal[fid][t])
                            {
                                double x = ZAlpha[fid - 1][pt] + ZBeta[fid][t] + TMtx[fid][pt][t] - logz;

                                GT[fid][pt][t] = Math.Exp(x);

                                if (Gold[fid][t] && Gold[fid - 1][pt])
                                {
                                    double xx = GAlpha[fid - 1][pt] + GBeta[fid][t] + TMtx[fid][pt][t] - loggz;
                                    GT[fid][pt][t] -= Math.Exp(xx);
                                }
                            }
                        }
                    }
                } 
            }

            GEmit = new double[SequenceLength][];
            for (int i = 0; i < GEmit.Length; ++i)
            {
                GEmit[i] = new double[TagCount];
            }

            for (int fid = 0; fid < SequenceLength; ++fid)
            {
                for (int t = 0; t < TagCount; ++t)
                {
                    if (Legal[fid][t])
                    {
                        double x = ZAlpha[fid][t] + ZBeta[fid][t] - Emit[fid][t] - logz;

                        GEmit[fid][t] = Math.Exp(x);

                        if (Gold[fid][t])
                        {
                            double xx = GAlpha[fid][t] + GBeta[fid][t] - Emit[fid][t] - loggz;
                            GEmit[fid][t] -= Math.Exp(xx);
                        }
                    }
                }
            }
        }

        public double LogProb()
        {
            return LogGZ() - LogZ();
        }

        public SimpleCRF(double[][][] TMtx, double[][] Emit, bool[][] Legal, bool[][] Gold)
        {
            this.TMtx = TMtx;
            this.Emit = Emit;
            this.Legal = Legal;
            this.Gold = Gold;
        }

        public SimpleCRF(double[][][] TMTx, double[][] Emit, bool[][] Legal)
            :this(TMTx, Emit, Legal, null)
        {
        }

    }

    public partial class SimpleCRF
    {
        private int TagCount { get { return Emit[0].Length; } }
        private int SequenceLength { get { return Emit.Length; } }
        private double[][] ZAlpha;
        private double[][] ZBeta;
        private double[][] GAlpha;
        private double[][] GBeta;
        private double _Z;
        private double _GZ;
        private bool ZComputed = false;
        private bool GZComputed = false;

        private void ForwardZ()
        {
            if (ZAlpha != null)
            {
                return;
            }

            ZAlpha = Forward(TMtx, Emit, Legal);
        }
        
        private void BackwardZ()
        {
            if (ZBeta != null)
            {
                return;
            }

            ZBeta = Backward(TMtx, Emit, Legal);
        }

        private void ForwardGZ()
        {
            if (GAlpha != null)
            {
                return;
            }

            GAlpha = Forward(TMtx, Emit, Gold);
        }

        private void BackwardGZ()
        {
            if (GBeta != null)
            {
                return;
            }

            GBeta = Backward(TMtx, Emit, Gold);
        }

        private double LogZ()
        {
            if (ZComputed)
            {
                return _Z;
            }

            ForwardZ();
            _Z = MathHelper.LogAdd(ZAlpha[SequenceLength - 1], Legal[SequenceLength - 1]);
            ZComputed = true;
            return _Z;
        }

        private double LogGZ()
        {
            if (GZComputed)
            {
                return _GZ;
            }

            ForwardGZ();
            _GZ = MathHelper.LogAdd(GAlpha[SequenceLength - 1], Gold[SequenceLength - 1]);
            GZComputed = true;
            return _GZ;
        }

        static private double[][] Forward(double[][][] TMtx, double[][] Emit, bool[][] Valid)
        {
            int SequenceLength = Emit.Length;
            int TagCount = Emit[0].Length;

            double[][] Alpha = new double[SequenceLength][];

            for (int i = 0; i < SequenceLength; ++i)
            {
                Alpha[i] = new double[TagCount];
            }

            // first frame
            for (int t = 0; t < TagCount; ++t)
            {
                Alpha[0][t] = Emit[0][t];
            }

            double[] logArr = new double[TagCount];
            // other frame
            for (int fid = 1; fid < SequenceLength; ++fid)
            {
                for (int t = 0; t < TagCount; ++t)
                {
                    if (!Valid[fid][t])
                    {
                        continue;
                    }

                    int ptcount = 0;

                    for (int pt = 0; pt < TagCount; ++pt)
                    {
                        if (Valid[fid - 1][pt])
                        {
                            logArr[ptcount] = Alpha[fid - 1][pt] + TMtx[fid][pt][t];
                            ptcount++;
                        }
                    }

                    Alpha[fid][t] = MathHelper.LogAdd(logArr, ptcount);
                    Alpha[fid][t] += Emit[fid][t];
                }
            }

            return Alpha;
        }

        static private double[][] Backward(double[][][] TMtx, double[][] Emit, bool[][] Valid)
        {
            int SequenceLength = Emit.Length;
            int TagCount = Emit[0].Length;

            double[][] Beta = new double[SequenceLength][];

            for (int i = 0; i < SequenceLength; ++i)
            {
                Beta[i] = new double[TagCount];
            }

            // last frame
            for (int t = 0; t < TagCount; ++t)
            {
                Beta[SequenceLength - 1][t] = Emit[SequenceLength - 1][t];
            }

            double[] logArr = new double[TagCount];
            // other frame
            for (int fid = SequenceLength - 2; fid >= 0; --fid)
            {
                for (int t = 0; t < TagCount; ++t)
                {
                    if (!Valid[fid][t])
                    {
                        continue;
                    }

                    int ntcount = 0;

                    for (int nt = 0; nt < TagCount; ++nt)
                    {
                        if (Valid[fid + 1][nt])
                        {
                            logArr[ntcount] = Beta[fid + 1][nt] + TMtx[fid + 1][t][nt];
                            ntcount++;
                        }
                    }

                    Beta[fid][t] = MathHelper.LogAdd(logArr, ntcount);
                    Beta[fid][t] += Emit[fid][t];
                }
            }

            return Beta;
        }
    }
}
