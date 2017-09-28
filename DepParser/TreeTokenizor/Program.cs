using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NanYUtilityLib.DepParUtil;
//using LinearDepParser.Util;
//using LinearDepParser;
//using LinearDepParser;
using RecoverWBAlignment;
//using MSRA.NLC.Lingo.NLP;
using ParserUtil;
using NanYUtilityLib.Sweets;
using MSRA.NLC.Lingo.NLP;

using TreeTokenizor;

namespace ConvertTreeTokenization
{
    class Program
    {
        static void DoConvert(string[] args)
        {
            string nameroot = args[0];//args[0];
            string orimalt = nameroot + ".malt";//"wsj.02-21.problem.txt";//

            string maltoutfn = nameroot + ".mttok";
            string treefn = nameroot + ".mttree";

            TreeTokenizationConverter ttc = new TreeTokenizationConverter(new PoSConverter(), new DepArcConverter());
            string[] WBArgs = { "-tenglish", "1", "-twn", "1", "-dnne", "0", "-tlower", "0" };
            ITokenizorWrapper tokenizor = new MTTokenizor(WBArgs);//new QATokenizor();//

            
            MaltTabFileReader mtfr = new MaltTabFileReader(orimalt);
            mtfr.SingleLineMode = true;
            MaltFileWriter mtfw = new MaltFileWriter(maltoutfn, false);
            StreamWriter treefw = new StreamWriter(treefn, false);
            MaltFileWriter problemfw = new MaltFileWriter(nameroot + ".problem");

            
            RecoverEngWBAlign wbRecover = new RecoverEngWBAlign(tokenizor);
            int linenum = 0;
            int correctline = 0;
            
            ConsoleTimer timer = new ConsoleTimer(100);

            while (!mtfr.EndOfStream)
            {
                timer.Up();
                string[] oriToken;
                string[] oriPoS;
                int[] oriHId;
                string[] oriArcType;
                try
                {
                    if (mtfr.GetNextSent(out oriToken, out oriPoS, out oriHId, out oriArcType))
                    {
                        for (int i = 0; i < oriToken.Length; ++i)
                        {
                            if (oriToken[i] == "`")
                            {
                                oriToken[i] = "LSINGLEQUOTE";
                                //Console.Error.WriteLine("!!");
                            }
                            else if (oriToken[i] == "\'")
                            {
                                oriToken[i] = "RSINGLEQUOTE";
                            }
                            else if (oriToken[i] == "''")
                            {
                                oriToken[i] = "RDOUBLEQUOTE";
                            }
                            else if (oriToken[i] == "``")
                            {
                                oriToken[i] = "LDOUBLEQUOTE";
                            }
                        }

                        linenum++;
                        string alignStr;
                        string[] wbToken;
                        if (!wbRecover.WBwithAlign(oriToken, out wbToken, out alignStr))
                        {
                            continue;
                        }
                        AlignSpan alignSpan = new AlignSpan(oriToken.Length, wbToken.Length, alignStr);
                        if (!alignSpan.IsCorrectAlignSpan)
                        {
                            continue;
                        }
                        string[] wbPoS;
                        int[] wbHid;
                        string[] wbArcType;
                        if (!ttc.Convert(oriToken, oriPoS, oriHId, oriArcType, alignSpan, wbToken, out wbPoS, out wbHid, out wbArcType))
                        {
                            continue;
                        }
                        if (!new DepTree(wbToken, wbPoS, wbHid, wbArcType).CheckWellFormedness())
                        {
                            Console.Error.WriteLine("Converted deptree is not well formed!");
                            problemfw.Write(oriToken, oriPoS, oriHId, oriArcType);
                            //problemfw.Write(wbToken, wbPoS, wbHid, wbArcType);
                            continue;
                        }
                        correctline++;

                        for (int i = 0; i < wbToken.Length; ++i)
                        {
                            if (wbToken[i] == "LSINGLEQUOTE")
                            {
                                wbToken[i] = "\'";
                                //Console.Error.WriteLine("!!");
                            }
                            else if (wbToken[i] == "RSINGLEQUOTE")
                            {
                                wbToken[i] = "'";
                            }
                            else if (wbToken[i] == "RDOUBLEQUOTE")
                            {
                                wbToken[i] = "\"";
                            }
                            else if (wbToken[i] == "LDOUBLEQUOTE")
                            {
                                wbToken[i] = "\"";
                            }
                        }

                        mtfw.Write(wbToken, wbPoS, wbHid, wbArcType);
                        treefw.WriteLine(new CTree(wbToken, wbPoS, wbHid, wbArcType).GetTxtTree());
                    }
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine("Error!");
                }
            }
            
            timer.Finish();
            Console.Error.WriteLine("{0} {1}", linenum, correctline);
            mtfr.Close();
            mtfw.Close();
            treefw.Close();
            problemfw.Close();
        }

        static void DoConvertChinese()
        {
            string orimalt = @"D:\users\nanyang\++++treebank\chinese\data\train.simple.conv";
            
            string maltoutfn = @"D:\users\nanyang\++++treebank\chinese\data\train.mttok";
            string treefn = @"D:\users\nanyang\++++treebank\chinese\data\train.mttree";

            string datapath = @"D:\users\nanyang\run\ChineseTokenizer\WBData";

            MaltTabFileReader mtfr = new MaltTabFileReader(orimalt);
            mtfr.SingleLineMode = false;
            
            MaltFileWriter mtfw = new MaltFileWriter(maltoutfn, false);
            mtfw.IsSingleLine = false;
            StreamWriter treefw = new StreamWriter(treefn, false);
            MaltFileWriter problemfw = new MaltFileWriter(@"D:\users\nanyang\++++treebank\chinese\data\problem.txt");

            TreeTokenizationConverter ttc = new TreeTokenizationConverter(new PoSConverter(), new DepArcConverter());

            ITokenizorWrapper tokenizor = new MTCWBreaker(datapath);

            RecoverEngWBAlign wbRecover = new RecoverEngWBAlign(tokenizor);
            int linenum = 0;
            int correctline = 0;

            ConsoleTimer timer = new ConsoleTimer(100);

            while (!mtfr.EndOfStream)
            {
                timer.Up();
                string[] oriToken;
                string[] oriPoS;
                int[] oriHId;
                string[] oriArcType;
                if (mtfr.GetNextSent(out oriToken, out oriPoS, out oriHId, out oriArcType))
                {
                    for (int i = 0; i < oriToken.Length; ++i)
                    {
                        if (oriToken[i] == "`")
                        {
                            oriToken[i] = "LSINGLEQUOTE";
                            //Console.Error.WriteLine("!!");
                        }
                        else if (oriToken[i] == "\'")
                        {
                            oriToken[i] = "RSINGLEQUOTE";
                        }
                        else if (oriToken[i] == "''")
                        {
                            oriToken[i] = "RDOUBLEQUOTE";
                        }
                        else if (oriToken[i] == "``")
                        {
                            oriToken[i] = "LDOUBLEQUOTE";
                        }
                    }

                    linenum++;
                    string alignStr;
                    string[] wbToken;
                    if (!wbRecover.WBwithAlign(oriToken, out wbToken, out alignStr))
                    {
                        continue;
                    }
                    AlignSpan alignSpan = new AlignSpan(oriToken.Length, wbToken.Length, alignStr);
                    if (!alignSpan.IsCorrectAlignSpan)
                    {
                        continue;
                    }
                    string[] wbPoS;
                    int[] wbHid;
                    string[] wbArcType;
                    if (!ttc.Convert(oriToken, oriPoS, oriHId, oriArcType, alignSpan, wbToken, out wbPoS, out wbHid, out wbArcType))
                    {
                        continue;
                    }

                    if (!new DepTree(wbToken, wbPoS, wbHid, wbArcType).CheckWellFormedness())
                    {
                        Console.Error.WriteLine("Converted deptree is not well formed!");
                        problemfw.Write(oriToken, oriPoS, oriHId, oriArcType);
                        //problemfw.Write(wbToken, wbPoS, wbHid, wbArcType);
                        continue;
                    }
                    correctline++;

                    for (int i = 0; i < wbToken.Length; ++i)
                    {
                        if (wbToken[i] == "LSINGLEQUOTE")
                        {
                            wbToken[i] = "\'";
                            //Console.Error.WriteLine("!!");
                        }
                        else if (wbToken[i] == "RSINGLEQUOTE")
                        {
                            wbToken[i] = "'";
                        }
                        else if (wbToken[i] == "RDOUBLEQUOTE")
                        {
                            wbToken[i] = "\"";
                        }
                        else if (wbToken[i] == "LDOUBLEQUOTE")
                        {
                            wbToken[i] = "\"";
                        }
                    }

                    mtfw.Write(wbToken, wbPoS, wbHid, wbArcType);
                    treefw.WriteLine(new CTree(wbToken, wbPoS, wbHid, wbArcType).GetTxtTree());
                }
            }
            timer.Finish();
            Console.Error.WriteLine("{0} {1}", linenum, correctline);
            mtfr.Close();
            //wbfr.Close();
            //alnfr.Close();
            mtfw.Close();
            treefw.Close();
            problemfw.Close();
        }

        static void DoConvertChinesePoS()
        {
            string orimalt = @"D:\users\nanyang\++++treebank\chinese\data\train.simple.conv";

            string maltoutfn = @"D:\users\nanyang\++++treebank\chinese\data\train.mttok";
            string treefn = @"D:\users\nanyang\++++treebank\chinese\data\train.mttree";

            string datapath = @"D:\users\nanyang\MSRA-SMT\CE-SMT\WordBreaker\CWB_20130311\Segment_Refine_Trans\Segment_Refine_Trans\bin\Release";

            MaltTabFileReader mtfr = new MaltTabFileReader(orimalt);
            mtfr.SingleLineMode = false;

            MaltFileWriter mtfw = new MaltFileWriter(maltoutfn, false);
            mtfw.IsSingleLine = false;
            StreamWriter treefw = new StreamWriter(treefn, false);
            MaltFileWriter problemfw = new MaltFileWriter(@"D:\users\nanyang\++++treebank\chinese\data\train.problem.txt");

            StreamWriter problemsnt = new StreamWriter(@"D:\users\nanyang\++++treebank\chinese\data\train.problem.snt");
            problemfw.IsSingleLine = false;

            TreeTokenizationConverter ttc = new TreeTokenizationConverter(new PoSConverter(), new DepArcConverter());

            ITokenizorWrapper tokenizor = new MTCWBreaker(datapath);

            RecoverEngWBAlign wbaligner = new RecoverEngWBAlign(tokenizor);

            int linenum = 0;
            int correctline = 0;

            ConsoleTimer timer = new ConsoleTimer(100);

            while (!mtfr.EndOfStream)
            {
                timer.Up();
                string[] oriToken;
                string[] oriPoS;
                int[] oriHId;
                string[] oriArcType;
                if (mtfr.GetNextSent(out oriToken, out oriPoS, out oriHId, out oriArcType))
                {
                    linenum++;

                    string rawline = string.Join("", oriToken);

                    string[] segparts = tokenizor.Tokenize(rawline);

                    string alignstr = null;

                    bool suc = false;

                    try
                    {
                        suc = wbaligner.RecoverAlign(oriToken, segparts, out alignstr);
                    }
                    catch
                    {
                    }

                    if (!suc)
                    {
                        problemfw.Write(oriToken, oriPoS, oriHId, oriArcType);
                        problemsnt.WriteLine(string.Join(" ", segparts));
                        continue;
                    }

                    //var map = LSC.RecoverRawEngInputMap_LCS(rawline, segparts);

                    //var goldSegs = CreateSegments(oriToken);

                    //var predictSegs = CreateSegments(segparts, map);

                    //var aligns = GetSegmentAlignment(goldSegs, predictSegs);

                    bool fail = false;

                    var aligns = GetSegmentAlignment(alignstr, oriToken, segparts);

                    List<string> ctoks = new List<string>();
                    List<string> cpos = new List<string>();

                    List<int> hids = new List<int>();
                    List<string> labels = new List<string>();
                    int gw = 0;

                    foreach (var align in aligns)
                    {
                        if (align.GoldSegs.Count == 1 && align.PredictSegs.Count == 1)
                        {
                            ctoks.Add(align.PredictSegs[0].Chars);
                            cpos.Add(oriPoS[gw]);
                            gw += align.GoldSegs.Count;
                            continue;
                        }
                        else if (align.GoldSegs.Count == 1)
                        {
                            for (int i = 0; i < align.PredictSegs.Count; ++i)
                            {
                                ctoks.Add(align.PredictSegs[i].Chars);
                                cpos.Add(oriPoS[gw] + "_X");
                                
                            }
                            gw += align.GoldSegs.Count;
                            continue;
                            //throw new Exception();
                        }
                        else if (align.PredictSegs.Count == 1)
                        {
                            bool consistentTags = true;
                            for (int i = 0; i < align.GoldSegs.Count; ++i)
                            {
                                if (oriPoS[gw + i] != oriPoS[gw])
                                {
                                    consistentTags = false;
                                    break;
                                }
                            }

                            if (consistentTags)
                            {
                                ctoks.Add(align.PredictSegs[0].Chars);
                                cpos.Add(oriPoS[gw]);
                                gw += align.GoldSegs.Count;
                                continue;
                            }
                            else
                            {
                                bool xhaveSpecial = false;

                                foreach (var p in align.PredictSegs)
                                {
                                    if (p.Chars.StartsWith("$"))
                                    {
                                        xhaveSpecial = true;
                                        break;
                                    }
                                }

                                if (xhaveSpecial)
                                {
                                    ctoks.Add(align.PredictSegs[0].Chars);
                                    cpos.Add(oriPoS[gw + align.GoldSegs.Count - 1]);
                                    gw += align.GoldSegs.Count;
                                    continue;
                                }
                                else
                                {
                                    for (int i = 0; i < align.GoldSegs.Count; ++i)
                                    {
                                        ctoks.Add(align.GoldSegs[i].Chars);
                                        cpos.Add(oriPoS[gw + i]);
                                    }
                                    gw += align.GoldSegs.Count;
                                    continue;
                                }
                            }
                        }
                        else
                        {
                            var gs = align.GoldSegs;
                            var ps = align.PredictSegs;

                            if (gs.Count == 2 && ps.Count == 2)
                            {
                                if (ps[0].Chars == "$number"
                                    && oriPoS[gw] == "CD" && oriPoS[gw + 1] == "M")
                                {
                                    ctoks.Add("$number");
                                    cpos.Add("CD");
                                    ctoks.Add(ps[1].Chars[ps[1].Chars.Length - 1].ToString());
                                    cpos.Add("M");
                                    gw += align.GoldSegs.Count;
                                    continue;
                                }
                            }
                              // rule based
                            if (gs.Count == 2 && gs[0].Chars == "最大" && gs[1].Chars == "规模")
                            {
                                ctoks.Add(gs[0].Chars);
                                cpos.Add(oriPoS[gw]);
                                ctoks.Add(gs[1].Chars);
                                cpos.Add(oriPoS[gw + 1]);
                                gw += align.GoldSegs.Count;
                                continue;
                            }

                            bool haveSpecial = false;

                            foreach (var p in ps)
                            {
                                if (p.Chars.StartsWith("$"))
                                {
                                    haveSpecial = true;
                                    break;
                                }
                            }

                            if (!haveSpecial)
                            {
                                for (int i = 0; i < gs.Count; ++i)
                                {
                                    ctoks.Add(gs[i].Chars);
                                    cpos.Add(oriPoS[gw + i]);
                                }
                                gw += align.GoldSegs.Count;
                                continue;
                            }

                            if (haveSpecial && ps.Count == gs.Count)
                            {
                                for (int i = 0; i < ps.Count; ++i)
                                {
                                    ctoks.Add(ps[i].Chars);
                                    cpos.Add(oriPoS[gw + i]);
                                }
                                gw += align.GoldSegs.Count;
                                continue;
                            }

                            if (ps.Count == 3 && ps[0].Chars == "$ord"
                                && ps[1].Chars == "条" && ps[2].Chars == "款")
                            {
                                ctoks.Add("$ord");
                                cpos.Add("OD");
                                ctoks.Add("条款");
                                cpos.Add("NN");
                                gw += align.GoldSegs.Count;
                                continue;
                            }

                            if (ps.Count == 2 && ps[0].Chars == "$date"
                                && ps[1].Chars == "中美")
                            {
                                ctoks.Add("$date");
                                cpos.Add("NT");
                                ctoks.Add("中");
                                cpos.Add("NR");
                                ctoks.Add("美");
                                cpos.Add("NR");
                                gw += align.GoldSegs.Count;
                                continue;
                            }

                            if (gs.Count == 4 && oriPoS[gw + 0] == "NT"
                                && oriPoS[gw + 1] == "CC" && oriPoS[gw + 2] == "NT"
                                && gs[3].Chars == "年度")
                            {
                                ctoks.Add("$date");
                                cpos.Add("NT");
                                ctoks.Add("至");
                                cpos.Add("CC");
                                ctoks.Add("$date");
                                cpos.Add("NT");
                                ctoks.Add("年度");
                                cpos.Add("NN");
                                gw += align.GoldSegs.Count;
                                continue;
                            }

                            if (gs.Count == 2 && oriPoS[gw + 0] == "NR"
                                && oriPoS[gw + 1] == "NT")
                            {
                                ctoks.Add(gs[0].Chars);
                                cpos.Add("NR");
                                ctoks.Add("$date");
                                cpos.Add("NT");
                                gw += align.GoldSegs.Count;
                                continue;
                            }

                            fail = true;
                            break;
                        }

                        throw new Exception();
                        gw += align.GoldSegs.Count;
                    }

                    if (fail)
                    {
                        problemfw.Write(oriToken, oriPoS, oriHId, oriArcType);
                        problemsnt.WriteLine(string.Join(" ", segparts));
                        continue;
                    }

                    for (int i = 0; i < ctoks.Count; ++i)
                    {
                        hids.Add(0);
                        labels.Add("ROOT");
                    }

                    mtfw.Write(ctoks.ToArray(), cpos.ToArray(), hids.ToArray(), labels.ToArray());
                    
                }
            }
            timer.Finish();
            Console.Error.WriteLine("{0} {1}", linenum, correctline);
            mtfr.Close();
            //wbfr.Close();
            //alnfr.Close();
            mtfw.Close();
            treefw.Close();
            problemfw.Close();
            problemsnt.Close();

        }

        static void FixTreeBank()
        {
            string dir = @"D:\users\nanyang\++++treebank\PTB-MTTokenized";

            string inputfile = @"wsj.02-21.mttok";

            string outputfile = @"wsj.02-21.fixhw.mttok";

            MaltTabFileReader maltr = new MaltTabFileReader(Path.Combine(dir, inputfile));
            MaltFileWriter maltw = new MaltFileWriter(Path.Combine(dir, outputfile));

            int totalline = 0;
            int modified = 0;
            int failure = 0;
            while (!maltr.EndOfStream)
            {
                ParserSentence ps;
                if (maltr.GetNextSent(out ps))
                {
                    totalline++;

                    bool isModified = false;

                    ParserSentence mps = ps.Clone();

                    for (int i = 0; i < mps.Length; ++i)
                    {
                        if (i + 1 < mps.Length && mps.tok[i] == "-"
                            && mps.tok[i + 1] == "-")
                        {
                            if (mps.DeleteWord(i))
                            {
                                mps.pos[i] = ":";
                                --i;
                                isModified = true;
                            }
                        }
                    }

                    if (isModified)
                    {
                        modified++;
                    }

                    DepTree tree = new DepTree(mps.tok, mps.pos, mps.hid, mps.label);
                    if (!DepTree.IsValidDepTree(tree) || !tree.CheckWellFormedness())
                    {
                        failure++;
                    }

                    maltw.Write(mps.tok, mps.pos, mps.hid, mps.label);
                }
            }

            Console.Error.WriteLine("{0} {1} {2}", totalline, modified, failure);

            maltw.Close();
            maltr.Close();

        }

        static string SimpleCTokenizor(string tok)
        {
            StringBuilder sb = new StringBuilder(tok);

            for (int i = 0; i < tok.Length; ++i)
            {
                sb[i] = CharConvertor.FulltoHalf(sb[i]);
            }

            string xtok = sb.ToString();

            if (xtok.StartsWith("www.") || xtok.StartsWith("www·"))
            {
                xtok = "$url";
            }

            //if (xtok.StartsWith("http"))
            //{
            //    xtok = "$url";
            //}

            return xtok;
        }

        static void ConvertToMalt()
        {
            string inputfn = @"D:\users\nanyang\++++treebank\chinese\data\test.zhang.malt";
            string outputfn = @"D:\users\nanyang\++++treebank\chinese\data\test.malt";

            using (StreamReader sr = new StreamReader(inputfn))
            {
                using (StreamWriter sw = new StreamWriter(outputfn))
                {
                    List<string> snt = new List<string>();
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine().Trim();

                        if (string.IsNullOrWhiteSpace(line))
                        {
                            if (snt.Count > 0)
                            {
                                sw.WriteLine(string.Join(" ", snt));
                            }

                            snt.Clear();
                        }
                        else
                        {
                            string[] parts = line.Split(new string[] { " ", "\t" }, StringSplitOptions.RemoveEmptyEntries);

                            if (parts.Length != 4)
                            {
                                throw new Exception();
                            }

                            snt.Add(string.Join("\t", parts));
                        }

                    }
                }
            }
        }

        static void SimpleTokenize()
        {
            string inputfn = @"D:\users\nanyang\++++treebank\chinese\data\test.malt";
            string outputfn = @"D:\users\nanyang\++++treebank\chinese\data\test.simple.conv";

            MaltTabFileReader mtfr = new MaltTabFileReader(inputfn);

            using (StreamWriter sw = new StreamWriter(outputfn))
            {

                while (!mtfr.EndOfStream)
                {
                    ParserSentence snt;
                    if (mtfr.GetNextSent(out snt))
                    {
                        for (int i = 0; i < snt.Length; ++i)
                        {
                            snt.tok[i] = SimpleCTokenizor(snt.tok[i]);

                            sw.WriteLine("{0}\t{1}\t{2}\t{3}", snt.tok[i], snt.pos[i], snt.hid[i], snt.label[i]);
                        }

                        sw.WriteLine();
                    }
                }

            }
            mtfr.Close();
        }

        static void GetPoSSequence()
        {
            string input = @"D:\users\nanyang\++++treebank\chinese\data\dev.mttok";
            string output = @"D:\users\nanyang\++++treebank\chinese\data\dev.pos.txt";

            MaltTabFileReader mtfr = new MaltTabFileReader(input);

            mtfr.SingleLineMode = false;

            using (StreamWriter sw = new StreamWriter(output))
            {
                while (!mtfr.EndOfStream)
                {
                    ParserSentence snt;

                    if (mtfr.GetNextSent(out snt))
                    {
                        List<string> pos = new List<string>();

                        foreach (var p in snt.pos)
                        {
                            if (p.EndsWith("_X"))
                            {
                                pos.Add(p.Substring(0, p.Length - 2));
                            }
                            else
                            {
                                pos.Add(p);
                            }
                        }

                        sw.WriteLine(string.Join(" ", pos));
                    }
                }
            }

            mtfr.Close();
        }

        static List<AlignedSegment> GetSegmentAlignment(List<Segment> Gold, List<Segment> Predict)
        {
            var aligns = new List<AlignedSegment>();

            var align = new AlignedSegment();

            int gnext = 0;
            int pnext = 0;

            while (gnext < Gold.Count && pnext < Predict.Count)
            {
                if (Gold[gnext].End == Predict[pnext].End)
                {
                    align.GoldSegs.Add(Gold[gnext]);
                    align.PredictSegs.Add(Predict[pnext]);
                    aligns.Add(align);
                    align = new AlignedSegment();
                    gnext++;
                    pnext++;
                }
                else if (Gold[gnext].End < Predict[pnext].End)
                {
                    align.GoldSegs.Add(Gold[gnext]);
                    gnext++;
                }
                else
                {
                    align.PredictSegs.Add(Predict[pnext]);
                    pnext++;
                }
            }

            if (gnext != Gold.Count || pnext != Predict.Count)
            {
                throw new Exception("Gold and predict does not match!");
            }

            return aligns;
        }

        static List<AlignedSegment> GetSegmentAlignment(string alignStr, string[] gold, string[] predict)
        {
            Dictionary<int, List<int>> goldToPred = new Dictionary<int, List<int>>();

            Dictionary<int, List<int>> predToGold = new Dictionary<int, List<int>>();

            GetSplitAlign(alignStr, goldToPred, predToGold);

            int gnext = 0;
            int pnext = 0;

            List<AlignedSegment> result = new List<AlignedSegment>();

            while (gnext < gold.Length && pnext < gold.Length)
            {
                var gpalign = goldToPred[gnext];

                var pmax = gpalign.Max();

                var pgalign = predToGold[pmax];

                var gmax = pgalign.Max();

                AlignedSegment a = new AlignedSegment();

                for (int i = gnext; i <= gmax; ++i)
                {
                    a.GoldSegs.Add(new Segment(gold[i], 0, 0));
                }

                for (int i = pnext; i <= pmax; ++i)
                {
                    a.PredictSegs.Add(new Segment(predict[i], 0, 0));
                }

                result.Add(a);

                gnext = gmax + 1;
                pnext = pmax + 1;
            }

            return result;
        }

        private static void GetSplitAlign(string alignStr, Dictionary<int, List<int>> goldToPred, Dictionary<int, List<int>> predToGold)
        {
            string[] alignparts = alignStr.Split(new string[] { " ", "\t" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var align in alignparts)
            {
                string[] parts = align.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);

                int g = int.Parse(parts[0]);
                int p = int.Parse(parts[1]);

                if (goldToPred.ContainsKey(g))
                {
                    goldToPred[g].Add(p);
                }
                else
                {
                    goldToPred[g] = new List<int>();
                    goldToPred[g].Add(p);
                }

                if (predToGold.ContainsKey(p))
                {
                    predToGold[p].Add(g);
                }
                else
                {
                    predToGold[p] = new List<int>();
                    predToGold[p].Add(g);
                }
            }
        }

        class Segment
        {
            public string Chars { get; set; }
            public int Start { get; set; }
            public int End { get; set; }

            public Segment() { }

            public Segment(string Chars, int Start, int End)
            {
                this.Chars = Chars;
                this.Start = Start;
                this.End = End;
            }
        }

        class AlignedSegment
        {
            public List<Segment> GoldSegs = new List<Segment>();
            public List<Segment> PredictSegs = new List<Segment>();
        }

        static List<Segment> CreateSegments(string[] segs, List<int> map)
        {
            int start = 0;

            List<Segment> result = new List<Segment>();

            for (int i = 0; i < segs.Length; ++i)
            {
                result.Add(new Segment(segs[i], start, map[i]));

                start = map[i];
            }

            return result;
        }

        static List<Segment> CreateSegments(string[] segs)
        {
            List<int> map = new List<int>();

            int end = 0;

            for (int i = 0; i < segs.Length; ++i)
            {
                end += segs[i].Length;

                map.Add(end);
            }

            return CreateSegments(segs, map);
        }


        static void DumpHunposFormat()
        {
            string inputfn = @"D:\users\nanyang\++++treebank\chinese\data\test.zhang.malt";
            string outputfn = @"D:\users\nanyang\++++treebank\chinese\data\test.hunpos";

            using (StreamReader sr = new StreamReader(inputfn))
            {
                using (StreamWriter sw = new StreamWriter(outputfn))
                {
                    List<string> snt = new List<string>();
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine().Trim();

                        if (string.IsNullOrWhiteSpace(line))
                        {
                            if (snt.Count > 0)
                            {
                                sw.Write(string.Join("\n", snt));
                                sw.Write("\n\n");
                            }

                            snt.Clear();
                        }
                        else
                        {
                            string[] parts = line.Split(new string[] { " ", "\t" }, StringSplitOptions.RemoveEmptyEntries);

                            if (parts.Length != 4)
                            {
                                throw new Exception();
                            }

                            snt.Add(string.Format("{0}\t{1}", parts[0], parts[1]));
                        }

                    }
                }
            }
        }

        static void DumpHunposTestFormat()
        {
            string inputfn = @"D:\users\nanyang\++++treebank\chinese\data\test.zhang.malt";
            string outputfn = @"D:\users\nanyang\++++treebank\chinese\data\test.word";

            using (StreamReader sr = new StreamReader(inputfn))
            {
                using (StreamWriter sw = new StreamWriter(outputfn))
                {
                    List<string> snt = new List<string>();
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine().Trim();

                        if (string.IsNullOrWhiteSpace(line))
                        {
                            if (snt.Count > 0)
                            {
                                sw.Write(string.Join("\n", snt));
                                sw.Write("\n\n");
                            }

                            snt.Clear();
                        }
                        else
                        {
                            string[] parts = line.Split(new string[] { " ", "\t" }, StringSplitOptions.RemoveEmptyEntries);

                            if (parts.Length != 4)
                            {
                                throw new Exception();
                            }

                            snt.Add(string.Format("{0}", parts[0]));
                        }

                    }
                }
            }
        }

        static void CompareCTBResult()
        {
            string goldf = @"D:\users\nanyang\++++treebank\chinese\data\test.hunpos";
            string predictf = @"C:\Users\v-nayang.MSLPA\Downloads\hunpos-1.0-win\hunpos-1.0-win\test.predict";

            int correct = 0;
            int total = 0;
            using (StreamReader srgold = new StreamReader(goldf))
            {
                using (StreamReader srpredict = new StreamReader(predictf))
                {
                    while (!srgold.EndOfStream && !srpredict.EndOfStream)
                    {
                        string goldline = srgold.ReadLine();
                        string predictline = srpredict.ReadLine();

                        if (string.IsNullOrWhiteSpace(goldline))
                        {
                            continue;
                        }

                        total++;
                        string[] gp = goldline.Split('\t');
                        string[] pp = predictline.Split('\t');

                        if (gp[1] == pp[1])
                        {
                            correct++;
                        }
                    }
                }
            }

            Console.Error.WriteLine("{0:F3}", correct / (double)total * 100.0);
        }

        static void DumpSentence()
        {
            string inputfn = @"D:\users\nanyang\++++treebank\chinese\data\test.zhang.malt";
            string outputfn = @"D:\users\nanyang\++++treebank\chinese\data\test.snt";

            using (StreamReader sr = new StreamReader(inputfn))
            {
                using (StreamWriter sw = new StreamWriter(outputfn))
                {
                    List<string> snt = new List<string>();
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine().Trim();

                        if (string.IsNullOrWhiteSpace(line))
                        {
                            if (snt.Count > 0)
                            {
                                sw.WriteLine(string.Join(" ", snt));
                            }

                            snt.Clear();
                        }
                        else
                        {
                            string[] parts = line.Split(new string[] { " ", "\t" }, StringSplitOptions.RemoveEmptyEntries);

                            if (parts.Length != 4)
                            {
                                throw new Exception();
                            }

                            snt.Add(string.Format("{0}", parts[0]));
                        }

                    }
                }
            }
        }

        static HashSet<string> GetSegments(string line)
        {
            string[] parts = line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            HashSet<string> seg = new HashSet<string>();

            int start = 0;

            foreach (var w in parts)
            {
                seg.Add(string.Format("{0}-{1}", start, w.Length));
                start += w.Length;
            }

            return seg;
        }

        static void CompareCTBSegment()
        {
            string gfile = @"D:\temp\test.snt";
            string pfile = @"D:\temp\test.predict.txt";

            int correct = 0;
            int ptotal = 0;
            int gtotal = 0;

            using (StreamReader sg = new StreamReader(gfile))
            {
                using (StreamReader sp = new StreamReader(pfile))
                {
                    while (!sg.EndOfStream && !sp.EndOfStream)
                    {
                        string gline = sg.ReadLine();
                        string pline = sp.ReadLine();

                        var gset = GetSegments(gline);
                        var pset = GetSegments(pline);

                        gtotal += gset.Count;
                        ptotal += pset.Count;

                        foreach (var s in gset)
                        {
                            if (pset.Contains(s))
                            {
                                correct++;
                            }
                        }
                        
                    }
                }
            }

            double p = correct / (double) ptotal;
            double r = correct / (double) gtotal;
            double f1 = 2.0 * p * r / (p + r);
            Console.Error.WriteLine("p={0}\tr={1}\tf1={2}", p * 100.0, r * 100.0, f1 * 100.0);
        }

        static void Main(string[] args)
        {
            //SimpleTokenize();
            //DoConvertChinesePoS();

            CompareCTBSegment();

            //CompareCTBResult();
            //DumpSentence();
            //CompareCTBResult();
            //DumpHunposTestFormat();

            //GetPoSSequence();
        }
    }
}
