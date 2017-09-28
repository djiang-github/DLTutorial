using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ParserUtil
{
    
    public class WordClusterDict
    {
        public WordClusterDict(string fn)
        {
            StreamReader sr = new StreamReader(fn);

            while (!sr.EndOfStream)
            {
                string[] parts = sr.ReadLine().Trim().Split('\t');
                if (parts.Length != 3)
                {
                    continue;
                }

                cd[parts[1]] = new ClusterWithCount
                {
                    cluster = parts[0],
                    count = int.Parse(parts[2])
                };
            }
            sr.Close();
        }

        public WordClusterDict(List<string> clusters)
        {
            foreach (string line in clusters)
            {
                string[] parts = line.Trim().Split('\t');
                if (parts.Length != 3)
                {
                    continue;
                }

                cd[parts[1]] = new ClusterWithCount
                {
                    cluster = parts[0],
                    count = int.Parse(parts[2])
                };
            }
        }

        public void GetClusters(string[] tok, out string[] cluster, out bool[] usefull)
        {
            cluster = new string[tok.Length];
            usefull = new bool[tok.Length];

            for (int i = 0; i < cluster.Length; ++i)
            {
                ClusterWithCount cc;
                if (cd.TryGetValue(tok[i], out cc))
                {
                    cluster[i] = cc.cluster;
                    if (cc.count > 300)
                    {
                        usefull[i] = true;
                    }
                }
                else
                {
                    cluster[i] = null;
                }
            }
        }

        public Dictionary<string, ClusterWithCount> cd = new Dictionary<string, ClusterWithCount>();

        public class ClusterWithCount
        {
            public string cluster;
            public int count;
        }
    }
}
