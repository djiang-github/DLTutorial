using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LinearFunction;

namespace EasyFirstDepPar
{
    public enum EasyFirstParserAction
    {
        LA,
        RA,
        NIL
    }

    public class EasyFirstParserVocab
    {
        public Dictionary<string, int> LabelName2IdDict;
        public string[] LabelId2Name;
        //public string[] commandNames;
        public EasyFirstParserAction[] command2Action;
        public int[] command2Label;
        public Dictionary<string, int> TokVocab;

        public EasyFirstParserVocab(LinearModelInfo lmInfo)
        {
            string[] strLabels = lmInfo.TagVocab.TagArr;
            string[] vocab = lmInfo.ModelVocab.VocabArr.ToArray();
            string[] actionArr = lmInfo.ActionVocab.TagArr;

            List<EasyFirstParserAction> actionList = new List<EasyFirstParserAction>();
            List<int> lablList = new List<int>();

            for (int i = 0; i < actionArr.Length; ++i)
            {
                string actionstr = actionArr[i];

                if (actionstr.StartsWith("LA"))
                {
                    actionList.Add(EasyFirstParserAction.LA);
                    string[] parts = actionstr.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    int lb = Array.FindIndex<string>(strLabels, x => (x == parts[1]));
                    if (lb < 0)
                    {
                        throw new Exception("bad templates!!");
                    }
                    lablList.Add(lb);
                }
                else if (actionstr.StartsWith("RA"))
                {
                    actionList.Add(EasyFirstParserAction.RA);
                    string[] parts = actionstr.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    int lb = Array.FindIndex<string>(strLabels, x => (x == parts[1]));
                    if (lb < 0)
                    {
                        throw new Exception("bad templates!!");
                    }
                    lablList.Add(lb);
                }
                else
                {
                    throw new Exception("bad templates!!");
                }
            }

            Init(vocab, strLabels, actionList.ToArray(), lablList.ToArray());
        }

        private void Init(string[] vocab, string[] strLabels, EasyFirstParserAction[] actions, int[] labels)
        {
            TokVocab = new Dictionary<string, int>();
            for (int i = 0; i < vocab.Length; ++i)
            {
                TokVocab[vocab[i]] = i + 2;
            }
            LabelCount = strLabels.Length;
            LabelId2Name = (string[])strLabels.Clone();
            command2Action = (EasyFirstParserAction[])actions.Clone();
            command2Label = (int[])labels.Clone();
            LabelName2IdDict = new Dictionary<string, int>();
            for (int i = 0; i < strLabels.Length; ++i)
            {
                LabelName2IdDict[strLabels[i]] = i;
            }
            lablToLACommand = new Dictionary<int, int>();
            lablToRACommand = new Dictionary<int, int>();
            for (int i = 0; i < actions.Length; ++i)
            {
                if (actions[i] == EasyFirstParserAction.LA)
                {
                    lablToLACommand[labels[i]] = i;
                }
                else if (actions[i] == EasyFirstParserAction.RA)
                {
                    lablToRACommand[labels[i]] = i;
                }
            }

            
        }

        public int[] BinarizeWithPadding(string[] tok)
        {
            int[] btok = new int[tok.Length + 2];
            btok[0] = 0;
            btok[btok.Length - 1] = 1;
            for (int i = 0; i < tok.Length; ++i)
            {
                int id;
                if (TokVocab.TryGetValue(tok[i], out id))
                {
                    btok[i + 1] = id;
                }
                else
                {
                    btok[i + 1] = -1;
                }
            }
            return btok;
        }

        public int[] BinarizeLabel(int[] hid, string[] labl)
        {
            int[] blabl = new int[labl.Length];

            for (int i = 0; i < labl.Length; ++i)
            {
                if (hid[i] == 0)
                {
                    blabl[i] = -1;
                }
                else
                {
                    if (!LabelName2IdDict.TryGetValue(labl[i], out blabl[i]))
                    {
                        blabl[i] = -1;
                    }
                }
            }

            return blabl;
        }

        public int[] BinarizeLabelWithPadding(int[] hid, string[] labl)
        {
            int[] blabl = new int[labl.Length + 2];

            for (int i = 0; i < labl.Length; ++i)
            {
                if (hid[i] == 0)
                {
                    blabl[i + 1] = -1;
                }
                else
                {
                    if (!LabelName2IdDict.TryGetValue(labl[i], out blabl[i + 1]))
                    {
                        blabl[i + 1] = -1;
                    }
                }
            }

            blabl[0] = -1;
            blabl[blabl.Length - 1] = -1;

            return blabl;
        }

        public int LabelCount { get; private set; }

        public int GetLabelId(string labelName)
        {
            int id;
            if (!LabelName2IdDict.TryGetValue(labelName, out id))
            {
                return -1;
            }
            else
            {
                return id;
            }
        }


        private Dictionary<int, int> lablToLACommand;
        private Dictionary<int, int> lablToRACommand;

        
        public int GetCommand(EasyFirstParserAction pa, int labl)
        {
            switch (pa)
            {
                case EasyFirstParserAction.LA:
                    int r;
                    if (!lablToLACommand.TryGetValue(labl, out r))
                    {
                        return -1;
                    }
                    return r;
                case EasyFirstParserAction.RA:
                    if (!lablToRACommand.TryGetValue(labl, out r))
                    {
                        return -1;
                    }
                    return r;
                default:
                    return -1;
            }
        }
    }
}
