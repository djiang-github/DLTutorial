using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DepPar
{
    public interface IPoSTagger
    {
        bool Run(string[] tok, out string[] pos);
        bool Run(string[] tok, List<string>[] preTags, out string[] pos);
    }

    public class LRTagger : IPoSTagger
    {
        public LRTagger(PoSTag.PoSTagDecoderWrapper tagger)
        {
            this.tagger = tagger;
        }

        public bool Run(string[] tok, out string[] pos)
        {
            return tagger.GenPOS(tok, out pos);
        }

        public bool Run(string[] tok, List<string>[] preTags, out string[] pos)
        {
            if (preTags == null)
            {
                return tagger.GenPOS(tok, out pos);
            }
            else
            {
                return tagger.GenPOS(tok, preTags, out pos);
            }
        }

        PoSTag.PoSTagDecoderWrapper tagger;
    }
}
