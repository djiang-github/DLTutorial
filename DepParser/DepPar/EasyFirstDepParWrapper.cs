using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PoSTag;
using EasyFirstDepPar;
using LinearFunction;

namespace DepPar
{
   
        public class EasyFirstDepParModelWrapper
        {
            public EasyFirstDepParModelWrapper(string taggerfn, string parserfn)
            {
                Console.Error.WriteLine("Loading Parser Model...");
                parserwrapper = new EasyFirstParserModelWrapper(parserfn);
                Console.Error.WriteLine("Loading PoS Model...");
                postagwrapper = new PoSTagModelWrapper(taggerfn);
            }

            public EasyFirstParserModelWrapper parserwrapper;
            public PoSTagModelWrapper postagwrapper;
        }

        public class EasyFirstDepParDecoderWrapper
        {
            public EasyFirstDepParDecoderWrapper(EasyFirstDepParModelWrapper dpmw)
                : this(dpmw, 8, 8)
            {
            }

            public EasyFirstDepParDecoderWrapper(EasyFirstDepParModelWrapper dpmw, int tagBeamSize, int parserBeamSize)
            {
                this.dpmw = dpmw;
                parserdc = new EasyParDecoder(dpmw.parserwrapper.pmInfo,
                                             dpmw.parserwrapper.parserModel
                                             );

                tagdc = new PoSTagDecoderWrapper(dpmw.postagwrapper, tagBeamSize);
            }

            public EasyFirstDepParDecoderWrapper(EasyFirstDepParModelWrapper dpmw, IObservGenerator PoSObservGen, int tagBeamSize, int parserBeamSize)
            {
                this.dpmw = dpmw;
                parserdc = new EasyParDecoder(dpmw.parserwrapper.pmInfo,
                                             dpmw.parserwrapper.parserModel
                                             );

                tagdc = new PoSTagDecoderWrapper(dpmw.postagwrapper, tagBeamSize, PoSObservGen);
            }

            public EasyFirstDepParDecoderWrapper(EasyFirstDepParModelWrapper dpmw, IObservGenerator PoSObservGen, int tagBeamSize, int parserBeamSize, IPoSDict dict)
            {
                this.dpmw = dpmw;
                parserdc = new EasyParDecoder(dpmw.parserwrapper.pmInfo,
                                             dpmw.parserwrapper.parserModel
                                             );

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

                
                if (!parserdc.Run(tok, pos, out hid, out label))
                {
                    return false;
                }

                return true;
            }

            public bool ParseTagged(string[] tok, string[] pos, out int[] hid, out string[] label)
            {
                return parserdc.Run(tok, pos, out hid, out label);
            }

            EasyParDecoder parserdc;
            PoSTagDecoderWrapper tagdc;
            EasyFirstDepParModelWrapper dpmw;
        }
    
}
