//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.IO;

//using System.Diagnostics;

//using LinearFunction;
//using NanYUtilityLib.DepParUtil;
//using NanYUtilityLib;
//using NanYTokenizor;

//namespace PoSTag
//{
//    class Program
//    {
//        static void DoTag(string[] args)
//        {
//            Configure.SetArgs(args);

//            int BeamSize = Configure.GetOptionInt("BeamSize", 5);

//            string modelfile = Configure.GetOptionString("Model");

//            bool useTokenizor = Configure.GetOptionBool("Tokenize", true);

//            var ETagModel
//                = new PoSTagModelWrapper(
//                Configure.GetOptionString("Model")
//                );

//            var ETagDecoder = new PoSTagDecoderWrapper(ETagModel, BeamSize);

//            EnTokenizor tokenizor = useTokenizor ? new EnTokenizor() : null;

//            string inputfn = Configure.GetOptionString("Input");
//            string outputfn = Configure.GetOptionString("Output");

//            using (var mtfr = new StreamReader(inputfn))
//            {
//                using (var mtfw = new StreamWriter(outputfn, false))
//                {
//                    var timer = new NanYUtilityLib.Sweets.ConsoleTimer(100);

//                    while (!mtfr.EndOfStream)
//                    {
//                        string line = mtfr.ReadLine().Trim();
//                        timer.Up();
//                        if (string.IsNullOrWhiteSpace(line))
//                        {
//                            mtfw.WriteLine();
//                        }
//                        else
//                        {
//                            if (useTokenizor)
//                            {
//                                try
//                                {
//                                    var tmpline = tokenizor.TokenizeToString(line);
//                                    line = tmpline;
//                                }
//                                catch
//                                {
//                                    Console.Error.WriteLine("Failure to tokenize for sentence: {0}", line);
//                                }
//                            }
//                            string[] tok = line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
//                            string[] pos;
//                            if (!ETagDecoder.GenPOS(tok, out pos))
//                            {
//                                Console.Error.WriteLine("Failure to generate PoS for sentence: {0}", line);
//                                mtfw.WriteLine();
//                            }
//                            else
//                            {
//                                StringBuilder sb = new StringBuilder();

//                                for (int i = 0; i < tok.Length; ++i)
//                                {
//                                    if (sb.Length > 0)
//                                    {
//                                        sb.Append(' ');
//                                    }

//                                    sb.AppendFormat("{0}|{1}", tok[i], pos[i]);
//                                }
//                                mtfw.WriteLine(sb.ToString());
//                            }
//                        }

//                    }
//                    timer.Finish();
//                }
//            }
//        }

//        static void TagMalt(string[] args)
//        {
//            Configure.SetArgs(args);

//            int BeamSize = Configure.GetOptionInt("BeamSize", 5);

//            string modelfile = Configure.GetOptionString("Model");

//            var ETagModel
//                = new PoSTagModelWrapper(
//                Configure.GetOptionString("Model")
//                );

//            var ETagDecoder = new PoSTagDecoderWrapper(ETagModel, BeamSize);

//            string inputfn = Configure.GetOptionString("Input");
//            string outputfn = Configure.GetOptionString("Output");

//            var parseset = MaltTabFileReader.ReadAll(inputfn);

//            MaltFileWriter mtfw = new MaltFileWriter(outputfn);

//            DateTime time = DateTime.Now;
//            Stopwatch watch = new Stopwatch();

//            for (int i = 0; i < 10; ++i)
//            {

//                foreach (var snt in parseset)
//                {
//                    string[] ptag;

//                    watch.Start();
//                    if (ETagDecoder.GenPOS(snt.tok, out ptag))
//                    {
//                        snt.pos = ptag;
//                    }
//                    watch.Stop();
//                    mtfw.Write(snt.tok, snt.pos, snt.hid, snt.label);
//                }
//            }

//            Console.Error.WriteLine("SENT PER SEC: {0}",parseset.Count * 10 / (double)watch.Elapsed.TotalSeconds );

//            mtfw.Close();

//            //Console.Error.WriteLine("{0}",  parseset.Count / (DateTime.Now - time).TotalSeconds);
//        }
//        static void Main(string[] args)
//        {
//            Configure.SetArgs(args);

//            bool tagMalt = Configure.GetOptionBool("Malt", false);
//            //DoTag(args);
//            //TestTag(args);
//            //TagMaltChinese(args);
//            if (tagMalt)
//            {
//                TagMalt(args);
//            }
//            else
//            {
//                DoTag(args);
//            }
//            return;
//        }
//    }
//}
