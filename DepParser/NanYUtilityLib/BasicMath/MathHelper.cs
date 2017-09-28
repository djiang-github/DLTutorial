using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NanYUtilityLib
{
    public class MathHelper
    {
        public static double LogAdd(double x, double y)
        {
            return x > y ?
                (x + Math.Log(1.0 + Math.Exp(y - x)))
                : (y + Math.Log(1.0 + Math.Exp(x - y)));
        }

        public static double[] ConvertToProb(double[] loggedArr)
        {
            double logsum = LogAdd(loggedArr);

            double[] prob = new double[loggedArr.Length];

            for (int i = 0; i < prob.Length; ++i)
            {
                prob[i] = Math.Exp(loggedArr[i] - logsum);
            }

            return prob;
        }

        public static double Sum(double[] a, int start = 0, int len = -1)
        {
            int end = len < 0 ? a.Length : start + len;
            double s = 0;
            for (int i = start; i < end; ++i)
            {
                s += a[i];
            }
            return s;
        }

        public static float Sum(float[] a, int start = 0, int len = -1)
        {
            int end = len < 0 ? a.Length : start + len;
            float s = 0;
            for (int i = start; i < end; ++i)
            {
                s += a[i];
            }
            return s;
        }

        public static int Sum(int[] a, int start = 0, int len = -1)
        {
            int end = len < 0 ? a.Length : start + len;
            int s = 0;
            for (int i = start; i < end; ++i)
            {
                s += a[i];
            }
            return s;
        }

        public static long Sum(long[] a, int start = 0, int len = -1)
        {
            int end = len < 0 ? a.Length : start + len;
            long s = 0;
            for (int i = start; i < end; ++i)
            {
                s += a[i];
            }
            return s;
        }

        public static float QuickInvSqrt(float number)
        {
            uint i;
            float x2, y;
            const float threehalfs = 1.5f;

            unsafe
            {
                x2 = number * 0.5f;
                y = number;
                i = *(uint*)&y;
                i = 0x5f3759df - (i >> 1);
                y = *(float*)&i;
                y = y * (threehalfs - (x2 * y * y));   // 1st iteration
                //      y  = y * ( threehalfs - ( x2 * y * y ) );   // 2nd iteration, this can be removed
            }

            return y;
        }
        
        public static int argmax<T>(T[] x) where T : IComparable<T>
        {
            return argmax(x, x.Length);
        }

        public static int argmax<T>(T[] x, int count) where T : IComparable<T>
        {
            if (count <= 0)
            {
                return -1;
            }

            count = Math.Min(x.Length, count);

            var max = x[0];
            int am = 0;

            for (int i = 1; i < count; ++i)
            {
                if (x[i].CompareTo(max) > 0)
                {
                    max = x[i];
                    am = i;
                }
            }

            return am;
        }

        public static int argmax<T>(T[] x, bool[] valid) where T : IComparable<T>
        {
            int am = 0;

            for (; am < valid.Length; ++am)
            {
                if (valid[am])
                {
                    break;
                }
            }

            if (am >= valid.Length)
            {
                return -1;
            }

            var max = x[am];

            for (int i = am + 1; i < x.Length; ++i)
            {
                if (valid[i] && x[i].CompareTo(max) > 0)
                {
                    max = x[i];
                    am = i;
                }
            }

            return am;
        }

        public static int argmax(double[] x)
        {
            return argmax(x, x.Length);
        }

        public static int argmax(double[] x, int count)
        {
            if (count <= 0)
            {
                return -1;
            }

            count = Math.Min(x.Length, count);

            var max = x[0];
            int am = 0;

            for (int i = 1; i < count; ++i)
            {
                if (x[i] > max)
                {
                    max = x[i];
                    am = i;
                }
            }

            return am;
        }

        public static int argmax(double[] x, bool[] valid)
        {
            int am = 0;

            for (; am < valid.Length; ++am)
            {
                if (valid[am])
                {
                    break;
                }
            }

            if (am >= valid.Length)
            {
                return -1;
            }

            var max = x[am];

            for (int i = am + 1; i < x.Length; ++i)
            {
                if (valid[i] && x[i] > max)
                {
                    max = x[i];
                    am = i;
                }
            }

            return am;
        }

        public static void Max(double[] x, out int argmax, out double max)
        {
            Max(x, x.Length, out argmax, out max);
        }

        public static void Max(double[] x, int count, out int am, out double max)
        {
            if (count <= 0)
            {
                am = -1;
                max = 0;
                return;
            }

            count = Math.Min(x.Length, count);

            max = x[0];
            am = 0;

            for (int i = 1; i < count; ++i)
            {
                if (x[i] > max)
                {
                    max = x[i];
                    am = i;
                }
            }
        }

        public static void Max(double[] x, bool[] valid, out int am, out double max)
        {
            am = 0;

            for (; am < valid.Length; ++am)
            {
                if (valid[am])
                {
                    break;
                }
            }

            if (am >= valid.Length)
            {
                max = 0;
                return;
            }

            max = x[am];

            for (int i = am + 1; i < x.Length; ++i)
            {
                if (valid[i] && x[i] > max)
                {
                    max = x[i];
                    am = i;
                }
            }

            return;
        }

        public static void Max<T>(T[] x, out int argmax, out T max) where T : IComparable<T>
        {
            Max(x, x.Length, out argmax, out max);
        }

        public static void Max<T>(T[] x, int count, out int am, out T max) where T : IComparable<T>
        {
            if (count <= 0)
            {
                am = -1;
                max = default(T);
                return;
            }

            count = Math.Min(x.Length, count);

            max = x[0];
            am = 0;

            for (int i = 1; i < count; ++i)
            {
                if (x[i].CompareTo(max) > 0)
                {
                    max = x[i];
                    am = i;
                }
            }
        }

        public static void Max<T>(T[] x, bool[] valid, out int am, out T max) where T : IComparable<T>
        {
            am = 0;

            for (; am < valid.Length; ++am)
            {
                if (valid[am])
                {
                    break;
                }
            }

            if (am >= valid.Length)
            {
                max = default(T);
                return;
            }

            max = x[am];

            for (int i = am + 1; i < x.Length; ++i)
            {
                if (valid[i] && x[i].CompareTo(max) > 0)
                {
                    max = x[i];
                    am = i;
                }
            }

            return;
        }

        public static double InnerProduct(double[] a, double[] b)
        {
            double sum = 0;

            for (int i = 0; i < a.Length; ++i)
            {
                sum += a[i] * b[i];
            }

            return sum;
        }

        public static float InnerProduct(float[] a, float[] b)
        {
            float sum = 0;

            for (int i = 0; i < a.Length; ++i)
            {
                sum += a[i] * b[i];
            }

            return sum;
        }

        public static void aXAddToY(float[] y, float a, float[] x)
        {
            for (int i = 0; i < y.Length; ++i)
            {
                y[i] += a * x[i];
            }
        }

        public static void aXAddToY(double[] y, double a, double[] x)
        {
            for (int i = 0; i < y.Length; ++i)
            {
                y[i] += a * x[i];
            }
        }

        public static double LogAdd(double[] loggedArr)
        {
            double max = loggedArr[0];

            for (int i = 1; i < loggedArr.Length; ++i)
            {
                max = Math.Max(max, loggedArr[i]);
            }

            double sum = 0;

            foreach (double x in loggedArr)
            {
                sum += Math.Exp(x - max);
            }

            double logsum = max + Math.Log(sum);

            return logsum;
        }

        public static double LogAdd(double[] loggedArr, int count)
        {
            if (count <= 0)
            {
                return 0;
            }

            count = Math.Min(count, loggedArr.Length);

            double max = loggedArr[0];

            for (int i = 1; i < count; ++i)
            {
                max = Math.Max(max, loggedArr[i]);
            }

            double sum = 0;

            for (int i = 0; i < count; ++i)
            {
                sum += Math.Exp(loggedArr[i] - max);
            }

            double logsum = max + Math.Log(sum);

            return logsum;
        }

        public static double LogAdd(double[] loggedArr, bool[] Valid)
        {
            double max = 0;
            bool started = false;

            for (int i = 0; i < loggedArr.Length; ++i)
            {
                if (Valid[i])
                {
                    if (!started)
                    {
                        max = loggedArr[i];
                        started = true;
                    }
                    else
                    {
                        max = Math.Max(max, loggedArr[i]);
                    }
                }
            }

            if (!started)
            {
                return 0;
            }

            double sum = 0;

            for (int i = 0; i < loggedArr.Length; ++i)
            {
                if (Valid[i])
                {
                    sum += Math.Exp(loggedArr[i] - max);
                }
            }

            double logsum = max + Math.Log(sum);

            return logsum;
        }

        public static float LogAdd(float[] loggedArr)
        {
            double max = loggedArr[0];

            for (int i = 1; i < loggedArr.Length; ++i)
            {
                max = Math.Max(max, loggedArr[i]);
            }

            double sum = 0;

            foreach (var x in loggedArr)
            {
                sum += Math.Exp(x - max);
            }

            var logsum = max + Math.Log(sum);

            return (float)logsum;
        }

        public static float LogAdd(float[] loggedArr, int count)
        {
            if (count <= 0)
            {
                return 0;
            }

            count = Math.Min(count, loggedArr.Length);

            double max = loggedArr[0];

            for (int i = 1; i < count; ++i)
            {
                max = Math.Max(max, loggedArr[i]);
            }

            double sum = 0;

            for (int i = 0; i < count; ++i)
            {
                sum += Math.Exp(loggedArr[i] - max);
            }

            double logsum = max + Math.Log(sum);

            return (float)logsum;
        }

        public static float LogAdd(float[] loggedArr, bool[] Valid)
        {
            double max = 0;
            bool started = false;

            for (int i = 0; i < loggedArr.Length; ++i)
            {
                if (Valid[i])
                {
                    if (!started)
                    {
                        max = loggedArr[i];
                        started = true;
                    }
                    else
                    {
                        max = Math.Max(max, loggedArr[i]);
                    }
                }
            }

            if (!started)
            {
                return 0;
            }

            double sum = 0;

            for (int i = 0; i < loggedArr.Length; ++i)
            {
                if (Valid[i])
                {
                    sum += Math.Exp(loggedArr[i] - max);
                }
            }

            double logsum = max + Math.Log(sum);

            return (float)logsum;
        }

        /// <summary>
        /// perform LogAdd(a_0 + b_0, ..., a_n + b_n)
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static double LogInnerProduct(double[] a, double[] b)
        {
            double maxSum = a[0] + b[0];

            for (int i = 1; i < a.Length; ++i)
            {
                double s = a[i] + b[i];

                maxSum = Math.Max(s, maxSum);
            }

            double expSum = 0;

            for (int i = 0; i < a.Length; ++i)
            {
                expSum += Math.Exp(a[i] + b[i] - maxSum);
            }

            return maxSum + Math.Log(expSum);
        }

        static public void x_mult_A_p_b(double[] x, double[] A, double[] b, double[] y)
        {
            b.CopyTo(y, 0);

            int rowCnt = x.Length;
            int colCnt = y.Length;

            for (int i = 0; i < rowCnt; ++i)
            {
                for (int j = 0; j < colCnt; ++j)
                {
                    y[j] += x[i] * A[i * colCnt + j];
                }
            }
        }

        static public void A_mult_x_p_b(double[] A, double[] x, double[] b, double[] y)
        {
            b.CopyTo(y, 0);

            int rowCnt = y.Length;
            int colCnt = x.Length;

            for (int i = 0; i < rowCnt; ++i)
            {
                for (int j = 0; j < colCnt; ++j)
                {
                    y[i] += A[i * colCnt + j] * x[j];
                }
            }
        }

        /// <summary>
        /// add x to y; y is modified
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        static public void ax_addto_y(double a, double[] x, double[] y)
        {
            for (int i = 0; i < x.Length; ++i)
            {
                y[i] += a * x[i];
            }
        }

        static public void normalize(double[] v)
        {
            double vmax = v[0];

            for (int i = 1; i < v.Length; ++i)
            {
                if (v[i] > vmax)
                {
                    vmax = v[i];
                }
            }

            for (int i = 0; i < v.Length; ++i)
            {
                v[i] = Math.Exp(v[i] - vmax);
            }

            double sum = 0;

            for (int i = 0; i < v.Length; ++i)
            {
                sum += v[i];
            }

            for (int i = 0; i < v.Length; ++i)
            {
                v[i] /= sum;
            }
        }

        static public void L2NormNormalize(double[] v)
        {
            double sum = 0;

            for (int i = 0; i < v.Length; ++i)
            {
                sum += v[i] * v[i];
            }

            if (sum == 0)
            {
                return;
            }

            sum = Math.Sqrt(sum);

            for (int i = 0; i < v.Length; ++i)
            {
                v[i] /= sum;
            }
        }
    }
}
