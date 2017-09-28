using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JapaneseChunker;

namespace ParserUtil
{
    public interface IPoSMapper
    {
        string Map(string Tag);
    }

    public class JPoSMapper:IPoSMapper
    {
        string IPoSMapper.Map(string Tag)
        {
            int tagNum;
            if (!int.TryParse(Tag, out tagNum))
            {
                return Tag;
            }

            string minor = posDict.MinorPoS(tagNum);

            if (minor != null)
            {
                return minor;
            }

            return Tag;
        }

        JPoSDict posDict = new JPoSDict();
    }
}
