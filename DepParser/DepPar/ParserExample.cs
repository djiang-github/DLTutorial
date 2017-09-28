using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ParserUtil;

namespace DepPar
{
    class ParserExample
    {
        public void RunExample()
        {
            string TaggerModelFileName = @"";
            string ParserModelFileName = @"";
            
            int TaggerBeamSize = 5;
            int ParserBeamSize = 8;

            // model is thread safe; i.e. multiple decoder can share one model.
            DepParModelWrapper model = new DepParModelWrapper(TaggerModelFileName, ParserModelFileName);

            // decoder is not thread safe; i.e. every thread should have its own decoder.
            DepParDecoderWrapper decoder = new DepParDecoderWrapper(model,
                new PoSTag.WSJCaseSensitiveObservGenerator(),
                TaggerBeamSize,
                ParserBeamSize,
                new PoSTag.MTWSJPoSDict());

            string[] Tokens = "This is a test .".Split(' ');

            string[] PoS;
            int[] HeadWordId;
            string[] ArcLabelType;

            if (decoder.Parse(Tokens, out PoS, out HeadWordId, out ArcLabelType))
            {
                // do something here...
            }
            else
            {
                // should never happens..
                Console.Error.WriteLine("Fail to parse");
            }

        }

        public void RunCExample()
        {
            string TaggerModelFileName = @"D:\users\nanyang\++++treebank\CTB-MTTokenized\output\ctag.ctb.mt.b8.model";
            string ParserModelFileName = @"D:\users\nanyang\++++treebank\CTB-MTTokenized\output\cparser.ctb.mt.b16.model";

            int TaggerBeamSize = 8;
            int ParserBeamSize = 16;

            // model is thread safe; i.e. multiple decoder can share one model.
            DepParModelWrapper model = new DepParModelWrapper(TaggerModelFileName, ParserModelFileName);

            // decoder is not thread safe; i.e. every thread should have its own decoder.
            DepParDecoderWrapper decoder = new DepParDecoderWrapper(model,
                new PoSTag.CTBMTObservGenerator(),
                TaggerBeamSize,
                ParserBeamSize,
                new PoSTag.MTCTBPoSDict());

            string[] Tokens = "中文 词性 标注 的 效果 很 差 .".Split(' ');

            string[] PoS;
            int[] HeadWordId;
            string[] ArcLabelType;

            if (decoder.Parse(Tokens, out PoS, out HeadWordId, out ArcLabelType))
            {
                // do something here...
                CTree tree = new CTree(Tokens, PoS, HeadWordId, ArcLabelType);
                Console.Error.WriteLine(tree.GetTxtTree());
                Console.Error.Write("!");
            }
            else
            {
                // should never happens..
                Console.Error.WriteLine("Fail to parse");
            }
        }
    }
}
