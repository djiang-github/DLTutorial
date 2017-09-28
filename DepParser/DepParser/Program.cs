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
using DepPar;


namespace DepParser
{
    class Program
    {
        static void Main(string[] args)
        {
            ParseExample();
        }

        private static void ParseExample()
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

            DependencyParser deppar = new DependencyParser(tagger, cdecoder);

            string input = null;

            while ((input = Console.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(input))
                {
                    Console.Error.WriteLine("Input is empty");
                    continue;
                }

                string[] tok = input.Split(new string[] { " ", "\t" }, StringSplitOptions.RemoveEmptyEntries);

                string[] pos;
                int[] hid;
                string[] label;

                if (!deppar.Run(tok, out pos, out hid, out label))
                {
                    // This should not happen;
                    throw new Exception("Fail to parse");
                }
                else
                {
                    var tree = new CTree(tok, pos, hid, label);

                    Console.Error.WriteLine(tree.GetTxtTree());
                }
            }
        }

    }
}
