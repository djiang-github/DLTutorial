using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using EasyFirstDepPar;
using NanYUtilityLib;
using NanYUtilityLib.DepParUtil;
using NanYUtilityLib.Sweets;
using AveragePerceptron;
using ParserUtil;
using LinearFunction;

namespace EasyFirstParserTraining
{
    class Train
    {
        public Train()
        {
        }

        public void DoTrain(string trainfn, string heldoutfn, string modelheader, string outputfn, int beamsize, int featCutoff, int UpdateThreshold)
        {
            //wrapper = new ParserModelTrainWrapper(modelheader, UpdateThreshold);
            ParserSentence[] traindata;
            ParserSentence[] heldout;
            LoadData(trainfn, out traindata);
            LoadData(heldoutfn, out heldout);

            LinearModelInfo lmInfo = new LinearModelInfo(modelheader, new EasyFirstParserStateDescriptor());
            AveragePerceptronModel apModel = new AveragePerceptronModel(lmInfo.FeatureTemplateCount, UpdateThreshold);
            EasyFirstParserModelInfo pmInfo = new EasyFirstParserModelInfo(lmInfo);
            //ParserDecoder decoder = new ParserDecoder(wrapper.featGroups, wrapper.vocab, wrapper.pmt, beamsize);

            EasyParDecoder decoder = new EasyParDecoder(pmInfo, apModel);

            int bestUA = 0;
            int bestLA = 0;
            int maxtry = 5;
            int burn = 2;

            Random r = new Random();
            for (int round = 0; round < 300; ++round)
            {
                Console.Error.WriteLine("Start training round {0}", round);

                
                //decoder.SetBeamSize(1);
                //wrapper.pmt.isTraining = true;
                apModel.IsTraining = true;

                int totalsnt = 0;
                int passedsnt = 0;
                int totaltry = 0;

                ConsoleTimer timer = new ConsoleTimer(100);

                ObjectShuffle.ShuffleArr<ParserSentence>(traindata, r);

                for (int i = 0; i < traindata.Length; ++i)
                {
                    ParserSentence snt = traindata[i];
                    List<FeatureUpdatePackage> fuplist;
                    bool pass;
                    int trynum = 0;
                    totalsnt++;
                    do
                    {
                        if (decoder.Train(snt.tok, snt.pos, snt.hid, snt.label, out pass, out fuplist))
                        {
                            apModel.Update(fuplist);
                        }
                        else
                        {
                            Console.Error.WriteLine("Fail to train sentence {0}!!!!", i);
                            break;
                        }
                        totaltry++;
                    } while (round > burn && !pass && trynum++ < maxtry);
                    if (pass)
                    {
                        passedsnt++;
                    }
                    timer.Up();
                }

                if (round > burn)
                {
                    maxtry = Math.Min(maxtry + 5, 100);
                }

                timer.Finish();

                Console.Error.WriteLine("Pass Rate: {0:F3}%", passedsnt / (double)totalsnt * 100.0);
                Console.Error.WriteLine("Average Try: {0:F3}", totaltry / (double)totalsnt);


                apModel.IsTraining = false;
                //decoder.SetBeamSize(1);
                int total = 0;
                int correctHead = 0;
                int correctLabel = 0;
                timer = new ConsoleTimer(100);
                for (int i = 0; i < heldout.Length; ++i)
                {
                    ParserSentence snt = heldout[i];
                    int[] hid;
                    string[] label;

                    foreach (string x in snt.tok)
                    {
                        if (!IsPunc(x))
                        {
                            total++;
                        }
                    }

                    if (decoder.Run(snt.tok, snt.pos, out hid, out label))
                    {
                        for (int j = 0; j < snt.Length; ++j)
                        {
                            if (!IsPunc(snt.tok[j]))
                            {
                                if (hid[j] == snt.hid[j])
                                {
                                    correctHead++;
                                    if (label[j] == snt.label[j] || hid[j] == 0)
                                    {
                                        correctLabel++;
                                    }
                                }
                            }
                        }
                    }
                    timer.Up();
                }

                timer.Finish();
                Console.Error.WriteLine("UAS: {0:F3} LAS: {1:F3}", (float)correctHead / total * 100, (float)correctLabel / total * 100);





                string comments = string.Format("UAS: {0:F3} LAS: {1:F3} Beam: {2}", (float)correctHead / total * 100, (float)correctLabel / total * 100, beamsize);
                if (correctHead > bestUA)
                {
                    bestUA = correctHead;


                    //.SaveModel(string.Format("{0}.bestUA", outputfn), comments);
                }
                if (correctLabel > bestLA)
                {
                    Console.Error.WriteLine("Saving model...");
                    bestLA = correctLabel;
                    lmInfo.WriteModel(outputfn, null, apModel.GetAllFeatures());

                    //Test test = new Test(heldoutfn, outputfn, beamsize);
                    //wrapper.SaveModel(string.Format("{0}.bestLA", outputfn), comments);
                }

                //string comments = string.Format("UAS: {0:F3} LAS: {1:F3} Beam: {2}", (float)correctHead / total * 100, (float)correctLabel / total * 100, beamsize);
                //wrapper.SaveModel(string.Format("{0}.{1}", outputfn, round), comments);
            }
        }

        
        static bool IsPunc(string tok)
        {
            string chinesetok =
                @"（、），。“”—；《》——－－：‘’－？━──〈〉━━『』———！…·—－「」∶＊／-＂．＜＞`＇----……?!://.~/＆*～【】";
            if (chinesetok.IndexOf(tok) >= 0)
            {
                return true;
            }
            return tok == "." || tok == "," || tok == "-"
                || tok == "$" || tok == "%" || tok == "\""
                || tok == "(" || tok == ")" || tok == "["
                || tok == "]" || tok == "{" || tok == "}"
                || tok == "\'" || tok == "?" || tok == "!"
                || tok == ":" || tok == ";" || tok == "--"
                || tok == "#" || tok == "、" || tok == "…"
                || tok == "《" || tok == "》" || tok == "--"
                || tok == "。" || tok == "“" || tok == "”"
                || tok == "；" || tok == "，" || tok == "："
                || tok == "（" || tok == "）" || tok == "？"
                || tok == "【" || tok == "】" || tok == "——"
                || tok == "……" || tok == "「" || tok == "」";
        }

        //private static bool IsPunc(string x)
        //{
        //    bool isPunc = true;
        //    foreach (char c in x)
        //    {
        //        if (!char.IsPunctuation(c))
        //        {
        //            isPunc = false;
        //            break;
        //        }
        //    }
        //    return isPunc;
        //}

        private void LoadData(string fn, out ParserSentence[] snts)
        {
            int linenum = 0;
            Console.Error.WriteLine("Loading data from {0}...", fn);
            MaltTabFileReader sr = new MaltTabFileReader(fn);
            List<ParserSentence> list = new List<ParserSentence>();
            while (!sr.EndOfStream)
            {
                linenum++;
                ParserSentence ps;
                if (sr.GetNextSent(out ps))
                {
                    try
                    {
                        DepTree tree = new DepTree(ps.tok, ps.pos, ps.hid, ps.label);
                        if (!DepTree.IsValidDepTree(tree) || !tree.CheckWellFormedness())
                        {
                            Console.Error.WriteLine("Error in treebank file at line {0}", linenum);
                        }
                        else
                        {
                            for (int i = 0; i < ps.Length; ++i)
                            {
                                if (ps.label[i].ToLower() == "dephypen")
                                {
                                    ps.label[i] = "dep";
                                }
                            }
                            list.Add(ps);
                        }
                    }
                    catch
                    {
                        Console.Error.WriteLine("Error in treebank file at line {0}", linenum);
                    }
                }
            }
            snts = list.ToArray();

            Console.Error.WriteLine("{0} trees loaded.", list.Count);

            sr.Close();
        }
        //ParserModelTrainWrapper wrapper;
    }

    class Test
    {
        static public void DoTest(string heldoutfn, string model, int beamsize)
        {
            ParserSentence[] heldout;

            LoadData(heldoutfn, out heldout);

            LinearModelInfo lmInfo = new LinearModelInfo(model, new EasyFirstParserStateDescriptor());
            BasicLinearFunction apModel = new BasicLinearFunction(lmInfo.FeatureTemplateCount, lmInfo.LinearFuncPackages);
            EasyFirstParserModelInfo pmInfo = new EasyFirstParserModelInfo(lmInfo);
            EasyParDecoder decoder = new EasyParDecoder(pmInfo, apModel);
            ConsoleTimer timer = new ConsoleTimer();
            int total = 0;
            int correctHead = 0;
            int correctLabel = 0;
            timer = new ConsoleTimer(100);
            for (int i = 0; i < heldout.Length; ++i)
            {
                ParserSentence snt = heldout[i];
                int[] hid;
                string[] label;

                foreach (string x in snt.tok)
                {
                    if (!IsPunc(x))
                    {
                        total++;
                    }
                }

                if (decoder.Run(snt.tok, snt.pos, out hid, out label))
                {
                    for (int j = 0; j < snt.Length; ++j)
                    {
                        if (!IsPunc(snt.tok[j]))
                        {
                            if (hid[j] == snt.hid[j])
                            {
                                correctHead++;
                                if (label[j] == snt.label[j] || hid[j] == 0)
                                {
                                    correctLabel++;
                                }
                            }
                        }
                    }
                }
                timer.Up();
            }

            timer.Finish();
            Console.Error.WriteLine("UAS: {0:F3} LAS: {1:F3}", (float)correctHead / total * 100, (float)correctLabel / total * 100);
        }

        static public void TestDir()
        {
            string testdir = Configure.GetOptionString("TestDir");

            string model = Configure.GetOptionString("Model");

            string logfile = Configure.GetOptionString("TestLog");

            Dictionary<string, List<ParserSentence>> testSets = new Dictionary<string, List<ParserSentence>>();

            DirectoryInfo dirinfo = new DirectoryInfo(testdir);

            foreach (var fi in dirinfo.GetFiles("*.autotag"))
            {
                testSets[fi.Name] = MaltTabFileReader.ReadAll(fi.FullName);
            }

            LinearModelInfo lmInfo = new LinearModelInfo(model, new EasyFirstParserStateDescriptor());
            BasicLinearFunction apModel = new BasicLinearFunction(lmInfo.FeatureTemplateCount, lmInfo.LinearFuncPackages);
            EasyFirstParserModelInfo pmInfo = new EasyFirstParserModelInfo(lmInfo);
            EasyParDecoder decoder = new EasyParDecoder(pmInfo, apModel);

            int total = 0;
            int uc = 0;
            int lc = 0;
            int sentnum = 0;
            ConsoleTimer tm = new ConsoleTimer(100);

            using (StreamWriter sw = new StreamWriter(logfile))
            {
                int ftotal = 0;
                int fuc = 0;
                int flc = 0;

                foreach (var testset in testSets)
                {
                    foreach (var ps in testset.Value)
                    {
                        int[] hid;
                        string[] labl;

                        decoder.Run(ps.tok, ps.pos, out hid, out labl);

                        for (int i = 0; i < ps.Length; ++i)
                        {
                            if (!IsPunc(ps.tok[i]))
                            {
                                ftotal++;
                                if (ps.hid[i] == hid[i])
                                {
                                    fuc++;
                                    if (ps.hid[i] == 0 || ps.label[i] == labl[i])
                                    {
                                        flc++;
                                    }
                                }
                            }
                        }

                        tm.Up();
                    }

                    sw.WriteLine("{0}\t{1}\t{2:F2}\t{3:F2}",
                        testset.Key, testset.Value.Count, fuc / (double)ftotal * 100, flc / (double)ftotal * 100);
                    sentnum += testset.Value.Count;
                    uc += fuc;
                    lc += flc;
                    total += ftotal;

                }

                sw.WriteLine("{0}\t{1}\t{2:F2}\t{3:F2}",
                        "OverAll", sentnum, uc / (double)total * 100, lc / (double)total * 100);

            }

            tm.Finish();
        }

        static bool IsPunc(string tok)
        {
            string chinesetok =
                @"（、），。“”—；《》——－－：‘’－？━──〈〉━━『』———！…·—－「」∶＊／-＂．＜＞`＇----……?!://.~/＆*～【】";
            if (chinesetok.IndexOf(tok) >= 0)
            {
                return true;
            }
            return tok == "." || tok == "," || tok == "-"
                || tok == "$" || tok == "%" || tok == "\""
                || tok == "(" || tok == ")" || tok == "["
                || tok == "]" || tok == "{" || tok == "}"
                || tok == "\'" || tok == "?" || tok == "!"
                || tok == ":" || tok == ";" || tok == "--"
                || tok == "#" || tok == "、" || tok == "…"
                || tok == "《" || tok == "》" || tok == "--"
                || tok == "。" || tok == "“" || tok == "”"
                || tok == "；" || tok == "，" || tok == "："
                || tok == "（" || tok == "）" || tok == "？"
                || tok == "【" || tok == "】" || tok == "——"
                || tok == "……" || tok == "「" || tok == "」"
                || tok == "``" || tok == "\'\'";
        }

        static private void LoadData(string fn, out ParserSentence[] snts)
        {
            int linenum = 0;
            Console.Error.WriteLine("Loading data from {0}...", fn);
            MaltTabFileReader sr = new MaltTabFileReader(fn);
            List<ParserSentence> list = new List<ParserSentence>();
            while (!sr.EndOfStream)
            {
                linenum++;
                ParserSentence ps;
                if (sr.GetNextSent(out ps))
                {
                    try
                    {
                        DepTree tree = new DepTree(ps.tok, ps.pos, ps.hid, ps.label);
                        if (!DepTree.IsValidDepTree(tree) || !tree.CheckWellFormedness())
                        {
                            Console.Error.WriteLine("Error in treebank file at line {0}", linenum);
                        }
                        else
                        {
                            for (int i = 0; i < ps.Length; ++i)
                            {
                                if (ps.label[i].ToLower() == "dephypen")
                                {
                                    ps.label[i] = "dep";
                                }
                            }
                            list.Add(ps);
                        }
                    }
                    catch
                    {
                        Console.Error.WriteLine("Error in treebank file at line {0}", linenum);
                    }
                }
            }
            snts = list.ToArray();

            Console.Error.WriteLine("{0} trees loaded.", list.Count);

            sr.Close();
        }
    }

}
