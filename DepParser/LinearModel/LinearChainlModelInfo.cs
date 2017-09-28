using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace LinearFunction
{
    public class LinearChainModelInfo
    {
        public LinearChainModelInfo(string fn)
        {
            StreamReader sr = new StreamReader(fn);

            //bool success = false;

            LoadHeader(sr);

            Templates = LoadTemplates(sr);
            
            LoadTagVocab(sr);
            
            LoadVocab(sr);
            
            LoadFeaturePackages(sr);

            sr.Close();
        }

        public LinearChainModelInfo(Stream fn)
        {
            StreamReader sr = new StreamReader(fn);

            //bool success = false;

            LoadHeader(sr);

            Templates = LoadTemplates(sr);

            LoadTagVocab(sr);

            LoadVocab(sr);

            LoadFeaturePackages(sr);

            sr.Close();
        }

        public LinearChainModelInfo()
        {
        }

        public void SetTags(string[] tags)
        {
            TagCount = tags.Length;
            TagVocab = new LinearModelTagVocab(tags);
        }

        public void SetVocab(string[] vocab)
        {
            VocabCount = vocab.Length;
            ModelVocab = new LinearModelVocab(vocab);
        }

        private void LoadHeader(StreamReader sr)
        {
            FeatureTemplateCount = 0;
            TagCount = 0;
            VocabCount = 0;
            ObservLen = 0;

            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (line.StartsWith("#") || string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                if (line.Trim() == "<ExtraInfo>")
                {
                    if (!LoadExtraInfo(sr))
                    {
                        throw new Exception("Error in model file !");
                    }
                    continue;
                }

                string[] parts = line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 4)
                {
                    throw new Exception("Wrong Model File!");
                }
                
                int ft, tc, vc, obl;

                if (!int.TryParse(parts[0], out ft)
                    || !int.TryParse(parts[1], out tc)
                    || !int.TryParse(parts[2], out vc)
                    || !int.TryParse(parts[3], out obl))
                {
                    throw new Exception("Wrong Model File!");
                }

                FeatureTemplateCount = ft;
                TagCount = tc;
                VocabCount = vc;
                ObservLen = obl;
                break;
            }
        }

        public void WriteModel(string fn, List<string> HeadLines, List<LinearFeatureFuncPackage> lffps)
        {
            StreamWriter sw = new StreamWriter(fn);

            if (HeadLines != null)
            {
                foreach (string hl in HeadLines)
                {
                    sw.WriteLine(hl);
                }
            }

            WriteExtraInfo(sw, ExtraInfo);

            sw.WriteLine("{0} {1} {2} {3}", FeatureTemplateCount, TagCount, VocabCount, ObservLen);

            WriteTemplates(sw);

            WriteTagVocab(sw);

            WriteVocab(sw);

            WriteFeatureFuncs(lffps, sw);

            sw.Close();
        }

        public void WriteModel(string fn)
        {
            using (StreamWriter sw = new StreamWriter(fn))
            {
                WriteExtraInfo(sw, ExtraInfo);

                sw.WriteLine("{0} {1} {2} {3}", FeatureTemplateCount, TagCount, VocabCount, ObservLen);

                WriteTemplates(sw);

                WriteTagVocab(sw);

                WriteVocab(sw);

                WriteFeatureFuncs(LinearFuncPackages, sw);
            }
        }

        public void SetFeatures(List<string> featstrs)
        {
            LinearFuncPackages = new List<LinearFeatureFuncPackage>();
            foreach(var line in featstrs)
            {
                int[] features;
                FeatureFunc[] ffs;
                int DictId;
                ParseFeatureLine(line, out DictId, out features, out ffs);
                LinearFuncPackages.Add(new LinearFeatureFuncPackage
                {
                    feature = new LinearModelFeature(DictId, features),
                    funcs = ffs
                });
            } 
        }

        public int TagCount { get; private set; }
        public int VocabCount { get; private set; }
        public int FeatureTemplateCount { get; private set; }
        public int ObservLen { get; private set; }

        public Dictionary<string, List<string>> ExtraInfo;

        //public List<LinearFeatureTemplate> TemplateSet;
        public LinearModelTagVocab TagVocab;
        public LinearModelVocab ModelVocab;

        public void SetTempate(string file)
        {
            var lines = new List<string>();
            using (StreamReader sr = new StreamReader(file))
            {
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine().Trim();

                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    lines.Add(line);
                }
            }

            SetTempate(lines);
        }

        public void SetTempate(List<string> featLines)
        {
            var lines = featLines;

            FeatureTemplateCount = lines.Count;

            LinearChainFeatureTemplate[] fetpls = new LinearChainFeatureTemplate[FeatureTemplateCount];

            Dictionary<int, int> bagOfWordsFeatureDict = new Dictionary<int, int>();

            int next = 0;

            for (int i = 0; i < FeatureTemplateCount; ++i)
            {
                string line = lines[i];
                ParseFeatureTemplate(i, ref next, bagOfWordsFeatureDict, line, out fetpls[i]);
            }
            Templates = fetpls;
        }

        public void SetObLen(int len)
        {
            ObservLen = len;
        }

        private void ParseFeatureTemplate(int templateId, ref int next, Dictionary<int, int> bagOfWordsFeatureDict, string ftstring, out LinearChainFeatureTemplate ft)
        {
            string[] parts = ftstring.ToLower().Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            int i = 0;
            List<int> offset = new List<int>();
            List<int> observId = new List<int>();
            int numbY = 0;

            int thisId;
            int groupId;
            if (parts[0] == "b")
            {
                //int groupId = 0;
                if (!int.TryParse(parts[1], out groupId))
                {
                    throw new Exception("wrong model file!");
                }
                if (!bagOfWordsFeatureDict.TryGetValue(groupId, out thisId))
                {
                    thisId = next++;
                    bagOfWordsFeatureDict[groupId] = thisId;
                }
                i = 2;
            }
            else
            {
                groupId = -1;
                thisId = next++;
            }

            while (i < parts.Length)
            {
                if (parts[i] == "x")
                {
                    offset.Add(int.Parse(parts[++i]));
                    observId.Add(int.Parse(parts[++i]));
                }
                else if (parts[i] == "y")
                {
                    offset.Add(int.Parse(parts[++i]));
                    observId.Add(-1);
                    numbY++;
                }
                else
                {
                    throw new Exception("Wrong Model File!");
                }
                ++i;
            }

            ft = new LinearChainFeatureTemplate(templateId, thisId, groupId, offset.ToArray(), observId.ToArray());
        }
      
        private LinearChainFeatureTemplate[] LoadTemplates(StreamReader sr)
        {
            LinearChainFeatureTemplate[] fetpls = new LinearChainFeatureTemplate[FeatureTemplateCount];

            Dictionary<int, int> bagOfWordsFeatureDict = new Dictionary<int, int>();

            int next = 0;

            for (int i = 0; i < FeatureTemplateCount; ++i)
            {
                string line = sr.ReadLine();
                ParseFeatureTemplate(i, ref next, bagOfWordsFeatureDict, line, out fetpls[i]);
            }
            return fetpls;
        }

        public LinearChainFeatureTemplate[] Templates;

        public List<LinearFeatureFuncPackage> LinearFuncPackages;

        private bool LoadExtraInfo(StreamReader sr)
        {
            ExtraInfo = new Dictionary<string, List<string>>();
            //Stack<string> headstack = new Stack<string>();

            string head = null;
            List<string> infoList = null;

            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine().Trim();
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                string resolved;

                bool IsSpecial = ResolveEscapeChar(line, out resolved);

                if (IsSpecial)
                {
                    if (head == null)
                    {
                        infoList = new List<string>();
                        head = resolved;
                        ExtraInfo[head] = infoList;
                    }
                    else
                    {
                        if (head != resolved)
                        {
                            throw new Exception("wrong model file");
                        }

                        head = null;
                        infoList = null;
                    }
                }
                else
                {
                    if (head == null)
                    {
                        if (resolved == "</ExtraInfo>")
                        {
                            return true;
                        }
                        
                        continue;
                    }
                    else
                    {
                        infoList.Add(resolved);
                    }
                }
            }
            return false;
        }

        public static void WriteExtraInfo(StreamWriter sw, Dictionary<string, List<string>> ExtraInfo)
        {
            if (ExtraInfo == null)
            {
                return;
            }
            sw.WriteLine("<ExtraInfo>");

            foreach (var key in ExtraInfo.Keys)
            {
                var info = ExtraInfo[key];

                sw.WriteLine(DoEscape(key, true));

                foreach (var s in info)
                {
                    sw.WriteLine(DoEscape(s, false));
                }

                sw.WriteLine(DoEscape(key, true));
            }

            sw.WriteLine("</ExtraInfo>");
        }

        private bool ResolveEscapeChar(string line, out string resolved)
        {
            if (line.StartsWith("\\") && !line.StartsWith("\\\\"))
            {
                if (line.Length == 1)
                {
                    throw new Exception("wrong model file!");
                }

                resolved = line.Substring(1);
                return true;
            }
            else
            {
                if(line.StartsWith("\\\\"))
                {
                    resolved = line.Substring(1);
                }
                else
                {
                    resolved = line;
                }

                return false;
            }
        }

       static private string DoEscape(string line, bool IsSpecial)
        {
            if (IsSpecial)
            {
                if (line.StartsWith("\\"))
                {
                    throw new Exception("special tokens should not contain char \\");
                }

                return "\\" + line;
            }
            else
            {
                if (line.StartsWith("\\"))
                {
                    return "\\" + line;
                }
                else
                {
                    return line;
                }
            }
        }

        private void WriteTagVocab(StreamWriter sw)
        {
            foreach (string tag in TagVocab.TagArr)
            {
                sw.WriteLine(tag);
            }
        }

        private void WriteVocab(StreamWriter sw)
        {
            foreach (string word in ModelVocab.VocabArr)
            {
                sw.WriteLine(word);
            }
        }

        private void WriteTemplates(StreamWriter sw)
        {
            foreach (LinearChainFeatureTemplate template in Templates)
            {
                sw.WriteLine(template.Name);
            }
        }

        private void WriteFeatureFuncs(List<LinearFeatureFuncPackage> lffps, StreamWriter sw)
        {
            foreach (LinearFeatureFuncPackage lffp in lffps)
            {
                sw.WriteLine(lffp.GetStringDescription());
            }
        }

        private void LoadTagVocab(StreamReader sr)
        {
            string[] tags = new string[TagCount];
            for (int i = 0; i < tags.Length; ++i)
            {
                tags[i] = sr.ReadLine().Trim();
            }

            TagVocab = new LinearModelTagVocab(tags);
        }

        private void LoadVocab(StreamReader sr)
        {
            string[] vocabs = new string[VocabCount];
            for (int i = 0; i < vocabs.Length; ++i)
            {
                vocabs[i] = sr.ReadLine().Trim();
            }

            ModelVocab = new LinearModelVocab(vocabs);
        }

        private void LoadFeaturePackages(StreamReader sr)
        {
            LinearFuncPackages = new List<LinearFeatureFuncPackage>();
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                int[] features;
                FeatureFunc[] ffs;
                int DictId;
                ParseFeatureLine(line, out DictId, out features, out ffs);
                LinearFuncPackages.Add(new LinearFeatureFuncPackage
                {
                    feature = new LinearModelFeature(DictId, features),
                    funcs = ffs
                });
            }
        }

        private void ParseFeatureLine(string line, out int DictId, out int[] features, out FeatureFunc[] funcs)
        {
            string[] parts = line.Split(new string[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
            DictId = int.Parse(parts[0]);

            string[] subparts = parts[1].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            features = new int[subparts.Length];

            for (int i = 0; i < subparts.Length; ++i)
            {
                features[i] = int.Parse(subparts[i]);
            }

            subparts = parts[2].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            funcs = new FeatureFunc[subparts.Length / 2];

            for (int i = 0; i < funcs.Length; ++i)
            {
                funcs[i] = new FeatureFunc();
                funcs[i].tag = int.Parse(subparts[i * 2]);
                funcs[i].weight = float.Parse(subparts[i * 2 + 1]);
            }
        }
    }
}
