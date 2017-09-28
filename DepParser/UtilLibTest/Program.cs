using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NanYUtilityLib.Optimizor;
using NanYUtilityLib;

namespace UtilLibTest
{
    class Program
    {
        static void Main(string[] args)
        {
            double[] x = { 0.0, -10000.0, 40 };

            double v = MathHelper.LogAdd(x);

            double[] y = null;

            double sum = 0;

            double[][] yy = new double[][] { x, y };

            Array.ForEach(y, num => { sum += num; });



            v = MathHelper.LogAdd(double.PositiveInfinity, 1.0);

            Console.Error.WriteLine(v);
        }
    }
}
