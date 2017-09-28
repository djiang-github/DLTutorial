using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NanYUtilityLib.DepParUtil;

namespace DepPar
{
    public interface IParserDecoder
    {
        bool Run(string[] tok, string[] pos, out int[] hid, out string[] labl);
    }

    public class LRDecoder : IParserDecoder
    {
        public LRDecoder(LinearDepParser.ParserDecoder decoder)
        {
            this.decoder = decoder;
        }

        public bool Run(string[] tok, string[] pos, out int[] hid, out string[] labl)
        {
            return decoder.Run(tok, pos, out hid, out labl);
        }

        LinearDepParser.ParserDecoder decoder;
    }

    public class RLDecoder : IParserDecoder
    {
        public RLDecoder(LinearDepParser.ParserDecoder decoder)
        {
            this.decoder = decoder;
        }

        public bool Run(string[] tok, string[] pos, out int[] hid, out string[] labl)
        {
            string[] rtok = tok.Reverse<string>().ToArray<string>();
            string[] rpos = pos.Reverse<string>().ToArray<string>();

            if (decoder.Run(rtok, rpos, out hid, out labl))
            {
                var snt = new NanYUtilityLib.DepParUtil.ParserSentence
                (
                    tok,
                    pos,
                    hid,
                    labl
                );

                var rsnt = snt.Reverse();

                hid = rsnt.hid;
                labl = rsnt.label;

                return true;
            }

            return false;
        }

        LinearDepParser.ParserDecoder decoder;
    }

    public class EFDecoder : IParserDecoder
    {
        public EFDecoder(EasyFirstDepPar.EasyParDecoder decoder)
        {
            this.decoder = decoder;
        }

        public bool Run(string[] tok, string[] pos, out int[] hid, out string[] labl)
        {
            return decoder.Run(tok, pos, out hid, out labl);
        }

        EasyFirstDepPar.EasyParDecoder decoder;
    }

    public class MSTDecoder : IParserDecoder
    {
        public MSTDecoder(MSTParser.MSTDecoder decoder)
        {
            this.decoder = decoder;
        }

        public bool Run(string[] tok, string[] pos, out int[] hid, out string[] labl)
        {
            return decoder.Run(tok, pos, out hid, out labl);
        }

        MSTParser.MSTDecoder decoder;
    }

    public class MST2ODecoder : IParserDecoder
    {
        public MST2ODecoder(SecondOrderGParser.SecondOrderDecoder decoder)
        {
            this.decoder = decoder;
        }

        public bool Run(string[] tok, string[] pos, out int[] hid, out string[] labl)
        {
            if(!decoder.Run(tok, pos, out hid))
            {
                labl = null;
                return false;
            }

            labl =  new string[tok.Length];

            for(int i = 0; i < labl.Length; ++i)
            {
                labl[i] = "dep";
            }

            return true;
        }

        SecondOrderGParser.SecondOrderDecoder decoder;
    }

    public class CombineDecoder : IParserDecoder
    {
        public CombineDecoder(IParserDecoder[] decoders, float[] weight)
        {
            this.decoders = decoders;
            this.weight = weight;
            cmbt = new ParserCombinator();
        }

        public bool Run(string[] tok, string[] pos, out int[] hid, out string[] labl)
        {
            hid = null;
            labl = null;

            List<ParserSentence> sntList = new List<ParserSentence>();

            for (int i = 0; i < decoders.Length; ++i)
            {
                int[] xhid;
                string[] xlabl;
                if (!decoders[i].Run(tok, pos, out xhid, out xlabl))
                {
                    return false;
                }

                sntList.Add(new ParserSentence(tok, pos, xhid, xlabl));
            }

            ParserSentence combinedsnt;
            if (cmbt.Combine(sntList.ToArray(), weight, out combinedsnt))
            {
                hid = combinedsnt.hid;
                labl = combinedsnt.label;
                return true;
            }

            return false;
        }


        ParserCombinator cmbt;
        IParserDecoder[] decoders;
        float[] weight;
    }
}
