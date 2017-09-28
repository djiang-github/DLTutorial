using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NanYUtilityLib;
using ParserUtil;
using NanYUtilityLib.DepParUtil;
using System.IO;
using PoSTag;
using LinearFunction;

namespace PoSTagTraining
{
    class ModelHeadGenerator
    {
        public ModelHeadGenerator() { }

        public void Generate()
        {
            string path = Configure.GetOptionString("TrainDataDir");

            string obfile = Configure.GetOptionString("ObserveGen");

            int wordtagcutoff = Configure.GetOptionInt("WordTagDictCutoff", 10);

            string wcfilename = Configure.GetOptionString("WordCluster");

            string lcwcfilename = Configure.GetOptionString("LCWordCluster");

            string featfile = Configure.GetOptionString("FeatureFile");

            int vocabcutoff = Configure.GetOptionInt("ModelVocabCutoff", 0);

            int wordcutoff = Configure.GetOptionInt("WordCutoff", 5);

            string ofn = Configure.GetOptionString("ModelHead");

            var data = LoadSentences(path);

            var extraInfo = new Dictionary<string, List<string>>();

            extraInfo["ObservElem"] = GetObserveElem(obfile);
            extraInfo["Tokens"] = CollectHighFrequencyTokens(data);
            extraInfo["TagDict"] = CollectWordTagDict(data, wordtagcutoff);

            var tags = CollectTags(data);

            var wcluster = GetWordCluster(wcfilename);

            var lcwcluster = GetLowerCaseWordCluster(lcwcfilename);

            if (wcluster != null)
            {
                extraInfo["WordCluster"] = wcluster;
            }

            if (lcwcluster != null)
            {
                extraInfo["LCWordCluster"] = lcwcluster;
            }

            IPoSDict dict = new FlexiblePoSDict(extraInfo);

            IObservGenerator obgen = new FlexibleGenerator(extraInfo);

            var features = GetFeatures(featfile);

            var modelVocab = CollectModelVocab(data, obgen, vocabcutoff);
            
            StreamWriter sw = new StreamWriter(ofn, false);

            LinearChainModelInfo.WriteExtraInfo(sw, extraInfo);

            sw.WriteLine("{0} {1} {2} {3}", features.Count, tags.Count, modelVocab.Count, extraInfo["ObservElem"].Count);

            foreach (string l in features)
            {
                sw.WriteLine(l);
            }

            foreach (string l in tags)
            {
                sw.WriteLine(l);
            }

            foreach (string l in modelVocab)
            {
                sw.WriteLine(l);
            }

            sw.Close();
        }

        public List<string> CollectHighFrequencyTokens(List<List<ParserSentence>> data, int cutoff = 10)
        {
            Dictionary<string, int> dict = new Dictionary<string, int>();
            foreach (var sntlist in data)
            {
                foreach (var snt in sntlist)
                {
                    foreach (var tok in snt.tok)
                    {
                        if (dict.ContainsKey(tok))
                        {
                            dict[tok]++;
                        }
                        else
                        {
                            dict[tok] = 1;
                        }
                    }
                }
            }

            List<string> hfw = new List<string>();

            foreach (var kvpair in dict)
            {
                if (kvpair.Value > cutoff)
                {
                    hfw.Add(kvpair.Key);
                }
            }

            return hfw;
        }

        public void Generate(List<List<ParserSentence>> data, string outputfile)
        {
            string path = Configure.GetOptionString("TrainDataDir");

            string obfile = Configure.GetOptionString("ObserveGen");

            int wordtagcutoff = Configure.GetOptionInt("WordTagDictCutoff", 10);

            string wcfilename = Configure.GetOptionString("WordCluster");

            string lcwcfilename = Configure.GetOptionString("LCWordCluster");

            string featfile = Configure.GetOptionString("FeatureFile");

            int vocabcutoff = Configure.GetOptionInt("ModelVocabCutoff", 0);

            int wordcutoff = Configure.GetOptionInt("WordCutoff", 5);

            var extraInfo = new Dictionary<string, List<string>>();

            extraInfo["ObservElem"] = GetObserveElem(obfile);
            //extraInfo["Tokens"] = CollectWordDict(data);
            extraInfo["TagDict"] = CollectWordTagDict(data, wordtagcutoff);

            var tags = CollectTags(data);

            var wcluster = GetWordCluster(wcfilename);

            var lcwcluster = GetLowerCaseWordCluster(lcwcfilename);

            if (wcluster != null)
            {
                extraInfo["WordCluster"] = wcluster;
            }

            if (lcwcluster != null)
            {
                extraInfo["LCWordCluster"] = lcwcluster;
            }

            IPoSDict dict = new FlexiblePoSDict(extraInfo);

            IObservGenerator obgen = new FlexibleGenerator(extraInfo);

            var features = GetFeatures(featfile);

            var modelVocab = CollectModelVocab(data, obgen, vocabcutoff);


            StreamWriter sw = new StreamWriter(outputfile, false);

            LinearChainModelInfo.WriteExtraInfo(sw, extraInfo);

            sw.WriteLine("{0} {1} {2} {3}", features.Count, tags.Count, modelVocab.Count, extraInfo["ObservElem"].Count);

            foreach (string l in features)
            {
                sw.WriteLine(l);
            }

            foreach (string l in tags)
            {
                sw.WriteLine(l);
            }

            foreach (string l in modelVocab)
            {
                sw.WriteLine(l);
            }

            sw.Close();
        }

        public List<List<ParserSentence>> LoadSentences(string path)
        {
            var result = new List<List<ParserSentence>>();

            DirectoryInfo dir = new DirectoryInfo(path);

            var files = dir.GetFiles();

            foreach (var file in files)
            {
                var snts = new List<ParserSentence>();
                var sr = new MaltTabFileReader(file.FullName);

                sr.SingleLineMode = true;

                while (!sr.EndOfStream)
                {
                    ParserSentence ps;
                    if (sr.GetNextSent(out ps))
                    {
                        snts.Add(ps);
                    }
                }

                sr.Close();
                result.Add(snts);
            }

            return result;
        }

        public List<string> CollectWordDict(List<List<ParserSentence>> data, int cutoff)
        {
            Dictionary<string, int> wordcount = new Dictionary<string, int>();

            foreach (var file in data)
            {
                foreach (var snt in file)
                {
                    foreach (string w in snt.tok)
                    {
                        string xw = Normalize(w);
                        if (wordcount.ContainsKey(xw))
                        {
                            wordcount[xw]++;
                        }
                        else
                        {
                            wordcount[xw] = 1;
                        }
                    }
                }
            }

            List<string> words = new List<string>();

            foreach (string w in wordcount.Keys)
            {
                if (wordcount[w] > cutoff)
                {
                    words.Add(w);
                }
            }

            return words;
        }

        public List<string> CollectModelVocab(List<List<ParserSentence>> data, IObservGenerator obgen, int cutoff)
        {
            

            Dictionary<string, int> wordcount = new Dictionary<string, int>();

            foreach (var file in data)
            {
                foreach (var snt in file)
                {
                    string[][] obs = obgen.GenerateObserv(snt.tok);

                    foreach (var ob in obs)
                    {
                        foreach (var v in ob)
                        {
                            if (v == null)
                            {
                                continue;
                            }
                            if (!wordcount.ContainsKey(v))
                            {
                                wordcount[v] = 1;
                            }
                            else
                            {
                                wordcount[v]++;
                            }
                        }
                    }

                    foreach (var v in snt.pos)
                    {
                        if (!wordcount.ContainsKey(v))
                        {
                            wordcount[v] = 1;
                        }
                        else
                        {
                            wordcount[v]++;
                        }
                    }
                }
            }

            List<string> words = new List<string>();

            foreach (string w in wordcount.Keys)
            {
                if (wordcount[w] > cutoff)
                {
                    words.Add(w);
                }
            }

            return words;
        }

        public List<string> GetLowerCaseWordCluster(string filename)
        {
            

            if (!File.Exists(filename))
            {
                return null;
            }

            List<string> r = new List<string>();

            using (StreamReader sr = new StreamReader(filename))
            {
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine().Trim();

                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    r.Add(line);
                }
            }

            return r;
        }

        public List<string> GetObserveElem(string file)
        {
            var r = new List<string>();

            using (StreamReader sr = new StreamReader(file))
            {
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine().Trim();
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    r.Add(line);
                }
            }

            return r;
        }

        public List<string> GetFeatures(string file)
        {
            var r = new List<string>();

            using (StreamReader sr = new StreamReader(file))
            {
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine().Trim();
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                    {
                        continue;
                    }

                    r.Add(line);
                }
            }

            return r;
        }

        public List<string> GetWordCluster(string filename)
        {
            if (!File.Exists(filename))
            {
                return null;
            }

            List<string> r = new List<string>();

            using (StreamReader sr = new StreamReader(filename))
            {
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine().Trim();

                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    r.Add(line);
                }
            }

            return r;
        }

        public List<string> CollectWordTagDict(List<List<ParserSentence>> data, int cutoff)
        {
            Dictionary<string, HashSet<string>> TagDict = new Dictionary<string, HashSet<string>>();

            Dictionary<string, int> wordcount = new Dictionary<string, int>();

            foreach (var file in data)
            {
                foreach (var snt in file)
                {
                    int wid = -1;
                    foreach (string w in snt.tok)
                    {
                        wid++;
                        string wx = Normalize(w);
                        HashSet<string> tset;

                        if (wordcount.ContainsKey(wx))
                        {
                            wordcount[wx]++;
                        }
                        else
                        {
                            wordcount[wx] = 1;
                        }

                        if (!TagDict.TryGetValue(wx, out tset))
                        {
                            tset = new HashSet<string>();
                            TagDict[wx] = tset;
                        }

                        if (!tset.Contains(snt.pos[wid]))
                        {
                            if (snt.pos[wid] == "VBN" || snt.pos[wid] == "VBD")
                            {
                                tset.Add("VBN");
                                tset.Add("VBD");
                            }
                            else if (snt.pos[wid] == "VB" || snt.pos[wid] == "VBP")
                            {
                                tset.Add("VB");
                                tset.Add("VBP");
                            }
                            else
                            {
                                tset.Add(snt.pos[wid]);
                            }
                        }
                    }
                }
            }

            List<string> r = new List<string>();

            foreach (string w in wordcount.Keys)
            {
                if (wordcount[w] > cutoff)
                {
                    var tagset = TagDict[w];

                    StringBuilder sb = new StringBuilder(w);

                    foreach (string t in tagset)
                    {
                        sb.Append(' ');
                        sb.Append(t);
                    }

                    r.Add(sb.ToString());
                }
            }

            return r;
        }

        public List<string> CollectTags(List<List<ParserSentence>> data)
        {
            Dictionary<string, int> wordcount = new Dictionary<string, int>();

            foreach (var file in data)
            {
                foreach (var snt in file)
                {
                    foreach (string w in snt.pos)
                    {
                        string xw = w;
                        if (wordcount.ContainsKey(xw))
                        {
                            wordcount[xw]++;
                        }
                        else
                        {
                            wordcount[xw] = 1;
                        }
                    }
                }
            }

            List<string> words = new List<string>();

            foreach (string w in wordcount.Keys)
            {
                
                    words.Add(w);
                
            }

            return words;
        }

        static private string Normalize(string tok)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in tok)
            {
                if (c >= '0' && c <= '9')
                {
                    sb.Append('0');
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
    }
}
