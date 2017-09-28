using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LinearFunction;
using System.IO;

namespace EasyFirstDepPar
{
    public class EasyFirstParserModelWrapper
    {
        public EasyFirstParserModelWrapper(Stream fn)
        {
            IStateElementDiscriptor descriptor = new EasyFirstParserStateDescriptor();

            lmInfo = new LinearModelInfo(fn, descriptor);
            pmInfo = new EasyFirstParserModelInfo(lmInfo);
            parserModel = new BasicLinearFunction(lmInfo.FeatureTemplateCount, lmInfo.LinearFuncPackages);
        }

        public EasyFirstParserModelWrapper(string fn)
        {
            IStateElementDiscriptor descriptor = new EasyFirstParserStateDescriptor();

            lmInfo = new LinearModelInfo(fn, descriptor);
            pmInfo = new EasyFirstParserModelInfo(lmInfo);
            parserModel = new BasicLinearFunction(lmInfo.FeatureTemplateCount, lmInfo.LinearFuncPackages);
        }

        public LinearModelInfo lmInfo { get; private set; }
        public EasyFirstParserModelInfo pmInfo { get; private set; }

        public BasicLinearFunction parserModel { get; private set; }
        public EasyFirstParserVocab vocab { get { return pmInfo.vocab; } }
    }
}
