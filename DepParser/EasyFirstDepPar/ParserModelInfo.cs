using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LinearFunction;

namespace EasyFirstDepPar
{
    public class EasyFirstParserModelInfo
    {
        public EasyFirstParserModelInfo(LinearModelInfo lmInfo)
        {
            this.lmInfo = lmInfo;
            vocab = new EasyFirstParserVocab(lmInfo);
            PrepositionPoSId = lmInfo.ModelVocab.GetId("IN");
        }

        public int CommandCount { get { return lmInfo.ActionCount; } }
        public int LabelCount { get { return lmInfo.TagCount; } }

        public int PrepositionPoSId { get; private set; }

        public EasyFirstParserVocab vocab { get; private set; }
        public LinearModelInfo lmInfo { get; private set; }
    }
}
