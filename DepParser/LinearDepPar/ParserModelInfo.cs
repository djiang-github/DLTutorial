using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LinearFunction;

namespace LinearDepParser
{
    public class ParserModelInfo
    {
        public ParserModelInfo(LinearModelInfo lmInfo)
        {
            this.lmInfo = lmInfo;
            vocab = new ParserVocab(lmInfo);

            List<string> clusterInfo;

            if (lmInfo.ExtraInfo != null && lmInfo.ExtraInfo.TryGetValue("LowerCaseWordCluster", out clusterInfo))
            {
                lowerCaseWordClusterDict = new Dictionary<string, string>();

                foreach (var wcPair in clusterInfo)
                {
                    string[] parts = wcPair.Split(new string[] { " ", "\t" }, StringSplitOptions.RemoveEmptyEntries);

                    if (parts.Length != 2)
                    {
                        continue;
                    }

                    lowerCaseWordClusterDict[parts[0]] = parts[1];
                }
            }
        }

        public int CommandCount { get { return lmInfo.ActionCount; } }
        public int LabelCount { get { return lmInfo.TagCount; } }

        public Dictionary<string, string> lowerCaseWordClusterDict { get; private set; }
        public ParserVocab vocab { get; private set; }
        public LinearModelInfo lmInfo { get; private set; }
    }
}
