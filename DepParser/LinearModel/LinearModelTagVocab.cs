using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinearFunction
{
    public class LinearModelTagVocab
    {
        public LinearModelTagVocab(string[] tags)
        {
            
            tagIdDict = new Dictionary<string,int>();
            tagDict = new Dictionary<int,string>();

            for(int i = 0; i < tags.Length; ++i)
            {
                tagIdDict[tags[i]] = i;
                tagDict[i] = tags[i];
            }

            TagArr = (string[])tags.Clone();
        }

        public string TagString(int tagId)
        {
            return tagDict[tagId];
        }

        public int TagId(string tag)
        {
            int r;
            if (tag == null || !tagIdDict.TryGetValue(tag, out r))
            {
                return -1;
            }
            return r;
        }

        public int[] GetBinarizedTagWithPadding(string[] tags)
        {
            int[] btags = new int[tags.Length + 2];

            btags[0] = TagArr.Length;

            for (int i = 0; i < tags.Length; ++i)
            {
                btags[i + 1] = TagId(tags[i]);
            }

            return btags;
        }
        private Dictionary<string, int> tagIdDict;

        private Dictionary<int, string> tagDict;

        public string[] TagArr;
    }
}
