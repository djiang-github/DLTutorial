using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using LinearFunction;

namespace LinearDepParser
{
    public class ParserModelWrapper
    {
        public ParserModelWrapper(Stream fn)
        {
            IStateElementDiscriptor descriptor = new ParserStateDescriptor();

            lmInfo = new LinearModelInfo(fn, descriptor);
            pmInfo = new ParserModelInfo(lmInfo);
            parserModel = new BasicLinearFunction(lmInfo.FeatureTemplateCount, lmInfo.LinearFuncPackages);
        }

        public ParserModelWrapper(string fn)
        {
            IStateElementDiscriptor descriptor = new ParserStateDescriptor();

            lmInfo = new LinearModelInfo(fn, descriptor);
            pmInfo = new ParserModelInfo(lmInfo);
            parserModel = new BasicLinearFunction(lmInfo.FeatureTemplateCount, lmInfo.LinearFuncPackages);
        }

        public LinearModelInfo lmInfo { get; private set; }
        public ParserModelInfo pmInfo { get; private set; }

        public BasicLinearFunction parserModel { get; private set; }
        public ParserVocab vocab { get { return pmInfo.vocab; } }
    }
}
