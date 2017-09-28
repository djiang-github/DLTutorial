using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NanYUtilityLib;
using System.IO;
using NanYUtilityLib.DepParUtil;

namespace PoSTagTraining
{
    class Program
    {
        static void HackForLight()
        {
            string inputfn = @"D:\users\nanyang\++++treebank\English\light-tokenized\wsj.23.ltok";
            string outputfn = @"D:\users\nanyang\++++treebank\English\light-tokenized\wsj.23.lc.ltok";

            MaltTabFileReader mtfr = new MaltTabFileReader(inputfn);

            MaltFileWriter mtfw = new MaltFileWriter(outputfn);

            while (!mtfr.EndOfStream)
            {
                ParserSentence snt;

                if (mtfr.GetNextSent(out snt))
                {
                    for (int i = 0; i < snt.Length; ++i)
                    {
                        snt.tok[i] = snt.tok[i].ToLower();

                        if (snt.pos[i] == "NNP" || snt.pos[i] == "NNPS"
                            || snt.pos[i] == "NNS")
                        {
                            snt.pos[i] = "NN";
                        }
                    }

                    mtfw.Write(snt.tok, snt.pos, snt.hid, snt.label);

                }
            }

            mtfw.Close();

            mtfr.Close();

            
        }

        static void MakeNFoldTrainSet()
        {
            string datafn = @"D:\users\nanyang\++++treebank\English\stanford-converted-new\nyatok\train";
            int fold = 10;
            string outputPath = @"D:\users\nanyang\++++treebank\English\stanford-converted-new\nyatok\train";

            var datasets = new List<List<ParserSentence>>();
            
            var filenames = Directory.GetFiles(datafn, ("*.malt"));

            foreach (var fn in filenames)
            {
                var dataset = new List<ParserSentence>();

                var ds = MaltTabFileReader.ReadAll(fn);

                foreach (var ps in ds)
                {
                    dataset.Add(ps);
                }
                datasets.Add(dataset);
            }

            for (int fnum = 0; fnum < fold; ++fnum)
            {
                int testfold = fnum;
                int devfold = (fnum + 1) % fold;

                string dirName = Path.Combine(outputPath, fnum.ToString());

                if (!Directory.Exists(dirName))
                {
                    Directory.CreateDirectory(dirName);
                }

                string traindir = Path.Combine(dirName, "train");
                string devdir = Path.Combine(dirName, "dev");
                string testdir = Path.Combine(dirName, "test");

                if (!Directory.Exists(traindir))
                {
                    Directory.CreateDirectory(traindir);
                }

                if (!Directory.Exists(devdir))
                {
                    Directory.CreateDirectory(devdir);
                }

                if (!Directory.Exists(testdir))
                {
                    Directory.CreateDirectory(testdir);
                }

                string trainfn = Path.Combine(traindir, "train.malt");
                string devfn = Path.Combine(devdir, "dev.malt");
                string testfn = Path.Combine(testdir, "test.malt");

                MaltFileWriter mtwtrain = new MaltFileWriter(trainfn);
                MaltFileWriter mtwdev = new MaltFileWriter(devfn);
                MaltFileWriter mtwtest = new MaltFileWriter(testfn);

                for (int j = 0; j < datasets.Count; ++j)
                {
                    var dataset = datasets[j];

                    int teststart = GetStart(dataset.Count, fold, testfold);
                    int testend = GetEnd(dataset.Count, fold, testfold);
                    int devstart = GetStart(dataset.Count, fold, devfold);
                    int devend = GetEnd(dataset.Count, fold, devfold);
                    
                    for (int i = 0; i < dataset.Count; ++i)
                    {
                        var ps = dataset[i];
                        if (i >= teststart && i < testend)
                        {
                            mtwtest.Write(ps.tok, ps.pos, ps.hid, ps.label);
                        }
                        else if (i >= devstart && i < devend)
                        {
                            mtwdev.Write(ps.tok, ps.pos, ps.hid, ps.label);
                        }
                        else
                        {
                            mtwtrain.Write(ps.tok, ps.pos, ps.hid, ps.label);
                        }
                    }

                }
                mtwtrain.Close();
                mtwdev.Close();
                mtwtest.Close();
            }
        }

        static int GetStart(int count, int fold, int fnum)
        {
            int foldsize = count / fold;

            return fnum * foldsize;
        }

        static int GetEnd(int count, int fold, int fnum)
        {
            int foldsize = count / fold;

            return fnum == fold - 1 ? count : (fnum + 1) * foldsize;
        }

        static void Main(string[] args)
        {
            //MakeNFoldTrainSet();
            //return;
            Configure.SetArgs(args);

            //HackForLight();
            //ModelHeadGenerator mhg = new ModelHeadGenerator();
            //mhg.Generate();
            Train t = new Train();
            //t.DoCounting();
            //t.DoTrainBigram();
            //t.GenerateMaxEntTrainingData();
            //t.GenerateCRFTrainingData();
            t.DoTestBigram();
            //t.DoTrainBigramNFold();
            //t.DoTest();
            //t.DoNFold();
        }
    }
}
