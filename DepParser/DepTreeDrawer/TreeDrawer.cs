using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ParserUtil;
using NanYUtilityLib;
using NanYUtilityLib.DepParUtil;


namespace DepTreeDrawer
{
    class TreeDrawer
    {
        static class GV
        {
            public static string posfn = null;
            public static string testfn = null;
            public static string treefn = null;
            public static bool IsMaltSingleLine = true;
            public static bool LoadParam(string[] args)
            {
                Configure.SetArgs(args);
                testfn = Configure.GetOptionString("ParseData");
                treefn = Configure.GetOptionString("OutputFile");
                IsMaltSingleLine = Configure.GetOptionBool("SingleLineMalt", false);
                return testfn != null && treefn != null;
            }
        }

        

        static void Usage()
        {
            string helpMsg =
                "Draw dependency tree from malt file.\n" +
                "-ParseData         Input File\n" +
                "-OutputFile        Output File";
            Console.Error.WriteLine(helpMsg);
        }

        static Dictionary<string, string> LoadPosMapping(string mappingfile)
        {
            StreamReader sr = new StreamReader(mappingfile);
            Dictionary<string, string> posmap = new Dictionary<string, string>();
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                string[] parts = line.Split(new string[] { " ", "\t" }, StringSplitOptions.RemoveEmptyEntries);
                posmap.Add(parts[0], parts[1]);
            }
            sr.Close();
            return posmap;
        }

        public static void Run(string[] args)
        {
            if (!GV.LoadParam(args))
            {
                Usage();
                return;
            }
            Dictionary<string, string> posmap = null;
            if (GV.posfn != null)
            {
                posmap = LoadPosMapping(GV.posfn);
            }

            StreamWriter sw = new StreamWriter(GV.treefn);
            MaltTabFileReader mtr = new MaltTabFileReader(GV.testfn);
            mtr.SingleLineMode = GV.IsMaltSingleLine;

            IPoSMapper posmapper = new JPoSMapper();

            while (!mtr.EndOfStream)
            {
                string[] tok;
                string[] pos;
                int[] hid;
                string[] arc;
                if (mtr.GetNextSent(out tok, out pos, out hid, out arc))
                {
                    if (posmap != null)
                    {
                        for (int i = 0; i < pos.Length; ++i)
                        {
                            string pm;
                            if (posmap.TryGetValue(pos[i], out pm))
                            {
                                pos[i] = pm;
                            }
                        }
                    }
                    CTree ct = new CTree(new DepTree(tok, pos, hid, arc));
                    sw.WriteLine(ct.GetTxtTree(posmapper));
                }
                
            }
            sw.Close();
            mtr.Close();
            //TestParser test = new TestParser(GV.pmfn, GV.posfn, GV.ftfn);
            //test.Test(GV.testfn);
        }
    }
}
