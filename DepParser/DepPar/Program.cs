using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NanYUtilityLib;
using NanYUtilityLib.DepParUtil;
using NanYUtilityLib.Sweets;
using LinearDepParser;
//using MSRA.NLC.Lingo.NLP;
using ParserUtil;
using System.Threading;
using System.Threading.Tasks;


namespace DepPar
{
    class Program
    {
        static void Main(string[] args)
        {
            Configure.SetArgs(args);

            //TestCombine();
            //return;
            
            string parserType = Configure.GetOptionString("ParserType");
            bool isInteractive = Configure.GetOptionBool("Interactive", false);
            bool isManualTag = Configure.GetOptionBool("ManualTag", false);
            bool isTest = Configure.GetOptionBool("Test", false);
            bool isCombine = Configure.GetOptionBool("Combine", false);
            bool isChinese = Configure.GetOptionBool("IsChinese", false);

            if (isTest)
            {
                Test();
                return;
            }

            if (isInteractive)
            {
                if (isCombine)
                {
                    ParseConsoleInputCombine();
                }
                else
                {
                    ParseConsoleG2();
                }
            }
            else if (isManualTag)
            {
                ManualTag();
            }
            else
            {
                if (isCombine)
                {
                    ParseDataFileCombine();
                }
                else
                {
                    ParseDataFile();
                }
            }
        }

        private static void ParseConsoleInput()
        {
            string parserfn = Configure.GetOptionString("Parser");
            string taggerfn = Configure.GetOptionString("Tagger");
            int taggerbeam = Configure.GetOptionInt("TaggerBeam", 8);
            int parserbeam = Configure.GetOptionInt("ParserBeam", 8);

            bool isBidir = Configure.GetOptionBool("IsBidir", false);
            bool useLightTokenizer = Configure.GetOptionBool("UseLightTokenizor", false);

            bool tolower = Configure.GetOptionBool("ToLower", false);
            DepParModelWrapper dpm = new DepParModelWrapper(taggerfn, parserfn);

            PoSTag.IObservGenerator observGen =
                new PoSTag.MTCaseSensitiveWordClusterObservGenerator();

            bool useTagDict = Configure.GetOptionBool("UseTagDict", false);

            DepParDecoderWrapper decoder = new DepParDecoderWrapper(dpm, observGen, taggerbeam, parserbeam, new PoSTag.MTWSJPoSDict());
            bool tagonly = Configure.GetOptionBool("TagOnly", false);

            Console.Error.WriteLine("Ready.");
            string line;
            while ((line = Console.ReadLine()) != null)//sr.EndOfStream)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    Console.Error.WriteLine("Empty Input!");
                }
                else
                {
                    string[] toks =
                        line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    if (tolower)
                    {
                        for (int i = 0; i < toks.Length; ++i)
                        {
                            toks[i] = toks[i].ToLower();
                        }
                    }

                    string[] pos;
                    int[][] hid;
                    string[][] label;
                    if (tagonly)
                    {
                        List<string>[] preTags = new List<string>[toks.Length];

                        if (!decoder.Tag(toks, preTags, out pos))
                        {
                            Console.Error.WriteLine("Fail To Parse!");
                        }
                        else
                        {
                            for (int i = 0; i < toks.Length; ++i)
                            {
                                Console.WriteLine("{0}\t{1}", toks[i], pos[i]);
                            }
                        }
                    }
                    else
                    {
                        if (!decoder.ParseNBest(toks, out pos, out hid, out label))
                        {
                            Console.Error.WriteLine("Fail To Parse!");
                        }
                        else
                        {
                            for (int i = 0; i < hid.Length && i < 4; ++i)
                            {
                                DepTree dtree = new DepTree(toks, pos, hid[i], label[i]);
                                CTree ctree = new CTree(dtree);
                                string treeline = ctree.GetTxtTree();
                                Console.WriteLine(treeline);
                            }
                        }
                    }
                }
            }
        }

        private static void ParseConsoleG2()
        {
            string MST2Gparserfn = Configure.GetOptionString("Parser-G2");
            string taggerfn = Configure.GetOptionString("Tagger");
            int taggerbeam = Configure.GetOptionInt("TaggerBeam", 8);
            int parserbeam = Configure.GetOptionInt("ParserBeam", 8);

            bool tolower = Configure.GetOptionBool("ToLower", false);

            PoSTag.IObservGenerator observGen =
                new PoSTag.MTCaseSensitiveWordClusterObservGenerator();

            bool useTagDict = Configure.GetOptionBool("UseTagDict", false);

            PoSTag.PoSTagModelWrapper tagModel = new PoSTag.PoSTagModelWrapper(taggerfn);

            PoSTag.PoSTagDecoderWrapper tagger
                = new PoSTag.PoSTagDecoderWrapper(
                    tagModel, taggerbeam, observGen,
                    new PoSTag.MTWSJPoSDict());

            var G2ParserModel = new HOGParser.HOGParserModelWrapper(MST2Gparserfn);

            var G2Decoder = new SecondOrderGParser.SecondOrderDecoder(G2ParserModel.pmInfo, G2ParserModel.parserModel);

            Console.Error.WriteLine("Ready.");
            string line;
            while ((line = Console.ReadLine()) != null)//sr.EndOfStream)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    Console.Error.WriteLine("Empty Input!");
                }
                else
                {
                    string[] toks = //useLightTokenizer ? tokenizor.Tokenize(line.Trim()) : line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                        line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    if (tolower)
                    {
                        for (int i = 0; i < toks.Length; ++i)
                        {
                            toks[i] = toks[i].ToLower();
                        }
                    }

                    string[] pos;


                    if (!tagger.GenPOS(toks, out pos))
                    {
                        Console.Error.WriteLine("Fail To Parse!");
                    }
                    else
                    {
                        int[][] G2Hid;

                        string[] G2Label;

                        //string[][] xlablsArr;
                        //int[][] xhidArr;
                        //LRDecoder.Run(toks, pos, out xhidArr, out xlablsArr);

                        //for(int n = 0; n < xlablsArr.Length; ++n)
                        //{
                        //    string treel = new CTree(toks, pos, xhidArr[n], xlablsArr[n]).GetTxtTree();
                        //    Console.Error.WriteLine(
                        //        treel
                        //        );
                        //}

                        int[] G2HidOneBest;

                        if (!G2Decoder.Run(toks, pos, out G2HidOneBest))
                        {
                            Console.Error.WriteLine("Fail to Parse!");
                        }
                        else
                        {
                            G2Label = new string[toks.Length];

                            for (int i = 0; i < G2Label.Length; ++i)
                            {
                                G2Label[i] = "dep";
                            }

                            Console.Error.WriteLine(new CTree(toks, pos, G2HidOneBest, G2Label).GetTxtTree());
                        }

                        if (!G2Decoder.RunKBest(toks, pos, parserbeam, out G2Hid))
                        {
                            Console.Error.WriteLine("Fail To Parse!");
                        }
                        else
                        {
                            G2Label = new string[toks.Length];

                            for (int i = 0; i < G2Label.Length; ++i)
                            {
                                G2Label[i] = "dep";
                            }

                            List<ParserSentence> sentenceList = new List<ParserSentence>();

                            for (int i = 0; i < G2Hid.Length; ++i)
                            {
                                Console.WriteLine(new CTree(toks, pos, G2Hid[i], G2Label).GetTxtTree());
                            }
                        }
                    }



                }
                //timer.Up();
            }

            //timer.Finish();
            //sr.Close();
            //mtfw.Close();
        }

        private static void ParseConsoleInputCombine()
        {
            string LRparserfn = Configure.GetOptionString("Parser-LR");

            string RLparserfn = Configure.GetOptionString("Parser-RL");
            string EFparserfn = Configure.GetOptionString("Parser-EF");
            string MSTparserfn = Configure.GetOptionString("Parser-MST");
            string MST2Gparserfn = Configure.GetOptionString("Parser-G2");
            string taggerfn = Configure.GetOptionString("Tagger");
            int taggerbeam = Configure.GetOptionInt("TaggerBeam", 8);
            int parserbeam = Configure.GetOptionInt("ParserBeam", 8);

            bool tolower = Configure.GetOptionBool("ToLower", false);
            
            PoSTag.IObservGenerator observGen =
                new PoSTag.MTCaseSensitiveWordClusterObservGenerator();

            bool useTagDict = Configure.GetOptionBool("UseTagDict", false);

            PoSTag.PoSTagModelWrapper tagModel = new PoSTag.PoSTagModelWrapper(taggerfn);
            
            PoSTag.PoSTagDecoderWrapper tagger 
                = new PoSTag.PoSTagDecoderWrapper(
                    tagModel, taggerbeam,observGen,
                    new PoSTag.MTWSJPoSDict());

            LinearDepParser.ParserModelWrapper LRParserModel = new ParserModelWrapper(LRparserfn);
            LinearDepParser.ParserDecoder LRDecoder 
                = new ParserDecoder(LRParserModel.pmInfo, LRParserModel.parserModel, parserbeam);


            LinearDepParser.ParserModelWrapper RLParserModel = new ParserModelWrapper(RLparserfn);
            LinearDepParser.ParserDecoder RLDecoder
                = new ParserDecoder(RLParserModel.pmInfo, RLParserModel.parserModel, parserbeam);


            EasyFirstDepPar.EasyFirstParserModelWrapper EFParserModel = new EasyFirstDepPar.EasyFirstParserModelWrapper(EFparserfn);
            EasyFirstDepPar.EasyParDecoder EFDecoder =
                new EasyFirstDepPar.EasyParDecoder(EFParserModel.pmInfo, EFParserModel.parserModel);

            MSTParser.MSTParserModelWrapper MSTParserModel = new MSTParser.MSTParserModelWrapper(MSTparserfn);
            MSTParser.MSTDecoder MSTDecoder = new MSTParser.MSTDecoder(MSTParserModel.pmInfo, MSTParserModel.parserModel);

            var G2ParserModel = new HOGParser.HOGParserModelWrapper(MST2Gparserfn);

            var G2Decoder = new SecondOrderGParser.SecondOrderDecoder(G2ParserModel.pmInfo, G2ParserModel.parserModel);

            ParserCombinator combinator = new ParserCombinator();

            Console.Error.WriteLine("Ready.");
            string line;
            while ((line = Console.ReadLine()) != null)//sr.EndOfStream)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    Console.Error.WriteLine("Empty Input!");
                }
                else
                {
                    string[] toks = //useLightTokenizer ? tokenizor.Tokenize(line.Trim()) : line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                        line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    if (tolower)
                    {
                        for (int i = 0; i < toks.Length; ++i)
                        {
                            toks[i] = toks[i].ToLower();
                        }
                    }

                    string[] pos;
                    

                    if (!tagger.GenPOS(toks, out pos))
                    {
                        Console.Error.WriteLine("Fail To Parse!");
                    }
                    else
                    {
                        int[] LRHid;
                        int[] RLHid;
                        int[] EFHid;
                        int[] MSTHid;
                        int[] G2Hid;

                        string[] LRLabel;
                        string[] RLLabel;
                        string[] EFLabel;
                        string[] MSTLabel;
                        string[] G2Label;

                        string[] rtok = toks.Reverse<string>().ToArray<string>();
                        string[] rpos = pos.Reverse<string>().ToArray<string>();

                        //string[][] xlablsArr;
                        //int[][] xhidArr;
                        //LRDecoder.Run(toks, pos, out xhidArr, out xlablsArr);

                        //for(int n = 0; n < xlablsArr.Length; ++n)
                        //{
                        //    string treel = new CTree(toks, pos, xhidArr[n], xlablsArr[n]).GetTxtTree();
                        //    Console.Error.WriteLine(
                        //        treel
                        //        );
                        //}

                        if (!LRDecoder.Run(toks, pos, out LRHid, out LRLabel)
                            || !RLDecoder.Run(rtok, rpos, out RLHid, out RLLabel)
                            || !EFDecoder.RunWithOrder(toks, pos, out EFHid, out EFLabel)
                            || !MSTDecoder.Run(toks, pos, out MSTHid, out MSTLabel)
                            || !G2Decoder.Run(toks, pos, out G2Hid))
                        {
                            Console.Error.WriteLine("Fail To Parse!");
                        }
                        else
                        {
                            G2Label = new string[G2Hid.Length];

                            for(int i= 0; i < G2Label.Length; ++i)
                            {
                                G2Label[i] = "dep";
                            }

                            List<ParserSentence> sentenceList = new List<ParserSentence>();
                            sentenceList.Add(new ParserSentence(toks, pos, LRHid, LRLabel));
                            sentenceList.Add(
                                new ParserSentence(rtok, rpos,
                                    RLHid,
                                    RLLabel
                                    ).Reverse());
                            sentenceList.Add(new ParserSentence(toks, pos, EFHid, EFLabel));
                            sentenceList.Add(new ParserSentence(toks, pos, MSTHid, MSTLabel));
                            sentenceList.Add(new ParserSentence(toks, pos, G2Hid, G2Label));

                            //float lrbest;
                            //float rlbest;
                            //float efbest;
                            //float mstbest;

                            //LRDecoder.Evaluate(toks, pos, LRHid, LRLabel, out lrbest);
                            //LRDecoder.Evaluate(toks, pos, sentenceList[1].hid, sentenceList[1].label, out rlbest);
                            //LRDecoder.Evaluate(toks, pos, EFHid, EFLabel, out efbest);
                            //LRDecoder.Evaluate(toks, pos, MSTHid, MSTLabel, out mstbest);

                            Console.WriteLine(new CTree(toks, pos, LRHid, LRLabel).GetTxtTree());
                            Console.WriteLine(new CTree(toks, pos, sentenceList[1].hid, sentenceList[1].label).GetTxtTree());

                            Console.WriteLine(new CTree(toks, pos, EFHid, EFLabel).GetTxtTree());

                            Console.WriteLine(new CTree(toks, pos, MSTHid, MSTLabel).GetTxtTree());

                            Console.WriteLine(new CTree(toks, pos, G2Hid, G2Label).GetTxtTree());

                            //string[] preLabel = new string[toks.Length];
                            //int[] preHid = new int[toks.Length];
                            //string[] preTags = new string[toks.Length];
                            //bool[] isTagged = new bool[toks.Length];
                            //for (int i = 0; i < preHid.Length; ++i)
                            //{
                            //    preHid[i] = -1;
                            //}

                            //float newScore;

                            //while (ProcessConsoleInput(isTagged, preTags, preHid, preLabel))
                            //{
                                
                            //    ParserForcedConstraints pfcs = new ParserForcedConstraints(LRParserModel.vocab, preHid, preLabel);
                            //    if (LRDecoder.Run(toks, pos, pfcs, out LRHid, out LRLabel))
                            //    {
                            //        Console.WriteLine(new CTree(toks, pos, LRHid, LRLabel).GetTxtTree());
                                    
                            //        LRDecoder.Evaluate(toks, pos, LRHid, LRLabel, out newScore);
                            //        Console.Error.WriteLine("{0}", newScore);
                            //    }
                            //}

                            float[] weights = { 0.921f, 0.92f, 0.901f, 0.90f, 0.91f };
                            ParserSentence combined;
                            combinator.Combine(sentenceList.ToArray(), weights, out combined);
                            DepTree dtree = new DepTree(toks, pos, combined.hid, combined.label);
                            CTree ctree = new CTree(dtree);
                            Console.WriteLine(ctree.GetTxtTree());

                        }
                    }
                    
                        
                    
                }
                //timer.Up();
            }

            //timer.Finish();
            //sr.Close();
            //mtfw.Close();
        }

        private static void TestMST()
        {
            string MSTparserfn = Configure.GetOptionString("Parser-MST");

            string taggerfn = Configure.GetOptionString("Tagger");

            PoSTag.IObservGenerator observGen = //new PoSTag.MTCaseSensitiveObservGenerator();
                new PoSTag.MTCaseSensitiveWordClusterObservGenerator();

            bool useTagDict = Configure.GetOptionBool("UseTagDict", false);

            string inputfn = Configure.GetOptionString("Input");

            PoSTag.PoSTagModelWrapper tagModel = new PoSTag.PoSTagModelWrapper(taggerfn);

            PoSTag.PoSTagDecoderWrapper tagger
                = new PoSTag.PoSTagDecoderWrapper(
                    tagModel, 5, observGen,
                    new PoSTag.MTWSJPoSDict());
            string[] dummy;
            tagger.GenPOS(new string[] { "Test", "Test" }, out dummy);

            
            MSTParser.MSTParserModelWrapper MSTParserModel = new MSTParser.MSTParserModelWrapper(MSTparserfn);
            MSTParser.MSTDecoder MSTDecoder = new MSTParser.MSTDecoder(MSTParserModel.pmInfo, MSTParserModel.parserModel);


            
            Console.Error.WriteLine("Ready.");



            MaltTabFileReader matr = new MaltTabFileReader(inputfn);
            ConsoleTimer timer = new ConsoleTimer(100);
            while (!matr.EndOfStream)//sr.EndOfStream)
            {
                ParserSentence refsnt;
                if (!matr.GetNextSent(out refsnt))
                {
                    continue;
                }
                else
                {

                    string[] toks = refsnt.tok;
                    //toks = "This is wrong".Split();
                    string[] pos;// = refsnt.pos;

                    timer.Up();
                    if (
                        !tagger.GenPOS(toks, out pos))
                        //false)//!tagger.GenPOS(toks, out pos))
                    {
                        Console.Error.WriteLine("Fail To Parse!");
                    }
                    else
                    {
                        
                        int[] MSTHid = null;
                        string[] MSTLabel = null;
                        
                        
                                MSTDecoder.Run(toks, pos, out MSTHid, out MSTLabel);

                           //     Console.Error.WriteLine(new CTree(toks, pos, MSTHid, MSTLabel).GetTxtTree());
                        //if (!LRDecoder.Run(toks, pos, out LRHid, out LRLabel)
                        //    || !RLDecoder.Run(rtok, rpos, out RLHid, out RLLabel)
                        //    || !EFDecoder.Run(toks, pos, out EFHid, out EFLabel))
                        
                    }



                }
                //timer.Up();
            }
            timer.Finish();
            matr.Close();

        }

        private static void TestCombine()
        {
            string inputfn = Configure.GetOptionString("Input");
            //string outputn = Configure.GetOptionString("Output");
            string LRparserfn = Configure.GetOptionString("Parser-LR");

            string RLparserfn = Configure.GetOptionString("Parser-RL");
            string EFparserfn = Configure.GetOptionString("Parser-EF");
            string MSTparserfn = Configure.GetOptionString("Parser-MST");
            string MST2Gparserfn = Configure.GetOptionString("Parser-G2");
            string taggerfn = Configure.GetOptionString("Tagger");
            int taggerbeam = Configure.GetOptionInt("TaggerBeam", 8);
            int parserbeam = Configure.GetOptionInt("ParserBeam", 8);

            bool tolower = Configure.GetOptionBool("ToLower", false);

            PoSTag.IObservGenerator observGen =
                new PoSTag.MTCaseSensitiveWordClusterObservGenerator();

            bool useTagDict = Configure.GetOptionBool("UseTagDict", false);

            PoSTag.PoSTagModelWrapper tagModel = new PoSTag.PoSTagModelWrapper(taggerfn);

            PoSTag.PoSTagDecoderWrapper tagger
                = new PoSTag.PoSTagDecoderWrapper(
                    tagModel, taggerbeam, observGen,
                    new PoSTag.MTWSJPoSDict());

            string[] dummy;
            tagger.GenPOS(new string[] { "dummy" }, out dummy);

            LinearDepParser.ParserModelWrapper LRParserModel = new ParserModelWrapper(LRparserfn);
            LinearDepParser.ParserDecoder LRDecoder
                = new ParserDecoder(LRParserModel.pmInfo, LRParserModel.parserModel, parserbeam);


            LinearDepParser.ParserModelWrapper RLParserModel = new ParserModelWrapper(RLparserfn);
            LinearDepParser.ParserDecoder RLDecoder
                = new ParserDecoder(RLParserModel.pmInfo, RLParserModel.parserModel, parserbeam);


            EasyFirstDepPar.EasyFirstParserModelWrapper EFParserModel = new EasyFirstDepPar.EasyFirstParserModelWrapper(EFparserfn);
            EasyFirstDepPar.EasyParDecoder EFDecoder =
                new EasyFirstDepPar.EasyParDecoder(EFParserModel.pmInfo, EFParserModel.parserModel);

            MSTParser.MSTParserModelWrapper MSTParserModel = new MSTParser.MSTParserModelWrapper(MSTparserfn);
            MSTParser.MSTDecoder MSTDecoder = new MSTParser.MSTDecoder(MSTParserModel.pmInfo, MSTParserModel.parserModel);

            var G2ParserModel = new HOGParser.HOGParserModelWrapper(MST2Gparserfn);

            var G2Decoder = new SecondOrderGParser.SecondOrderDecoder(G2ParserModel.pmInfo, G2ParserModel.parserModel);

            ParserCombinator combinator = new ParserCombinator();


            bool isparallel = Configure.GetOptionBool("IsParallel", false);

            Console.Error.WriteLine("Ready.");

            

            MaltTabFileReader matr = new MaltTabFileReader(inputfn);

            
            int total = 0;
            int lrcorrect = 0;
            int rlcorrect = 0;
            int efcorrect = 0;
            int mstcorrect = 0;
            int g2correct = 0;

            int lrlabl = 0;
            int rllabl = 0;
            int eflabl = 0;
            int mstlabl = 0;
            int comblabl = 0;

            int combcorrect = 0;
            ConsoleTimer timer = new ConsoleTimer(100);
            while (!matr.EndOfStream)//sr.EndOfStream)
            {
                ParserSentence refsnt;
                if (!matr.GetNextSent(out refsnt))
                {
                    continue;
                }
                else
                {
                    timer.Up();
                    string[] toks = refsnt.tok;

                    if (tolower)
                    {
                        for (int i = 0; i < toks.Length; ++i)
                        {
                            toks[i] = toks[i].ToLower();
                        }
                    }

                    string[] pos;

                    total += toks.Length;

                    if (!tagger.GenPOS(toks, out pos))
                    {
                        Console.Error.WriteLine("Fail To Parse!");
                    }
                    else
                    {
                        for (int i = 0; i < pos.Length; ++i)
                        {
                            pos[i] = refsnt.pos[i];
                        }

                        int[] LRHid = null ;
                        int[] RLHid = null;
                        int[] EFHid = null;
                        int[] MSTHid = null;
                        int[] G2Hid = null;
                        string[] LRLabel = null;
                        string[] RLLabel = null;
                        string[] EFLabel = null;
                        string[] MSTLabel = null;
                        string[] G2Label = null;
                        string[] rtok = toks.Reverse<string>().ToArray<string>();
                        string[] rpos = pos.Reverse<string>().ToArray<string>();
                        if (isparallel)
                        {
                            Parallel.For(0, 4, (i) =>
                                {
                                    if (i == 0)
                                    {
                                        LRDecoder.Run(toks, pos, out LRHid, out LRLabel);
                                    }
                                    else if (i == 1)
                                    {
                                        RLDecoder.Run(rtok, rpos, out RLHid, out RLLabel);
                                    }
                                    else if (i == 2)
                                    {
                                        EFDecoder.Run(toks, pos, out EFHid, out EFLabel);
                                    }
                                    else if (i == 3)
                                    {
                                        MSTDecoder.Run(toks, pos, out MSTHid, out MSTLabel);
                                    }
                                });
                        }
                        else
                        {
                            
                                LRDecoder.Run(toks, pos, out LRHid, out LRLabel);
                            
                                RLDecoder.Run(rtok, rpos, out RLHid, out RLLabel);
                            
                                EFDecoder.Run(toks, pos, out EFHid, out EFLabel);
                            
                                MSTDecoder.Run(toks, pos, out MSTHid, out MSTLabel);

                                G2Decoder.Run(toks, pos, out G2Hid);

                            G2Label = new string[G2Hid.Length];

                            for (int i = 0; i < G2Label.Length; ++i)
                            {
                                G2Label[i] = "dep";
                            }
                            
                        }
                        //if (!LRDecoder.Run(toks, pos, out LRHid, out LRLabel)
                        //    || !RLDecoder.Run(rtok, rpos, out RLHid, out RLLabel)
                        //    || !EFDecoder.Run(toks, pos, out EFHid, out EFLabel))
                        
                            List<ParserSentence> sentenceList = new List<ParserSentence>();
                            sentenceList.Add(new ParserSentence(toks, pos, LRHid, LRLabel));

                            sentenceList.Add(
                                new ParserSentence(rtok, rpos,
                                    RLHid,
                                    RLLabel
                                    ).Reverse());


                            sentenceList.Add(new ParserSentence(toks, pos, EFHid, EFLabel));

                            sentenceList.Add(new ParserSentence(toks, pos, MSTHid, MSTLabel));

                            sentenceList.Add(new ParserSentence(toks, pos, G2Hid, G2Label));


                            float[] weights = { 0.92f, 0.915f, 0.906f, 0.905f, 0.912f };//}; //, 0.9f};
                               // { 0.92f, 0.91f, 0.9f, 0.88f };


                            ParserSentence combined;
                            if (!combinator.Combine(sentenceList.ToArray(), weights, out combined))
                            {
                                Console.Error.WriteLine("!!!");
                            }
                            
                            if (combined == null)
                            {
                                combined = sentenceList[0];
                            }

                            
                            for (int i = 0; i < refsnt.Length; ++i)
                            {
                                if (IsPunc(refsnt.tok[i]))
                                {
                                    total--;
                                }
                                else
                                {
                                    if (refsnt.hid[i] == sentenceList[0].hid[i])
                                    {
                                        lrcorrect++;
                                        if (refsnt.label[i] == sentenceList[0].label[i])
                                        {
                                            lrlabl++;
                                        }
                                    }
                                    if (refsnt.hid[i] == sentenceList[1].hid[i])
                                    {
                                        rlcorrect++;
                                        if (refsnt.label[i] == sentenceList[1].label[i])
                                        {
                                            rllabl++;
                                        }
                                    }

                                    if (refsnt.hid[i] == sentenceList[2].hid[i])
                                    {
                                        efcorrect++;
                                        if (refsnt.label[i] == sentenceList[2].label[i])
                                        {
                                            eflabl++;
                                        }
                                    }

                                    if (refsnt.hid[i] == sentenceList[3].hid[i])
                                    {
                                        mstcorrect++;
                                        if (refsnt.label[i] == sentenceList[3].label[i])
                                        {
                                            mstlabl++;
                                        }
                                    }

                                    if (refsnt.hid[i] == G2Hid[i])
                                    {
                                        g2correct++;
                                    }

                                    if (refsnt.hid[i] == combined.hid[i])
                                    {
                                        combcorrect++;
                                        if (refsnt.label[i] == combined.label[i])
                                        {
                                            comblabl++;
                                        }
                                    }

                                    
                                }
                            }
                        }
                    }



                
                //timer.Up();
            }
            timer.Finish();
            matr.Close();

            Console.Error.WriteLine("LR: {0:F3}%", lrcorrect / (float)total * 100);
            Console.Error.WriteLine("RL: {0:F3}%", rlcorrect / (float)total * 100);
            Console.Error.WriteLine("EF: {0:F3}%", efcorrect / (float)total * 100);
            Console.Error.WriteLine("MST: {0:F3}%", mstcorrect / (float)total * 100);
            Console.Error.WriteLine("G2: {0:F3}%", g2correct / (float)total * 100);
            Console.Error.WriteLine("CB: {0:F3}%", combcorrect / (float)total * 100);

            Console.Error.WriteLine("LR: {0:F3}%", lrlabl / (float)total * 100);
            Console.Error.WriteLine("RL: {0:F3}%", rllabl / (float)total * 100);
            Console.Error.WriteLine("EF: {0:F3}%", eflabl / (float)total * 100);
            Console.Error.WriteLine("MST: {0:F3}%", mstlabl / (float)total * 100);
            Console.Error.WriteLine("CB: {0:F3}%", comblabl / (float)total * 100);
        }

        private static void CalculateCombineWeight()
        {
            //string inputfn = Configure.GetOptionString("Input");
            //string outputn = Configure.GetOptionString("Output");
            string LRparserfn = Configure.GetOptionString("Parser-LR");

            string RLparserfn = Configure.GetOptionString("Parser-RL");
            string EFparserfn = Configure.GetOptionString("Parser-EF");

            string taggerfn = Configure.GetOptionString("Tagger");
            int taggerbeam = Configure.GetOptionInt("TaggerBeam", 8);
            int parserbeam = Configure.GetOptionInt("ParserBeam", 8);

            bool tolower = Configure.GetOptionBool("ToLower", false);

            PoSTag.IObservGenerator observGen = //new PoSTag.MTCaseSensitiveObservGenerator();
                new PoSTag.MTCaseSensitiveWordClusterObservGenerator();

            bool useTagDict = Configure.GetOptionBool("UseTagDict", false);

            string inputfn = Configure.GetOptionString("Input");

            PoSTag.PoSTagModelWrapper tagModel = new PoSTag.PoSTagModelWrapper(taggerfn);

            PoSTag.PoSTagDecoderWrapper tagger
                = new PoSTag.PoSTagDecoderWrapper(
                    tagModel, taggerbeam, observGen,
                    new PoSTag.MTWSJPoSDict());
            string[] dummy;
            tagger.GenPOS(new string[] { "Test", "Test" }, out dummy);

            LinearDepParser.ParserModelWrapper LRParserModel = new ParserModelWrapper(LRparserfn);
            LinearDepParser.ParserDecoder LRDecoder
                = new ParserDecoder(LRParserModel.pmInfo, LRParserModel.parserModel, parserbeam);


            LinearDepParser.ParserModelWrapper RLParserModel = new ParserModelWrapper(RLparserfn);
            LinearDepParser.ParserDecoder RLDecoder
                = new ParserDecoder(RLParserModel.pmInfo, RLParserModel.parserModel, parserbeam);


            EasyFirstDepPar.EasyFirstParserModelWrapper EFParserModel = new EasyFirstDepPar.EasyFirstParserModelWrapper(EFparserfn);
            EasyFirstDepPar.EasyParDecoder EFDecoder =
                new EasyFirstDepPar.EasyParDecoder(EFParserModel.pmInfo, EFParserModel.parserModel);

            ParserCombinator combinator = new ParserCombinator();

            Console.Error.WriteLine("Ready.");

            MaltTabFileReader matr = new MaltTabFileReader(inputfn);

            
            int total = 0;
            int lrcorrect = 0;
            int rlcorrect = 0;
            int efcorrect = 0;
            int combcorrect = 0;
            ConsoleTimer timer = new ConsoleTimer(100);

            Dictionary<string, int[]> PoSDict = new Dictionary<string, int[]>();

            while (!matr.EndOfStream)//sr.EndOfStream)
            {
                ParserSentence refsnt;
                if (!matr.GetNextSent(out refsnt))
                {
                    continue;
                }
                else
                {
                    timer.Up();
                    string[] toks = refsnt.tok;

                    if (tolower)
                    {
                        for (int i = 0; i < toks.Length; ++i)
                        {
                            toks[i] = toks[i].ToLower();
                        }
                    }

                    string[] pos;

                    total += toks.Length;

                    if (!tagger.GenPOS(toks, out pos))
                    {
                        Console.Error.WriteLine("Fail To Parse!");
                    }
                    else
                    {
                        int[] LRHid;
                        int[] RLHid;
                        int[] EFHid;
                        string[] LRLabel;
                        string[] RLLabel;
                        string[] EFLabel;

                        string[] rtok = toks.Reverse<string>().ToArray<string>();
                        string[] rpos = pos.Reverse<string>().ToArray<string>();

                        if (!LRDecoder.Run(toks, pos, out LRHid, out LRLabel)
                            || !RLDecoder.Run(rtok, rpos, out RLHid, out RLLabel)
                            || !EFDecoder.Run(toks, pos, out EFHid, out EFLabel))
                        {
                            Console.Error.WriteLine("Fail To Parse!");
                        }
                        else
                        {
                            List<ParserSentence> sentenceList = new List<ParserSentence>();
                            sentenceList.Add(new ParserSentence(toks, pos, LRHid, LRLabel));
                            sentenceList.Add(
                                new ParserSentence(rtok, rpos,
                                    RLHid,
                                    RLLabel
                                    ).Reverse());
                            sentenceList.Add(new ParserSentence(toks, pos, EFHid, EFLabel));



                            float[] weights = { 0.92f, 0.91f, 0.9f };
                            ParserSentence combined;
                            if (!combinator.Combine(sentenceList.ToArray(), weights, out combined))
                            {
                                Console.Error.WriteLine("!!!");
                            }

                            for (int i = 0; i < refsnt.Length; ++i)
                            {
                                int[] posW;
                                if(!PoSDict.TryGetValue(pos[i], out posW))
                                {
                                    posW = new int[4];
                                    PoSDict[pos[i]] = posW;
                                }

                                posW[0]++;
                                    if (refsnt.hid[i] == sentenceList[0].hid[i])
                                    {
                                        posW[1]++;
                                        lrcorrect++;
                                    }
                                    if (refsnt.hid[i] == sentenceList[1].hid[i])
                                    {
                                        posW[2]++;
                                        rlcorrect++;
                                    }

                                    if (refsnt.hid[i] == sentenceList[2].hid[i])
                                    {
                                        posW[3]++;
                                        efcorrect++;
                                    }

                                    if (refsnt.hid[i] == combined.hid[i])
                                    {
                                        combcorrect++;
                                    }
                                
                            }
                        }
                    }



                }
                //timer.Up();
            }
            timer.Finish();
            matr.Close();

            string output = Configure.GetOptionString("Output");

            StreamWriter sw = new StreamWriter(output, false);

            foreach (string xpos in PoSDict.Keys)
            {
                int[] w = PoSDict[xpos];
                sw.WriteLine("{0}\t{1}\t{2}\t{3}", xpos,
                    w[1] / (float)w[0],
                    w[2] / (float)w[0],
                    w[3] / (float)w[0]);
            }

            sw.Close();

            Console.Error.WriteLine("LR: {0:F3}%", lrcorrect / (float)total * 100);
            Console.Error.WriteLine("RL: {0:F3}%", rlcorrect / (float)total * 100);
            Console.Error.WriteLine("EF: {0:F3}%", efcorrect / (float)total * 100);
            Console.Error.WriteLine("CB: {0:F3}%", combcorrect / (float)total * 100);
        }

        private static bool ProcessConsoleInput(bool[] isTagged, string[] preTags, int[] preHid, string[] preLabels)
        {
            string line = Console.ReadLine();

                    if (line[0] == 'q')
                    {
                        return false;
                    }
                    if (line[0] == 'h')
                    {
                        Console.Error.WriteLine(
                            string.Join("\n",
                            new string[] {
                                "enter/e        Write empty sentence and next Line",
                                "w              Write the result and next line",
                                "p id POS       Change the pos tag of word[id] to POS",
                                "l id hid arc   Change the head word of id to hid with label",
                                "c              Clear all changes for this line",
                                "q              Quit",
                                "h              Show this help",
                                "s              Split all"
                            })
                            );
                        return true;
                    }

                    if (line[0] == 's')
                    {
                        for(int i = 0; i < preHid.Length; ++i)
                        {
                            preHid[i] = 0;
                        }
                        return true;
                    }
                    else if (line[0] == 'c')
                    {
                        Array.Clear(isTagged, 0, isTagged.Length);
                        Array.Clear(preTags, 0, preTags.Length);
                        Array.Clear(preHid, 0, preHid.Length);
                        Array.Clear(preLabels, 0, preLabels.Length);
                        return true;
                    }
                    else if (line[0] == 'e')
                    {
                        return true;
                    }
                    else if (line[0] == 'w')
                    {
                        return true;
                    }
                    else if (line[0] == 'p')
                    {
                        if (line.Length == 1)
                        {
                            return true;
                        }

                        line = line.Substring(1);

                        string[] commandParts = line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                        if (commandParts.Length >= 2)
                        {
                            int id;
                            if (!int.TryParse(commandParts[0], out id))
                            {
                                return true;
                            }
                            if (id < 1 || id > isTagged.Length)
                            {
                                return true;
                            }
                            preTags[id - 1] = commandParts[1];
                            isTagged[id - 1] = true;
                            return true;
                        }
                    }
                    else if (line[0] == 'l')
                    {
                        if (line.Length == 1)
                        {
                            return true;
                        }

                        line = line.Substring(1);

                        string[] commandParts = line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                        if (commandParts.Length >= 2)
                        {
                            int id;
                            if (!int.TryParse(commandParts[0], out id))
                            {
                                return true;
                            }

                            int xhid;
                            if (!int.TryParse(commandParts[1], out xhid))
                            {
                                return true;
                            }
                            if (id < 1 || id > isTagged.Length || xhid < 0 || xhid > isTagged.Length || xhid == id)
                            {
                                return true;
                            }
                            string arc = null;
                            if (xhid > 0 && commandParts.Length >= 3)
                            {
                                arc = commandParts[2];
                            }
                            preHid[id - 1] = xhid;
                            preLabels[id - 1] = arc;
                            return true;
                        }
                    }
            return false;
        }
                
        private static void ManualTag()
        {
            string inputfn = Configure.GetOptionString("Input");
            string outputn = Configure.GetOptionString("Output");
            string parserfn = Configure.GetOptionString("Parser");
            string taggerfn = Configure.GetOptionString("Tagger");
            int taggerbeam = Configure.GetOptionInt("TaggerBeam", 8);
            int parserbeam = Configure.GetOptionInt("ParserBeam", 8);

            StreamReader sr = new StreamReader(inputfn);
            MaltFileWriter sw = new MaltFileWriter(outputn);
            sw.IsSingleLine = true;
            DepParModelWrapper dpm = new DepParModelWrapper(taggerfn, parserfn);

            PoSTag.IObservGenerator observGen =
                new PoSTag.MTCaseSensitiveWordClusterObservGenerator();

            DepParDecoderWrapper decoder = new DepParDecoderWrapper(dpm, observGen, taggerbeam, parserbeam, new PoSTag.MTWSJPoSDict());

            

            ConsoleTimer timer = new ConsoleTimer(100);
            int linenum = 0;
            while (!sr.EndOfStream)
            {
                bool[] isTagged = null;
                string[] preTags = null;
                string[] lastToks = null;
                int[] preHid = null;
                string[] preLabels = null;
                string xline = sr.ReadLine();
                lastToks = xline.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                if (lastToks.Length <= 0)
                {
                    continue;
                }
                Console.Error.WriteLine("#line: {0}", linenum++);
                Console.Error.WriteLine(xline);
                isTagged = new bool[lastToks.Length];
                preTags = new string[lastToks.Length];
                preHid = new int[lastToks.Length];
                preLabels = new string[lastToks.Length];
                for(int i = 0; i < preHid.Length; ++i)
                {
                    preHid[i] = -1;
                }
                //ParserForcedConstraints constraints = new ParserForcedConstraints(new bool[lastToks.Length], new int[lastToks.Length], new string[lastToks.Length]);

                bool isquitting = false;
                //string[] preReadPoS = null;
                
                while (true)
                {
                    string[] pos;
                    int[] hid;
                    string[] label;
                    bool suc;
                    
                    suc = decoder.Parse(lastToks, preTags, preHid, preLabels, out pos, out hid, out label);
                    DepTree tree = null;
                    if(!suc)
                    {
                        Console.Error.WriteLine("Fail to Parse!");
                    }
                    else
                    {
                        tree = new DepTree(lastToks, pos, hid, label);
                        CTree ctree = new CTree(tree);
                        Console.WriteLine(ctree.GetTxtTree());
                    }

                    //DepTree tree = null;
                    string line = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        sw.Write();
                        break;
                    }


                    if (line[0] == 'q')
                    {
                        isquitting = true;
                        break;
                    }
                    if (line[0] == 'h')
                    {
                        Console.Error.WriteLine(
                            string.Join("\n",
                            new string[] {
                                "enter/e        Write empty sentence and next Line",
                                "w              Write the result and next line",
                                "p id POS       Change the pos tag of word[id] to POS",
                                "l id hid arc   Change the head word of id to hid with label",
                                "c              Clear all changes for this line",
                                "q              Quit",
                                "h              Show this help",
                                "s              Split all"
                            })
                            );
                    }

                    if (line[0] == 's')
                    {
                        for(int i = 0; i < preHid.Length; ++i)
                        {
                            preHid[i] = 0;
                        }
                    }
                    else if (line[0] == 'c')
                    {
                        isTagged = new bool[lastToks.Length];
                        preTags = new string[lastToks.Length];
                        preHid = new int[lastToks.Length];
                        for (int i = 0; i < preHid.Length; ++i)
                        {
                            preHid[i] = 0;
                        }
                        preLabels = new string[lastToks.Length];
                    }
                    else if (line[0] == 'e')
                    {
                        sw.Write();
                        break;
                    }
                    else if (line[0] == 'w')
                    {
                        if (suc)
                        {
                            sw.Write(lastToks, pos, hid, label);
                            sw.Flush();
                        }
                        else
                        {
                            decoder.Parse(lastToks, out pos, out hid, out label);
                            sw.Write(lastToks, pos, hid, label);
                            sw.Flush();
                        }
                        break;
                    }
                    else if (line[0] == 'p')
                    {
                        if (line.Length == 1)
                        {
                            continue;
                        }

                        line = line.Substring(1);

                        string[] commandParts = line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                        if (commandParts.Length >= 2)
                        {
                            int id;
                            if (!int.TryParse(commandParts[0], out id))
                            {
                                continue;
                            }
                            if (id < 1 || id > lastToks.Length)
                            {
                                continue;
                            }
                            preTags[id - 1] = commandParts[1];
                            isTagged[id - 1] = true;
                        }
                    }
                    else if (line[0] == 'l')
                    {
                        if (line.Length == 1)
                        {
                            continue;
                        }

                        line = line.Substring(1);

                        string[] commandParts = line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                        if (commandParts.Length >= 2)
                        {
                            int id;
                            if (!int.TryParse(commandParts[0], out id))
                            {
                                continue;
                            }

                            int xhid;
                            if (!int.TryParse(commandParts[1], out xhid))
                            {
                                continue;
                            }
                            if (id < 1 || id > lastToks.Length || xhid < 0 || xhid > lastToks.Length || xhid == id)
                            {
                                continue;
                            }
                            string arc = null;
                            if (xhid > 0 && commandParts.Length >= 3)
                            {
                                arc = commandParts[2];
                            }
                            preHid[id - 1] = xhid;
                            preLabels[id - 1] = arc;
                        }
                    }
                }
                if (isquitting)
                {
                    break;
                }
            }

            //timer.Finish();
            sr.Close();
            sw.Close();
        }

        private static Dictionary<string, float>[] LoadCombinationWeights()
        {
            Dictionary<string, float>[] dicts = new Dictionary<string, float>[3];

            for (int i = 0; i < 3; ++i)
            {
                dicts[i] = new Dictionary<string, float>();
            }

            string cbfn = Configure.GetOptionString("CombineWeight");
            StreamReader sr = new StreamReader(cbfn);

            while (!sr.EndOfStream)
            {
                string[] parts = sr.ReadLine().Trim().Split('\t');

                dicts[0][parts[0]] = float.Parse(parts[1]);
                dicts[1][parts[0]] = float.Parse(parts[2]);
                dicts[2][parts[0]] = float.Parse(parts[3]);
            }

            sr.Close();

            return dicts;
        }

        private static List<ParserSentence> ParseSentences(List<string> sentences, DepParDecoderWrapper decoder)
        {
            List<ParserSentence> parsed = new List<ParserSentence>();

            foreach (var sentence in sentences)
            {
                if (string.IsNullOrWhiteSpace(sentence))
                {
                    Console.Error.WriteLine("Warning: Empty Input!!!");
                    parsed.Add(null);
                    continue;
                }

                string[] toks = sentence.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                string[] pos;
                int[] hid;
                string[] label;
                if (!decoder.Parse(toks, out pos, out hid, out label))
                {
                    parsed.Add(null);
                    Console.Error.WriteLine("Warning: Fail to Parse Input Sentence!!!");
                }
                else
                {
                    parsed.Add(new ParserSentence(toks, pos, hid, label));
                }
            }

            return parsed;
        }

        private delegate List<ParserSentence> ParseDelegate(List<string> sentences, DepParDecoderWrapper decoder);
        
        private static void ParseDataFile()
        {
            string inputfn = Configure.GetOptionString("Input");
            string outputn = Configure.GetOptionString("Output");
            string parserfn = Configure.GetOptionString("Parser");
            string taggerfn = Configure.GetOptionString("Tagger");
            int taggerbeam = Configure.GetOptionInt("TaggerBeam", 8);
            int parserbeam = Configure.GetOptionInt("ParserBeam", 8);
            int NThread = Configure.GetOptionInt("NThread", 1);
            bool useTagDict = Configure.GetOptionBool("UseTagDict", false);
            bool isChinese = Configure.GetOptionBool("IsChinese", false);

            if (NThread <= 1)
            {
                ParseSingleThread(inputfn, outputn, parserfn, taggerfn, taggerbeam, parserbeam);
                return;
            }

            StreamReader sr = new StreamReader(inputfn);
            MaltFileWriter mtfw = new MaltFileWriter(outputn);
            mtfw.IsSingleLine = true;

            PoSTag.IObservGenerator observGen = isChinese ?//new PoSTag.MTCaseSensitiveObservGenerator();
                (PoSTag.IObservGenerator)(new PoSTag.CTBMTObservGenerator()) : 
                (PoSTag.IObservGenerator)(new PoSTag.MTCaseSensitiveWordClusterObservGenerator());

            DepParModelWrapper dpm = new DepParModelWrapper(taggerfn, parserfn);

            DepParDecoderWrapper[] decoders = new DepParDecoderWrapper[NThread];

            for (int i = 0; i < NThread; ++i)
            {
                decoders[i] = new DepParDecoderWrapper(dpm,
                    observGen,
                    taggerbeam,
                    parserbeam,
                    isChinese ?
                    (PoSTag.IPoSDict)new PoSTag.MTCTBPoSDict() :
                    (PoSTag.IPoSDict)new PoSTag.MTWSJPoSDict()
                    );
            }

            ParseDelegate[] dd = new ParseDelegate[NThread];
            IAsyncResult[] iAR = new IAsyncResult[NThread];

            for (int i = 0; i < NThread; i++)
            {
                dd[i] = new ParseDelegate(ParseSentences);
                
            }
           

            ConsoleTimer timer = new ConsoleTimer(100);

            int BatchSize = 200;

            int LineParsed = 0;

            while (!sr.EndOfStream)
            {
                //int ThreadCount;

                List<string>[] SentencePool = new List<string>[NThread];

                List<ParserSentence>[] ParsedPool = new List<ParserSentence>[NThread];

                for (int i = 0; i < SentencePool.Length; ++i)
                {
                    SentencePool[i] = new List<string>();
                }

                int CurrentPool = -1;

                while (!sr.EndOfStream && CurrentPool < NThread - 1)
                {
                    CurrentPool++;
                    List<string> pool = SentencePool[CurrentPool];
                    while (!sr.EndOfStream && pool.Count < BatchSize)
                    {
                        pool.Add(sr.ReadLine());
                    }                   
                }

                int ThreadCount = CurrentPool + 1;

                for (int i = 0; i < ThreadCount; i++)
                {
                    iAR[i] = dd[i].BeginInvoke(SentencePool[i], decoders[i], null, null);
                }

                for (int i = 0; i < ThreadCount; i++)
                {
                    iAR[i].AsyncWaitHandle.WaitOne();
                    ParsedPool[i] = dd[i].EndInvoke(iAR[i]);
                }

                for (int i = 0; i < ThreadCount; ++i)
                {
                    foreach (ParserSentence snt in ParsedPool[i])
                    {
                        LineParsed++;
                        if (snt != null)
                        {
                            mtfw.Write(snt.tok, snt.pos, snt.hid, snt.label);
                        }
                        else
                        {
                            mtfw.Write();
                        }
                    }
                }


                Console.Error.Write("Parsed: {0}...\r", LineParsed);
                
            }
            Console.Error.WriteLine("Parsed: {0}...Done", LineParsed);
            //timer.Finish();
            sr.Close();
            mtfw.Close();
        }

        private static void ParseDataFileCombine()
        {
            string LRparserfn = Configure.GetOptionString("Parser-LR");

            string RLparserfn = Configure.GetOptionString("Parser-RL");
            string EFparserfn = Configure.GetOptionString("Parser-EF");
            string MSTparserfn = Configure.GetOptionString("Parser-MST");

            string taggerfn = Configure.GetOptionString("Tagger");
            int taggerbeam = Configure.GetOptionInt("TaggerBeam", 8);
            int parserbeam = Configure.GetOptionInt("ParserBeam", 8);

            bool tolower = Configure.GetOptionBool("ToLower", false);

            PoSTag.IObservGenerator observGen = //new PoSTag.MTCaseSensitiveObservGenerator();
                new PoSTag.MTCaseSensitiveWordClusterObservGenerator();

            bool useTagDict = Configure.GetOptionBool("UseTagDict", false);

            string inputfn = Configure.GetOptionString("Input");
            
            string outputfn = Configure.GetOptionString("Output");

            bool isParallel = Configure.GetOptionBool("SentenceLevelParallel", false);
            

            PoSTag.PoSTagModelWrapper tagModel = new PoSTag.PoSTagModelWrapper(taggerfn);

            IPoSTagger tagger = new LRTagger(
                    new PoSTag.PoSTagDecoderWrapper(
                    tagModel, taggerbeam, observGen,
                    new PoSTag.MTWSJPoSDict()));
            string[] dummy;
            tagger.Run(new string[] { "Test", "Test" }, out dummy);

            List<IParserDecoder> decoderList = new List<IParserDecoder>();
            List<float> weightList = new List<float>();

            LinearDepParser.ParserModelWrapper LRParserModel = new ParserModelWrapper(LRparserfn);
            
            decoderList.Add(
                new LRDecoder(new ParserDecoder(LRParserModel.pmInfo, LRParserModel.parserModel, parserbeam)));
            weightList.Add(0.92f);

            LinearDepParser.ParserModelWrapper RLParserModel = new ParserModelWrapper(RLparserfn);
            decoderList.Add(new RLDecoder(
                new ParserDecoder(RLParserModel.pmInfo, RLParserModel.parserModel, parserbeam)));
            weightList.Add(0.91f);

            EasyFirstDepPar.EasyFirstParserModelWrapper EFParserModel = new EasyFirstDepPar.EasyFirstParserModelWrapper(EFparserfn);
            decoderList.Add(new EFDecoder(
                new EasyFirstDepPar.EasyParDecoder(EFParserModel.pmInfo, EFParserModel.parserModel)));
            weightList.Add(0.905f);

            MSTParser.MSTParserModelWrapper MSTParserModel = new MSTParser.MSTParserModelWrapper(MSTparserfn);
            decoderList.Add(new MSTDecoder(new MSTParser.MSTDecoder(MSTParserModel.pmInfo, MSTParserModel.parserModel)));
            weightList.Add(0.90f);

            
            var cdecoder = new CombineDecoder(decoderList.ToArray(), weightList.ToArray());

            Console.Error.WriteLine("Ready.");

            DependencyParser deppar = new DependencyParser(tagger, cdecoder);

            StreamReader matr =
                new StreamReader(inputfn);
                

            MaltFileWriter matw =
                new MaltFileWriter(outputfn);
                

            
            ConsoleTimer timer = new ConsoleTimer(100);
            while (!matr.EndOfStream)//sr.EndOfStream)
            {
                string line = matr.ReadLine().Trim();
                if (string.IsNullOrWhiteSpace(line))
                {
                    Console.Error.WriteLine("Empty Line!");
                    matw.Write();
                    continue;
                }

                    timer.Up();
                    string[] toks = line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                    if (tolower)
                    {
                        for (int i = 0; i < toks.Length; ++i)
                        {
                            toks[i] = toks[i].ToLower();
                        }
                    }

                    string[] pos;
                    int[] hid;
                    string[] labl;

                    if (!deppar.Run(toks, out pos, out hid, out labl))
                    {
                        Console.Error.WriteLine("Fail To Parse!");
                        matw.Write();
                    }
                    else
                    {
                        matw.Write(toks, pos, hid, labl);
                    }

                   
            }
            timer.Finish();
            matr.Close();
            matw.Close();
            
        }

        private static void ParseConsoleCombine()
        {
            string LRparserfn = Configure.GetOptionString("Parser-LR");

            string RLparserfn = Configure.GetOptionString("Parser-RL");
            string EFparserfn = Configure.GetOptionString("Parser-EF");
            string MSTparserfn = Configure.GetOptionString("Parser-MST");

            string taggerfn = Configure.GetOptionString("Tagger");
            int taggerbeam = Configure.GetOptionInt("TaggerBeam", 8);
            int parserbeam = Configure.GetOptionInt("ParserBeam", 8);

            bool tolower = Configure.GetOptionBool("ToLower", false);

            PoSTag.IObservGenerator observGen = //new PoSTag.MTCaseSensitiveObservGenerator();
                new PoSTag.MTCaseSensitiveWordClusterObservGenerator();

            bool useTagDict = Configure.GetOptionBool("UseTagDict", false);

            string inputfn = Configure.GetOptionString("Input");

            string outputfn = Configure.GetOptionString("Output");

            bool isParallel = Configure.GetOptionBool("SentenceLevelParallel", false);


            PoSTag.PoSTagModelWrapper tagModel = new PoSTag.PoSTagModelWrapper(taggerfn);

            IPoSTagger tagger = new LRTagger(
                    new PoSTag.PoSTagDecoderWrapper(
                    tagModel, taggerbeam, observGen,
                    new PoSTag.MTWSJPoSDict()));
            string[] dummy;
            tagger.Run(new string[] { "Test", "Test" }, out dummy);

            List<IParserDecoder> decoderList = new List<IParserDecoder>();
            List<float> weightList = new List<float>();

            LinearDepParser.ParserModelWrapper LRParserModel = new ParserModelWrapper(LRparserfn);

            decoderList.Add(
                new LRDecoder(new ParserDecoder(LRParserModel.pmInfo, LRParserModel.parserModel, parserbeam)));
            weightList.Add(0.92f);

            LinearDepParser.ParserModelWrapper RLParserModel = new ParserModelWrapper(RLparserfn);
            decoderList.Add(new RLDecoder(
                new ParserDecoder(RLParserModel.pmInfo, RLParserModel.parserModel, parserbeam)));
            weightList.Add(0.91f);

            EasyFirstDepPar.EasyFirstParserModelWrapper EFParserModel = new EasyFirstDepPar.EasyFirstParserModelWrapper(EFparserfn);
            decoderList.Add(new EFDecoder(
                new EasyFirstDepPar.EasyParDecoder(EFParserModel.pmInfo, EFParserModel.parserModel)));
            weightList.Add(0.905f);

            MSTParser.MSTParserModelWrapper MSTParserModel = new MSTParser.MSTParserModelWrapper(MSTparserfn);
            decoderList.Add(new MSTDecoder(new MSTParser.MSTDecoder(MSTParserModel.pmInfo, MSTParserModel.parserModel)));
            weightList.Add(0.90f);


            var cdecoder = new CombineDecoder(decoderList.ToArray(), weightList.ToArray());

            Console.Error.WriteLine("Ready.");

            DependencyParser deppar = new DependencyParser(tagger, cdecoder);

            while (true)
            {
                string line = //@"value ( ITB + Hd . theta . + HdT ) , ( ITB - Hd . theta . + HdT ) - - HdT addition correction - - .";
                    @"The direction agreement control signal calculating section 58 adds together the first target correction value ( ITB . + - . Hd . theta . ) supplied from the selecting section 55 and the target correction value HdT output from the differential value to control signal converter 45 and outputs the resultant sum signal as a second target correction value ( ITB + Hd . theta . + HdT ) , ( ITB - Hd . theta . + HdT ) - - HdT addition correction - - .";

                string[] parts = line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                string[] pos;
                int[] hid;
                string[] labl;
                deppar.Run(parts, out pos, out hid, out labl);

                CTree tree = new CTree(parts, pos, hid, labl);

                Console.Error.WriteLine(tree.GetTxtTree());
            }

        }

        private static void ParseDataFileCombine2()
        {
            string LRparserfn = Configure.GetOptionString("Parser-LR");

            string RLparserfn = Configure.GetOptionString("Parser-RL");
            string EFparserfn = Configure.GetOptionString("Parser-EF");
            string MSTparserfn = Configure.GetOptionString("Parser-MST");

            string taggerfn = Configure.GetOptionString("Tagger");
            int taggerbeam = Configure.GetOptionInt("TaggerBeam", 8);
            int parserbeam = Configure.GetOptionInt("ParserBeam", 8);

            bool tolower = Configure.GetOptionBool("ToLower", false);

            PoSTag.IObservGenerator observGen = //new PoSTag.MTCaseSensitiveObservGenerator();
                new PoSTag.MTCaseSensitiveWordClusterObservGenerator();

            bool useTagDict = Configure.GetOptionBool("UseTagDict", false);

            string inputfn = Configure.GetOptionString("Input");

            string outputfn = Configure.GetOptionString("Output");

            bool isParallel = Configure.GetOptionBool("SentenceLevelParallel", false);


            PoSTag.PoSTagModelWrapper tagModel = new PoSTag.PoSTagModelWrapper(taggerfn);

            PoSTag.PoSTagDecoderWrapper tagger
                = new PoSTag.PoSTagDecoderWrapper(
                    tagModel, taggerbeam, observGen,
                    new PoSTag.MTWSJPoSDict());
            string[] dummy;
            tagger.GenPOS(new string[] { "Test", "Test" }, out dummy);

            LinearDepParser.ParserModelWrapper LRParserModel = new ParserModelWrapper(LRparserfn);
            LinearDepParser.ParserDecoder LRDecoder
                = new ParserDecoder(LRParserModel.pmInfo, LRParserModel.parserModel, parserbeam);


            LinearDepParser.ParserModelWrapper RLParserModel = new ParserModelWrapper(RLparserfn);
            LinearDepParser.ParserDecoder RLDecoder
                = new ParserDecoder(RLParserModel.pmInfo, RLParserModel.parserModel, parserbeam);


            EasyFirstDepPar.EasyFirstParserModelWrapper EFParserModel = new EasyFirstDepPar.EasyFirstParserModelWrapper(EFparserfn);
            EasyFirstDepPar.EasyParDecoder EFDecoder =
                new EasyFirstDepPar.EasyParDecoder(EFParserModel.pmInfo, EFParserModel.parserModel);

            MSTParser.MSTParserModelWrapper MSTParserModel = new MSTParser.MSTParserModelWrapper(MSTparserfn);
            MSTParser.MSTDecoder MSTDecoder = new MSTParser.MSTDecoder(MSTParserModel.pmInfo, MSTParserModel.parserModel);


            ParserCombinator combinator = new ParserCombinator();

            Console.Error.WriteLine("Ready.");



            StreamReader matr =
                new StreamReader(inputfn);


            MaltFileWriter matw =
                new MaltFileWriter(outputfn);



            ConsoleTimer timer = new ConsoleTimer(100);
            while (!matr.EndOfStream)//sr.EndOfStream)
            {
                string line = matr.ReadLine().Trim();
                if (string.IsNullOrWhiteSpace(line))
                {
                    Console.Error.WriteLine("Empty Line!");
                    matw.Write();
                    continue;
                }

                timer.Up();
                string[] toks = line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                if (tolower)
                {
                    for (int i = 0; i < toks.Length; ++i)
                    {
                        toks[i] = toks[i].ToLower();
                    }
                }

                string[] pos;


                if (!tagger.GenPOS(toks, out pos))
                {
                    Console.Error.WriteLine("Fail To Parse!");
                    matw.Write();
                }
                else
                {
                    int[] LRHid = null;
                    int[] RLHid = null;
                    int[] EFHid = null;
                    int[] MSTHid = null;

                    string[] LRLabel = null;
                    string[] RLLabel = null;
                    string[] EFLabel = null;
                    string[] MSTLabel = null;

                    string[] rtok = toks.Reverse<string>().ToArray<string>();
                    string[] rpos = pos.Reverse<string>().ToArray<string>();

                    if (isParallel)
                    {
                        Parallel.For(0, 4, (i) =>
                        {
                            if (i == 0)
                            {
                                LRDecoder.Run(toks, pos, out LRHid, out LRLabel);
                            }
                            else if (i == 1)
                            {
                                RLDecoder.Run(rtok, rpos, out RLHid, out RLLabel);
                            }
                            else if (i == 2)
                            {
                                EFDecoder.Run(toks, pos, out EFHid, out EFLabel);
                            }
                            else if (i == 3)
                            {
                                MSTDecoder.Run(toks, pos, out MSTHid, out MSTLabel);
                            }
                        });

                    }
                    else
                    {

                        LRDecoder.Run(toks, pos, out LRHid, out LRLabel);

                        RLDecoder.Run(rtok, rpos, out RLHid, out RLLabel);


                        EFDecoder.Run(toks, pos, out EFHid, out EFLabel);

                        MSTDecoder.Run(toks, pos, out MSTHid, out MSTLabel);

                    }

                    List<ParserSentence> sentenceList = new List<ParserSentence>();
                    sentenceList.Add(new ParserSentence(toks, pos, LRHid, LRLabel));
                    sentenceList.Add(
                        new ParserSentence(rtok, rpos,
                            RLHid,
                            RLLabel
                            ).Reverse());
                    sentenceList.Add(new ParserSentence(toks, pos, EFHid, EFLabel));

                    sentenceList.Add(new ParserSentence(toks, pos, MSTHid, MSTLabel));

                    float[] weights = { 0.92f, 0.91f, 0.9f, 0.89f };
                    ParserSentence combined;
                    if (!combinator.Combine(sentenceList.ToArray(), weights, out combined))
                    {
                        Console.Error.WriteLine("Fail to combine!");
                        matw.Write();
                    }
                    else
                    {
                        matw.Write(sentenceList[2].tok, sentenceList[2].pos, sentenceList[2].hid, sentenceList[2].label);
                        //matw.Write(combined.tok, combined.pos, combined.hid, combined.label);
                        //matw.Flush();
                    }


                }

            }
            timer.Finish();
            matr.Close();
            matw.Close();

        }


        private static void ParseDataFileCombineConsole()
        {
            string LRparserfn = Configure.GetOptionString("Parser-LR");

            string RLparserfn = Configure.GetOptionString("Parser-RL");
            string EFparserfn = Configure.GetOptionString("Parser-EF");

            string taggerfn = Configure.GetOptionString("Tagger");
            int taggerbeam = Configure.GetOptionInt("TaggerBeam", 8);
            int parserbeam = Configure.GetOptionInt("ParserBeam", 8);

            bool tolower = Configure.GetOptionBool("ToLower", false);

            PoSTag.IObservGenerator observGen = //new PoSTag.MTCaseSensitiveObservGenerator();
                new PoSTag.MTCaseSensitiveWordClusterObservGenerator();

            bool useTagDict = Configure.GetOptionBool("UseTagDict", false);

            
            
            PoSTag.PoSTagModelWrapper tagModel = new PoSTag.PoSTagModelWrapper(taggerfn);

            PoSTag.PoSTagDecoderWrapper tagger
                = new PoSTag.PoSTagDecoderWrapper(
                    tagModel, taggerbeam, observGen,
                    new PoSTag.MTWSJPoSDict());
            string[] dummy;
            tagger.GenPOS(new string[] { "Test", "Test" }, out dummy);

            LinearDepParser.ParserModelWrapper LRParserModel = new ParserModelWrapper(LRparserfn);
            LinearDepParser.ParserDecoder LRDecoder
                = new ParserDecoder(LRParserModel.pmInfo, LRParserModel.parserModel, parserbeam);


            LinearDepParser.ParserModelWrapper RLParserModel = new ParserModelWrapper(RLparserfn);
            LinearDepParser.ParserDecoder RLDecoder
                = new ParserDecoder(RLParserModel.pmInfo, RLParserModel.parserModel, parserbeam);


            EasyFirstDepPar.EasyFirstParserModelWrapper EFParserModel = new EasyFirstDepPar.EasyFirstParserModelWrapper(EFparserfn);
            EasyFirstDepPar.EasyParDecoder EFDecoder =
                new EasyFirstDepPar.EasyParDecoder(EFParserModel.pmInfo, EFParserModel.parserModel);

            ParserCombinator combinator = new ParserCombinator();

            Console.Error.WriteLine("Ready.");



            

            ConsoleTimer timer = new ConsoleTimer(100);
            string line;

            int tmpc = 0;
            while ((tmpc = Console.In.Peek()) >= 0)//sr.EndOfStream)
            {
                line = Console.ReadLine();
                tmpc++;
                if (string.IsNullOrWhiteSpace(line))
                {
                    Console.Error.WriteLine("Empty Line!");
                    Console.WriteLine();
                    continue;
                }

                timer.Up();
                string[] toks = line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                if (tolower)
                {
                    for (int i = 0; i < toks.Length; ++i)
                    {
                        toks[i] = toks[i].ToLower();
                    }
                }

                string[] pos;


                if (!tagger.GenPOS(toks, out pos))
                {
                    Console.Error.WriteLine("Fail To Parse!");
                    Console.WriteLine();
                }
                else
                {
                    int[] LRHid = null;
                    int[] RLHid = null;
                    int[] EFHid = null;
                    string[] LRLabel = null;
                    string[] RLLabel = null;
                    string[] EFLabel = null;

                    string[] rtok = toks.Reverse<string>().ToArray<string>();
                    string[] rpos = pos.Reverse<string>().ToArray<string>();

                    Parallel.For(0, 3, (i) =>
                    {
                        if (i == 0)
                        {
                            LRDecoder.Run(toks, pos, out LRHid, out LRLabel);
                        }
                        else if (i == 1)
                        {
                            RLDecoder.Run(rtok, rpos, out RLHid, out RLLabel);
                        }
                        else if (i == 2)
                        {
                            EFDecoder.Run(toks, pos, out EFHid, out EFLabel);
                        }
                    });


                    List<ParserSentence> sentenceList = new List<ParserSentence>();
                    sentenceList.Add(new ParserSentence(toks, pos, LRHid, LRLabel));
                    sentenceList.Add(
                        new ParserSentence(rtok, rpos,
                            RLHid,
                            RLLabel
                            ).Reverse());
                    sentenceList.Add(new ParserSentence(toks, pos, EFHid, EFLabel));



                    float[] weights = { 5.0f, 0.91f, 0.9f };
                    ParserSentence combined;
                    if (!combinator.Combine(sentenceList.ToArray(), weights, out combined))
                    {
                        Console.Error.WriteLine("Fail to combine!");
                        Console.WriteLine();
                    }
                    else
                    {
                        Console.WriteLine(MaltFileWriter.GetParseLine(combined.tok, combined.pos, combined.hid, combined.label));
                        //matw.Flush();
                    }


                }

            }
            timer.Finish();
            
        }

        static public void RunExample()
        {
            // run left-to-right shift-reduce parser
            string LRparserfn = Configure.GetOptionString("Parser-LR");

            string taggerfn = Configure.GetOptionString("Tagger");
            int taggerbeam = Configure.GetOptionInt("TaggerBeam", 8);
            int parserbeam = Configure.GetOptionInt("ParserBeam", 8);

            // initialize tagger

            // models are thread safe.
            var tagmodel = new PoSTag.PoSTagModelWrapper(taggerfn);

            // decoders are not thread safe, but they can share the same model.
            IPoSTagger tagger = new LRTagger(
                    new PoSTag.PoSTagDecoderWrapper(
                    tagmodel,
                    taggerbeam,
                    new PoSTag.CTBMTObservGenerator(),
                    new PoSTag.MTCTBPoSDict()));

            // initialize parser
            // models are thread safe
            LinearDepParser.ParserModelWrapper LRParserModel = new ParserModelWrapper(LRparserfn);

            // decoders are not thread safe, but they can share the same models.
            var parser = new LRDecoder(new ParserDecoder(LRParserModel.pmInfo, LRParserModel.parserModel, parserbeam));

            string[] Tokens = "This is a NNP_Test .".Split(' ');

            List<string>[] forcedTags = new List<string>[Tokens.Length];

            // if you want to specify the 4th word NNP_Test to be NNP...
            forcedTags[3] = new List<string>();

            // you can be specify multiple tags as candidates.
            forcedTags[3].Add("NNP");

            // for words you do not have tag preference, simply leave forcedTags[i] as null;

            string[] PoS;
            int[] HeadWordId;
            string[] ArcLabelType;

            if (tagger.Run(Tokens, forcedTags, out PoS))
            {
                if (parser.Run(Tokens, PoS, out HeadWordId, out ArcLabelType))
                {
                    // do something here
                    string txtTree = new CTree(Tokens, PoS, HeadWordId, ArcLabelType).GetTxtTree();
                    Console.Error.WriteLine(txtTree);
                }
                else
                {
                    // should never reach here, you must have encountered a BUG!!
                    Console.Error.WriteLine("fail to parse!");
                }
            }
            else
            {
                // should never happen, unless you specify a tag which is impossible to generate
                // i.e. not in the tag set.
                Console.Error.WriteLine("fail to tag!");
            }

        }


        private static void ParseSingleThread(string inputfn, string outputn, string parserfn, string taggerfn, int taggerbeam, int parserbeam)
        {
            StreamReader sr = new StreamReader(inputfn);
            MaltFileWriter mtfw = new MaltFileWriter(outputn);
            mtfw.IsSingleLine = true;
            bool isChinese = Configure.GetOptionBool("IsChinese", false);

            PoSTag.IObservGenerator observGen = isChinese ?//new PoSTag.MTCaseSensitiveObservGenerator();
                 (PoSTag.IObservGenerator)(new PoSTag.CTBMTObservGenerator()) :
                 (PoSTag.IObservGenerator)(new PoSTag.MTCaseSensitiveWordClusterObservGenerator());

            DepParModelWrapper dpm = new DepParModelWrapper(taggerfn, parserfn);

            bool useTagDict = Configure.GetOptionBool("UseTagDict", true);

            DepParDecoderWrapper decoder = new DepParDecoderWrapper(dpm, observGen, taggerbeam, parserbeam, 
                isChinese ?
                    (PoSTag.IPoSDict)new PoSTag.MTCTBPoSDict() :
                    (PoSTag.IPoSDict)new PoSTag.MTWSJPoSDict());

            ConsoleTimer timer = new ConsoleTimer(100);
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (string.IsNullOrWhiteSpace(line))
                {
                    timer.Message("Empty Input!");
                    mtfw.Write();
                }
                else
                {
                    string[] toks = line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    string[] pos;
                    int[] hid;
                    string[] label;
                    if (!decoder.Parse(toks, out pos, out hid, out label))
                    {
                        mtfw.Write();
                        timer.Message("Fail To Parse!");
                    }
                    else
                    {
                        mtfw.Write(toks, pos, hid, label);
                    }
                }
                timer.Up();
            }

            timer.Finish();
            sr.Close();
            mtfw.Close();
        }

        private static void Test()
        {
            string inputfn = Configure.GetOptionString("Input");
            string outputn = Configure.GetOptionString("Output");
            string parserfn = Configure.GetOptionString("Parser");
            string taggerfn = Configure.GetOptionString("Tagger");
            int taggerbeam = Configure.GetOptionInt("TaggerBeam", 8);
            int parserbeam = Configure.GetOptionInt("ParserBeam", 8);
            bool useGoldPoS = Configure.GetOptionBool("GoldTag", false);

            MaltTabFileReader sr = new MaltTabFileReader(inputfn);
            MaltFileWriter mtfw = new MaltFileWriter(outputn);
            mtfw.IsSingleLine = true;
           
            LinearDepParser.ParserModelWrapper LRParserModel = new ParserModelWrapper(parserfn);
            LinearDepParser.ParserDecoder LRDecoder
                = new ParserDecoder(LRParserModel.pmInfo, LRParserModel.parserModel, parserbeam);

            ConsoleTimer timer = new ConsoleTimer(100);
            int total = 0;
            int correcthead = 0;
            int correctlabel = 0;

            while (!sr.EndOfStream)
            {
                string[] toks;
                string[] refpos;
                int[] refhid;
                string[] reflabel;

                if (sr.GetNextSent(out toks, out refpos, out refhid, out reflabel))
                {
                    DepTree refdtree = new DepTree(toks, refpos, refhid, reflabel);

                    if (!DepTree.IsValidDepTree(refdtree) || !refdtree.CheckWellFormedness())
                    {
                        Console.Error.WriteLine("Error in reftree!");
                        continue;
                    }

                    string[] pos;
                    int[] hid;
                    string[] label;


                    
                    pos = refpos;
                    LRDecoder.Run(toks, refpos, out hid, out label);
                    
                    
                    for (int i = 0; i < toks.Length; ++i)
                    {
                        if (!IsPunc(toks[i]))
                        {
                            total++;
                            if (hid[i] == refhid[i])
                            {
                                correcthead++;
                                if (hid[i] == 0 || label[i] == reflabel[i])
                                {
                                    correctlabel++;
                                }
                            }
                        }
                    }

                    mtfw.Write(toks, pos, hid, label);
                }
                
                timer.Up();
            }

            timer.Finish();

            float uas = (float)correcthead / total * 100;
            float las = (float)correctlabel / total * 100;
            Console.Error.WriteLine("UAS: {0:F3} LAS: {1:F3}", uas, las);
            
            sr.Close();
            mtfw.Close();
        }

        static Dictionary<string, List<string>> GetTagDict(string fn)
        {
            Dictionary<string, List<string>> tagDict = new Dictionary<string, List<string>>();

            StreamReader sr = new StreamReader(fn);

            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                string[] parts = line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2)
                {
                    continue;
                }

                List<string> tags = new List<string>();

                for (int i = 1; i < parts.Length; ++i)
                {
                    tags.Add(parts[i]);
                }

                tagDict[parts[0]] = tags;
            }

            sr.Close();

            return tagDict;

        }

        private static bool IsPunc(string tok)
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
                || tok == "\'\'" || tok == "``" || tok == "...";

            
        }

    }
}
