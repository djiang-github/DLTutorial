using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoSTag;
using NanYUtilityLib.Sweets;
using NanYUtilityLib.DepParUtil;
using NanYUtilityLib;
using AveragePerceptron;
using LinearFunction;
using System.IO;

namespace PoSTagTraining
{
    class Train
    {
        class TrainingSentence
        {
            public string[][] obs;
            public string[] tags;

            public string[] tok;
            public int[] hid;
            public string[] arc;

            public string[] autotags;

            public TrainingSentence(List<string[]> partList)
            {
                obs = new string[partList.Count][];
                tags = new string[partList.Count];
                for (int i = 0; i < partList.Count; ++i)
                {
                    tags[i] = partList[i][partList[i].Length - 1];
                    obs[i] = new string[partList[i].Length - 1];
                    for (int j = 0; j < obs[i].Length; ++j)
                    {
                        obs[i][j] = partList[i][j];
                    }
                }
            }

            public TrainingSentence(string[][] obs, string[] tags)
            {
                this.obs = obs;
                this.tags = tags;
            }

            public void Reverse()
            {
                tags = tags.Reverse<string>().ToArray<string>();
                obs = obs.Reverse<string[]>().ToArray<string[]>();
            }

            public void AddConstraints(Dictionary<string, List<string>> tagDict)
            {
                possibleTags = new List<string>[tags.Length];
                for (int i = 0; i < tags.Length; ++i)
                {
                    List<string> ptlist;
                    if (obs[i][0] == null)
                    {
                        continue;
                    }
                    if (tagDict.TryGetValue(obs[i][0], out ptlist))
                    {
                        possibleTags[i] = ptlist;
                    }
                }
            }

            public string[][] forbidTags;
            public List<string>[] possibleTags;
        }

        public void DoTrain()
        {
            string trainfn = Configure.GetOptionString("Train");
            string heldoutfn = Configure.GetOptionString("Heldout");
            string modelheadfn = Configure.GetOptionString("ModelHead");

            string outputfn = Configure.GetOptionString("Model");

            int beamsize = Configure.GetOptionInt("BeamSize", 5);
            int updateThreshold = Configure.GetOptionInt("UpdateThreshold", 20);

            var PoSDict = new WSJPoSDict();
            var ObservGen = new WSJObservGenerator();
            var trainSet = LoadTrainingInstance(trainfn, PoSDict.TagDict, ObservGen);
            var heldoutSet = LoadTrainingInstance(heldoutfn, PoSDict.TagDict, ObservGen);

            var modelInfo = new LinearChainModelInfo(modelheadfn);

            var apmodel = new AveragePerceptronModel(modelInfo.FeatureTemplateCount, updateThreshold);

            var decoder = new TrigramChainDecoder(modelInfo, apmodel, beamsize);

            var rd = new Random();

            int TotalRound = 100;
            int bestRound = 0;
            int bestCorrect = 0;
            int burnIn = -1;

            float testAcc = 0;
            for (int r = 0; r < TotalRound; ++r)
            {
                if (r == burnIn)
                {
                    Console.Error.WriteLine("BurnIn...");
                }

                Console.Error.WriteLine("Training Round {0}: ", r);
                ConsoleTimer timer = new ConsoleTimer(100);
                
                apmodel.IsTraining = true;

                int[] indexArr = new int[trainSet.Length];

                for (int i = 0; i < trainSet.Length; ++i)
                {
                    indexArr[i] = i;
                }

                for (int i = 0; i < trainSet.Length - 1; ++i)
                {
                    int shid = rd.Next(trainSet.Length - i) + i;
                    int tmp = indexArr[i];
                    indexArr[i] = indexArr[shid];
                    indexArr[shid] = tmp;
                }

                for (int i = 0; i < trainSet.Length; ++i)
                {
                    var ts = trainSet[indexArr[i]];
                    List<FeatureUpdatePackage> fup;

                    if (!decoder.TrainMultiTag(ts.obs, ts.possibleTags, ts.tags, out fup))
                    {
                        Console.Error.WriteLine("Fail to train!");
                    }
                    else
                    {
                        apmodel.Update(fup);

                        timer.Up();
                    }
                }

                timer.Finish();

                timer = new ConsoleTimer(100);
                Console.Error.WriteLine("Test on HeldOut...");
                apmodel.IsTraining = false;
                int total = 0;
                int correct = 0;
                foreach (TrainingSentence ts in heldoutSet)
                {
                    string[] outputTag;
                    decoder.RunMultiTag(ts.obs, ts.possibleTags, out outputTag);

                    for (int i = 0; i < ts.tags.Length; ++i)
                    {
                        if (!IsPunc(ts.obs[i][0]))
                        {
                            total++;
                            if (ts.tags[i] == outputTag[i])
                            {
                                correct++;
                            }
                        }
                    }
                    timer.Up();
                }
                timer.Finish();
                Console.Error.WriteLine("{0} {1} {2:F3}", total, correct, (float)correct / total * 100);
                if (bestCorrect < correct)
                {
                    bestCorrect = correct;
                    bestRound = r;
                    Console.Error.WriteLine("Save Model...");
                    modelInfo.WriteModel(outputfn, null, apmodel.GetAllFeatures());
                }

                Console.Error.WriteLine("TestAcc: {0:F3}", testAcc * 100);

                Console.Error.WriteLine("BestRound {0} Acc: {1:F3}", bestRound, (float)bestCorrect / total * 100);
            }
        }

        public void DoTrainBigram()
        {
            string trainfn = Configure.GetOptionString("TrainDataDir");
            string heldoutfn = Configure.GetOptionString("DevDataDir");
            string testfn = Configure.GetOptionString("TestDataDir");
            string modelheadfn = Configure.GetOptionString("ModelHead");

            string resultfn = Configure.GetOptionString("OutputFile");

            string resultdevfn = Configure.GetOptionString("OutputFileDev");

            string outputfn = Configure.GetOptionString("Model");

            int beamsize = Configure.GetOptionInt("BeamSize", 5);
            int updateThreshold = Configure.GetOptionInt("UpdateThreshold", 20);

            var modelInfo = new LinearChainModelInfo(modelheadfn);

            var PoSDict = new FlexiblePoSDict(modelInfo.ExtraInfo);

            var ObservGen = new FlexibleGenerator(modelInfo.ExtraInfo);

            var trainSet = LoadTrainingInstanceFromDir(trainfn, PoSDict.TagDict, ObservGen);
            var heldoutSet = LoadTrainingInstanceFromDir(heldoutfn, PoSDict.TagDict, ObservGen);
            var testSet = LoadTrainingInstanceFromDir(testfn, PoSDict.TagDict, ObservGen);
            
            var apmodel = new AveragePerceptronModel(modelInfo.FeatureTemplateCount, updateThreshold);

            var decoder = new BigramChainDecoder(modelInfo, apmodel, beamsize);
            decoder.PA_C = 0.001;
            var rd = new Random();

            int TotalRound = 20;
            int bestRound = 0;
            int bestCorrect = 0;
            int burnIn = 0;

            float testAcc = 0;
            for (int r = 0; r < TotalRound; ++r)
            {
                if (r == burnIn)
                {
                    Console.Error.WriteLine("BurnIn...");
                    apmodel.Burn();
                }

                Console.Error.WriteLine("Training Round {0}: ", r);
                ConsoleTimer timer = new ConsoleTimer(100);

                apmodel.IsTraining = true;

                int[] indexArr = new int[trainSet.Length];

                for (int i = 0; i < trainSet.Length; ++i)
                {
                    indexArr[i] = i;
                }

                for (int i = 0; i < trainSet.Length - 1; ++i)
                {
                    int shid = rd.Next(trainSet.Length - i) + i;
                    int tmp = indexArr[i];
                    indexArr[i] = indexArr[shid];
                    indexArr[shid] = tmp;
                }

                for (int i = 0; i < trainSet.Length; ++i)
                {
                    var ts = trainSet[indexArr[i]];
                    List<FeatureUpdatePackage> fup;

                    if (!decoder.TrainMultiTag(ts.obs, null, ts.tags, out fup))//ts.possibleTags, ts.tags, out fup))
                    {
                        Console.Error.WriteLine("Fail to train!");
                    }
                    else
                    {
                        apmodel.Update(fup);

                        timer.Up();
                    }
                }

                timer.Finish();

                timer = new ConsoleTimer(100);
                Console.Error.WriteLine("Test on HeldOut...");
                apmodel.IsTraining = false;
                int total = 0;
                int correct = 0;
                MaltFileWriter mtfwdev = new MaltFileWriter(resultdevfn);
                foreach (TrainingSentence ts in heldoutSet)
                {
                    string[] outputTag;
                    decoder.RunMultiTag(ts.obs, ts.possibleTags, out outputTag);

                    for (int i = 0; i < ts.tags.Length; ++i)
                    {
                        if (!IsPunc(ts.obs[i][0]))
                        {
                            total++;
                            if (ts.tags[i] == outputTag[i])
                            {
                                correct++;
                            }
                        }
                    }

                    mtfwdev.Write(ts.tok, outputTag, ts.hid, ts.arc);
                    timer.Up();
                }
                timer.Finish();
                mtfwdev.Close();
                Console.Error.WriteLine("{0} {1} {2:F3}", total, correct, (float)correct / total * 100);
                if (bestCorrect < correct)
                {
                    bestCorrect = correct;
                    bestRound = r;
                    Console.Error.WriteLine("Save Model...");
                    modelInfo.WriteModel(outputfn, null, apmodel.GetAllFeatures());

                    timer = new ConsoleTimer(100);
                    Console.Error.WriteLine("Test on Test...");
                    apmodel.IsTraining = false;
                    int testtotal = 0;
                    int testcorrect = 0;
                    MaltFileWriter mtfw = new MaltFileWriter(resultfn);
                    foreach (TrainingSentence ts in testSet)
                    {
                        string[] outputTag;
                        decoder.RunMultiTag(ts.obs, ts.possibleTags, out outputTag);
                        for (int i = 0; i < ts.tags.Length; ++i)
                        {
                            if (!IsPunc(ts.obs[i][0]))
                            {
                                testtotal++;
                                if (ts.tags[i] == outputTag[i])
                                {
                                    testcorrect++;
                                }
                            }
                        }
                        mtfw.Write(ts.tok, outputTag, ts.hid, ts.arc);
                        timer.Up();
                    }
                    timer.Finish();
                    mtfw.Close();

                    testAcc = testcorrect / (float)testtotal;
                }

                Console.Error.WriteLine("TestAcc: {0:F3}", testAcc * 100);

                Console.Error.WriteLine("BestRound {0} Acc: {1:F3}", bestRound, (float)bestCorrect / total * 100);
            }
        }

        public void DoTrainBigramNFold()
        {
            string trainfn = Configure.GetOptionString("TrainDataDir");

            string outfn = Configure.GetOptionString("TaggedFile");

            string modelheadfn = Configure.GetOptionString("ModelHead");

            int beamsize = Configure.GetOptionInt("BeamSize", 5);
            int updateThreshold = 0;

            var modelInfo = new LinearChainModelInfo(modelheadfn);

            var PoSDict = new FlexiblePoSDict(modelInfo.ExtraInfo);

            var ObservGen = new FlexibleGenerator(modelInfo.ExtraInfo);

            var trainSets = LoadTrainingInstanceFromFiles(trainfn, PoSDict.TagDict, ObservGen);

            foreach (var ts in trainSets)
            {
                foreach (var s in ts.Value)
                {
                    s.AddConstraints(PoSDict.TagDict);
                }
            }

            int NFold = 10;

            
            int fold = Configure.GetOptionInt("FoldNum", 0);
            
                Console.Error.WriteLine("Training {0} fold..", fold);

                List<TrainingSentence> xtrainSet;
                List<TrainingSentence> xheldoutSet;

                MakeNFoldSet(NFold, fold, trainSets, out xtrainSet, out xheldoutSet);

                var trainSet = xtrainSet.ToArray();
                var heldoutSet = xheldoutSet.ToArray();

                var apmodel = new AveragePerceptronModel(modelInfo.FeatureTemplateCount, updateThreshold);

                var decoder = new BigramChainDecoder(modelInfo, apmodel, beamsize);

                var rd = new Random();

                int TotalRound = 10;

                for (int r = 0; r < TotalRound; ++r)
                {

                    Console.Error.WriteLine("Training Round {0}: ", r);
                    ConsoleTimer timer = new ConsoleTimer(1000);

                    apmodel.IsTraining = true;

                    int[] indexArr = new int[trainSet.Length];

                    for (int i = 0; i < trainSet.Length; ++i)
                    {
                        indexArr[i] = i;
                    }

                    for (int i = 0; i < trainSet.Length - 1; ++i)
                    {
                        int shid = rd.Next(trainSet.Length - i) + i;
                        int tmp = indexArr[i];
                        indexArr[i] = indexArr[shid];
                        indexArr[shid] = tmp;
                    }

                    for (int i = 0; i < trainSet.Length; ++i)
                    {
                        var ts = trainSet[indexArr[i]];
                        List<FeatureUpdatePackage> fup;

                        if (!decoder.TrainMultiTag(ts.obs, ts.possibleTags, ts.tags, out fup))
                        {
                            Console.Error.WriteLine("Fail to train!");
                        }
                        else
                        {
                            apmodel.Update(fup);

                            timer.Up();
                        }
                    }

                    timer.Finish();
                }

                //ConsoleTimer xtimer = new ConsoleTimer(100);
                Console.Error.WriteLine("Test on HeldOut...");
                apmodel.IsTraining = false;
                int total = 0;
                int correct = 0;

            MaltFileWriter mfw = new MaltFileWriter(outfn + fold.ToString());

            
                foreach (TrainingSentence ts in heldoutSet)
                {
                    string[] outputTag;

                    decoder.RunMultiTag(ts.obs, ts.possibleTags, out outputTag);

                    for (int i = 0; i < ts.tags.Length; ++i)
                    {
                        if (!IsPunc(ts.obs[i][0]))
                        {
                            total++;
                            if (ts.tags[i] == outputTag[i])
                            {
                                correct++;
                            }
                        }
                    }

                    mfw.Write(ts.tok, outputTag, ts.hid, ts.arc);
                    ts.autotags = outputTag;

                    //xtimer.Up();
                }
            mfw.Close();
                Console.Error.WriteLine("Test ACC: {0}", correct / (double)total);
                //xtimer.Finish();
            
            
        }

        public void GenerateMaxEntTrainingData()
        {
            string trainfn = Configure.GetOptionString("TrainDataDir");
            string modelheadfn = Configure.GetOptionString("ModelHead");

            string outputfn = Configure.GetOptionString("CRFData");

            var modelInfo = new LinearChainModelInfo(modelheadfn);

            var PoSDict = new FlexiblePoSDict(modelInfo.ExtraInfo);

            var ObservGen = new FlexibleGenerator(modelInfo.ExtraInfo);

            var trainSet = LoadTrainingInstanceFromDir(trainfn, PoSDict.TagDict, ObservGen);

            var model = new BasicLinearFunction(modelInfo.FeatureTemplateCount, modelInfo.LinearFuncPackages);

            var modelCache = new LinearChainModelCache(model, modelInfo.TagCount, modelInfo.Templates);

            using (StreamWriter sw = new StreamWriter(outputfn))
            {
                foreach (var trainsnt in trainSet)
                {
                    var obs = modelInfo.ModelVocab.ConvertToBinary(trainsnt.obs);
                    var tags = modelInfo.TagVocab.GetBinarizedTagWithPadding(trainsnt.tags);

                    modelCache.Clear();
                    modelCache.StartNextInstance(tags.Length);

                    for (int i = 1; i < tags.Length - 1; ++i)
                    {
                        var fs = modelCache.GetAllFeatures(obs, tags, i);
                        List<string> featstr = new List<string>();

                        foreach (var f in fs)
                        {
                            if (!f.IsValid)
                            {
                                continue;
                            }
                            featstr.Add(f.ToString());
                        }

                        if (featstr.Count > 0)
                        {
                            string featline = string.Format("{0} {1}",
                                tags[i], string.Join(" ", featstr));

                            sw.WriteLine(featline);
                        }
                    }
                }
            }
        }

        private void MakeNFoldSet(int N, int id,
            Dictionary<string, List<TrainingSentence>> Data,
            out List<TrainingSentence> trainSet,
            out List<TrainingSentence> heldOutSet
            )
        {
            trainSet = new List<TrainingSentence>();
            heldOutSet = new List<TrainingSentence>();

            foreach (var dset in Data)
            {
                int start;
                int end;

                GetHeldoutId(dset.Value.Count, N, id, out start, out end);

                for (int i = 0; i < dset.Value.Count; ++i)
                {
                    if (i >= start && i < end)
                    {
                        heldOutSet.Add(dset.Value[i]);
                    }
                    else
                    {
                        trainSet.Add(dset.Value[i]);
                    }
                }
            }
        }

        private static void GetHeldoutId(int Count, int N, int id, out int start, out int end)
        {
            int average = Count / N;

            start = id * average;

            end = id == N - 1 ? Count : id * average + average;
        }

        public void GenerateCRFTrainingData()
        {
            string trainfn = Configure.GetOptionString("TrainDataDir");
            string modelheadfn = Configure.GetOptionString("ModelHead");

            string outputfn = Configure.GetOptionString("CRFData");

            string dummytok = "DUMMY_TOK";

            var modelInfo = new LinearChainModelInfo(modelheadfn);

            var PoSDict = new FlexiblePoSDict(modelInfo.ExtraInfo);

            var ObservGen = new FlexibleGenerator(modelInfo.ExtraInfo);

            var trainSet = LoadTrainingInstanceFromDir(trainfn, PoSDict.TagDict, ObservGen);

            using (StreamWriter sw = new StreamWriter(outputfn))
            {
                foreach (var trainsnt in trainSet)
                {
                    for (int i = 0; i < trainsnt.tok.Length; ++i)
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (string ob in trainsnt.obs[i])
                        {
                            if (ob != null)
                            {
                                sb.Append(ob + "\t");
                            }
                            else
                            {
                                sb.Append(dummytok + "\t");
                            }
                        }

                        sb.Append(trainsnt.tags[i]);

                        sw.WriteLine(sb.ToString());
                    }

                    sw.WriteLine();
                }
            }
        }
        public void DoCounting()
        {
            string trainfn = Configure.GetOptionString("TrainDataDir");

            string modelheadfn = Configure.GetOptionString("ModelHead");

            var modelInfo = new LinearChainModelInfo(modelheadfn);

            var PoSDict = new FlexiblePoSDict(modelInfo.ExtraInfo);

            var ObservGen = new FlexibleGenerator(modelInfo.ExtraInfo);

            var data = LoadTrainingInstanceFromDir(trainfn, PoSDict.TagDict, ObservGen);

            int wc = 0;

            foreach (var s in data)
            {
                wc += s.tags.Length;
            }

            Console.Error.WriteLine("{0} {1}", wc, data.Length);


        }

        public void DoNFold()
        {
            string trainfn = Configure.GetOptionString("Train");
            //string heldoutfn = Configure.GetOptionString("Heldout");
            string modelheadfn = Configure.GetOptionString("ModelHead");

            string outputfn = Configure.GetOptionString("Model");

            string taggedmalt = Configure.GetOptionString("Output");

            int beamsize = Configure.GetOptionInt("BeamSize", 5);
            int updateThreshold = Configure.GetOptionInt("UpdateThreshold", 20);

            int NFold = Configure.GetOptionInt("NFold", 10);

            int FoldNum = Configure.GetOptionInt("FoldNum", 0);

            var PoSDict = new WSJPoSDict();
            var ObservGen = new WSJObservGenerator();
            var AllSet = LoadTrainingInstance(trainfn, PoSDict.TagDict, ObservGen);
            TrainingSentence[] trainSet;
            TrainingSentence[] heldoutSet;

            Split(AllSet, NFold, FoldNum, out trainSet, out heldoutSet);

            var modelInfo = new LinearChainModelInfo(modelheadfn);

            var apmodel = new AveragePerceptronModel(modelInfo.FeatureTemplateCount, updateThreshold);

            var decoder = new TrigramChainDecoder(modelInfo, apmodel, beamsize);

            var rd = new Random();

            int TotalRound = 100;
            int bestRound = 0;
            int bestCorrect = 0;
            int burnIn = 5;

            float testAcc = 0;
            for (int r = 0; r < TotalRound; ++r)
            {
                if (r == burnIn)
                {
                    Console.Error.WriteLine("BurnIn...");
                }

                Console.Error.WriteLine("Training Round {0}: ", r);
                ConsoleTimer timer = new ConsoleTimer(100);

                apmodel.IsTraining = true;

                int[] indexArr = new int[trainSet.Length];

                for (int i = 0; i < trainSet.Length; ++i)
                {
                    indexArr[i] = i;
                }

                for (int i = 0; i < trainSet.Length - 1; ++i)
                {
                    int shid = rd.Next(trainSet.Length - i) + i;
                    int tmp = indexArr[i];
                    indexArr[i] = indexArr[shid];
                    indexArr[shid] = tmp;
                }

                for (int i = 0; i < trainSet.Length; ++i)
                {
                    var ts = trainSet[indexArr[i]];
                    List<FeatureUpdatePackage> fup;

                    if (!decoder.TrainMultiTag(ts.obs, ts.possibleTags, ts.tags, out fup))
                    {
                        Console.Error.WriteLine("Fail to train!");
                    }
                    else
                    {
                        apmodel.Update(fup);

                        timer.Up();
                    }
                }

                timer.Finish();

                timer = new ConsoleTimer(100);
                Console.Error.WriteLine("Test on HeldOut...");
                apmodel.IsTraining = false;
                int total = 0;
                int correct = 0;
                foreach (TrainingSentence ts in heldoutSet)
                {
                    string[] outputTag;
                    decoder.RunMultiTag(ts.obs, ts.possibleTags, out outputTag);

                    for (int i = 0; i < ts.tags.Length; ++i)
                    {
                        if (!IsPunc(ts.obs[i][0]))
                        {
                            total++;
                            if (ts.tags[i] == outputTag[i])
                            {
                                correct++;
                            }
                        }
                    }
                    timer.Up();
                }
                timer.Finish();
                Console.Error.WriteLine("{0} {1} {2:F3}", total, correct, (float)correct / total * 100);
                if (bestCorrect < correct)
                {
                    bestCorrect = correct;
                    bestRound = r;
                    Console.Error.WriteLine("Save Model...");
                    modelInfo.WriteModel(outputfn + "." + FoldNum.ToString(), null, apmodel.GetAllFeatures());
                }

                Console.Error.WriteLine("TestAcc: {0:F3}", testAcc * 100);

                Console.Error.WriteLine("BestRound {0} Acc: {1:F3}", bestRound, (float)bestCorrect / total * 100);
            }
        }

        public void DoTest()
        {
            string testfn = Configure.GetOptionString("TestDataDir");

            string modelfn = Configure.GetOptionString("Model");

            int beamsize = Configure.GetOptionInt("BeamSize", 5);

            string outputfn = Configure.GetOptionString("OutputDataDir");

            var modelInfo = new LinearChainModelInfo(modelfn);

            var PoSDict = new FlexiblePoSDict(modelInfo.ExtraInfo);

            var ObservGen = new FlexibleGenerator(modelInfo.ExtraInfo);
            //new MTCaseSensitiveWordClusterObservGenerator();
            var testSets = LoadTrainingInstanceFromFiles(
                    testfn,
                    PoSDict.TagDict,
                    ObservGen);



            var tagmodel = new BasicLinearFunction(modelInfo.FeatureTemplateCount, modelInfo.LinearFuncPackages);
          
            var decoder = new LinearChainDecoder(modelInfo, tagmodel, beamsize);

            var rd = new Random();


            StreamWriter sw = new StreamWriter(Configure.GetOptionString("TestLog"));
            //ConsoleTimer timer = new ConsoleTimer(100);

            int total = 0;
            int correct = 0;

            int totalsnt = 0;
            int correctsnt = 0;

            ConsoleTimer timer = new ConsoleTimer();

            foreach (var key in testSets.Keys)
            {
                var testSet = testSets[key];

                int ftotal = 0;
                int fcorrect = 0;

                int ftotalsnt = 0;
                int fcorrectsnt = 0;
                foreach (TrainingSentence ts in testSet)
                {
                    timer.Up();
                    string[] outputTag;

                    ts.AddConstraints(PoSDict.TagDict);

                    decoder.RunMultiTag(ts.obs, ts.possibleTags, out outputTag);
                    totalsnt++;
                    ftotalsnt++;

                    bool error = false;

                    for (int i = 0; i < ts.tags.Length; ++i)
                    {
                        if (!IsPunc(ts.obs[i][0]))
                        {
                            total++;
                            ftotal++;
                            if (ts.tags[i] == outputTag[i])
                            {
                                correct++;
                                fcorrect++;
                            }
                            else
                            {
                                error = true;
                            }
                        }
                    }

                    if (!error)
                    {
                        correctsnt++;
                        fcorrectsnt++;
                    }

                    //timer.Up();

                }
                sw.WriteLine("{0}\t{1}\t{2}\t{3:F2}\t{4:F2}", key, ftotal, ftotalsnt, (float)fcorrect / ftotal * 100, (float)fcorrectsnt / ftotalsnt * 100);
            }
            timer.Finish();
            //timer.Finish();
            //sw.WriteLine("Overall:");
            //sw.WriteLine("{0} {1} {2:F3}", total, correct, (float)correct / total * 100);
            //sw.WriteLine("{0} {1} {2:F3}", totalsnt, correctsnt, (float)correctsnt / totalsnt * 100);
            sw.WriteLine("{0}\t{1}\t{2}\t{3:F2}\t{4:F2}", "Overall", total, totalsnt, (float)correct / total * 100, (float)correctsnt / totalsnt * 100);
            sw.Close();
        }

        public void DoTestBigram()
        {
            string testfn = Configure.GetOptionString("TestDataDir");

            string modelfn = Configure.GetOptionString("Model");

            int beamsize = Configure.GetOptionInt("BeamSize", 5);

            string outputfn = Configure.GetOptionString("OutputDataDir");

            var modelInfo = new LinearChainModelInfo(modelfn);

            var PoSDict = new FlexiblePoSDict(modelInfo.ExtraInfo);

            var ObservGen = new FlexibleGenerator(modelInfo.ExtraInfo);
                //new MTCaseSensitiveWordClusterObservGenerator();
            var testSets = LoadTrainingInstanceFromFiles(
                    testfn,
                    PoSDict.TagDict,
                    ObservGen);

            

            var tagmodel = new BasicLinearFunction(modelInfo.FeatureTemplateCount, modelInfo.LinearFuncPackages);

            var decoder = new BigramChainDecoder(modelInfo, tagmodel, beamsize);
            var rd = new Random();


            StreamWriter sw = new StreamWriter(Configure.GetOptionString("TestLog"));
            //ConsoleTimer timer = new ConsoleTimer(100);

            int total = 0;
            int correct = 0;

            int totalsnt = 0;
            int correctsnt = 0;

            foreach (var key in testSets.Keys)
            {
                MaltFileWriter mfw = new MaltFileWriter(Path.Combine(outputfn, key));
                var testSet = testSets[key];

                int ftotal = 0;
                int fcorrect = 0;

                int ftotalsnt = 0;
                int fcorrectsnt = 0;
                ConsoleTimer timer = new ConsoleTimer(100);
                foreach (TrainingSentence ts in testSet)
                {
                    string[] outputTag;

                    ts.AddConstraints(PoSDict.TagDict);

                    decoder.RunMultiTag(ts.obs, ts.possibleTags, out outputTag);
                    totalsnt++;
                    ftotalsnt++;
                    mfw.Write(ts.tok, outputTag, ts.hid, ts.arc);

                    bool error = false;

                    for (int i = 0; i < ts.tags.Length; ++i)
                    {
                        if (!IsPunc(ts.obs[i][0]))
                        {
                            total++;
                            ftotal++;
                            if (ts.tags[i] == outputTag[i])
                            {
                                correct++;
                                fcorrect++;
                            }
                            else
                            {
                                error = true;
                            }
                        }
                    }

                    if (!error)
                    {
                        correctsnt++;
                        fcorrectsnt++;
                    }

                    //timer.Up();

                    timer.Up();
                }
                timer.Finish();
                mfw.Close();
                sw.WriteLine("{0}\t{1}\t{2}\t{3:F2}\t{4:F2}", key, ftotal, ftotalsnt, (float)fcorrect / ftotal * 100, (float)fcorrectsnt / ftotalsnt * 100);
            }
            //timer.Finish();
            //sw.WriteLine("Overall:");
            //sw.WriteLine("{0} {1} {2:F3}", total, correct, (float)correct / total * 100);
            //sw.WriteLine("{0} {1} {2:F3}", totalsnt, correctsnt, (float)correctsnt / totalsnt * 100);
            sw.WriteLine("{0}\t{1}\t{2}\t{3:F2}\t{4:F2}", "Overall", total, totalsnt, (float)correct / total * 100, (float)correctsnt / totalsnt * 100);
            sw.Close();
        }

        private bool IsPunc(string token)
        {
            return false;
        }

        private void Split(TrainingSentence[] All, int N, int foldN, out TrainingSentence[] train, out TrainingSentence[] heldout)
        {
            var trainlist = new List<TrainingSentence>();
            var testlist = new List<TrainingSentence>();

            for (int i = 0; i < All.Length; ++i)
            {
                if (i % N == foldN)
                {
                    testlist.Add(All[i]);
                }
                else
                {
                    trainlist.Add(All[i]);
                }
            }

            train = trainlist.ToArray();
            heldout = testlist.ToArray();
        }

        private TrainingSentence[] LoadTrainingInstance(
            string fn,
            Dictionary<string, List<string>> tagDict,
            IObservGenerator obGen)
        {
            MaltTabFileReader mtr = new MaltTabFileReader(fn);

            var trainlist = new List<TrainingSentence>();
            ConsoleTimer timer = new ConsoleTimer(100);

            while (!mtr.EndOfStream)
            {
                ParserSentence ps;

                if (mtr.GetNextSent(out ps))
                {
                    var ob = obGen.GenerateObserv(ps.tok);

                    var transnt = new TrainingSentence(ob, ps.pos);

                    transnt.tok = ps.tok;
                    transnt.hid = ps.hid;
                    transnt.arc = ps.label;
                    //transnt.possibleTags = new List<string>[ob.Length];

                    //for (int i = 0; i < ob.Length; ++i)
                    //{
                    //    List<string> tagCand;
                    //    if (tagDict.TryGetValue(ob[i][0], out tagCand))
                    //    {
                    //        transnt.possibleTags[i] = tagCand;
                    //    }
                    //}

                    trainlist.Add(transnt);
                    timer.Up();
                }
            }
            timer.Finish();
            return trainlist.ToArray();
        }

        private TrainingSentence[] LoadTrainingInstanceFromDir(
            string path,
            Dictionary<string, List<string>> tagDict,
            IObservGenerator obGen)
        {
            

            var trainlist = new List<TrainingSentence>();
            ConsoleTimer timer = new ConsoleTimer(100);

            DirectoryInfo dir = new DirectoryInfo(path);

            foreach (var file in dir.GetFiles("*.*"))
            {

                MaltTabFileReader mtr = new MaltTabFileReader(file.FullName);

                mtr.SingleLineMode = true;
                while (!mtr.EndOfStream)
                {
                    ParserSentence ps;

                    if (mtr.GetNextSent(out ps))
                    {
                        var ob = obGen.GenerateObserv(ps.tok);

                        var transnt = new TrainingSentence(ob, ps.pos);

                        transnt.tok = ps.tok;
                        transnt.hid = ps.hid;
                        transnt.arc = ps.label;
                        transnt.possibleTags = new List<string>[ob.Length];

                        for (int i = 0; i < ob.Length; ++i)
                        {
                            if (ob[i][0] == null)
                            {
                                continue;
                            }
                            List<string> tagCand;
                            if (tagDict.TryGetValue(ob[i][0], out tagCand))
                            {
                                transnt.possibleTags[i] = tagCand;
                            }
                        }

                        trainlist.Add(transnt);
                        timer.Up();
                    }
                }
                mtr.Close();

            }
            timer.Finish();
            return trainlist.ToArray();
        }

        private Dictionary<string, List<TrainingSentence>> LoadTrainingInstanceFromFiles(
           string path,
           Dictionary<string, List<string>> tagDict,
           IObservGenerator obGen)
        {

            var traindata = new Dictionary<string, List<TrainingSentence>>();
            
            ConsoleTimer timer = new ConsoleTimer(100);

            DirectoryInfo dir = new DirectoryInfo(path);

            foreach (var file in dir.GetFiles("*.malt"))
            {
                var trainlist = new List<TrainingSentence>();
                MaltTabFileReader mtr = new MaltTabFileReader(file.FullName);
                
                while (!mtr.EndOfStream)
                {
                    ParserSentence ps;

                    if (mtr.GetNextSent(out ps))
                    {
                        var ob = obGen.GenerateObserv(ps.tok);

                        var transnt = new TrainingSentence(ob, ps.pos);

                        transnt.possibleTags = new List<string>[ob.Length];

                        for (int i = 0; i < ob.Length; ++i)
                        {
                            if (ob[i][0] == null)
                            {
                                continue;
                            }
                            List<string> tagCand;
                            if (tagDict.TryGetValue(ob[i][0], out tagCand))
                            {
                                transnt.possibleTags[i] = tagCand;
                            }
                        }

                        transnt.tok = ps.tok;
                        transnt.hid = ps.hid;
                        transnt.arc = ps.label;

                        trainlist.Add(transnt);
                        timer.Up();
                    }
                }
                mtr.Close();

                traindata[file.Name] = trainlist;

            }
            timer.Finish();
            return traindata;
        }


        private void RuleBasedFixVBD(string[] tags, string[] toks, string[] reftags)
        {
            for (int i = 0; i < tags.Length; ++i)
            {
                if (tags[i] == "VBD")
                {
                    for (int j = i - 1; j >= 0; --j)
                    {
                        string t = tags[j];
                        if (t == "DT"
                            || t.StartsWith("J")
                            || (t.StartsWith("N") && (t != "NFP"))
                            || t == "CD"
                            )
                        {
                            continue;
                        }
                        else if ((t == "VB" || t == "VBZ"
                            || t == "VBP") && toks[j].ToLower() != "please")
                        {
                            tags[i] = "VBN";
                            if (reftags[i] == "VBD")
                            {
                                fixLoss++;
                            }
                            else if (reftags[i] == "VBN")
                            {
                                fixGain++;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
        }

        private int fixGain = 0;
        private int fixLoss = 0;
    }
}
