using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NanYUtilityLib
{
    public class BernoulliDistribution
    {
        public double p { get; private set; }

        public BernoulliDistribution(double p, Random r)
        {
            if (p < 0.0 || p > 1.0)
            {
                throw new Exception("p for Bernoulli Distribution must fall between 0 and 1");
            }

            this.p = p;
            this.r = r;
        }

        public int Next()
        {
            double rd = r.NextDouble();

            if (rd <= p)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        private Random r;
    }
}
