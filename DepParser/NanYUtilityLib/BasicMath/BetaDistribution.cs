using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NanYUtilityLib
{
    public class BetaDistribution
    {
        public double alpha { get; private set; }
        public double beta { get; private set; }
        public BetaDistribution(double alpha, double beta, Random r)
        {
            this.alpha = alpha;
            this.beta = beta;

            X = new GammaDistribution(alpha, 1.0, r);
            Y = new GammaDistribution(beta, 1.0, r);
        }

        public double Next()
        {
            double x = X.Next();
            double y = Y.Next();

            return x / (x + y);
        }

        private GammaDistribution X;
        private GammaDistribution Y;
    }
}
