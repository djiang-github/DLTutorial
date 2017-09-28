using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using LinearDepParser;
using NanYUtilityLib;
using NanYUtilityLib.DepParUtil;
using NanYUtilityLib.Sweets;
using AveragePerceptron;
using ParserUtil;
using LinearFunction;

namespace DepParTraining
{
    class Train
    {
        public Train()
        {
        }

        public void DoTrain(string trainfn, string heldoutfn, string modelheader, string outputfn, int beamsize, int featCutoff, int UpdateThreshold)
        {
            ParserSentence[] traindata;
            ParserSentence[] heldout;
            LoadData(trainfn, out traindata);
            LoadData(heldoutfn, out heldout);

            LinearModelInfo lmInfo = new LinearModelInfo(modelheader, new ParserStateDescriptor());
            AveragePerceptronModel apModel = new AveragePerceptronModel(lmInfo.FeatureTemplateCount, UpdateThreshold);
            ParserModelInfo pmInfo = new ParserModelInfo(lmInfo);

            ParserDecoder decoder = new ParserDecoder(pmInfo, apModel, beamsize);

            int bestUA = 0;
            int bestLA = 0;

            Random r = new Random();
            for (int round = 0; round < 300; ++round)
            {
                Console.Error.WriteLine("Start training round {0}", round);

                apModel.IsTraining = true;

                ConsoleTimer timer = new ConsoleTimer(100);

                ObjectShuffle.ShuffleArr<ParserSentence>(traindata, r);

                for (int i = 0; i < traindata.Length; ++i)
                {
                    ParserSentence snt = traindata[i];
                    List<FeatureUpdatePackage> fuplist;

                    if (decoder.Train(snt.tok, snt.pos, snt.hid, snt.label, out fuplist))
                    {
                        apModel.Update(fuplist);
                    }
                    else
                    {
                        Console.Error.WriteLine("Fail to train sentence {0}!!!!", i);
                    }
                    timer.Up();
                }

                timer.Finish();

                apModel.IsTraining = false;

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
                }
                if (correctLabel > bestLA)
                {
                    Console.Error.WriteLine("Saving model...");
                    bestLA = correctLabel;
                    lmInfo.WriteModel(outputfn, null, apModel.GetAllFeatures());

                }


            }
        }

        public void ExtractTrainingInstanceForMaxEnt(string trainfn, string modelheader, string outputfn)
        {
            ParserSentence[] traindata;
            
            LoadData(trainfn, out traindata);
            
            LinearModelInfo lmInfo = new LinearModelInfo(modelheader, new ParserStateDescriptor());
            AveragePerceptronModel apModel = new AveragePerceptronModel(lmInfo.FeatureTemplateCount, 0);
            ParserModelInfo pmInfo = new ParserModelInfo(lmInfo);

            ParserDecoder decoder = new ParserDecoder(pmInfo, apModel, 1);

            using (StreamWriter sw = new StreamWriter(outputfn))
            {
                ConsoleTimer timer = new ConsoleTimer(100);
                for (int i = 0; i < traindata.Length; ++i)
                {
                    ParserSentence snt = traindata[i];

                    List<List<FeatureUpdatePackage>> trainingInstanceList;

                    if (decoder.CollectMETrainingInstance(snt.tok, snt.pos, snt.hid, snt.label, out trainingInstanceList))
                    {
                        foreach (var trainIns in trainingInstanceList)
                        {
                            if (trainIns.Count > 0)
                            {
                                List<string> trainingLine = new List<string>();
                                trainingLine.Add(trainIns[0].tag.ToString());
                                foreach (var ufeature in trainIns)
                                {
                                    trainingLine.Add(ufeature.feature.ToString());
                                }

                                sw.WriteLine(string.Join(" ", trainingLine));
                            }

                        }
                        timer.Up();
                    }
                }
                timer.Finish();
            }
            
        }

        public void ConvertMaxEntModel(string MEfn, string modelheader, string outputmodel)
        {
            LinearModelInfo lmInfo = new LinearModelInfo(modelheader, new ParserStateDescriptor());

            var featurePackages = new List<LinearFeatureFuncPackage>();
            
            using (StreamReader sw = new StreamReader(MEfn))
            {
                int id = 0;
                var labelMapping = new Dictionary<int,int>();
                while (!sw.EndOfStream)
                {
                    string line = sw.ReadLine().Trim();
                    if (line.StartsWith("++"))
                    {
                        break;
                    }
                    labelMapping.Add(id++, int.Parse(line));
                }

                List<FeatureFunc> func = new List<FeatureFunc>();
                LinearModelFeature feature = null;
                string lastfeaturestring = null;

                while (!sw.EndOfStream)
                {
                    string line = sw.ReadLine();
                    string[] parts = line.Split(new string[] { "\t", ":" }, StringSplitOptions.RemoveEmptyEntries);
                    int label = labelMapping[int.Parse(parts[0])];
                    float score = float.Parse(parts[2]);
                    string featurestring = parts[1];

                    if (lastfeaturestring != featurestring)
                    {
                        if (func.Count > 0)
                        {
                            var lffp = new LinearFeatureFuncPackage
                            {
                                funcs = func.ToArray(),
                                feature = feature
                            };
                            featurePackages.Add(lffp);
                            func.Clear();
                            
                        }
                        lastfeaturestring = featurestring;
                        feature = new LinearModelFeature(featurestring);
                    }

                    func.Add(new FeatureFunc { tag = label, weight = score });
                }

                if (func.Count > 0)
                {
                    var lffp = new LinearFeatureFuncPackage
                    {
                        funcs = func.ToArray(),
                        feature = feature
                    };
                    featurePackages.Add(lffp);
                    func.Clear();
                }
            }

            lmInfo.WriteModel(outputmodel, null, featurePackages);
        }

        public void TrainDistributed(string trainfn, string heldoutfn, string modelheader, string outputfn, int beamsize, int featCutoff, int UpdateThreshold, int NThread)
        {
            ParserSentence[] traindata;
            ParserSentence[] heldout;
            LoadData(trainfn, out traindata);
            LoadData(heldoutfn, out heldout);

            LinearModelInfo lmInfo = new LinearModelInfo(modelheader, new ParserStateDescriptor());
            AveragePerceptronModel apModel = new AveragePerceptronModel(lmInfo.FeatureTemplateCount, UpdateThreshold);
            ParserModelInfo pmInfo = new ParserModelInfo(lmInfo);

            int bestUA = 0;
            int bestLA = 0;

            Random r = new Random();
            for (int round = 0; round < 300; ++round)
            {
                Console.Error.WriteLine("Start training round {0}", round);
                
                apModel.IsTraining = true;

                // split data
                // splitting data
                Console.Error.WriteLine("Mapping...");
                var models = new AveragePerceptronModel[NThread];
                models[0] = apModel;
                for (int i = 1; i < NThread; ++i)
                {
                    models[i] = models[0].Clone();
                }

                ParserDecoder[] decoders = new ParserDecoder[NThread];

                for (int i = 0; i < NThread; ++i)
                {
                    decoders[i] = new ParserDecoder(pmInfo, models[i], beamsize);
                }

                ObjectShuffle.ShuffleArr<ParserSentence>(traindata, r);

                List<ParserSentence>[] dataList = new List<ParserSentence>[NThread];

                for (int i = 0; i < dataList.Length; ++i)
                {
                    dataList[i] = new List<ParserSentence>();
                }

                for (int i = 0; i < traindata.Length; ++i)
                {
                    dataList[i % NThread].Add(traindata[i]);
                }

                ParserSentence[][] splitdatas = new ParserSentence[NThread][];

                for (int i = 0; i < NThread; ++i)
                {
                    splitdatas[i] = dataList[i].ToArray();
                }

                Console.Error.WriteLine("Start training...");
                DateTime tstart = DateTime.Now;
                Parallel.For(0, NThread, (i) =>
                    {
                        TrainWorker(splitdatas[i], models[i], decoders[i]);
                    });

                DateTime tend = DateTime.Now;

                Console.Error.WriteLine("Training finished in {0}", tend - tstart);

                Console.Error.WriteLine("Average sentence per sencond: {0:F1}", traindata.Length /(float)(tend - tstart).TotalSeconds);

                Console.Error.WriteLine("Merging...");

                for (int i = 1; i < NThread; ++i)
                {
                    float otherweight = 1 / (float)(i + 1);
                    models[0].Merge(otherweight, models[i]);
                    models[i] = null;
                }

                apModel = models[0];

                apModel.IsTraining = false;
                var decoder = new ParserDecoder(pmInfo, apModel, beamsize);
                int total = 0;
                int correctHead = 0;
                int correctLabel = 0;
                ConsoleTimer timer = new ConsoleTimer(100);
                
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
                }

            }
        }

        private static void TrainWorker(ParserSentence[] traindata, AveragePerceptronModel apModel, ParserDecoder decoder)
        {
            for (int i = 0; i < traindata.Length; ++i)
            {
                ParserSentence snt = traindata[i];
                List<FeatureUpdatePackage> fuplist;

                if (decoder.Train(snt.tok, snt.pos, snt.hid, snt.label, out fuplist))
                {
                    apModel.Update(fuplist);
                }
                else
                {
                    Console.Error.WriteLine("Fail to train sentence {0}!!!!", i);
                }
                //timer.Up();
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

        public void DoTrainGreedy(string trainfn, string heldoutfn, string modelheader, string outputfn, int beamsize, int featCutoff, int UpdateThreshold)
        {
            ParserSentence[] traindata;
            ParserSentence[] heldout;
            LoadData(trainfn, out traindata);
            LoadData(heldoutfn, out heldout);

            LinearModelInfo lmInfo = new LinearModelInfo(modelheader, new ParserStateDescriptor());
            AveragePerceptronModel apModel = new AveragePerceptronModel(lmInfo.FeatureTemplateCount, UpdateThreshold);
            ParserModelInfo pmInfo = new ParserModelInfo(lmInfo);

            GreedyParDecoder decoder = new GreedyParDecoder(pmInfo, apModel, beamsize);

            int bestUA = 0;
            int bestLA = 0;

            GreedyParDecoder.UpdateDelegate updator = (List<FeatureUpdatePackage> xup) =>
                {
                    apModel.UpdateNoTimer(xup);
                };

            Random r = new Random();
            for (int round = 0; round < 300; ++round)
            {
                Console.Error.WriteLine("Start training round {0}", round);

                apModel.IsTraining = true;

                ConsoleTimer timer = new ConsoleTimer(100);

                ObjectShuffle.ShuffleArr<ParserSentence>(traindata, r);

                double exploreProb = round < 2 ? -1.0 : 0.9;

                for (int i = 0; i < traindata.Length; ++i)
                {
                    ParserSentence snt = traindata[i];

                    if (decoder.Train(r, snt.tok, snt.pos, snt.hid, snt.label, exploreProb, updator))
                    {
                        apModel.Update();
                    }
                    else
                    {
                        Console.Error.WriteLine("Fail to train sentence {0}!!!!", i);
                    }
                    timer.Up();
                }

                timer.Finish();

                apModel.IsTraining = false;

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
                }
                if (correctLabel > bestLA)
                {
                    Console.Error.WriteLine("Saving model...");
                    bestLA = correctLabel;
                    lmInfo.WriteModel(outputfn, null, apModel.GetAllFeatures());

                }


            }
        }


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
    }

    class Test
    {
        static public void DoTest(string heldoutfn, string model, int beamsize)
        {
            ParserSentence[] heldout;

            LoadData(heldoutfn, out heldout);

            LinearModelInfo lmInfo = new LinearModelInfo(model, new ParserStateDescriptor());
            BasicLinearFunction apModel = new BasicLinearFunction(lmInfo.FeatureTemplateCount, lmInfo.LinearFuncPackages);
            ParserModelInfo pmInfo = new ParserModelInfo(lmInfo);
            ParserDecoder decoder = new ParserDecoder(pmInfo, apModel, beamsize);
            decoder.IsME = false;
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

            int beamsize = Configure.GetOptionInt("BeamSize", 16);

            string logfile = Configure.GetOptionString("TestLog");

            Dictionary<string, List<ParserSentence>> testSets = new Dictionary<string,List<ParserSentence>>();

            DirectoryInfo dirinfo = new DirectoryInfo(testdir);

            foreach(var fi in dirinfo.GetFiles("*.autotag"))
            {
                testSets[fi.Name] = MaltTabFileReader.ReadAll(fi.FullName);
            }

            LinearModelInfo lmi = new LinearModelInfo(model, new ParserStateDescriptor());

            BasicLinearFunction blf = new BasicLinearFunction(lmi.FeatureTemplateCount, lmi.LinearFuncPackages);
            
            ParserModelInfo pmInfo = new ParserModelInfo(lmi);
            
            ParserDecoder decoder = new ParserDecoder(pmInfo, blf, beamsize);
            
            int total = 0;
            int uc = 0;
            int lc = 0;
            int sentnum = 0;
            ConsoleTimer tm = new ConsoleTimer(100);

            using(StreamWriter sw = new StreamWriter(logfile))
            {
                int ftotal = 0;
                int fuc = 0;
                int flc = 0;

                foreach(var testset in testSets)
                {
                    foreach(var ps in testset.Value)
                    {
                        int[] hid;
                        string[] labl;

                        decoder.Run(ps.tok, ps.pos, out hid, out labl);

                        for(int i = 0; i < ps.Length; ++i)
                        {
                            if(!IsPunc(ps.tok[i]))
                            {
                                ftotal++;
                                if(ps.hid[i] == hid[i])
                                {
                                    fuc++;
                                    if(ps.hid[i] == 0 || ps.label[i] == labl[i])
                                    {
                                        flc++;
                                    }
                                }
                            }
                        }

                        tm.Up();
                    }

                    sw.WriteLine("{0}\t{1}\t{2:F2}\t{3:F2}",
                        testset.Key, testset.Value.Count, fuc / (double) ftotal * 100, flc / (double) ftotal * 100);
                    sentnum += testset.Value.Count;
                    uc += fuc;
                    lc += flc;
                    total += ftotal;

                }

                sw.WriteLine("{0}\t{1}\t{2:F2}\t{3:F2}",
                        "OverAll", sentnum, uc / (double) total * 100, lc / (double) total * 100);

            }

            tm.Finish();
        }
        static public void TestDirGreedy()
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

            LinearModelInfo lmi = new LinearModelInfo(model, new ParserStateDescriptor());

            BasicLinearFunction blf = new BasicLinearFunction(lmi.FeatureTemplateCount, lmi.LinearFuncPackages);

            ParserModelInfo pmInfo = new ParserModelInfo(lmi);

            GreedyParDecoder decoder = new GreedyParDecoder(pmInfo, blf, 0);

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
