using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConvertTreeTokenization
{
    public class TreeTokenizationConverter
    {
        public TreeTokenizationConverter(PoSConverter PoSGen, DepArcConverter DepArcGen)
        {
            this.PoSGen = PoSGen;
            this.DepArcGen = DepArcGen;
        }

        public bool Convert(string[] oriToken, string[] oriPoS, int[] oriHId, string[] oriArcType,
            RecoverWBAlignment.AlignSpan align, string[] wbToken,
            out string[] wbPoS, out int[] wbHId, out string[] wbArcType)
        {
            if (!IsValidInput(oriToken, oriPoS, oriHId, oriArcType, align, wbToken)
                || !this.PoSGen.MapPoS(oriPoS, wbToken, align, out wbPoS)
                || !this.DepArcGen.MapDepArc(oriPoS, oriHId, oriArcType, wbToken, align, out wbHId, out wbArcType))
            {
                wbPoS = null;
                wbHId = null;
                wbArcType = null;
                return false;
            }

            System.Diagnostics.Debug.Assert(wbPoS != null && wbHId != null && wbArcType != null &&
                wbPoS.Length == wbHId.Length && wbPoS.Length == wbArcType.Length && wbPoS.Length == wbToken.Length);

            return true;
        }

        private bool IsValidInput(string[] oriToken, string[] oriPoS, int[] oriHId, string[] oriArcType,
            RecoverWBAlignment.AlignSpan align, string[] wbToken)
        {
            return oriToken != null &&
                oriPoS != null &&
                oriHId != null &&
                oriArcType != null &&
                align != null &&
                wbToken != null &&
                oriToken.Length == oriPoS.Length &&
                oriToken.Length == oriHId.Length &&
                oriToken.Length == oriArcType.Length &&
                oriToken.Length == align.SrcLen &&
                align.TrgLen == wbToken.Length;
        }

        private PoSConverter PoSGen;
        private DepArcConverter DepArcGen;
    }
}
