using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ParserUtil;
using NanYUtilityLib;

namespace DepPar
{
    public class DependencyParser
    {
        public DependencyParser(IPoSTagger tagger, IParserDecoder parser)
        {
            this.tagger = tagger;
            this.parser = parser;
        }

        private List<int> GetSentenceBreakingPoint(LRTreeNode<LRParseWord>[] pw)
        {
            List<int> sentenceBreakingPoint = new List<int>();
            for (int i = 2; i < pw.Length - 1; ++i)
            {
                var thisW = pw[i].Content;
                var nextW = pw[i + 1].Content;
                var preW = pw[i - 1].Content;

                if (thisW.Tok == ";")
                {
                    sentenceBreakingPoint.Add(i);
                    continue;
                }

                if (thisW.Tok == ".")
                {
                    if (nextW.Tok == "According" || nextW.Tok == "The")
                    {
                        sentenceBreakingPoint.Add(i);
                        continue;
                    }

                    if (preW.PoS != "NNP"
                        && preW.PoS != "NNPS" && preW.Tok != "$number"
                        )
                    {
                        sentenceBreakingPoint.Add(i);
                        continue;
                    }

                    if (nextW.PoS == "IN" && char.IsUpper(nextW.Tok[0]))
                    {
                        sentenceBreakingPoint.Add(i);
                        continue;
                    }
                }

                if (thisW.Tok == "-" || thisW.Tok == "–")
                {
                    if (nextW.Tok == "According" || nextW.Tok == "The")
                    {
                        sentenceBreakingPoint.Add(i);
                        continue;
                    }
                }
            }

            return sentenceBreakingPoint;
        }


        public bool Run(string[] tok, out string[] pos, out int[] hid, out string[] labl)
        {
            // remove $xmltag
            List<LRTreeNode<LRParseWord>> CleanWordList = new List<LRTreeNode<LRParseWord>>();
            List<LRTreeNode<LRParseWord>> AllWordList = new List<LRTreeNode<LRParseWord>>();

            List<string> xtokList = new List<string>();

            for (int i = 0; i < tok.Length; ++i)
            {
                var pword = new LRTreeNode<LRParseWord>(new LRParseWord());
                pword.Content.id = i + 1;
                pword.Content.Tok = tok[i];

                if (tok[i] != "$xmltag")
                {
                    CleanWordList.Add(pword);
                    xtokList.Add(tok[i]);
                }

                AllWordList.Add(pword);
            }

            if (CleanWordList.Count == 0)
            {
                pos = new string[tok.Length];
                hid = new int[tok.Length];
                labl = new string[tok.Length];
                for (int i = 0; i < tok.Length; ++i)
                {
                    pos[i] = ".";
                    hid[i] = 0;
                    labl[i] = "root";
                }

                return true;
            }

            string[] xtok = xtokList.ToArray();

            string[] xpos;
            int[] xhid;
            string[] xlabl;
            
            if (!DoParsing(xtok, out xpos, out xhid, out xlabl))
            {
                pos = null;
                hid = null;
                labl = null;
                return false;
            }

            if (CleanWordList.Count == tok.Length)
            {
                pos = xpos;
                hid = xhid;
                labl = xlabl;
                return true;
            }

            pos = new string[tok.Length];
            hid = new int[tok.Length];
            labl = new string[tok.Length];

            for (int i = 0; i < xhid.Length; ++i)
            {
                var mod = CleanWordList[i];

                int modid = mod.Content.id;

                pos[modid - 1] = xpos[i];
                labl[modid - 1] = xlabl[i];

                if (xhid[i] == 0)
                {
                    hid[modid - 1] = 0;
                }
                else
                {
                    var head = CleanWordList[xhid[i] - 1];
                    hid[modid - 1] = head.Content.id;
                }
            }

            // attach $xmltag

            for (int i = 0; i < tok.Length; ++i)
            {
                if (tok[i] == "$xmltag")
                {
                    pos[i] = "XX";
                    labl[i] = "slice";
                    
                    hid[i] = i;
                    
                    
                }
            }
            return true;
        }

        private bool DoParsing(string[] tok, out string[] pos, out int[] hid, out string[] labl)
        {
            LRTreeNode<LRParseWord>[] pw = new LRTreeNode<LRParseWord>[tok.Length + 1];
            //int[] hids = new int[OriginalSentence.Length + 1];



            if (!DoTaggingAndParsing(tok, pw, null, out pos, out hid, out labl))
            {
                return false;
            }

            if (!UseTaggingParsingIteration)
            {
                return true;
            }

            List<string>[] preTags = new List<string>[tok.Length];
            if (!FixPoSTags(preTags, pw[0]))
            {
                return true;
            }

            return DoTaggingAndParsing(tok, pw, preTags, out pos, out hid, out labl);
        }


        private bool FixPoSTags(List<string>[] preTags, LRTreeNode<LRParseWord> treeNode)
        {
            // only root node needs to be fixed

            if (!treeNode.HaveChild)
            {
                return false;
            }

            bool updated = false;

            if (treeNode.Content.id != 0)
            {
                int id = treeNode.Content.id;
                string pos = treeNode.Content.PoS;
                if (pos == "NN" || pos == "NNS" || pos == "NNP"
                    || pos == "NNPS")
                {
                    bool isVerb = false;
                    foreach (var chd in treeNode.Children)
                    {
                        string label = chd.Content.Label;
                        if (label == "nsubj" || label == "nsubjpass"
                        || label == "csubj" || label == "csubjpass"
                        || label == "dobj" || label == "iobj"
                        || label == "advcl" || label == "purpcl"
                        || label.EndsWith("comp") || label == "attr"
                        || (label == "advmod" && !chd.Content.PoS.StartsWith("J")))
                        {
                            isVerb = true;
                            break;
                        }
                    }

                    if (isVerb)
                    {
                        preTags[id - 1] = new List<string>(new string[] {"VB", "VBD", "VBP", "VBN", "VBZ", "VBG"});
                        updated = true;
                        //preTags[id - 1]
                    }
                }
            }

            foreach (var chd in treeNode.Children)
            {
                updated = updated || FixPoSTags(preTags, chd);
            }

            return updated;
        }

        private bool DoTaggingAndParsing(string[] tok, LRTreeNode<LRParseWord>[] pw, List<string>[] preTags, out string[] pos, out int[] hid, out string[] labl)
        {
            pw[0] = new LRTreeNode<LRParseWord>(new LRParseWord());
            pw[0].Content.id = 0;

            if (!tagger.Run(tok, preTags, out pos))
            {
                pos = null;
                hid = null;
                labl = null;
                return false;
            }

            for (int i = 0; i < tok.Length; ++i)
            {
                pw[i + 1] = new LRTreeNode<LRParseWord>(new LRParseWord
                {
                    Tok = tok[i],
                    PoS = pos[i],
                    Label = null,
                    id = i + 1
                });

            }




            List<int> traceList = new List<int>();
            List<int> headIdList = new List<int>();

            bool isSpecial = false;

            if (tok.Length >= 3)
            {
                int last = tok.Length - 1;
                if (tok[last] == ":"
                    && tok[last - 1] == "Text"
                    && tok[last - 2] == "Full")
                {
                    isSpecial = true;
                    if (!Run(pw, preTags, 0, 1, pw.Length - 3) || !Run(pw, preTags, 0, pw.Length - 3, pw.Length))
                    {
                        pos = null;
                        hid = null;
                        labl = null;
                        return false;
                    }
                }
            }
            else if (tok.Length >= 2)
            {
                int last = tok.Length - 1;
                if (tok[last] == "Text"
                    && tok[last - 1] == "Full"
                    )
                {
                    isSpecial = true;
                    if (!Run(pw, preTags, 0, 1, pw.Length - 2) || !Run(pw, preTags, 0, pw.Length - 2, pw.Length))
                    {
                        pos = null;
                        hid = null;
                        labl = null;
                        return false;
                    }
                }
            }
            if (!isSpecial)
            {
                if (UseSentenceBreaking)
                {
                    var breakingPoint = GetSentenceBreakingPoint(pw);
                    pos = null;
                    hid = null;
                    labl = null;
                    if (breakingPoint.Count > 0)
                    {
                        for (int i = 0; i < breakingPoint.Count; ++i)
                        {
                            int start = i == 0 ? 1 : breakingPoint[i - 1];
                            int end = breakingPoint[i] + 1;
                            if (!Run(pw, preTags, 0, start, end))
                            {
                                return false;
                            }
                            if (i == breakingPoint.Count - 1)
                            {
                                if (!Run(pw, preTags, 0, end, pw.Length))
                                {
                                    return false;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (!Run(pw, preTags, 0, 1, pw.Length))
                        {
                            return false;
                        }
                    }
                }
                else
                {

                    if (!Run(pw, preTags, 0, 1, pw.Length))
                    {
                        pos = null;
                        hid = null;
                        labl = null;
                        return false;
                    }
                }
            }



            GetVisitTrace(pw[0], traceList, headIdList);

            pos = new string[tok.Length];
            hid = new int[tok.Length];
            labl = new string[tok.Length];

            for (int i = 0; i < traceList.Count - 1; ++i)
            {
                pos[i] = pw[i + 1].Content.PoS;
                labl[i] = pw[i + 1].Content.Label;
                hid[i] = pw[i + 1].parent.Content.id;//headIdList[i + 1];

                if (hid[i] == 0)
                {
                    labl[i] = "root";
                }
            }

            return true;
        }

        private void GetVisitTrace(LRTreeNode<LRParseWord> node, List<int> trace, List<int> HeadIds)
        {
            if (node.HaveLeftChild)
            {
                foreach (var chd in node.LeftChildren)
                {
                    GetVisitTrace(chd, trace, HeadIds);
                }
            }

            if (node.HaveParent)
            {
                trace.Add(node.Content.id);
                HeadIds.Add(node.HaveParent ? node.parent.Content.id : -1);
            }
            else
            {
                trace.Add(0);
                HeadIds.Add(node.HaveParent ? node.parent.Content.id : -1);
            }
            if (node.HaveRightChild)
            {
                foreach (var chd in node.RightChildren)
                {
                    GetVisitTrace(chd, trace, HeadIds);
                }
            }
        }


        private bool Run(LRTreeNode<LRParseWord>[] Terminals, List<string>[] preTags, int rootId, int start, int end)
        {
            if(start >= end)
            {
                return true;
            }

            

            if(start == end - 1)
            {
                string[] xtok = { Terminals[start].Content.Tok };
                string[] xpos;
                List<string>[] xPreTags = new List<string>[1];
                
                if (preTags != null && preTags[start - 1] != null && start > 0)
                {
                    xPreTags[0] = preTags[start - 1];
                }
                if(!tagger.Run(xtok, xPreTags, out xpos))
                {
                    return false;
                }

                Terminals[start].Content.PoS = xpos[0];
                Terminals[start].Content.Label = "slice";
                Attach(Terminals, rootId, start);
                return true;
            }

            int lbracket;
            int rbracket;
            FindBracketStructure(Terminals, start, end, out lbracket, out rbracket);

            if (lbracket < 0 && rbracket < 0)
            {
                // do clean parse
                return ParseClean(Terminals, preTags, rootId, start, end);
            }
            else if (!(lbracket >= 0 && rbracket >= 0))
            {
                int bracket = lbracket >= 0 ? lbracket : rbracket;
                string pos = lbracket >= 0 ? "LRB" : "RRB";

                Terminals[bracket].Content.PoS = pos;
                Terminals[bracket].Content.Label = "slice";
                Attach(Terminals, rootId, bracket);

                return Run(Terminals, preTags, rootId, start, bracket)
                    && Run(Terminals, preTags, rootId, bracket + 1, end);
            }
            else
            {
                if (lbracket == start || rbracket == end - 1)
                {
                    Terminals[lbracket].Content.PoS = "LRB";
                    Terminals[lbracket].Content.Label = "slice";
                    Attach(Terminals, rootId, lbracket);

                    return Run(Terminals, preTags, rootId, start, lbracket)
                        && Run(Terminals, preTags, rootId, lbracket + 1, end);
                }
                else
                {
                    // most annoying case
                    var xterminals = new List<LRTreeNode<LRParseWord>>();
                    List<List<string>> xpretags = preTags == null ? null : new List<List<string>>();
                    xterminals.Add(Terminals[rootId]);
                    for (int i = start; i < end; ++i)
                    {
                        if (i < lbracket || i > rbracket)
                        {
                            if (preTags != null)
                            {
                                xpretags.Add(preTags[i - 1]);
                            }
                            xterminals.Add(Terminals[i]);
                        }
                    }

                    if (!Run(xterminals.ToArray(), preTags == null ? null : xpretags.ToArray(), 0, 1, xterminals.Count))
                    {
                        return false;
                    }

                    // find attach point
                    int rootwid = Terminals[rootId].Content.id;

                    LRTreeNode<LRParseWord> lnode = Terminals[lbracket - 1];
                    while (lnode.Content.id != rootwid)
                    {
                        if (lnode.Content.Label != "punct" || lnode.parent.Content.id > lbracket - 1)
                        {
                            break;
                        }
                        lnode = lnode.parent;
                    }

                    int newrootid = -1;

                    if (lnode.Content.id == rootwid)
                    {
                        newrootid = rootId;
                    }
                    else
                    {
                        for (int i = start; i < end; ++i)
                        {
                            if (lnode.Content.id == Terminals[i].Content.id)
                            {
                                newrootid = i;
                                break;
                            }
                        }
                    }

                    if (newrootid < 0)
                    {
                        return false;
                    }

                    return Run(Terminals, preTags, newrootid, lbracket, rbracket + 1);
                }
                
            }
            
        }

        private static void FindBracketStructure(LRTreeNode<LRParseWord>[] Terminals, int start, int end, out int lbracket, out int rbracket)
        {
            Stack<int> BracketStack = new Stack<int>();

            lbracket = -1;
            rbracket = -1;

            for (int i = start; i < end; ++i)
            {
                string t = Terminals[i].Content.Tok;

                if (t == "("
                    || t == "["
                    || t == "{")
                {
                    BracketStack.Push(i);
                }
                else if (t == "]"
                   || t == ")"
                   || t == "}")
                {
                    if (BracketStack.Count <= 0)
                    {
                        rbracket = i;
                        break;
                    }
                    int lid = BracketStack.Pop();

                    string lt = Terminals[lid].Content.Tok;

                    if (!(lt == "(" && t == ")")
                        && !(lt == "[" && t == "]")
                        && !(lt == "{" && t == "}"))
                    {
                        rbracket = i;
                        break;
                    }

                    lbracket = lid;
                    rbracket = i;
                    break;
                }
            }

            if (lbracket < 0 && rbracket < 0 && BracketStack.Count > 0)
            {
                lbracket = BracketStack.Pop();
            }
        }

        private List<string>[] GetPreTags(List<string>[] preTags, int start, int end)
        {
            if (preTags == null || end - start <= 0 || start <= 0 || end > preTags.Length + 1)
            {
                return null;
            }

            List<string>[] xtags = new List<string>[end - start];

            for (int i = 0; i < xtags.Length; ++i)
            {
                xtags[i] = preTags[i + start - 1];
            }

            return xtags;
        }

        private bool ParseClean(LRTreeNode<LRParseWord>[] Terminals, List<string>[] preTags, int rootId, int start, int end)
        {
            if (start >= end)
            {
                return true;
            }

            if (start == end - 1)
            {
                string[] xtok = { Terminals[start].Content.Tok };
                string[] xpos;
                var pret = GetPreTags(preTags, start, end);
                if (!tagger.Run(xtok, pret, out xpos))
                {
                    return false;
                }

                Terminals[start].Content.PoS = xpos[0];
                Terminals[start].Content.Label = "slice";
                Attach(Terminals, rootId, start);
                return true;
            }

            string[] tok = new string[end - start];
            string[] pos;
            for (int i = 0; i < tok.Length; ++i)
            {
                tok[i] = Terminals[i + start].Content.Tok;
            }

            var xpret = GetPreTags(preTags, start, end);

            if (!tagger.Run(tok, xpret, out pos))
            {
                return false;
            }

            int[] hid;
            string[] labl;

            if (parser.Run(tok, pos, out hid, out labl))
            {
                for (int i = 0; i < hid.Length; ++i)
                {
                    int modId = start + i;
                    Terminals[modId].Content.PoS = pos[i];
                    if (hid[i] == 0)
                    {
                        Terminals[modId].Content.Label = "slice";
                        Attach(Terminals, rootId, modId);
                    }
                    else
                    {
                        int headId = start + hid[i] - 1;
                        Terminals[modId].Content.Label = labl[i];
                        Attach(Terminals, headId, modId);
                    }
                }

                return true;
            }

            return false;
        }

        private void Attach(LRTreeNode<LRParseWord>[] Terminals, int rootId, int modId)
        {
            LRTreeNode<LRParseWord> head = Terminals[rootId];
            LRTreeNode<LRParseWord> dep = Terminals[modId];

            dep.RemoveWithTrace();

            if (dep.Content.id < head.Content.id)
            {
                // leftchild
                if (head.HaveLeftChild)
                {
                    if (head.farlchild.Content.id > dep.Content.id)
                    {
                        head.InsertLeftChildFar(dep);
                    }
                    else
                    {
                        LRTreeNode<LRParseWord> lchd = head.farlchild;

                        while (lchd != null)
                        {
                            if (lchd.rsib == null
                                || lchd.rsib.Content.id > dep.Content.id)
                            {
                                lchd.InsertAsRightSibling(dep);
                                break;
                            }

                            lchd = lchd.rsib;
                        }
                    }
                }
                else
                {
                    head.InsertLeftChildFar(dep);
                }
            }
            else if (dep.Content.id > head.Content.id)
            {
                if (head.HaveRightChild)
                {
                    if (head.nearrchild.Content.id > dep.Content.id)
                    {
                        head.InsertRightChildNear(dep);
                    }
                    else
                    {
                        LRTreeNode<LRParseWord> rchd = head.nearrchild;

                        while (rchd != null)
                        {
                            if (rchd.rsib == null
                                || rchd.rsib.Content.id > dep.Content.id)
                            {
                                rchd.InsertAsRightSibling(dep);
                                break;
                            }

                            rchd = rchd.rsib;
                        }
                    }
                }
                else
                {
                    head.InsertRightChildFar(dep);
                }
            }
            else
            {
                throw new Exception("Tree node cannot attach to itself");
            }
        }


        public bool UseSentenceBreaking { get; set; }

        public bool UseTaggingParsingIteration { get; set; }

        IPoSTagger tagger;
        IParserDecoder parser;
    }
}
