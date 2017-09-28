using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using LinearFunction;

namespace PoSTag
{
   
    public class PoSTagModelWrapper
    {
        public PoSTagModelWrapper(string fn)
        {
            modelInfo = new LinearChainModelInfo(fn);
            model = new BasicLinearFunction(modelInfo.FeatureTemplateCount, modelInfo.LinearFuncPackages);
        }

        public PoSTagModelWrapper(Stream fn)
        {
            modelInfo = new LinearChainModelInfo(fn);
            model = new BasicLinearFunction(modelInfo.FeatureTemplateCount, modelInfo.LinearFuncPackages);
        }

        public LinearChainModelInfo modelInfo { get; private set; }

        public BasicLinearFunction model { get; private set; }
    }

    public class PoSTagDecoderWrapper
    {
        public PoSTagDecoderWrapper(PoSTagModelWrapper ptmw, int BeamSize, IObservGenerator ObservGen, IPoSDict posDict, bool IsBigram)
        {
            beamsize = BeamSize;
            this.ptmw = ptmw;
            this.ObservGen = ObservGen;
            //decoder = new LinearChainDecoder(ptmw.modelInfo, ptmw.model, beamsize);

            //decoder = new BigramChainDecoder(ptmw.modelInfo, ptmw.model, beamsize);
            if (IsBigram)
            {
                decoder = new BigramChainDecoder(ptmw.modelInfo, ptmw.model, beamsize);
            }
            else
            {
                decoder = new LinearChainDecoder(ptmw.modelInfo, ptmw.model, beamsize);
            }
                
            this.posDict = posDict;
        }

        public PoSTagDecoderWrapper(PoSTagModelWrapper ptmw, int BeamSize, IObservGenerator ObservGen, IPoSDict posDict)
            :this(ptmw, BeamSize, ObservGen, posDict, true)
        {
        }

        public PoSTagDecoderWrapper(PoSTagModelWrapper ptmw, int BeamSize, IObservGenerator ObservGen)
        {
            beamsize = BeamSize;
            this.ptmw = ptmw;
            this.ObservGen = ObservGen;
            //decoder = //new LinearChainDecoder(ptmw.modelInfo, ptmw.model, beamsize);

            decoder = new BigramChainDecoder(ptmw.modelInfo, ptmw.model, beamsize);
        }

        public PoSTagDecoderWrapper(PoSTagModelWrapper ptmw, int BeamSize)
        {
            beamsize = BeamSize;
            this.ptmw = ptmw;
            ObservGen = new FlexibleGenerator(ptmw.modelInfo.ExtraInfo);
            if (ptmw.modelInfo.ExtraInfo.ContainsKey("TagDict"))
            {
                posDict = new FlexiblePoSDict(ptmw.modelInfo.ExtraInfo);
            }
            else
            {
                UsePoSDict = false;
            }
            decoder = new BigramChainDecoder(ptmw.modelInfo, ptmw.model, beamsize);
                //new LinearChainDecoder(ptmw.modelInfo, ptmw.model, beamsize);
        }

        public PoSTagDecoderWrapper(PoSTagModelWrapper ptmw)
            :this(ptmw, 8)
        {
        }

        public bool GenPOS(string[] tokens, out string[] pos)
        {
            string[][] obs = ObservGen.GenerateObserv(tokens);

            List<string>[] preTags = null;

            if (UsePoSDict)
            {
                preTags = new List<string>[tokens.Length];
                for (int i = 0; i < tokens.Length; ++i)
                {
                    preTags[i] = posDict.GetPoSCandidates(tokens[i]);
                }
            }

            bool success = decoder.RunMultiTag(obs, preTags, out pos);

            if (!success)
            {
                pos = null;
                return false;
            }

            return true;
        }

        public bool GenPOS(string[] tokens, int N, out List<string[]> posList)
        {
            posList = null;
            //throw new Exception("Not implemented yet!");
            string[][] obs = ObservGen.GenerateObserv(tokens);

            List<string>[] preTags = null;

            if (UsePoSDict)
            {
                preTags = new List<string>[tokens.Length];
                for (int i = 0; i < tokens.Length; ++i)
                {
                    preTags[i] = posDict.GetPoSCandidates(tokens[i]);
                }
            }
            List<float> scores;

            bool success = decoder.RunMultiTagNBest(obs, preTags, N, out posList, out scores);

            if (!success)
            {
                posList = null;
                return false;
            }

            return true;
            
        }

        public bool GenPOS(string[] tokens, string[] preTag, out string[] pos)
        {
            string[][] obs = ObservGen.GenerateObserv(tokens);

            List<string>[] preTags = null;

            
            preTags = new List<string>[tokens.Length];
            for (int i = 0; i < tokens.Length; ++i)
            {
                if (preTag[i] != null)
                {
                    preTags[i] = new List<string>();
                    preTags[i].Add(preTag[i]);
                }
                else if(UsePoSDict)
                {
                    preTags[i] = posDict.GetPoSCandidates(tokens[i]);
                }
            }
            

            bool success = decoder.RunMultiTag(obs, preTags, out pos);

            if (!success)
            {
                pos = null;
                return false;
            }

            return true;
        }

        public bool GenPOS(string[] tokens, List<string>[] preTag, out string[] pos)
        {
            string[][] obs = ObservGen.GenerateObserv(tokens);

            List<string>[] preTags = null;


            preTags = new List<string>[tokens.Length];
            for (int i = 0; i < tokens.Length; ++i)
            {
                if (preTag[i] != null && preTag[i].Count > 0)
                {
                    preTags[i] = preTag[i];
                }
                else if (UsePoSDict)
                {
                    preTags[i] = posDict.GetPoSCandidates(tokens[i]);
                }
            }

            bool success = decoder.RunMultiTag(obs, preTags, out pos);

            if (!success)
            {
                pos = null;
                return false;
            }

            return true;
        }

        ILinearChainDecoder decoder;

        PoSTagModelWrapper ptmw;

        IObservGenerator ObservGen;

        IPoSDict posDict;

        bool UsePoSDict = true;
        //BiDirAPDecoder bidirDecoder;

        //bool isBir = false;

        int beamsize;
    }

}
