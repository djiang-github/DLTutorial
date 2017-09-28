using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

using LinearDepParser;

using NanYUtilityLib;

using NanYUtilityLib.DepParUtil;

using ParserUtil;

using LinearModel;
using LinearFunction;

namespace DepParTraining
{
    class Program
    {
        static void CreateParserModelHeader(string datafn, string templatefn, string wclusterfn, string ofn)
        {
            Encoding enc = Encoding.GetEncoding("utf-8");
            //string datafn = @"D:\user\nyang\deppar\data\wsj-ontonote\nw.wsj.train.autotag.malt";
            //string templatefn = @"D:\user\nyang\deppar\data\wsj-ontonote\parser.feat.wc.txt";
            //string wclusterfn = @"D:\user\nyang\deppar\data\wsj-ontonote\wiki.c.1024.txt";
            int cutoff = 3;

            //string ofn = @"D:\user\nyang\deppar\data\wsj-ontonote\parser.modelhead.wc.txt";

            List<string> wordcluster = LoadWordClusterFile(wclusterfn);

            MaltTabFileReader mtfr = new MaltTabFileReader(datafn);

            ItemCounterDict<string> vocabDict = new ItemCounterDict<string>();
            ItemCounterDict<string> labelDict = new ItemCounterDict<string>();
            ItemCounterDict<string> actionDict = new ItemCounterDict<string>();

            foreach (var wc in wordcluster)
            {
                string[] parts = wc.Split(new string[] { " ", "\t" }, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length == 2)
                {
                    vocabDict.Add(parts[1], cutoff + 1);
                }
            }
            int linenum = 0;
            int error = 0;
            while (!mtfr.EndOfStream)
            {
                ParserSentence snt;
                if (mtfr.GetNextSent(out snt))
                {
                    foreach (string w in snt.tok)
                    {
                        vocabDict.Add(w);
                    }
                    foreach (string w in snt.pos)
                    {
                        vocabDict.Add(w);
                    }
                    foreach (string w in snt.label)
                    {
                        vocabDict.Add(w);
                        labelDict.Add(w);
                    }
                    ParserAction[] actions;
                    string[] actionlabel;

                    DepTree tree = new DepTree(snt.tok, snt.pos, snt.hid, snt.label);
                    linenum++;
                    if (!DepTree.IsValidDepTree(tree) || !tree.CheckWellFormedness())
                    {
                        Console.Error.Write("!");
                        error++;
                        continue;
                    }

                    if (ParserConfig.InferAction(snt.hid, snt.label, out actions, out actionlabel))
                    {
                        for (int i = 0; i < actions.Length; ++i)
                        {
                            ParserAction pa = actions[i];
                            string pl = actionlabel[i];
                            switch (pa)
                            {
                                case ParserAction.LA:
                                    actionDict.Add("LA " + pl);
                                    break;
                                case ParserAction.RA:
                                    actionDict.Add("RA " + pl);
                                    break;
                                case ParserAction.SHIFT:
                                    actionDict.Add("SH");
                                    break;
                                case ParserAction.REDUCE:
                                    actionDict.Add("RE");
                                    break;
                            }
                        }
                    }
                }
            }

            string[] actionArr = actionDict.GetSortedItemArray();
            string[] labelArr = labelDict.GetSortedItemArray();
            string[] vocabArr = vocabDict.GetSortedItemArray(cutoff);

            LinearModelInfo lminfo = new LinearModelInfo();

            lminfo.descriptor = new ParserStateDescriptor();

            lminfo.AddExtraInfo("LowerCaseWordCluster", wordcluster);

            lminfo.SetActionVocab(actionArr);

            lminfo.SetVocab(vocabArr);

            lminfo.SetTags(labelArr);

            StreamReader sr = new StreamReader(templatefn, enc);

            List<string> tptlist = new List<string>();

            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }
                tptlist.Add(line);
            }

            sr.Close();

            lminfo.SetFeatureTemplates(tptlist.ToArray());

            Console.Error.WriteLine("{0} {1}", linenum, error);

            lminfo.WriteModel(ofn, null, null);

        }
        static List<string> LoadWordClusterFile(string fn)
        {
            List<string> wc = new List<string>();

            using (StreamReader sr = new StreamReader(fn))
            {
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine().Trim();

                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        wc.Add(line);
                    }
                }
            }

            return wc;
        }

        static public void ConvertShiftReduceTemplate(string input, string output)
        {
            //string input = @"D:\user\nyang\deppar\data\wsj-ontonote\parser.feat.readable.txt";
            //string output = @"D:\user\nyang\deppar\data\wsj-ontonote\parser.feat.wc.txt";

            var descripter = new ParserStateDescriptor();

            StreamReader sr = new StreamReader(input);
            StreamWriter sw = new StreamWriter(output, false);

            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine().Trim();

                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                {
                    continue;
                }

                string[] parts = line.Split(new string[] { " ", "\t" }, StringSplitOptions.RemoveEmptyEntries);

                List<int> ids = new List<int>();
                foreach (string p in parts)
                {
                    int id;
                    int det;
                    if (!descripter.GetElementId(p, out id, out det))
                    {
                        throw new Exception();
                    }
                    ids.Add(id);
                }

                string outline = string.Join(" ", ids.ToArray());

                sw.WriteLine(outline);
            }

            sr.Close();
            sw.Close();

        }

        static void Main(string[] args)
        {
            Configure.SetArgs(args);

            string inputtemplatefile = @"D:\user\nyang\dptoolkit\auxiliary\parser.feat.readable.txt";
            string outputtemplatefile = @"D:\user\nyang\dptoolkit\auxiliary\parser.feat.wc.txt";
            string datafile = @"D:\user\nyang\dptoolkit\data\train.autotag.malt";

            string modelheader = @"D:\user\nyang\dptoolkit\auxiliary\parser.modelhead.txt";

            string wcfile = @"D:\user\nyang\dptoolkit\auxiliary\wiki.c.1024.txt";

            //ConvertShiftReduceTemplate(inputtemplatefile, outputtemplatefile);

            //CreateParserModelHeader(datafile, outputtemplatefile, wcfile, modelheader);

            string trainfn = //@"D:\users\nanyang\run\train-eparser\toy.txt";
                            Configure.GetOptionString("Train");//@"D:\users\nanyang\++++treebank\LightTokenized\wsj.02-21.lighttok.tagged.txt";
            string heloutfn = //@"D:\users\nanyang\run\train-eparser\toy.txt";
                            Configure.GetOptionString("Heldout");//@"D:\users\nanyang\++++treebank\LightTokenized\wsj.23.lighttok.tagged.txt";

            string outputfn = Configure.GetOptionString("Model");//@"D:\users\nanyang\++++treebank\LightTokenized\model\eparser.ap.wsj.light.casesensitive.beam16";
            int beamsize = Configure.GetOptionInt("BeamSize", 16) ;
            int updateTheshold = Configure.GetOptionInt("UpdateThreshold", 0);

            //Test.DoTest(heloutfn, outputfn, beamsize);
            Test.TestDirGreedy();
            return;

            Train t = new Train();
            //string mefile = Configure.GetOptionString("METraining");
            //t.DoTrain(trainfn, heloutfn, modelheader, outputfn, beamsize, 3, updateTheshold);
            t.DoTrainGreedy(trainfn, heloutfn, modelheader, outputfn, beamsize, 3, updateTheshold);
            //t.ExtractTrainingInstanceForMaxEnt(trainfn, modelheader, mefile);
            //t.ConvertMaxEntModel(
            //    @"D:\users\nanyang\run\run-me\output\parser.me",
            //    modelheader,
            //    @"D:\users\nanyang\++++treebank\PTB-MTTokenized\model\parse.unlabeled.me.model"
            //    );
            ////t.TrainDistributed(trainfn, heloutfn, modelheader, outputfn, beamsize, 3, updateTheshold, 2);
            //Test tx = new Test(heloutfn,
            //    //outputfn,
            //    @"D:\users\nanyang\++++treebank\PTB-MTTokenized\model\parse.unlabeled.me.model",
            //    5);
        }
    }
}
