using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LinearFunction;

namespace LinearDepParser
{
    public enum ParserAction
    {
        SHIFT,
        REDUCE,
        LA,
        RA,
        NIL
    }

    public class ParserVocab
    {
        public Dictionary<string, int> LabelName2IdDict;
        public string[] LabelId2Name;
        //public string[] commandNames;
        public ParserAction[] command2Action;
        public int[] command2Label;
        public Dictionary<string, int> TokVocab;

        public ParserVocab(LinearModelInfo lmInfo)
        {
            string[] strLabels = lmInfo.TagVocab.TagArr;
            string[] vocab = lmInfo.ModelVocab.VocabArr.ToArray();
            string[] actionArr = lmInfo.ActionVocab.TagArr;

            List<ParserAction> actionList = new List<ParserAction>();
            List<int> lablList = new List<int>();
            
            for (int i = 0; i < actionArr.Length; ++i)
            {
                string actionstr = actionArr[i];
                
                if (actionstr.StartsWith("SH"))
                {
                    actionList.Add(ParserAction.SHIFT);
                    lablList.Add(-1);
                }
                else if (actionstr.StartsWith("RE"))
                {
                    actionList.Add(ParserAction.REDUCE);
                    lablList.Add(-1);
                }
                else if (actionstr.StartsWith("LA"))
                {
                    actionList.Add(ParserAction.LA);
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
                    actionList.Add(ParserAction.RA);
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

        private void Init(string[] vocab, string[] strLabels, ParserAction[] actions, int[] labels)
        {
            TokVocab = new Dictionary<string, int>();
            for (int i = 0; i < vocab.Length; ++i)
            {
                TokVocab[vocab[i]] = i + 2;
            }
            LabelCount = strLabels.Length;
            LabelId2Name = (string[])strLabels.Clone();
            command2Action = (ParserAction[])actions.Clone();
            command2Label = (int[])labels.Clone();
            LabelName2IdDict = new Dictionary<string, int>();
            for (int i = 0; i < strLabels.Length; ++i)
            {
                LabelName2IdDict[strLabels[i]] = i;
            }
            lablToLACommand = new Dictionary<int, int>();
            lablToRACommand = new Dictionary<int, int>();
            shiftCmd = -1;
            reduceCmd = -1;

            var LAList = new List<int>();
            var RAList = new List<int>();

            for (int i = 0; i < actions.Length; ++i)
            {
                if (actions[i] == ParserAction.SHIFT)
                {
                    shiftCmd = i;
                }
                else if (actions[i] == ParserAction.REDUCE)
                {
                    reduceCmd = i;
                }
                else if (actions[i] == ParserAction.LA)
                {
                    lablToLACommand[labels[i]] = i;
                    LAList.Add(i);
                }
                else if (actions[i] == ParserAction.RA)
                {
                    lablToRACommand[labels[i]] = i;
                    RAList.Add(i);
                }
            }

            LACommands = LAList.ToArray();
            RACommands = RAList.ToArray();

            if (shiftCmd < 0 || reduceCmd < 0)
            {
                throw new Exception("Error in model file!!!");
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
                if (tok[i] != null && TokVocab.TryGetValue(tok[i], out id))
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

        public int[] LACommands;
        public int[] RACommands;


        private Dictionary<int, int> lablToLACommand;
        private Dictionary<int, int> lablToRACommand;



        private int shiftCmd;
        private int reduceCmd;

        public int ShiftCMD { get { return shiftCmd; } }
        public int ReduceCMD { get { return reduceCmd; } }

        public int GetCommand(ParserAction pa, int labl)
        {
            switch (pa)
            {
                case ParserAction.SHIFT:
                    return shiftCmd;
                case ParserAction.REDUCE:
                    return reduceCmd;
                case ParserAction.LA:
                    int r;
                    if (!lablToLACommand.TryGetValue(labl, out r))
                    {
                        return -1;
                    }
                    return r;
                case ParserAction.RA:
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
