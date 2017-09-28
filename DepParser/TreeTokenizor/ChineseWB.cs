using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MSRA.NLC.Lingo.NLP;

namespace MSRA.NLC.Lingo.NLP
{
    public class ChineseWordBreaker : ITokenizer
    {
        public ChineseWordBreaker(string splitResourceFile)
        {
            string chiParameters = "-tenglish 0 -numthre 11 -trepneforchilm 0 -twn 0 -dnne 0 -tcs 1 -tmb 1 ";
            chiParameters += @"-datapath " + splitResourceFile;
            engine = new Microsoft.MT.Common.Tokenization.SegRT(chiParameters);
        }

        public string[] Tokenize(string sentence, Dictionary<int, string> replaces)
        {
            string result = engine.preSegSent(sentence);

            int index = result.IndexOf("||||");
            if (index != -1)
            {
                string first = result.Substring(0, index).Trim();
                string second = result.Substring(index + 4).Trim();

                Parse(second, replaces);

                result = first;
            }

            string[] words = result.Split(splitCharSet, StringSplitOptions.RemoveEmptyEntries);
            if (words != null && words.Length > 0)
            {
                for (int i = 0; i < words.Length; ++i)
                {
                    string word = words[i];
                    int pos = word.LastIndexOf('/');
                    if (pos > 0)
                    {
                        words[i] = word.Substring(0, pos);
                    }
                }
            }

            return words;
        }

        private void Parse(string second, Dictionary<int, string> replaces)
        {
            string[] items = second.Split(splitCharSet2, StringSplitOptions.RemoveEmptyEntries);
            foreach (string item in items)
            {
                int index = item.IndexOf("|||");
                if (index == -1)
                    continue;

                try
                {
                    int id = Int32.Parse(item.Substring(0, index).Trim());

                    index = item.LastIndexOf("|||");
                    if (index == -1)
                        continue;

                    index += 3;
                    string value = item.Substring(index, item.Length - index).Trim();

                    if (!replaces.ContainsKey(id))
                    {
                        replaces.Add(id, value);
                    }
                }
                catch (Exception ex)
                {
                    replaces = new Dictionary<int, string>();
                    return;
                }
            }
        }

        public string[] Tokenize(string sentence)
        {
            if (string.IsNullOrWhiteSpace(sentence))
                return null;

            Dictionary<int, string> replaces = new Dictionary<int, string>();
            string[] words = Tokenize(sentence, replaces);
            if (replaces.Count > 0)
            {
                foreach (int index in replaces.Keys)
                {
                    if (index >= 0 && index < words.Length)
                    {
                        words[index] = replaces[index];
                    }
                }
            }

            return words;
        }

        public static readonly char[] splitCharSet = new char[] { ' ' };
        public static readonly char[] splitCharSet2 = new char[] { '{', '}' };

        private Microsoft.MT.Common.Tokenization.SegRT engine = null;

        public static readonly char[] splitCharset = new char[] { ' ' };
    }

}

