using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PoSTag;
using LinearDepParser;
using LinearFunction;

namespace DepPar
{
    public class DepParModelWrapper
    {
        public DepParModelWrapper(string taggerfn, string parserfn)
        {
            Console.Error.WriteLine("Loading Parser Model...");
            parserwrapper = new ParserModelWrapper(parserfn);
            Console.Error.WriteLine("Loading PoS Model...");
            postagwrapper = new PoSTagModelWrapper(taggerfn);
        }

        public ParserModelWrapper parserwrapper;
        public PoSTagModelWrapper postagwrapper;
    }

    public class DepParDecoderWrapper
    {
        public DepParDecoderWrapper(DepParModelWrapper dpmw)
            :this(dpmw, 8, 8)
        {           
        }

        public DepParDecoderWrapper(DepParModelWrapper dpmw, int tagBeamSize, int parserBeamSize)
        {
            this.dpmw = dpmw;
            parserdc = new ParserDecoder(dpmw.parserwrapper.pmInfo, 
                                         dpmw.parserwrapper.parserModel,
                                         parserBeamSize);

            tagdc = new PoSTagDecoderWrapper(dpmw.postagwrapper, tagBeamSize);
        }

        public DepParDecoderWrapper(DepParModelWrapper dpmw, IObservGenerator PoSObservGen, int tagBeamSize, int parserBeamSize)
        {
            this.dpmw = dpmw;
            parserdc = new ParserDecoder(dpmw.parserwrapper.pmInfo,
                                         dpmw.parserwrapper.parserModel,
                                         parserBeamSize);

            tagdc = new PoSTagDecoderWrapper(dpmw.postagwrapper, tagBeamSize, PoSObservGen);
        }

        public DepParDecoderWrapper(DepParModelWrapper dpmw, IObservGenerator PoSObservGen, int tagBeamSize, int parserBeamSize, IPoSDict dict)
        {
            this.dpmw = dpmw;
            parserdc = new ParserDecoder(dpmw.parserwrapper.pmInfo,
                                         dpmw.parserwrapper.parserModel,
                                         parserBeamSize);

            tagdc = new PoSTagDecoderWrapper(dpmw.postagwrapper, tagBeamSize, PoSObservGen, dict);
        }

        public bool Parse(string[] tok, out string[] pos, out int[] hid, out string[] label)
        {
            pos = null;
            hid = null;
            label = null;

            if (!tagdc.GenPOS(tok, out pos))
            {
                return false;
            }
            else if(!parserdc.Run(tok, pos, out hid, out label))
            {
                return false;
            }
            return true;
        }

        public bool ParseNBest(string[] tok, out string[] pos, out int[][] hid, out string[][] label)
        {
            pos = null;
            hid = null;
            label = null;

            if (!tagdc.GenPOS(tok, out pos))
            {
                return false;
            }
            else if (!parserdc.Run(tok, pos, out hid, out label))
            {
                return false;
            }
            return true;
        }

        public bool Tag(string[] tok, out string[] pos)
        {
            pos = null;
            if (tagdc.GenPOS(tok, out pos))
            {
                return true;
            }

            return false;
        }

        public bool Tag(string[] tok, List<string>[] preTags, out string[] pos)
        {
            pos = null;
            if (tagdc.GenPOS(tok, preTags, out pos))
            {
                return true;
            }

            return false;
        }

        public bool Parse(string[] tok, string[] forcedTag, int[] forcedHid, string[] forcedLabel,
                          out string[] pos, out int[] hid, out string[] label)
        {
            pos = null;
            hid = null;
            label = null;
            
            if (forcedTag == null)
            {
                forcedTag = new string[tok.Length];
            }
            
            if (forcedHid == null)
            {
                forcedHid = new int[tok.Length];
                for (int i = 0; i < forcedHid.Length; ++i)
                {
                    forcedHid[i] = -1;
                }
            }
            
            if (forcedLabel == null)
            {
                forcedLabel = new string[tok.Length];
            }

            if (!tagdc.GenPOS(tok, forcedTag, out pos))
            {
                return false;
            }

            ParserForcedConstraints constraints = new ParserForcedConstraints(dpmw.parserwrapper.vocab, forcedHid, forcedLabel);

            if (!parserdc.Run(tok, pos, constraints, out hid, out label))
            {
                return false;
            }

            return true;
        }

        public bool ParseTagged(string[] tok, string[] pos, out int[] hid, out string[] label)
        {
            return parserdc.Run(tok, pos, out hid, out label);
        }

        ParserDecoder parserdc;
        PoSTagDecoderWrapper tagdc;
        DepParModelWrapper dpmw;
    }
}
