using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NanYUtilityLib
{
    /// <summary>
    /// A simple and inefficient random number generator for
    /// Gamma distribution.
    /// The algorithm is Ahrens-Dieter acceptance-rejection
    /// method.
    /// See Wikipedia page of Gamma distribution for reference.
    /// </summary>
    public class GammaDistribution
    {
        public GammaDistribution()
            : this(1.0, 1.0)
        {
        }

        public GammaDistribution(double k, double theta)
            : this(k, theta, new Random())
        {
        }

        public GammaDistribution(double k, double theta, Random r)
        {
            this.k = k;
            this.theta = theta;
            this.r = r;
        }

        public double Next()
        {
            double N = Math.Floor(k);

            double leftOver = k - N;

            double randSum = 0;

            for (int i = 0; i < N; ++i)
            {
                randSum += generateExpRand();
            }

            if (leftOver > 0)
            {
                randSum += generateGammaLeftOver(leftOver);
            }

            return randSum * theta;
        }

        double generateExpRand()
        {
            double x = 0;
            do
            {
                x = r.NextDouble();
            } while (x <= 0 || x >= 1.0);

            return -Math.Log(x);
        }

        double generateGammaLeftOver(double leftOver)
        {
            double logEps = 0;
            double logEta = 0;
            do
            {
                double vm2 = 0;
                double vm1 = 0;
                double vm = 0;

                do
                {
                    vm2 = r.NextDouble();
                } while (vm2 == 0);

                do
                {
                    vm1 = r.NextDouble();
                } while (vm1 == 0);

                do
                {
                    vm = r.NextDouble();
                } while (vm == 0);

                double logvm1 = Math.Log(vm1);
                double logvm = Math.Log(vm);

                double v0 = 1.0 / (1.0 + leftOver / Math.E);

                if (vm2 <= v0)
                {
                    logEps = logvm1 / leftOver;
                    logEta = logvm + (leftOver - 1.0) * logEps;
                    //epsilon = Math.Pow(vm1, 1.0 / theta);
                    //eta = vm * vm1 / epsilon;
                }
                else
                {
                    logEps = Math.Log(1.0 - logvm1);
                    //epsilon = 1.0 - Math.Log(vm1);
                    //eta = vm * vm1 / Math.E;
                    logEta = logvm + logvm1 - 1.0;
                }

            } while (logEta > logEps * (leftOver - 1.0) - Math.Exp(logEps));
            //while (eta > Math.Pow(epsilon, theta - 1.0) * Math.Exp(-epsilon));

            return Math.Exp(logEta);
        }

        readonly double k;
        readonly double theta;
        Random r;
    }
}
