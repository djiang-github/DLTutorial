using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserUtil
{
    public class DepNode
    {
        public int id;
        public string token;
        public string POS;
        public string depType;
        public List<DepNode> children;
        public DepNode head;
        public int bid;
        public int eid;
        public int HeadID
        {
            get
            {
                return head == null ? 0 : head.id;
            }
        }
        public bool HaveChild
        {
            get { return children != null && children.Count > 0; }
        }
        public int ChildCnt
        {
            get
            {
                return children == null ? 0 : children.Count;
            }
        }
    }

    public class DepTree
    {
        public DepNode[] nodeArr;
        public DepNode root
        {
            get { return nodeArr == null ? null : nodeArr[0]; }
        }
        public int Length
        {
            get { return nodeArr == null ? 0 : nodeArr.Length; }
        }

        public void Init(string[] token, string[] POS, int[] hID, string[] depType)
        {
            nodeArr = null;
            if (token == null || POS == null)
            {
                return;
            }
            if (token.Length != POS.Length || (hID != null && token.Length != hID.Length) || (depType != null && depType.Length != token.Length))
            {
                return;
            }
            nodeArr = new DepNode[token.Length + 1];

            for (int i = 0; i < nodeArr.Length; ++i)
            {
                nodeArr[i] = new DepNode();
                nodeArr[i].id = i;
                nodeArr[i].children = new List<DepNode>();
            }
            // initialize root;
            nodeArr[0].token = "ROOT";
            nodeArr[0].POS = "ROOT";
            nodeArr[0].depType = this.nullArcType;
            nodeArr[0].head = null;
            // initialize other node;
            for (int i = 1; i < nodeArr.Length; ++i)
            {
                nodeArr[i].token = token[i - 1];
                nodeArr[i].POS = POS[i - 1];
                nodeArr[i].depType = depType == null ? null : depType[i - 1];
                if (hID != null)
                {
                    nodeArr[i].head = nodeArr[hID[i - 1]];
                    nodeArr[hID[i - 1]].children.Add(nodeArr[i]);
                }
            }
        }

        public DepTree(string[] token, string[] POS, int[] hID, string[] depType)
        {
            Init(token, POS, hID, depType);
        }
        public DepTree(string[] token, string[] POS)
        {
            Init(token, POS, null, null);
        }
        public DepTree(string[] token, string[] POS, int[] hID)
        {
            Init(token, POS, hID, null);
        }

        public DepNode this[int id]
        {
            get
            {
                if (nodeArr == null || id < 0 || id >= Length)
                {
                    return null;
                }
                return nodeArr[id];
            }
        }

        public DepTree Clone()
        {
            // Generate a clone to current DepTree.
            // It is a deep clone.
            DepTree clone = new DepTree();
            if (this.nodeArr == null || this.nodeArr.Length == 0)
            {
                return clone;
            }
            int n = nodeArr.Length;
            clone.nodeArr = new DepNode[n];
            for (int i = 0; i < n; ++i)
            {
                clone.nodeArr[i] = new DepNode();
                clone.nodeArr[i].id = nodeArr[i].id;
                clone.nodeArr[i].POS = nodeArr[i].POS;
                clone.nodeArr[i].token = nodeArr[i].token;
                clone.nodeArr[i].depType = nodeArr[i].depType;
                clone.nodeArr[i].bid = nodeArr[i].bid;
                clone.nodeArr[i].eid = nodeArr[i].eid;
            }
            for (int i = 0; i < n; ++i)
            {
                DepNode clonenode = clone.nodeArr[i];
                DepNode thisnode = nodeArr[i];
                clonenode.head = thisnode.head == null ? null : clone[thisnode.HeadID];
                if (thisnode.children != null)
                {
                    clonenode.children = new List<DepNode>();
                    foreach (DepNode chd in thisnode.children)
                    {
                        clonenode.children.Add(clone[chd.id]);
                    }
                }
            }
            return clone;
        }

        private DepTree()
        { }

        public string[] tokenArr
        {
            get
            {
                if (this.Length == 0)
                {
                    return null;
                }
                string[] tk = new string[this.Length - 1];
                for (int i = 1; i < this.Length; ++i)
                {
                    tk[i - 1] = this[i].token;
                }
                return tk;
            }
        }

        public string[] posArr
        {
            get
            {
                if (this.Length == 0)
                {
                    return null;
                }
                string[] tk = new string[this.Length - 1];
                for (int i = 1; i < this.Length; ++i)
                {
                    tk[i - 1] = this[i].POS;
                }
                return tk;
            }
        }

        public int[] hidArr
        {
            get
            {
                if (this.Length == 0)
                {
                    return null;
                }
                int[] tk = new int[this.Length - 1];
                for (int i = 1; i < this.Length; ++i)
                {
                    tk[i - 1] = this[i].HeadID;
                }
                return tk;
            }
        }

        public string[] depTypeArr
        {
            get
            {
                if (this.Length == 0)
                {
                    return null;
                }
                string[] tk = new string[this.Length - 1];
                for (int i = 1; i < this.Length; ++i)
                {
                    tk[i - 1] = this[i].depType;
                }
                return tk;
            }
        }

        public void Reverse()
        {
            if (nodeArr == null)
            {
                return;
            }
            DepNode[] newnodeArr = new DepNode[nodeArr.Length];
            newnodeArr[0] = nodeArr[0];
            int n = nodeArr.Length;
            for (int i = 1; i < n; ++i)
            {
                newnodeArr[i] = nodeArr[n - i];
                newnodeArr[i].id = i;
            }
            nodeArr = newnodeArr;
        }

        public bool IsIsomorphicTo(DepTree other)
        {
            return DepTree.IsIsomorphic(this, other);
        }

        public bool IsUnlabeledIsomorphicTo(DepTree other)
        {
            return DepTree.IsUnlabeledIsomorphic(this, other);
        }

        public void ComputeSpan()
        {
            if (!IsValidDepTree(this))
            {
                return;
            }
            ComputeSpan(root);
        }

        private void ComputeSpan(DepNode node)
        {
            if (node == null)
            {
                return;
            }

            node.bid = node.id;
            node.eid = node.id + 1;
            foreach (DepNode chd in node.children)
            {
                ComputeSpan(chd);
                node.bid = Math.Min(chd.bid, node.bid);
                node.eid = Math.Max(chd.eid, node.eid);
            }
        }

        public bool CheckWellFormedness()
        {
            if (!IsValidDepTree(this))
            {
                return false;
            }
            if (Length == 0)
            {
                return true;
            }
            //Dictionary<int, int> checkedNodes = new Dictionary<int, int>();
            bool[] bChecked = new bool[Length];
            
            for (int i = 0; i < Length; ++i)
            {
                if(bChecked[i])
                {
                    continue;
                }
                
                bool[] bPassed = new bool[Length];
                DepNode node = nodeArr[i];
                while (node != null)
                {
                    if(bPassed[node.id])
                    {
                        return false;
                    }
                    bPassed[node.id] = true;
                    if (bChecked[node.id])
                    {
                        break;
                    }
                    bChecked[node.id] = true;
                    if (node.head != null)
                    {
                        bool flag = false;
                        foreach (DepNode chd in node.head.children)
                        {
                            if (chd.id == node.id)
                            {
                                if (chd != node)
                                {
                                    return false;
                                }
                                if (flag)
                                {
                                    return false;
                                }
                                flag = true;
                            }
                        }
                        if (!flag)
                        {
                            return false;
                        }
                    }
                    node = node.head;
                }

               // bChecked[i] = true;
            }
            for (int i = 0; i < Length; ++i)
            {
                DepNode node = nodeArr[i];
                foreach (DepNode chd in node.children)
                {
                    if (chd.head != node)
                    {
                        return false;
                    }
                }
            }

            // check projectivity

            for (int i = 1; i < Length; ++i)
            {
                int headid = nodeArr[i].HeadID;
                int min = Math.Min(headid, nodeArr[i].id);
                int max = Math.Max(headid, nodeArr[i].id);
                for (int j = min + 1; j < max; ++j)
                {
                    if (nodeArr[j].HeadID < min || nodeArr[j].HeadID > max)
                    {
                        return false;
                    }
                }
            }
            return true;

        }

        public void SortChildren()
        {
            if (root == null)
            {
                return;
            }
            SortChildren(root);
        }

        private void SortChildren(DepNode node)
        {
            if (node == null || node.children.Count < 1)
            {
                return;
            }
            for (int i = 0; i < node.children.Count; ++i)
            {
                int smallID = node.children[i].id;
                int smallPos = i;
                for (int j = i + 1; j < node.children.Count; ++j)
                {
                    if (node.children[j].id < smallID)
                    {
                        smallID = node.children[j].id;
                        smallPos = j;
                    }
                }
                if (smallPos != i)
                {
                    DepNode tmpnode = node.children[i];
                    node.children[i] = node.children[smallPos];
                    node.children[smallPos] = tmpnode;
                }
            }
            foreach (DepNode chd in node.children)
            {
                SortChildren(chd);
            }
        }

        private string rootArcType
        {
            get
            {
                return GlobalVariables.rootArcType;
            }
        }

        private string nullArcType
        {
            get
            {
                return GlobalVariables.nullArcType;
            }
        }
        static public bool IsIsomorphic(DepTree A, DepTree B, bool bCheckLabel)
        {
            // trivil case first;
            if (A == null && B == null)
            {
                return true;
            }
            if (A == null || B == null)
            {
                return false;
            }
            if (A.root == null && B.root == null)
            {
                return true;
            }
            if (A.root == null || B.root == null)
            {
                return false;
            }
            if (A.Length != B.Length)
            {
                return false;
            }
            for (int i = 1; i < A.Length; ++i)
            {
                DepNode anode = A[i];
                DepNode bnode = B[i];
                if (anode.head.id != bnode.head.id)
                {
                    return false;
                }
                if (bCheckLabel && anode.depType != bnode.depType)
                {
                    return false;
                }
            }
            return true;
        }
        static public bool IsIsomorphic(DepTree A, DepTree B)
        {
            return IsIsomorphic(A, B, true);
        }
        static public bool IsUnlabeledIsomorphic(DepTree A, DepTree B)
        {
            return IsIsomorphic(A, B, false);
        }
        static public bool IsValidDepTree(DepTree A)
        {
            return A != null && A.root != null;
        }
        
    }


    public class JBunsetsu : IComparable<JBunsetsu>
    {
        public string functionalForm
        {
            get
            {
                return "";
            }
        }

        public List<CNode> innerNodes = new List<CNode>();

        public CNode headNode;

        public CNode functionNode;

        public List<JBunsetsu> childBunsetsu = new List<JBunsetsu>();

        public JBunsetsu parentBunsetsu;

        public int bid;
        public int eid;

        int IComparable<JBunsetsu>.CompareTo(JBunsetsu other)
        {
            if (other == null || other.headNode == null)
            {
                return -1;
            }

            return this.headNode.id.CompareTo(other.headNode.id);
        }

        public JBunsetsu LeftChild
        {
            get
            {
                if (childBunsetsu.Count == 0)
                {
                    return null;
                }

                foreach (JBunsetsu chd in childBunsetsu)
                {
                    if (chd.headNode.depType != "br")
                    {
                        return chd;
                    }
                }

                return null;
            }
        }

        public JBunsetsu RightChild
        {
            get
            {
                if (childBunsetsu.Count == 0)
                {
                    return null;
                }

                for(int i = childBunsetsu.Count - 1; i >= 0; --i)
                {
                    JBunsetsu chd = childBunsetsu[i];
                    if (chd.headNode.depType != "br")
                    {
                        return chd;
                    }
                }

                return null;
            }
        }

        public JBunsetsu LeftSibling
        {
            get
            {
                if (parentBunsetsu == null)
                {
                    return null;
                }

                // find this child;
                bool FoundThisChd = false;

                for (int i = parentBunsetsu.childBunsetsu.Count - 1; i >= 0; --i)
                {
                    JBunsetsu tmp = parentBunsetsu.childBunsetsu[i];
                    if (tmp.headNode.id == headNode.id)
                    {
                        FoundThisChd = true;
                    }
                    else if (FoundThisChd && tmp.headNode.depType != "br")
                    {
                        return tmp;
                    }
                }

                return null;
            }
        }

        public JBunsetsu RightSibling
        {
            get
            {
                if (parentBunsetsu == null)
                {
                    return null;
                }

                // find this child;
                bool FoundThisChd = false;

                foreach(JBunsetsu tmp in parentBunsetsu.childBunsetsu)
                {
                    if (tmp.headNode.id == headNode.id)
                    {
                        FoundThisChd = true;
                    }
                    else if (FoundThisChd && tmp.headNode.depType != "br")
                    {
                        return tmp;
                    }
                }

                return null;
            }
        }

        public string FuncW
        {
            get
            {
                return GetFuncW(headNode);
            }
        }

        public string Text
        {
            get
            {
                List<string> tokens = new List<string>();
                foreach (CNode x in innerNodes)
                {
                    tokens.Add(x.token);
                }

                return string.Join(" ", tokens);
            }
        }

        private string GetFuncW(CNode node)
        {
            if (node == null || node.children == null || node.children.Count <= 1)
            {
                return "<null>";
            }

            List<string> funcs = new List<string>();

            foreach (CNode chd in node.children)
            {
                if (chd.id > node.id)
                {
                    // make some exceptions, some words are not reorder indicators

                    string tok = chd.token;
                    string pos = chd.POS;
                    if (tok == "?" || tok == "!" || pos == "PUNC" || tok == "(" || tok == ")"
                        || chd.depType == "br")
                    {
                        continue;
                    }

                    funcs.Add(chd.token);
                }
            }

            if (funcs.Count == 0)
            {
                return "<null>";
            }

            return string.Join("_", funcs);

            //StringBuilder sb = new StringBuilder();
            //foreach (CNode chd in node.children)
            //{
            //    if (chd.id > node.id &&
            //        ((chd.depType == "f"// && funcSet.Contains(chd.token))
            //        || (chd.depType == "i" && (chd.token == "、" || chd.token == ".")))))
            //    {
            //        sb.Append(chd.token + "|"); ;
            //    }
            //}

            //return sb.Length == 0 ? "<null>" : sb.ToString();

        }
    }


    public class CNode : IComparable<CNode>
    {
        public int id;
        public string token;
        public string POS;
        public string depType;
        public List<CNode> children;
        public CNode head;
        public int bid;
        public int eid;

        public CNode()
        {
        }

        public CNode(DepNode dnode)
        {
            id = dnode.id;
            token = dnode.token;
            POS = dnode.POS;
            depType = dnode.depType;
            bid = dnode.bid;
            eid = dnode.eid;
        }

        public CNode leftChd
        {
            get
            {
                if (children == null || children.Count == 0)
                {
                    return null;
                }
                if (children[0].id < id)
                {
                    return children[0];
                }
                return null;
            }
        }
        public CNode rightChd
        {
            get
            {
                if (children == null || children.Count == 0)
                {
                    return null;
                }
                if (children[children.Count - 1].id > id)
                {
                    return children[children.Count - 1];
                }
                return null;
            }
        }

        public int leftChdCnt
        {
            get
            {
                if (children == null || children.Count == 0)
                {
                    return 0;
                }
                int cnt = 0;
                for (int i = 0; i < children.Count; ++i)
                {
                    if (children[i].id >= id)
                    {
                        return cnt;
                    }
                    cnt++;
                }
                return cnt;
            }
        }

        public int rightChdCnt
        {
            get
            {
                if (children == null || children.Count == 0)
                {
                    return 0;
                }
                int cnt = 0;
                for (int i = children.Count - 1; i >= 0; --i)
                {
                    if (children[i].id <= id)
                    {
                        return cnt;
                    }
                    cnt++;
                }
                return cnt;
            }
        }

        public int PositionToHead
        {
            get
            {
                if (head == null)
                {
                    return 999;
                }
                int hid = head.id;
                int headpos = -1;
                int thispos = -1;
                for (int i = 0; i < head.children.Count; ++i)
                {
                    if (head.children[i].id == id)
                    {
                        thispos = i;
                    }
                    if (head.children[i].id == hid)
                    {
                        headpos = i;
                    }
                }
                if (headpos == -1 || thispos == -1)
                {
                    return 999;
                }
                else
                {
                    return thispos - headpos;
                }
            }
        }

        public void CopyContent(DepNode dnode)
        {
            id = dnode.id;
            token = dnode.token;
            POS = dnode.POS;
            depType = dnode.depType;
            bid = dnode.bid;
            eid = dnode.eid;
        }

        int IComparable<CNode>.CompareTo(CNode other)
        {
            if (other == null)
            {
                return -1;
            }

            return id.CompareTo(other.id);
        }
    }
    public class CTree
    {
        public JBunsetsu[] Bunsetsu;

        public JBunsetsu GetBunsetsu(int id)
        {
            return widToBunsetsu[id];
        }

        public void InitBunsetsu()
        {
            widToBunsetsu = new JBunsetsu[Length];
            List<JBunsetsu> blist = new List<JBunsetsu>();
            // make root bunsetsu
            blist.Add(MakeRootBunsetsu());
            widToBunsetsu[0] = blist[0];

            foreach (CNode node in Root.children)
            {
                SetBunsetsu(node, Root, blist);
            }

            Bunsetsu = blist.ToArray();

            for (int i = 0; i < Bunsetsu.Length; ++i)
            {
                Bunsetsu[i].childBunsetsu.Sort();
                Bunsetsu[i].innerNodes.Sort();
            }
            for (int i = 0; i < Length; ++i)
            {
                JBunsetsu x = GetBunsetsu(i);
                if (x.bid == 0 && x.eid == 0)
                {
                    x.bid = i;
                    x.eid = i + 1;
                }
                else
                {
                    x.bid = Math.Min(x.bid, i);
                    x.eid = Math.Max(x.eid, i + 1);
                }
            }
        }

        private JBunsetsu MakeRootBunsetsu()
        {
            JBunsetsu rootBst = new JBunsetsu();
            rootBst.innerNodes.Add(Root);
            rootBst.headNode = Root;
            return rootBst;
        }

        private void SetBunsetsu(CNode node, CNode parent, List<JBunsetsu> blist)
        {
            if(node.id == parent.id)
            {
                return;
            }
            JBunsetsu thisBunsetsu = new JBunsetsu();
            JBunsetsu parentBunsetsu =widToBunsetsu[parent.id];
            widToBunsetsu[node.id] = thisBunsetsu;
            thisBunsetsu.parentBunsetsu = parentBunsetsu;
            parentBunsetsu.childBunsetsu.Add(thisBunsetsu);
            thisBunsetsu.headNode = node;
            thisBunsetsu.innerNodes.Add(node);
            blist.Add(thisBunsetsu);
            // set innernodes
            foreach (CNode chd in node.children)
            {
                if (chd.depType == "i" || chd.depType == "f")
                {
                    thisBunsetsu.innerNodes.Add(chd);
                    if (chd.depType == "f")
                    {
                        thisBunsetsu.functionNode = chd;
                    }
                    for (int i = chd.bid; i < chd.eid; ++i)
                    {
                        widToBunsetsu[i] = thisBunsetsu;
                    }
                }
            }
            // set outernode
            foreach (CNode chd in node.children)
            {
                if (chd.depType != "i" && chd.depType != "f")
                {
                    SetBunsetsu(chd, node, blist);
                }
            }
        }

        private JBunsetsu[] widToBunsetsu;

        public CNode[] terminalArr;

        private CNode _root;

        public CNode Root
        {
            get { return _root; }
        }

        public int Length
        {
            get { return terminalArr == null ? 0 : terminalArr.Length; }
        }

        public void Init(DepTree dtree)
        {
            dtree.SortChildren();
            CopyTerminalArr(dtree);
            _root = new CNode();
            ConstructSubTree(_root, dtree.root);
            ComputeSpan();
        }

        public CTree(DepTree dtree)
        {
            dtree.SortChildren();
            Init(dtree);
            ComputeSpan();
            InitBunsetsu();
        }

        public CTree(string[] tokenArr, string[] PoSArr, int[] HIdArr, string[] arcLabelArr)
        {
            DepTree dtree = new DepTree(tokenArr, PoSArr, HIdArr, arcLabelArr);
            if (DepTree.IsValidDepTree(dtree))
            {
                Init(dtree);
            }
        }

        public void ConstructSubTree(CNode cnode, DepNode dnode)
        {
            // copy contents first;
            
            cnode.CopyContent(dnode);
            terminalArr[cnode.id].head = cnode;
            cnode.children = new List<CNode>();
            if (dnode.children.Count == 0)
            {
                cnode.children.Add(terminalArr[cnode.id]);
                return;
            }
            bool flag = false;
            foreach (DepNode chd in dnode.children)
            {
                CNode cchd = new CNode();
                if (chd.id > dnode.id && !flag)
                {
                    // first right chd;
                    flag = true;
                    cnode.children.Add(terminalArr[cnode.id]);
                }
                ConstructSubTree(cchd, chd);
                cchd.head = cnode;
                cnode.children.Add(cchd);
            }
            if (!flag)
            {
                // no right chd;
                cnode.children.Add(terminalArr[cnode.id]);
            }
        }

        private void CopyTerminalArr(DepTree dtree)
        {
            terminalArr = new CNode[dtree.Length];
            for (int i = 0; i < terminalArr.Length; ++i)
            {
                terminalArr[i] = new CNode();
                terminalArr[i].id = dtree[i].id;
                terminalArr[i].token = dtree[i].token;
                terminalArr[i].POS = dtree[i].POS;
                terminalArr[i].depType = "SELF";
                terminalArr[i].children = null;
                terminalArr[i].bid = dtree[i].bid;
                terminalArr[i].eid = dtree[i].eid;
            }
        }

        public string GetSentence()
        {
            if (Root == null || Root.children == null || Root.children.Count <= 1)
            {
                return "";
            }
            StringBuilder sb = new StringBuilder();
            foreach (CNode chd in Root.children)
            {
                if (chd.depType != "SELF")
                {
                    GetSubTreeText(chd, sb);
                }
            }
            return sb.ToString().Trim();
        }

        private void GetSubTreeText(CNode cnode, StringBuilder sb)
        {
            if (cnode == null)
            {
                return;
            }
            if (cnode.children == null || cnode.children.Count == 0)
            {
                sb.Append(cnode.token);
                sb.Append(' ');
                return;
            }
            foreach (CNode chd in cnode.children)
            {
                GetSubTreeText(chd, sb);
            }
        }

        public int[] GetReorderMapExcludeRoot()
        {
            int[] map = new int[Length - 1];
            int next = 0;
            GetReorderMap(Root, map, ref next);
            return map;
        }

        private void GetReorderMap(CNode node, int[] map, ref int next)
        {
            if (node == null)
            {
                return;
            }
            if (node.children == null || node.children.Count == 0)
            {
                if (node.id != 0)
                {
                    map[node.id - 1] = next++;
                }
                return;
            }
            foreach (CNode chd in node.children)
            {
                GetReorderMap(chd, map, ref next);
            }
        }

        private bool bSpanReady = false;

        public void ComputeSpan()
        {
            ComputeSpan(Root);
            bSpanReady = true;
        }

        private void ComputeSpan(CNode node)
        {
            if (node == null)
            {
                return;
            }
            if (node.children == null || node.children.Count == 0)
            {
                node.bid = node.id;
                node.eid = node.id + 1;
                return;
            }

            foreach (CNode chd in node.children)
            {
                ComputeSpan(chd);
            }

            bool flag = false;
            int bid = 0;
            int eid = 0;
            foreach (CNode chd in node.children)
            {
                if (!flag)
                {
                    flag = true;
                    bid = chd.bid;
                    eid = chd.eid;
                }
                else
                {
                    bid = Math.Min(bid, chd.bid);
                    eid = Math.Max(eid, chd.eid);
                }
            }
            node.bid = bid;
            node.eid = eid;
        }

        public bool IsSubTreeSpan(int bid, int eid)
        {
            if (bid <= 0 || eid > terminalArr.Length || bid >= eid)
            {
                // invalid span;
                return false;
            }
            if (bid == 1 && eid == terminalArr.Length)
            {
                // whole sentence span.
                return true;
            }
            if (bid == eid - 1)
            {
                // always true for 1-length span
                return true;
            }
            if (!bSpanReady)
            {
                ComputeSpan();
            }
            CNode thisnode = terminalArr[bid];
            while (thisnode != null)
            {
                if (thisnode.bid != bid)
                {
                    bool success = false;
                    foreach (CNode chd in thisnode.children)
                    {
                        if (chd.bid == bid)
                        {
                            success = true;
                            break;
                        }
                    }
                    if (!success)
                    {
                        return false;
                    }
                }
                if (thisnode.eid == eid)
                {
                    return true;
                }
                if (thisnode.eid < eid)
                {
                    thisnode = thisnode.head;
                    continue;
                }
                // thisnode.eid > eid, check if any child match eid;
                foreach (CNode chd in thisnode.children)
                {
                    if (chd.eid == eid)
                    {
                        return true;
                    }
                }
                return false;
            }
            return false;
        }

        public int[] GetReorderedIdSequence()
        {
            if (Root == null || Root.children == null || Root.children.Count <= 1)
            {
                return null;
            }
            List<int> idlist = new List<int>();
            foreach (CNode chd in Root.children)
            {
                if (chd.depType != "SELF")
                {
                    GetReorderedIdSequence(chd, idlist);
                }
            }
            int[] arr = idlist.ToArray();
            if (arr.Length != Length - 1)
            {
                return null;
            }
            return arr;
        }

        private void GetReorderedIdSequence(CNode node, List<int> idlist)
        {
            if (node == null)
            {
                return;
            }
            if (node.children == null || node.children.Count < 1)
            {
                idlist.Add(node.id);
                return;
            }
            foreach (CNode chd in node.children)
            {
                GetReorderedIdSequence(chd, idlist);
            }
        }

        public string GetTxtTree()
        {
            if (_root == null)
            {
                return null;
            }
            StringBuilder sb = new StringBuilder();
            //int[] map = GetReorderMapExcludeRoot();
            //GetTxtTree(sb, 0, Root, map, 0);
            GetTxtTree(sb, "", Root, 0);//map, 0);
            //foreach (CNode chd in Root.children)
            //{
            //    if (chd.children != null)
            //    {
            //        GetTxtTree(sb, "", chd, map, 0);
            //    }
            //}
            return sb.ToString();
        }

        public string GetTxtTree(IPoSMapper mapper)
        {
            if (_root == null)
            {
                return null;
            }
            StringBuilder sb = new StringBuilder();
            //int[] map = GetReorderMapExcludeRoot();
            //GetTxtTree(sb, 0, Root, map, 0);
            GetTxtTree(sb, "", Root, 0, mapper);//map, 0);
            //foreach (CNode chd in Root.children)
            //{
            //    if (chd.children != null)
            //    {
            //        GetTxtTree(sb, "", chd, map, 0);
            //    }
            //}
            return sb.ToString();
        }

        public static string GetTxtSubTree(CNode node)
        {
            if (node == null)
            {
                return null;
            }
            StringBuilder sb = new StringBuilder();
            GetTxtTree(sb, "", node, 0);
            return sb.ToString();
        }

        public static string GetTxtSubTree(CNode node, IPoSMapper mapper)
        {
            if (node == null)
            {
                return null;
            }
            StringBuilder sb = new StringBuilder();
            GetTxtTree(sb, "", node, 0, mapper);
            return sb.ToString();
        }

        //private void GetTxtTree(StringBuilder sb, int identation, CNode node, int[] map)
        //{
        //    if (node == null)
        //    {
        //        return;
        //    }
        //    if (node.children == null || node.children.Count == 0)
        //    {
        //        for (int i = 0; i < identation; ++i)
        //        {
        //            sb.Append(' ');
        //        }
        //        if (node.head == null || node.head.head == null || node.head.head.id == 0 || node.id == 0)
        //        {
        //            sb.Append("+---");
        //        }
        //        else if (map[node.id - 1] < map[node.head.head.id - 1])
        //        {
        //            sb.Append("/---");
        //        }
        //        else
        //        {
        //            sb.Append("\\---");
        //        }
        //        string deptype = "NULL";
        //        if (node.head != null && node.head.depType != null)
        //        {
        //            deptype = node.head.depType;
        //        }
        //        sb.AppendLine(string.Format("{0} {1} {2} {3} {4}", 
        //            node.token == null ? "NULL" : node.token,
        //            node.POS == null ? "NULL" : node.POS,
        //            deptype,
        //            node.id,
        //            node.head == null ? -1 : node.head.id));
        //    }
        //    if (node.children == null || node.children.Count == 0)
        //    {
        //        return;
        //    }
        //    //identa
        //    foreach (CNode chd in node.children)
        //    {
        //        GetTxtTree(sb, identation + 4, chd, map);
        //    }
        //}

        static private void GetTxtTree(StringBuilder sb, string linehead, CNode node, int headDir)//int[] map, int headDir)
        {
            if (node == null)
            {
                return;
            }
            if (node.children == null || node.children.Count == 0)
            {
                sb.Append(linehead + "--- ");
                string deptype = "NULL";
                if (node.head != null && node.head.depType != null)
                {
                    deptype = node.head.depType;
                }

                sb.AppendLine(string.Format("{0} {1} {2} {3} {4}",
                    node.token == null ? "NULL" : node.token,
                    node.POS == null ? "NULL" : node.POS,
                    deptype,
                    node.id,
                    (node.head == null || node.head.head == null) ? -1 : node.head.head.id));
                return;
            }
            bool bhead = false;
            for (int i = 0; i < node.children.Count; ++i)
            {
                CNode chd = node.children[i];
                if (chd.children == null || node.children.Count == 0)
                {
                    bhead = true;
                    string suffix;
                    switch (headDir)
                    {
                        case -1:
                            suffix = @"/";
                            break;
                        case 0:
                            suffix = @"|";
                            break;
                        default:
                            suffix = @"\";
                            break;
                    }
                    GetTxtTree(sb, linehead + suffix, chd, 0);//map, 0);
                }
                else
                {
                    string suffix = "|";
                    switch (headDir)
                    {
                        case -1:
                            if (!bhead)
                            {
                                suffix = " ";
                            }
                            break;
                        case 0:
                            break;
                        default:
                            if (bhead)
                            {
                                suffix = " ";
                            }
                            break;
                    }
                    int newDir = 0;
                    if (i == 0)
                    {
                        newDir = -1;
                    }
                    else if(i == node.children.Count - 1)
                    {
                        newDir = 1;
                    }
                    GetTxtTree(sb, linehead + suffix + "   ", chd, newDir);//map, newDir);
                }
            }
        }

        static private void GetTxtTree(StringBuilder sb, string linehead, CNode node, int headDir, IPoSMapper mapper)//int[] map, int headDir)
        {
            if (node == null)
            {
                return;
            }
            if (node.children == null || node.children.Count == 0)
            {
                sb.Append(linehead + "--- ");
                string deptype = "NULL";
                if (node.head != null && node.head.depType != null)
                {
                    deptype = node.head.depType;
                }

                sb.AppendLine(string.Format("{0} {1} {2} {3} {4} {5}",
                    node.token == null ? "NULL" : node.token,
                    node.POS == null ? "NULL" : node.POS,
                    node.POS == null ? "NULL" : mapper.Map(node.POS),
                    deptype,
                    node.id,
                    (node.head == null || node.head.head == null) ? -1 : node.head.head.id));
                return;
            }
            bool bhead = false;
            for (int i = 0; i < node.children.Count; ++i)
            {
                CNode chd = node.children[i];
                if (chd.children == null || node.children.Count == 0)
                {
                    bhead = true;
                    string suffix;
                    switch (headDir)
                    {
                        case -1:
                            suffix = @"/";
                            break;
                        case 0:
                            suffix = @"|";
                            break;
                        default:
                            suffix = @"\";
                            break;
                    }
                    GetTxtTree(sb, linehead + suffix, chd, 0, mapper);//map, 0);
                }
                else
                {
                    string suffix = "|";
                    switch (headDir)
                    {
                        case -1:
                            if (!bhead)
                            {
                                suffix = " ";
                            }
                            break;
                        case 0:
                            break;
                        default:
                            if (bhead)
                            {
                                suffix = " ";
                            }
                            break;
                    }
                    int newDir = 0;
                    if (i == 0)
                    {
                        newDir = -1;
                    }
                    else if (i == node.children.Count - 1)
                    {
                        newDir = 1;
                    }
                    GetTxtTree(sb, linehead + suffix + "   ", chd, newDir, mapper);//map, newDir);
                }
            }
        }

        
    }
}
