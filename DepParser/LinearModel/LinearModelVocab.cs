using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinearFunction
{
    public class LinearModelVocab
    {
        public LinearModelVocab(string[] vocabs)
            :this(vocabs, 2)
        {
        }

        public LinearModelVocab(string[] vocabs, int offSet)
        {
            dict = new Dictionary<string, int>();
            for (int i = 0; i < vocabs.Length; ++i)
            {
                dict[vocabs[i]] = i + offSet;
            }

            VocabArr = new List<string>();
            foreach (string x in vocabs)
            {
                VocabArr.Add(x);
            }
        }

        public int[][] ConvertToBinary(string[][] observations)
        {
            int ObserveLength = observations[0].Length;
            int[][] bOb = new int[observations.Length + 2][];
            // create start and end padding
            bOb[0] = new int[ObserveLength];
            bOb[bOb.Length - 1] = new int[ObserveLength];

            for (int i = 0; i < ObserveLength; ++i)
            {
                bOb[0][i] = 0;
                bOb[bOb.Length - 1][i] = 1;
            }

            for (int i = 1; i < bOb.Length - 1; ++i)
            {
                bOb[i] = new int[ObserveLength];
                for (int j = 0; j < ObserveLength; ++j)
                {
                    int id = -1;
                    if (j < observations[i - 1].Length)
                    {
                        if (observations[i - 1][j] == null || !dict.TryGetValue(observations[i - 1][j], out id))
                        {
                            id = -1;
                        }
                    }
                    bOb[i][j] = id;
                }
            }
            return bOb;
        }

        public int VocabCount { get { return VocabArr.Count; } }

        private Dictionary<string, int> dict;

        public void AddVocab(string tok)
        {
            if (dict.ContainsKey(tok))
            {
                return;
            }

            dict[tok] = VocabArr.Count + 2;

            VocabArr.Add(tok);
        }

        public void AddVocab(List<string> toks)
        {
            if (toks == null)
            {
                return;
            }
            HashSet<string> newTok = new HashSet<string>();
            foreach (string t in toks)
            {
                if (!dict.ContainsKey(t) && !newTok.Contains(t))
                {
                    newTok.Add(t);
                }
            }

            if (newTok.Count == 0)
            {
                return;
            }

            //string[] newVocabArr = new string[VocabArr.Count + newTok.Count];

            //VocabArr.CopyTo(newVocabArr, 0);

            foreach (string t in toks)
            {
                AddVocab(t);
            }
        }

        public List<string> VocabArr;

        public int GetId(string word)
        {
            int id;
            return dict.TryGetValue(word, out id) ? id : -1;
        }
    }
}
