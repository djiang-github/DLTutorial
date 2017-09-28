using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace LinearFunction
{
    public class LinearModelInfo
    {
        public LinearModelInfo(string fn, IStateElementDiscriptor descriptor)
        {
            StreamReader sr = new StreamReader(fn);
            Init(descriptor, sr);
            sr.Close();
        }

        public LinearModelInfo(Stream fn, IStateElementDiscriptor descriptor)
        {
            StreamReader sr = new StreamReader(fn);

            Init(descriptor, sr);

            sr.Close();
        }

        public LinearModelInfo()
        {
        }

        public void AddExtraInfo(string Name, List<string> Contents)
        {
            if (ExtraInfo == null)
            {
                ExtraInfo = new Dictionary<string, List<string>>();
            }

            ExtraInfo.Add(Name, Contents);
        }

        public void SetVocab(string[] vocabArr)
        {
            VocabCount = vocabArr.Length;
            ModelVocab = new LinearModelVocab(vocabArr);
        }

        public void SetTags(string[] tags)
        {
            TagCount = tags.Length;
            TagVocab = new LinearModelTagVocab(tags);
        }

        public void SetActionVocab(string[] actions)
        {
            ActionCount = actions.Length;
            ActionVocab = new LinearModelTagVocab(actions);
        }

        public void SetFeatureTemplates(string[] featList)
        {
            int next = 0;
            FeatureTemplateCount = featList.Length;
            TemplateSet = new List<LinearFeatureTemplate>();
            Dictionary<int, int> bagOfWordsFeatureDict = new Dictionary<int, int>();
            for (int i = 0; i < FeatureTemplateCount; ++i)
            {
                string line = featList[i];
                LinearFeatureTemplate template;
                ParseFeatureTemplate(i, ref next, bagOfWordsFeatureDict, line, out template);
                TemplateSet.Add(template);
            }
        }

        //private bool LoadExtraInfo(StreamReader sr)
        //{
        //    ExtraInformation = new List<string>();
        //    while (!sr.EndOfStream)
        //    {
        //        string line = sr.ReadLine();
        //        if (line == "</ExtraInfo>")
        //        {
        //            return true;
        //        }
        //        else
        //        {
        //            ExtraInformation.Add(line);
        //        }
        //    }
        //    return false;
        //}

        private void Init(IStateElementDiscriptor descriptor, StreamReader sr)
        {
            this.descriptor = descriptor;
            bool success = false;
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

                string[] parts = line.Split(' ');
                if (parts.Length == 3)
                {
                    FeatureTemplateCount = int.Parse(parts[0]);
                    TagCount = int.Parse(parts[1]);
                    
                    VocabCount = int.Parse(parts[2]);
                    success = true;
                }
                else if (parts.Length == 4)
                {
                    FeatureTemplateCount = int.Parse(parts[0]);
                    TagCount = int.Parse(parts[1]);
                    ActionCount = int.Parse(parts[2]);
                    
                    VocabCount = int.Parse(parts[3]);
                    success = true;
                }
                break;
            }

            if (!success)
            {
                throw new Exception();
            }

            LoadTemplates(sr);
            LoadTagVocab(sr);
            LoadActionVocab(sr);
            
            LoadVocab(sr);
            LoadFeaturePackages(sr);
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

            if (ActionCount > 0)
            {
                sw.WriteLine("{0} {1} {2} {3}", FeatureTemplateCount, TagCount, ActionCount, VocabCount);
            }
            else
            {
                sw.WriteLine("{0} {1} {2}", FeatureTemplateCount, TagCount, VocabCount);
            }

            

            WriteTemplates(sw);

            WriteTagVocab(sw);

            WriteActionVocab(sw);

            WriteVocab(sw);

            WriteFeatureFuncs(lffps, sw);

            sw.Close();
        }

        public int TagCount;
        public int ActionCount;
        public int VocabCount;
        public int FeatureTemplateCount;

        public List<LinearFeatureTemplate> TemplateSet;
        
        public LinearModelTagVocab TagVocab;
        public LinearModelTagVocab ActionVocab;

        public LinearModelVocab ModelVocab;

        public IStateElementDiscriptor descriptor;

        public List<LinearFeatureFuncPackage> LinearFuncPackages;

        public List<string> ExtraInformation;

        public Dictionary<string, List<string>> ExtraInfo;

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

        static private bool ResolveEscapeChar(string line, out string resolved)
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
                if (line.StartsWith("\\\\"))
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
            foreach (LinearFeatureTemplate template in TemplateSet)
            {
                sw.WriteLine(template.Name);
            }
        }

        private void WriteFeatureFuncs(List<LinearFeatureFuncPackage> lffps, StreamWriter sw)
        {
            if (lffps == null)
            {
                return;
            }
            foreach (var lffp in lffps)
            {
                sw.WriteLine(lffp.GetStringDescription());
            }
        }

        private void WriteActionVocab(StreamWriter sw)
        {
            if (ActionCount > 0)
            {
                foreach (string action in ActionVocab.TagArr)
                {
                    sw.WriteLine(action);
                }
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

        private void LoadActionVocab(StreamReader sr)
        {
            if (ActionCount <= 0)
            {
                return;
            }
            string[] tags = new string[ActionCount];
            for (int i = 0; i < tags.Length; ++i)
            {
                tags[i] = sr.ReadLine().Trim();
            }

            ActionVocab = new LinearModelTagVocab(tags);
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

                LinearFeatureFuncPackage lffp;

                ParseFeatureLine(line, out lffp);

                LinearFuncPackages.Add(lffp);
            }
        }

        private void ParseFeatureTemplate(int templateId, ref int next, Dictionary<int, int> bagOfWordsFeatureDict, string ftstring, out LinearFeatureTemplate ft)
        {
            string[] parts = ftstring.ToLower().Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            int i = 0;
            List<int> offset = new List<int>();
            List<int> observId = new List<int>();
            //int numbY = 0;

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

            List<int> elemIdList = new List<int>();
            List<int> detIdList = new List<int>();

            while (i < parts.Length)
            {
                int id = int.Parse(parts[i]);
                int det;
                if (!descriptor.GetElememtDet(id, out det))
                {
                    throw new Exception("Wrong model File!!!");
                }
                elemIdList.Add(id);
                detIdList.Add(det);
                i++;
            }

            ft = new LinearFeatureTemplate(templateId, thisId, groupId, elemIdList.ToArray(), detIdList.ToArray());
        }


        private void ParseFeatureLine(string line, out LinearFeatureFuncPackage lffp)
        {
            string[] parts = line.Split(new string[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
            int DictId = int.Parse(parts[0]);

            string[] subparts = parts[1].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            int[] features = new int[subparts.Length];

            for (int i = 0; i < subparts.Length; ++i)
            {
                features[i] = int.Parse(subparts[i]);
            }

            subparts = parts[2].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            FeatureFunc[] funcs = new FeatureFunc[subparts.Length / 2];

            for (int i = 0; i < funcs.Length; ++i)
            {
                funcs[i] = new FeatureFunc();
                funcs[i].tag = int.Parse(subparts[i * 2]);
                funcs[i].weight = float.Parse(subparts[i * 2 + 1]);
            }

            lffp = new LinearFeatureFuncPackage
            {
                feature = new LinearModelFeature(DictId, features),
                funcs = funcs
            };
        }


        private void LoadTemplates(StreamReader sr)
        {
            int next = 0;
            TemplateSet = new List<LinearFeatureTemplate>();
            Dictionary<int, int> bagOfWordsFeatureDict = new Dictionary<int, int>();
            for (int i = 0; i < FeatureTemplateCount; ++i)
            {
                string line = sr.ReadLine();

                LinearFeatureTemplate template;
                ParseFeatureTemplate(i, ref next, bagOfWordsFeatureDict, line, out template);
                TemplateSet.Add(template);
            }
        }
    }
}
