using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ParserUtil;
using NanYUtilityLib.DepParUtil;


namespace DepPar
{
    class ParseFragment
    {
        public int rootNR;
        public float score;
        public ParseFragment Head;
        public ParseFragment Mod;
        public bool IsTerminal { get { return Head == null || Mod == null; } }
    }

    class ParseCell
    {
        public int bidNR;
        public int eidNR;
        public int Length { get { return eidNR - bidNR; } }

        public bool IsEmptyCell { get { return fragmentList.Count <= 0; } }

        public List<ParseFragment> fragmentList;

        static public bool IsNullOrEmpty(ParseCell cell)
        {
            return cell == null || cell.IsEmptyCell;
        }

        public ParseCell()
        {
            fragmentList = new List<ParseFragment>();
        }

        public ParseCell(int bidNR, int spanLength)
        {
            this.bidNR = bidNR;
            eidNR = spanLength + bidNR;
            fragmentList = new List<ParseFragment>();
        }

        public bool Insert(ParseFragment frag)
        {
            for (int i = 0; i < fragmentList.Count; ++i)
            {
                if (fragmentList[i].rootNR == frag.rootNR)
                {
                    if (frag.score < fragmentList[i].score)
                    {
                        return false;
                    }
                    else
                    {
                        fragmentList.RemoveAt(i);
                        fragmentList.Add(frag);
                        return true;
                    }
                }
            }

            fragmentList.Add(frag);
            return true;
        }
    }

    class ParseChart
    {
        public readonly int Length;

        public ParseCell[,] Cells;

        public ParseChart(int Length)
        {
            this.Length = Length;
            Cells = new ParseCell[Length, Length];
            InitTerminalCells();
        }

        public bool Parse(LinkScores scoreDict)
        {
            for (int spanLen = 2; spanLen <= Length; ++spanLen)
            {
                for (int bidNR = 0; bidNR + spanLen <= Length; ++bidNR)
                {
                    int eidNR = bidNR + spanLen;
                    InitCell(bidNR, eidNR);
                    ComputeCell(bidNR, eidNR, scoreDict);
                }
            }

            ParseCell majorCell = GetCell(0, Length);
            
            if (ParseCell.IsNullOrEmpty(majorCell))
            {
                return false;
            }

            // add root score

            foreach (ParseFragment frag in majorCell.fragmentList)
            {
                int rootId = frag.rootNR;
                float score;
                if (scoreDict.TryGetScore(-1, rootId, out score))
                {
                    frag.score += score;
                }
                else
                {
                    frag.score = float.NegativeInfinity;
                }
            }
            return true;
        }

        public bool ExtractBestParse(out int[] headId)
        {
            ParseCell majorCell = GetCell(0, Length);

            if (ParseCell.IsNullOrEmpty(majorCell))
            {
                headId = null;
                return false;
            }

            

            // Find the best parse
            ParseFragment bestFrag = majorCell.fragmentList[0];

            for (int i = 1; i < majorCell.fragmentList.Count; ++i)
            {
                ParseFragment frag = majorCell.fragmentList[i];
                if (bestFrag.score < frag.score)
                {
                    bestFrag = frag;
                }
            }

            headId = new int[Length];

            headId[bestFrag.rootNR] = 0;

            HavestSubtree(bestFrag, headId);

            return true;
        }

        private void HavestSubtree(ParseFragment frag, int[] headId)
        {
            if (frag.IsTerminal)
            {
                return;
            }

            int headNR = frag.Head.rootNR;
            int modNR = frag.Mod.rootNR;

            headId[modNR] = headNR + 1;

            HavestSubtree(frag.Head, headId);
            HavestSubtree(frag.Mod, headId);
        }

        private void InitCell(int bidNR, int eidNR)
        {
            int spanLen = eidNR - bidNR;
            int cellFirst = spanLen - 1;
            int cellSecond = bidNR;
            Cells[cellFirst, cellSecond] = new ParseCell(bidNR, eidNR);
        }

        private void ComputeCell(int bidNR, int eidNR, LinkScores scoreDict)
        {
            ParseCell cell = GetCell(bidNR, eidNR);

            if (cell == null)
            {
                return;
            }

            for (int midNR = bidNR + 1; midNR < eidNR; ++midNR)
            {
                ParseCell leftCell = GetCell(bidNR, midNR);
                ParseCell rightCell = GetCell(midNR, eidNR);
                if (ParseCell.IsNullOrEmpty(leftCell) || ParseCell.IsNullOrEmpty(rightCell))
                {
                    continue;
                }
                foreach (ParseFragment leftFrag in leftCell.fragmentList)
                {
                    foreach (ParseFragment rightFrag in rightCell.fragmentList)
                    {
                        int leftRoot = leftFrag.rootNR;
                        int rightRoot = rightFrag.rootNR;

                        float score;
                        if (scoreDict.TryGetScore(leftRoot, rightRoot, out score))
                        {
                            ParseFragment frag = new ParseFragment
                            {
                                 rootNR = leftRoot,
                                 score = leftFrag.score + rightFrag.score + score,
                                 Head = leftFrag,
                                 Mod = rightFrag,
                            };
                            cell.Insert(frag);
                        }

                        if (scoreDict.TryGetScore(rightRoot, leftRoot, out score))
                        {
                            ParseFragment frag = new ParseFragment
                            {
                                rootNR = rightRoot,
                                score = leftFrag.score + rightFrag.score + score,
                                Head = rightFrag,
                                Mod = leftFrag,
                            };
                            cell.Insert(frag);
                        }
                    }
                }
            }
            
        }

        private ParseCell GetCell(int bidNR, int eidNR)
        {
            int spanLen = eidNR - bidNR;
            int cellFirst = spanLen - 1;
            int cellSecond = bidNR;
            return Cells[cellFirst, cellSecond];
        }

        private void InitTerminalCells()
        {
            for (int i = 0; i < Length; ++i)
            {
                Cells[0, i] = new ParseCell(i, 1);
                Cells[0, i].Insert(
                    new ParseFragment
                    {
                        rootNR = i,
                        score = 0,
                        Head = null,
                        Mod = null
                    }
                    );
            }
        }
    }

    class LinkScores
    {
        public LinkScores(int LenWR)
        {
            this.LenWR = LenWR;
            linkScoreDict = new Dictionary<int,float>();
        }

        public void AddScore(float score, int HeadIDNR, int ModIDNR)
        {
            int index = MakeIndex(HeadIDNR, ModIDNR);
            if(linkScoreDict.ContainsKey(index))
            {
                linkScoreDict[index] += score;
            }
            else
            {
                linkScoreDict[index] = score;
            }
        }
        
        public bool TryGetScore(int HeadIDNR, int ModIDNR, out float score)
        {
            return linkScoreDict.TryGetValue(MakeIndex(HeadIDNR, ModIDNR), out score);
        }

        private int MakeIndex(int HeadIDNR, int ModIDNR)
        {
            return LenWR * (HeadIDNR + 1) + ModIDNR + 1;
        }

        private int LenWR;
        Dictionary<int, float> linkScoreDict;
    }

    class ParserCombinator
    {
        public ParserCombinator()
        {

        }

        public bool Combine(ParserSentence[] ParsedSentences, float[] weights, out ParserSentence CombinedParse)
        {
            CombinedParse = null;
            for (int i = 1; i < ParsedSentences.Length; ++i)
            {
                if (ParsedSentences[i].Length != ParsedSentences[0].Length)
                {
                    return false;
                }
            }

            LinkScores linkSDict = GetLinkScore(ParsedSentences, weights);

            ParseChart chart = new ParseChart(ParsedSentences[0].Length);

            if (!chart.Parse(linkSDict))
            {
                return false;
            }

            int[] headId;

            if (!chart.ExtractBestParse(out headId))
            {
                return false;
            }

            string[] Labels = new string[headId.Length];

            for (int i = 0; i < Labels.Length; ++i)
            {
                if (headId[i] == 0)
                {
                    Labels[i] = "root";
                }
                else
                {
                    Labels[i] = "dep";

                    for (int j = 0; j < ParsedSentences.Length; ++j)
                    {
                        if (ParsedSentences[j].hid[i] == headId[i] && ParsedSentences[j].label[i] != "dep")
                        {
                            Labels[i] = ParsedSentences[j].label[i];
                            break;
                        }
                    }

                    if (Labels[i] == null)
                    {
                        Labels[i] = "dep";
                    }
                }
            }

            CombinedParse = new ParserSentence
            (
                ParsedSentences[0].tok,
                ParsedSentences[0].pos,
                headId,
                Labels
            );
            return true;
        }

        public bool Combine(ParserSentence[] ParsedSentences, Dictionary<string, float>[] weights, out ParserSentence CombinedParse)
        {
            CombinedParse = null;
            for (int i = 1; i < ParsedSentences.Length; ++i)
            {
                if (ParsedSentences[i].Length != ParsedSentences[0].Length)
                {
                    return false;
                }
            }

            LinkScores linkSDict = GetLinkScore(ParsedSentences, weights);

            ParseChart chart = new ParseChart(ParsedSentences[0].Length);

            if (!chart.Parse(linkSDict))
            {
                return false;
            }

            int[] headId;

            if (!chart.ExtractBestParse(out headId))
            {
                return false;
            }

            string[] Labels = new string[headId.Length];

            for (int i = 0; i < Labels.Length; ++i)
            {
                if (headId[i] == 0)
                {
                    Labels[i] = "root";
                }
                else
                {
                    for (int j = 0; j < ParsedSentences.Length; ++j)
                    {
                        if (ParsedSentences[j].hid[i] == headId[i])
                        {
                            Labels[i] = ParsedSentences[j].label[i];
                            break;
                        }
                    }

                    if (Labels[i] == null)
                    {
                        Labels[i] = "dep";
                    }
                }
            }

            CombinedParse = new ParserSentence
            (
                ParsedSentences[0].tok,
                ParsedSentences[0].pos,
                headId,
                Labels
            );
            return true;
        }

        
        private LinkScores GetLinkScore(ParserSentence[] ParsedSentences, float[] weights)
        {
            
            int LenWR = ParsedSentences[0].Length + 1;
            int RealTokLength = ParsedSentences[0].Length;

            LinkScores scoreDict = new LinkScores(LenWR);
            for (int i = 0; i < RealTokLength; ++i)
            {
                for (int pid = 0; pid < ParsedSentences.Length; ++pid)
                {
                    ParserSentence snt = ParsedSentences[pid];
                    float s = weights[pid];

                    int modId = i + 1;
                    int headId = snt.hid[i];

                    // penalize sick
                    if (headId != 0)
                    {
                        string HeadTok = snt.tok[headId - 1];
                        if (IsSickLink(HeadTok))
                        {
                            s -= SickLinkPenalty;
                        }

                        if (IsCrossingBracketLink(i, headId - 1, ParsedSentences[0].tok))
                        {
                            s -= SickLinkPenalty;
                        }

                        if (snt.label[i] == "nsubj" || snt.label[i] == "csubj"
                            || snt.label[i] == "dobj" || snt.label[i] == "iobj")
                        {
                            for (int cid = 0; cid < RealTokLength; ++cid)
                            {
                                if (cid != i && snt.hid[cid] == headId && snt.label[cid] == snt.label[i])
                                {
                                    s -= SickLinkPenalty;
                                }
                            }
                        }
                        //if(snt.tok[i] != "-" && (i == RealTokLength - 1 || snt.tok[i + 1] != "-"))
                        //{
                        //    if(snt.label[i] == "dep")
                        //    {
                        //        s -= SickLinkPenalty;
                        //    }
                        //}
                    }

                    scoreDict.AddScore(s, headId - 1, modId - 1);
                }
            }
            return scoreDict;
        }

        private LinkScores GetLinkScore(ParserSentence[] ParsedSentences, Dictionary<string, float>[] weights)
        {
            
            int LenWR = ParsedSentences[0].Length + 1;
            int RealTokLength = ParsedSentences[0].Length;

            LinkScores scoreDict = new LinkScores(LenWR);
            for (int i = 0; i < RealTokLength; ++i)
            {
                for (int pid = 0; pid < ParsedSentences.Length; ++pid)
                {
                    ParserSentence snt = ParsedSentences[pid];
                    float s;
                    if(!weights[pid].TryGetValue(ParsedSentences[pid].pos[i], out s))
                    {
                        s = 1;
                    }

                    int modId = i + 1;
                    int headId = snt.hid[i];

                    // penalize sick
                    if (headId != 0)
                    {
                        string HeadTok = snt.tok[headId - 1];
                        if (IsSickLink(HeadTok))
                        {
                            s -= SickLinkPenalty;
                        }
                    }

                    scoreDict.AddScore(s, headId - 1, modId - 1);
                }
            }
            return scoreDict;
        }

        private bool IsCrossingBracketLink(int modid, int headid, string[] tok)
        {
            int leftid = Math.Min(modid, headid);
            int rightid = Math.Max(modid, headid);
            int LRB = 0;
            int LSB = 0;
            int LCB = 0;
            for (int i = leftid + 1; i < rightid; ++i)
            {
                switch (tok[i])
                {
                    case "(":
                        LRB++;
                        break;
                    case "[":
                        LSB++;
                        break;
                    case "{":
                        LCB++;
                        break;
                    case ")":
                        LRB--;
                        break;
                    case "]":
                        LSB--;
                        break;
                    case "}":
                        LCB--;
                        break;
                    default:
                        break;
                }
            }

            if (headid < modid)
            {
                return LRB < 0 || LSB < 0 || LCB < 0;
            }
            else if(headid > modid)
            {
                return LRB > 0 || LSB > 0 || LCB > 0;
            }

            return false;
        }

        static ParserCombinator()
        {
            InitSickWordDict();
        }
        static private bool IsSickLink(string HeadWordToken)
        {
            string lcw = HeadWordToken.ToLower();
            return SickHeadWordSet.Contains(lcw);
        }
        static private void InitSickWordDict()
        {
            SickHeadWordSet = new HashSet<string>();
            string[] sw = {
                              "the", ".", ",", "!", "(",
                              ")", "{", "}", "\"", "...",
                              "?", ";", "!!", "!!!", "??",
                              "!?", "?!"
                          };
            foreach (string w in sw)
            {
                SickHeadWordSet.Add(w);
            }
        }
        static private HashSet<string> SickHeadWordSet;
        private const float SickLinkPenalty = 100.0f;

        
    }
}
