using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace LinearFunction
{
    public class MaxEntModelConvertor
    {
        

        public void Convert(IStateElementDiscriptor descriptor, string modelheadfn, string maxEntModelfn, string outputfn)
        {
            LinearModelInfo lmInfo = new LinearModelInfo(modelheadfn, descriptor);

            
            Dictionary<string, int> tagDict = new Dictionary<string, int>();

            StreamReader sr = new StreamReader(maxEntModelfn);

            int next = 0;
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine().Trim();
                if (line.StartsWith("++++"))
                {
                    break;
                }
                int id = lmInfo.TagVocab.TagId(line);
                tagDict[next.ToString()] = id;
                next++;
            }

            List<LinearFeatureFuncPackage> lffpList = new List<LinearFeatureFuncPackage>();

            List<FeatureFunc> funcList = new List<FeatureFunc>();
            LinearModelFeature feature = null;
            string lastPart = null;

            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine().Trim();

                string[] parts = line.Split(new string[] { "\t", ":" }, StringSplitOptions.RemoveEmptyEntries);

                if (parts[1] != lastPart)
                {
                    if (funcList.Count > 0)
                    {
                        LinearFeatureFuncPackage lffp = new LinearFeatureFuncPackage 
                        {
                             feature = feature,
                             funcs = funcList.ToArray()
                        };
                        lffpList.Add(lffp);
                    }

                    string[] FeatIds = parts[1].Split('_');

                    int[] ElemIds = new int[FeatIds.Length - 1];
                    int DictId = int.Parse(FeatIds[0]);

                    for(int i = 0; i < ElemIds.Length; ++i)
                    {
                        ElemIds[i] = int.Parse(FeatIds[i + 1]);
                    }

                    feature = new LinearModelFeature(DictId, ElemIds.Length);

                    ElemIds.CopyTo(feature.ElemArr, 0);

                    lastPart = parts[1];

                    funcList.Clear();
                }

                int tagId = tagDict[parts[0]];
                float score = float.Parse(parts[2]);
                funcList.Add(new FeatureFunc { weight = score, tag = tagId });
            }

            if (funcList.Count > 0)
            {
                LinearFeatureFuncPackage lffp = new LinearFeatureFuncPackage
                {
                    feature = feature,
                    funcs = funcList.ToArray()
                };
                lffpList.Add(lffp);
            }

            sr.Close();

            lmInfo.WriteModel(outputfn, null, lffpList);

        }
    }
}
