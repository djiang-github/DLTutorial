using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LinearFunction;

namespace AveragePerceptron
{
    public class PerceptronFeatureFuncPackage : IComparable<PerceptronFeatureFuncPackage>
    {
        public APFunc[] funcs;
        public LinearModelFeature feature;
        public long Time;
        public int CompareTo(PerceptronFeatureFuncPackage other)
        {
            return feature.CompareTo(other.feature);
        }
    }
}
