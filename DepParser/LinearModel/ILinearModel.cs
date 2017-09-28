using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinearFunction
{
    public interface ILinearFunction
    {
        void GetScores(LinearModelFeature feature, float[] scores);

        void AddScores(LinearModelFeature feature, float[] scores);

        void GetScores(LinearModelFeature[] features, float[] scores);

        void AddScores(LinearModelFeature[] features, float[] scores);

        void GetScores(LinearModelFeature feature, float featurevalue, float[] scores);

        void AddScores(LinearModelFeature feature, float featurevalue, float[] scores);

        void GetScores(LinearModelFeature[] features, float[] featurevalues, float[] scores);

        void AddScores(LinearModelFeature[] features, float[] featurevalues, float[] scores);

    }
}
