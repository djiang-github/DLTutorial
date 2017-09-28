using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LinearDepParser;
using System.IO;
using NanYUtilityLib;
using NanYUtilityLib.DepParUtil;
using PoSTag;
using NanYUtilityLib.Sweets;

namespace LinearDepParUtil
{
    class Program
    {
        static void FixBRPos(string[] args)
        {
            string ifn = @"D:\users\nanyang\run\train-eparser\4000qs.malt.txt";
            string ofn = @"D:\users\nanyang\run\train-eparser\4000qs.malt.fix.txt";

            MaltTabFileReader mtr = new MaltTabFileReader(ifn);
            MaltFileWriter mtw = new MaltFileWriter(ofn, false);

            while (!mtr.EndOfStream)
            {
                ParserSentence ps;
                if (mtr.GetNextSent(out ps))
                {
                    for (int i = 0; i < ps.Length; ++i)
                    {
                        if (ps.pos[i] == ")")
                        {
                            ps.pos[i] = "RRB";
                        }
                        else if (ps.pos[i] == "(")
                        {
                            ps.pos[i] = "LRB";
                        }
                        else if (ps.tok[i] == "?")
                        {
                            ps.pos[i] = ".";
                            ps.label[i] = "punct";
                        }
                    }
                }

                mtw.Write(ps.tok, ps.pos, ps.hid, ps.label);
            }

            mtr.Close();
            mtw.Close();
        }


        static void ConvertTemplateFile(string[] args)
        {
            string ifn = @"D:\users\nanyang\run\train-parser\input\TemplateNames.txt";
            string ofn = @"D:\users\nanyang\run\train-parser\input\TemplateConverted.txt";
            StreamReader sr = new StreamReader(ifn);
            StreamWriter sw = new StreamWriter(ofn);

            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine().Trim();
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                {
                    continue;
                }
                int[] template;
                if (!FeatureTemplateConvertor.Convert(line, out template))
                {
                    Console.Error.WriteLine("Error: {0}", line);
                }
                else
                {
                    sw.WriteLine(string.Join(" ", template));
                }
            }

            sr.Close();
            sw.Close();
        }

        static List<string> GetFeatureTemplates(string fn)
        {
            List<string> templates = new List<string>();
            StreamReader sr = new StreamReader(fn);
            //StreamWriter sw = new StreamWriter(ofn);

            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine().Trim();
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                {
                    continue;
                }
                int[] template;
                if (!FeatureTemplateConvertor.Convert(line, out template))
                {
                    Console.Error.WriteLine("Error: {0}", line);
                }
                else
                {
                    templates.Add(string.Join(" ", template));
                }
            }

            sr.Close();

            return templates;
        }

        static void ConvertCommands(string[] args)
        {
            string ifn = @"D:\users\nanyang\binarizedDepPar\Model\commands.txt";
            string ofn = @"D:\users\nanyang\binarizedDepPar\Model\commandsconverted.txt";
            StreamReader sr = new StreamReader(ifn);
            StreamWriter sw = new StreamWriter(ofn);
            HashSet<string> labels = new HashSet<string>();
            List<string> commands = new List<string>();
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine().Trim();
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                {
                    continue;
                }
                if (line.StartsWith("SH"))
                {
                    commands.Add("SH");
                }
                else if (line.StartsWith("RE"))
                {
                    commands.Add("RE");
                }
                else if (line.StartsWith("LA"))
                {
                    string[] parts = line.Split('_');
                    if (!labels.Contains(parts[1]))
                    {
                        labels.Add(parts[1]);
                    }
                    commands.Add(string.Join(" ", parts));
                }
                else if (line.StartsWith("RA"))
                {
                    string[] parts = line.Split('_');
                    if (!labels.Contains(parts[1]))
                    {
                        labels.Add(parts[1]);
                    }
                    commands.Add(string.Join(" ", parts));
                }
            }

            sw.WriteLine("labelCount: {0}", labels.Count);
            foreach (string x in labels)
            {
                sw.WriteLine(x);
            }
            sw.WriteLine("commandCount: {0}", commands.Count);
            foreach (string x in commands)
            {
                sw.WriteLine(x);
            }
            sr.Close();
            sw.Close();
        }

        static void CountVocab()
        {
            string ifn = @"D:\users\nanyang\++++treebank\LightTokenized\lowercase\wsj.02-21.lighttok.tagged.txt";
            string ofn = @"D:\users\nanyang\++++treebank\LightTokenized\lowercase\wsj.02-21.vocab";

            int cutoff = 3;

            MaltTabFileReader sr = new MaltTabFileReader(ifn);

            ItemCounterDict<string> cntDict = new ItemCounterDict<string>();

            while (!sr.EndOfStream)
            {
                ParserSentence ps;
                if (sr.GetNextSent(out ps))
                {
                    for (int i = 0; i < ps.Length; ++i)
                    {
                        cntDict.Add(ps.tok[i]);
                        cntDict.Add(ps.pos[i]);
                    }
                }
            }

            sr.Close();

            List<ItemCounter<string>> toklist = cntDict.GetSortedList();
            StreamWriter sw = new StreamWriter(ofn, false);

            foreach (ItemCounter<string> c in toklist)
            {
                if (c.cnt <= cutoff)
                {
                    break;
                }

                sw.WriteLine(c.item);
            }

            sw.Close();

        }

        static List<string> GetVocab(string ifn)
        {
            int cutoff = 3;

            MaltTabFileReader sr = new MaltTabFileReader(ifn);

            ItemCounterDict<string> cntDict = new ItemCounterDict<string>();

            while (!sr.EndOfStream)
            {
                ParserSentence ps;
                if (sr.GetNextSent(out ps))
                {
                    for (int i = 0; i < ps.Length; ++i)
                    {
                        cntDict.Add(ps.tok[i]);
                        cntDict.Add(ps.pos[i]);
                    }
                }
            }

            sr.Close();

            List<ItemCounter<string>> toklist = cntDict.GetSortedList();


            List<string> vocabs = new List<string>();
            foreach (ItemCounter<string> c in toklist)
            {
                if (c.cnt <= cutoff)
                {
                    break;
                }

                vocabs.Add(c.item);
            }

            return vocabs;

        }

        static List<string> GetDepLabl(string ifn)
        {
            MaltTabFileReader sr = new MaltTabFileReader(ifn);

            ItemCounterDict<string> cntDict = new ItemCounterDict<string>();

            while (!sr.EndOfStream)
            {
                ParserSentence ps;
                if (sr.GetNextSent(out ps))
                {
                    for (int i = 0; i < ps.Length; ++i)
                    {
                        cntDict.Add(ps.label[i]);
                    }
                }
            }

            sr.Close();

            List<ItemCounter<string>> toklist = cntDict.GetSortedList();


            List<string> vocabs = new List<string>();
            foreach (ItemCounter<string> c in toklist)
            {
                vocabs.Add(c.item);
            }

            return vocabs;

        }

        static List<string> GetCommands(string ifn)
        {
            MaltTabFileReader sr = new MaltTabFileReader(ifn);

            ItemCounterDict<string> cntDict = new ItemCounterDict<string>();

            while (!sr.EndOfStream)
            {
                ParserSentence ps;
                if (sr.GetNextSent(out ps))
                {
                    for (int i = 0; i < ps.Length; ++i)
                    {
                        if (ps.hid[i] == 0)
                        {
                            continue;
                        }
                        else if (ps.hid[i] - 1 < i)
                        {
                            cntDict.Add(string.Format("RA {0}", ps.label[i]));
                        }
                        else
                        {
                            cntDict.Add(string.Format("LA {0}", ps.label[i]));
                        }
                    }
                }
            }
            cntDict.Add("SH");
            cntDict.Add("RE");
            sr.Close();

            List<ItemCounter<string>> toklist = cntDict.GetSortedList();


            List<string> vocabs = new List<string>();
            foreach (ItemCounter<string> c in toklist)
            {
                vocabs.Add(c.item);
            }

            return vocabs;
        }

        static void TagMaltFile()
        {

            string mdfn = @"D:\users\nanyang\++++treebank\LightTokenized\lowercase\model\etag.wsj.light.beam8.lowercase.model";
            int beamsize = 8;

            PoSTagModelWrapper ptmw = new PoSTagModelWrapper(mdfn);

            PoSTagDecoderWrapper dcrp = new PoSTagDecoderWrapper(ptmw, beamsize);
            MaltTabFileReader mtr = new MaltTabFileReader(@"D:\users\nanyang\++++treebank\LightTokenized\lowercase\wsj.23.lighttok.fix.txt");

            MaltFileWriter mtw = new MaltFileWriter(@"D:\users\nanyang\++++treebank\LightTokenized\lowercase\wsj.23.lighttok.tagged.txt");
            mtw.IsSingleLine = true;
            ConsoleTimer timer = new ConsoleTimer(100);
            while (!mtr.EndOfStream)
            {
                ParserSentence ps;
                if (mtr.GetNextSent(out ps))
                {
                    string[] tag;
                    dcrp.GenPOS(ps.tok, out tag);
                    mtw.Write(ps.tok, tag, ps.hid, ps.label);
                    timer.Up();
                }
            }
            timer.Finish();
            mtr.Close();
            mtw.Close();
        }


        static void ExtractCRF(string[] args)
        {


            //string ftfile = Configure.GetOptionString("FeatureTemplate");
            string tbfn = @"D:\users\nanyang\run\train-eparser\4000qs.test.txt";
            string outputfn = @"D:\users\nanyang\run\train-eparser\4000qs.test.pos.txt";

            StreamWriter sw = new StreamWriter(outputfn, false);

            MaltTabFileReader mtfr = new MaltTabFileReader(tbfn);
            

            //FeatureExtractor fe = new FeatureExtractor(ftfile);

            //ExtractTrainingInstance eti = new ExtractTrainingInstance(fe);
            int linenum = 0;
            while (!mtfr.EndOfStream)
            {
                string[] tok, pos, arc;
                int[] hid;

                if (mtfr.GetNextSent(out tok, out pos, out hid, out arc))
                {
                    for (int i = 0; i < tok.Length; ++i)
                    {
                        string t = tok[i].ToLower();
                        List<string> features = new List<string>();
                        features.Add(t);
                        for (int j = 1; j < 5; ++j)
                        {
                            features.Add(GetPrefix(t, j));
                        }

                        for (int j = 1; j < 5; ++j)
                        {
                            features.Add(GetSuffix(t, j));
                        }

                        features.Add(pos[i]);
                        sw.Write(string.Join("\t", features) + "\n");
                    }

                    sw.Write("\n");

                    linenum++;
                }
            }

            Console.Error.WriteLine(linenum);

            sw.Close();
            mtfr.Close();
        }

        static string GetPrefix(string t, int n)
        {
            if (t == "$number" || t == "$xmlesc"
                || t == "$date" || t == "$url"
                || t == "$time" || t == "$modeltype"
                || t == "$day" || t == "$literal"
              )
            {
                return t;
            }
            if (t.Length <= n)
            {
                return t;
            }

            return t.Substring(0, n);
        }

        static string GetSuffix(string t, int n)
        {
            if (t == "$number" || t == "$xmlesc"
                || t == "$date" || t == "$url"
                || t == "$time" || t == "$modeltype"
                || t == "$day" || t == "$literal"
              )
            {
                return t;
            }
            if (t.Length <= n)
            {
                return t;
            }

            return t.Substring(t.Length - n);
        }

        static void LowerCaseMaltTok()
        {
            string ifn = @"D:\users\nanyang\++++treebank\LightTokenized\keepcase\wsj.23.lighttok.fix.txt";
            string ofn = @"D:\users\nanyang\++++treebank\LightTokenized\lowercase\wsj.23.lighttok.fix.txt";

            MaltTabFileReader mtfr = new MaltTabFileReader(ifn);
            MaltFileWriter mtw = new MaltFileWriter(ofn);

            while (!mtfr.EndOfStream)
            {
                ParserSentence snt;
                if (mtfr.GetNextSent(out snt))
                {
                    for (int i = 0; i < snt.tok.Length; ++i)
                    {
                        snt.tok[i] = snt.tok[i].ToLower();
                        if (snt.pos[i] == "NN" || snt.pos[i] == "NNS"
                            || snt.pos[i] == "NNP" || snt.pos[i] == "NNPS")
                        {
                            snt.pos[i] = "NN";
                        }
                    }

                    mtw.Write(snt.tok, snt.pos, snt.hid, snt.label);
                }
            }

            mtfr.Close();
            mtw.Close();
        }

        static void GetModelHead()
        {
            string maltfn = @"D:\users\nanyang\++++treebank\ctbdep\train.tagged.malt";
            string featfn = @"D:\users\nanyang\++++treebank\ctbdep\parser.featuretemplate.txt";

            string modelfn = @"D:\users\nanyang\++++treebank\ctbdep\cparse.modelhead.txt";

            List<string> templates = GetFeatureTemplates(featfn);
            List<string> vocabs = GetVocab(maltfn);
            List<string> labels = GetDepLabl(maltfn);
            List<string> commands = GetCommands(maltfn);

            StreamWriter sw = new StreamWriter(modelfn, false);

            sw.WriteLine("{0} {1} {2} {3}", templates.Count, labels.Count, commands.Count, vocabs.Count);

            foreach (string x in templates)
            {
                sw.WriteLine(x);
            }

            foreach (string x in labels)
            {
                sw.WriteLine(x);
            }
            foreach (string x in commands)
            {
                sw.WriteLine(x);
            }
            foreach (string x in vocabs)
            {
                sw.WriteLine(x);
            }

            sw.Close();
        }

        static void Main(string[] args)
        {
            //ConvertCommands(args);
            //ConvertTemplateFile(args);
            //FixBRPos(args);

            //TagMaltFile();
            //CountVocab();
            GetModelHead();
            //ExtractCRF(args);
            //LowerCaseMaltTok();
        }
    }
}
