using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NanYUtilityLib
{
    public class PoissonFunction
    {
        public PoissonFunction(double lambda)
        {
            if (lambda <= 0)
            {
                throw new Exception();
            }

            result = new double[CacheCount];

            double loglambda = Math.Log(lambda);

            result[0] = -lambda + loglambda - Math.Log(1);

            for (int i = 1; i < result.Length; ++i)
            {
                result[i] = result[i - 1] + loglambda - Math.Log(i + 1);
            }

            for (int i = 0; i < result.Length; ++i)
            {
                result[i] = Math.Exp(result[i]);
            }
        }

        public double this[int k]
        {
            get
            {
                if (k <= 0 || k > CacheCount)
                {
                    return 0;
                }
                else
                {
                    return result[k - 1];
                }
            }
        }

        const int CacheCount = 100;

        private double[] result;
    }
}
