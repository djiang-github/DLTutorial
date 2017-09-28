using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NanYUtilityLib;

namespace LinearDepParser
{
    public class SimpleStack<T>
    {
        public SimpleStack(int cap)
        {
            this.cap = cap;
            this.stackArr = new T[cap];
            this.top = -1;
        }

        public void Clear()
        {
            top = -1;
        }

        public void Push(T obj)
        {
            stackArr[++top] = obj;
        }

        public T Peek()
        {
            return stackArr[top];
        }

        public T this[int idx]
        {
            get
            {
                return stackArr[top - idx];
            }
        }

        public T Pop()
        {
            return stackArr[top--];
        }

        public bool IsEmpty
        {
            get
            {
                return top < 0;
            }
        }

        public int Count
        {
            get
            {
                return top + 1;
            }
        }

        public int Cap
        {
            get { return stackArr.Length; }
        }

        public IEnumerable<T> StackItems
        {
            get
            {
                for (int i = 0; i <= top; ++i)
                {
                    yield return stackArr[i];
                }
            }
        }

        public void CopyTo(SimpleStack<T> dest)
        {
            dest.cap = this.cap;
            dest.top = this.top;
            this.stackArr.CopyTo(dest.stackArr, 0);
        }

        public SimpleStack<T> Clone()
        {
            SimpleStack<T> r = new SimpleStack<T>(this.cap);
            r.top = this.top;
            this.stackArr.CopyTo(r.stackArr, 0);
            return r;
        }

        private int top;
        private T[] stackArr;
        private int cap;
    }

    public class ParserConfig
    {
        public float score;
        private int[] tok;
        private int[] pos;
        private int[] wcluster;
        private int[] hid;
        private int[] label;

        public int[] state { get; private set; }

        private int next { get; set; }

        private SimpleStack<int> stack;
        
        // some auxiliary to help keep track of the parser state;
        private int[] lchdcnt;
        private int[] rchdcnt;
        private int[] lchdid;
        private int[] rchdid;
        private int[] llchdid;
        private int[] rrchdid;

        public List<int> CommandTrace;

        public bool Compare(ParserConfig other)
        {
            return //CompareArr(tok, other.tok)
                //&& CompareArr(pos, other.pos)
                CompareArr(hid, other.hid)
                && CompareArr(label, other.label)
                // && CompareArr(state, other.state)
                && next == other.next
                && CompareArr(lchdcnt, other.lchdcnt)
                && CompareArr(rchdcnt, other.rchdcnt)
                && CompareArr(lchdid, other.lchdid)
                && CompareArr(rchdid, other.rchdid)
                && CompareArr(llchdid, other.llchdid)
                && CompareArr(rrchdid, other.rrchdid)
                && CompareStack(stack, other.stack);

        }

        static private bool CompareArr(int[] a, int[] b)
        {
            if (a == null ^ b == null)
            {
                return false;
            }
            if (a == null && b == null)
            {
                return true;
            }

            if (a.Length != b.Length)
            {
                return false;
            }

            for (int i = 0; i < a.Length; ++i)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }

            return true;
        }

        static private bool CompareStack(SimpleStack<int> a, SimpleStack<int> b)
        {
            if (a == null ^ b == null)
            {
                return false;
            }
            if (a == null && b == null)
            {
                return true;
            }

            if (a.Cap != b.Cap)
            {
                return false;
            }

            if (a.Count != b.Count)
            {
                return false;
            }

            for (int i = 0; i < a.Count; ++i)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }

            return true;
        }

        public int[] HeadIDs()
        {
            int[] r = new int[hid.Length - 2];
            for (int i = 0; i < r.Length; ++i)
            {
                r[i] = hid[i + 1];
            }
            return r;
        }

        public int[] ArcLabels()
        {
            int[] r = new int[label.Length - 2];
            for (int i = 0; i < r.Length; ++i)
            {
                r[i] = label[i + 1];
            }
            return r;
        }

        public bool IsEnd { get { return IsStackEmpty && IsBufferEmpty; } }

        public bool IsStackEmpty { get { return stack.IsEmpty; } }

        public bool IsBufferEmpty { get { return next >= tok.Length - 1; } }

        public ParserConfig(int[] tok, int[] pos, int[] wcluster)
        {
            int len = tok.Length;
            this.tok = (int[])tok.Clone();
            this.pos = (int[])pos.Clone();
            this.wcluster = wcluster;
            if (wcluster == null)
            {
                this.wcluster = new int[this.tok.Length];

                for (int i = 0; i < this.wcluster.Length; ++i)
                {
                    this.wcluster[i] = -1;
                }
            }
            hid = new int[len];
            label = new int[len];
            lchdcnt = new int[len];
            lchdid = new int[len];
            rchdcnt = new int[len];
            rchdid = new int[len];
            llchdid = new int[len];
            rrchdid = new int[len];
            
            for (int i = 0; i < len; ++i)
            {
                hid[i] = -1;
            }

            for (int i = 0; i < len; ++i)
            {
                label[i] = -1;
            }

            for (int i = 0; i < len; ++i)
            {
                lchdid[i] = -1;
            }

            for (int i = 0; i < len; ++i)
            {
                rchdid[i] = -1;
            }

            for (int i = 0; i < len; ++i)
            {
                llchdid[i] = -1;
            }

            for (int i = 0; i < len; ++i)
            {
                rrchdid[i] = -1;
            }

            stack = new SimpleStack<int>(len);

            state = new int[ParserStateDescriptor.Count];

            stack.Push(1);

            next = 2;

            CommandTrace = new List<int>();

            SetKernals();
        }

        public ParserConfig Clone()
        {
            ParserConfig clone = new ParserConfig();

            clone.tok = (int[])tok.Clone();
            clone.pos = (int[])pos.Clone();
            clone.hid = (int[])hid.Clone();
            clone.wcluster = (int[])wcluster.Clone();
            clone.label = (int[])label.Clone();
            clone.lchdcnt = (int[]) lchdcnt.Clone();
            clone.lchdid = (int[])lchdid.Clone();
            clone.rchdcnt = (int[])rchdcnt.Clone();
            clone.rchdid = (int[])rchdid.Clone();
            clone.llchdid = (int[])llchdid.Clone();
            clone.rrchdid = (int[])rrchdid.Clone();
            clone.stack = stack.Clone();
            clone.next = next;
            clone.score = score;
            clone.state = (int[])state.Clone();

            clone.CommandTrace = new List<int>();

            foreach (var c in CommandTrace)
            {
                clone.CommandTrace.Add(c);
            }

            return clone;
        }

        /// <summary>
        /// no safety check whatever
        /// </summary>
        /// <param name="backup"></param>
        public void CopyTo(ParserConfig backup)
        {
            hid.CopyTo(backup.hid, 0);
            label.CopyTo(backup.label, 0);
            lchdcnt.CopyTo(backup.lchdcnt, 0);
            rchdcnt.CopyTo(backup.rchdcnt, 0);
            lchdid.CopyTo(backup.lchdid, 0);
            rchdid.CopyTo(backup.rchdid, 0);
            llchdid.CopyTo(backup.llchdid, 0);
            rrchdid.CopyTo(backup.rrchdid, 0);
            state.CopyTo(backup.state, 0);
            backup.next = next;
            backup.score = score;
            stack.CopyTo(backup.stack);
            backup.CommandTrace.Clear();

            foreach (var c in CommandTrace)
            {
                backup.CommandTrace.Add(c);
            }

        }

        private ParserConfig()
        {
        }

        private void SetKernals()
        {
            // inremental update is not used;
            // clear all state;
            for (int i = 0; i < state.Length; ++i)
            {
                state[i] = -1;
            }
            // feature on s0
            if(!IsStackEmpty)
            {
                int s0id = stack[0];
                state[ParserStateDescriptor.s0ID] = s0id;
                state[ParserStateDescriptor.s0w] = tok[s0id];
                state[ParserStateDescriptor.s0p] = pos[s0id];
                state[ParserStateDescriptor.s0arc] = hid[s0id] == -1 ? -1 : label[s0id];
                state[ParserStateDescriptor.s0vl] = QuantValence(lchdcnt[s0id]);
                state[ParserStateDescriptor.s0vr] = QuantValence(rchdcnt[s0id]);
                state[ParserStateDescriptor.s0c] = wcluster[s0id];
            }

            // feature on s0h
            if (!IsStackEmpty && hid[stack[0]] >= 0)
            {
                int id = hid[stack[0]];
                state[ParserStateDescriptor.s0hID] = id;
                state[ParserStateDescriptor.s0hw] = tok[id];
                state[ParserStateDescriptor.s0hp] = pos[id];
                state[ParserStateDescriptor.s0harc] = hid[id] == -1 ? -1 : label[id];
                state[ParserStateDescriptor.s0hc] = wcluster[id];
            }

            // feature on s0h2
            if(!IsStackEmpty && hid[stack[0]] >= 0 && hid[hid[stack[0]]] >= 0)
            {
                int id = hid[hid[stack[0]]];
                state[ParserStateDescriptor.s0h2ID] = id;
                state[ParserStateDescriptor.s0h2w] = tok[id];
                state[ParserStateDescriptor.s0h2p] = pos[id];
                state[ParserStateDescriptor.s0h2c] = wcluster[id];
            }

            // feature on s0l
            if(!IsStackEmpty && lchdid[stack[0]] >= 0)
            {
                int id = lchdid[stack[0]];
                state[ParserStateDescriptor.s0lID] = id;
                state[ParserStateDescriptor.s0lw] = tok[id];
                state[ParserStateDescriptor.s0lp] = pos[id];
                state[ParserStateDescriptor.s0larc] = hid[id] == -1 ? -1 : label[id];
                state[ParserStateDescriptor.s0lc] = wcluster[id];
            }

            // feature on s0r
            if (!IsStackEmpty && rchdid[stack[0]] >= 0)
            {
                int id = rchdid[stack[0]];
                state[ParserStateDescriptor.s0rID] = id;
                state[ParserStateDescriptor.s0rw] = tok[id];
                state[ParserStateDescriptor.s0rp] = pos[id];
                state[ParserStateDescriptor.s0rarc] = hid[id] == -1 ? -1 : label[id];
                state[ParserStateDescriptor.s0rc] = wcluster[id];
            }

            // feature on s0l2
            if (!IsStackEmpty && llchdid[stack[0]] >= 0)
            {
                int id = llchdid[stack[0]];
                state[ParserStateDescriptor.s0l2ID] = id;
                state[ParserStateDescriptor.s0l2w] = tok[id];
                state[ParserStateDescriptor.s0l2p] = pos[id];
                state[ParserStateDescriptor.s0l2arc] = hid[id] == -1 ? -1 : label[id];
            }

            // feature on s0r2
            if (!IsStackEmpty && rrchdid[stack[0]] >= 0)
            {
                int id = rrchdid[stack[0]];
                state[ParserStateDescriptor.s0r2ID] = id;
                state[ParserStateDescriptor.s0r2w] = tok[id];
                state[ParserStateDescriptor.s0r2p] = pos[id];
                state[ParserStateDescriptor.s0r2arc] = hid[id] == -1 ? -1 : label[id];
            }

            // feature on n0
            if (next < tok.Length)
            {
                int id = next;
                state[ParserStateDescriptor.n0ID] = id;
                state[ParserStateDescriptor.n0w] = tok[id];
                state[ParserStateDescriptor.n0p] = pos[id];
                //state[KernalFeature.n0arc] = hid[s0id] == -1 ? -1 : label[s0id];
                state[ParserStateDescriptor.n0vl] = QuantValence(lchdcnt[id]);
                state[ParserStateDescriptor.n0c] = wcluster[id];
                //state[KernalFeature.s0vr] = QuantValence(rchdcnt[s0id]);
            }

            // feature on n0l
            if (next < tok.Length && lchdid[next] >= 0)
            {
                int id = lchdid[next];
                state[ParserStateDescriptor.n0lID] = id;
                state[ParserStateDescriptor.n0lw] = tok[id];
                state[ParserStateDescriptor.n0lp] = pos[id];
                state[ParserStateDescriptor.n0larc] = hid[id] == -1 ? -1 : label[id];
                state[ParserStateDescriptor.n0lc] = wcluster[id];
            }

            // feature on n0l2
            if (next < tok.Length && llchdid[next] >= 0)
            {
                int id = llchdid[next];
                state[ParserStateDescriptor.n0l2ID] = id;
                state[ParserStateDescriptor.n0l2w] = tok[id];
                state[ParserStateDescriptor.n0l2p] = pos[id];
                state[ParserStateDescriptor.n0l2arc] = hid[id] == -1 ? -1 : label[id];
            }

            // feature on n1
            if (next + 1 < tok.Length)
            {
                int id = next + 1;
                state[ParserStateDescriptor.n1p] = pos[id];
                state[ParserStateDescriptor.n1w] = tok[id];
                state[ParserStateDescriptor.n1c] = wcluster[id];
            }

            // feature on n1
            if (next + 2 < tok.Length)
            {
                int id = next + 2;
                state[ParserStateDescriptor.n2p] = pos[id];
                state[ParserStateDescriptor.n2w] = tok[id];
                state[ParserStateDescriptor.n2c] = wcluster[id];
            }

            // misc
            state[ParserStateDescriptor.dst] = (IsBufferEmpty || IsStackEmpty) ? -1 : QuantDistance(next - stack[0]);
        }

        private int QuantValence(int v)
        {
            if (v < 5)
            {
                return v;
            }
            else if (v < 8)
            {
                return 5;
            }
            else
            {
                return 6;
            }
        }

        private int QuantDistance(int d)
        {
            if (d < 4)
            {
                return d;
            }
            else if (d < 5)
            {
                return 4;
            }
            else if (d < 10)
            {
                return 5;
            }
            else
            {
                return 6;
            }
        }

        public bool IsApplicableAction(ParserAction pa)
        {
            switch (pa)
            {
                case ParserAction.LA:
                    return !IsStackEmpty && !IsBufferEmpty && hid[stack.Peek()] == -1;
                case ParserAction.RA:
                    return !IsStackEmpty && !IsBufferEmpty && hid[next] == -1;
                case ParserAction.REDUCE:
                    return !IsStackEmpty && (hid[stack[0]] != -1 || IsBufferEmpty || stack.Count == 1);
                case ParserAction.SHIFT:
                    return !IsBufferEmpty;
                default:
                    return false;
            }
        }

        public bool IsApplicableAction(ParserAction pa, int labl, ParserForcedConstraints constraints)
        {
            if (constraints == null)
            {
                return IsApplicableAction(pa);
            }

            bool canReduce = true;
            bool canLA = true;
            bool canRA = true;
            bool canSH = true;

            if (!IsStackEmpty && !IsBufferEmpty && constraints != null)
            {
                int s0 = stack.Peek();
                int b0 = next;

                if (constraints.HaveForcedChildOnBuffer(s0, b0))
                {
                    canLA = false;
                    canReduce = false;
                }

                if (constraints.IsForcedHead(s0))
                {
                    int forceHid = constraints.ForcedHeadId(s0);
                    if (forceHid == 0)
                    {
                        canLA = false;
                        //canSH = false;
                    }
                    else if (forceHid > b0)
                    {
                        canReduce = false;
                        canLA = false;
                    }
                    else if (forceHid == b0)
                    {
                        canReduce = false;
                        canRA = false;
                        canSH = false;
                        if (constraints.IsForcedDepArc(s0))
                        {
                            canLA = labl == constraints.ForcedLabel(s0);
                        }
                    }
                }

                if (constraints.HaveForcedChildrenOnStack(b0, s0))
                {
                    canRA = false;
                    canSH = false;
                }

                if (constraints.IsForcedHead(b0))
                {
                    int fid = constraints.ForcedHeadId(b0);

                    if (fid == 0)
                    {
                        canRA = false;
                    }
                    else if (fid < s0)
                    {
                        canRA = false;
                        canSH = false;
                    }
                    else if (fid == s0)
                    {
                        canLA = false;
                        canReduce = false;
                        canSH = false;
                        if (constraints.IsForcedDepArc(b0))
                        {
                            canRA = labl == constraints.ForcedLabel(b0);
                        }
                    }
                    else if (fid > b0)
                    {
                        canRA = false;
                    }
                }
            }

            switch (pa)
            {
                case ParserAction.LA:
                    return canLA && IsApplicableAction(pa);
                case ParserAction.RA:
                    return canRA && IsApplicableAction(pa);
                case ParserAction.SHIFT:
                    return canSH && IsApplicableAction(pa);
                case ParserAction.REDUCE:
                    return canReduce && IsApplicableAction(pa);
                default:
                    return false;
            }
        }

        public bool ApplyParsingAction(ParserAction pa, int labl)
        {
            switch (pa)
            {
                case ParserAction.LA:
                    return LeftArc(labl);
                case ParserAction.RA:
                    return RightArc(labl);
                case ParserAction.REDUCE:
                    return Reduce();
                case ParserAction.SHIFT:
                    return Shift();
                default:
                    break;
            }
            return false;
        }

        public bool Shift()
        {
            if (IsApplicableAction(ParserAction.SHIFT))
            {
                stack.Push(next++);
               // FinishOff();
                SetKernals();
                return true;
            }
            else
            {
                return false;
            }
        }

        //private void FinishOff()
        //{
        //    if (next >= tok.Length - 1)
        //    {
        //        if (stack.Count == 1)
        //        {
        //            int s0 = stack.Pop();
        //            if (hid[s0] < 0)
        //            {
        //                hid[s0] = 0;
        //            }
        //        }
        //    }
        //    else if (next == tok.Length - 2 && IsStackEmpty)
        //    {
        //        hid[next] = 0;
        //        next++;
        //    }
        //}

        public bool Reduce()
        {
            if (IsApplicableAction(ParserAction.REDUCE))
            {
                int s0 = stack.Pop();
                if (hid[s0] < 0)
                {
                    hid[s0] = 0;
                }
                //FinishOff();
                SetKernals();
                
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool LeftArc(int labl)
        {
            if (IsApplicableAction(ParserAction.LA))
            {
                int s0 = stack.Pop();
                hid[s0] = next;
                label[s0] = labl;
                lchdcnt[next]++;
                llchdid[next] = lchdid[next];
                lchdid[next] = s0;
                //FinishOff();
                SetKernals();
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool RightArc(int labl)
        {
            if (IsApplicableAction(ParserAction.RA))
            {
                int s0 = stack[0];
                hid[next] = s0;
                label[next] = labl;
                rchdcnt[s0]++;
                rrchdid[s0] = rchdid[s0];
                rchdid[s0] = next;
                stack.Push(next);
                next++;
                //FinishOff();
                SetKernals();
                return true;
            }
            else
            {
                return false;
            }
        }

        static public bool InferAction(int[] hids, string[] labels, out ParserAction[] pa, out string[] xlabels)
        {
            int[] fakeTok = new int[hids.Length + 2];
            int[] fakePos = new int[hids.Length + 2];

            ParserConfig fakeConfig = new ParserConfig(fakeTok, fakePos, null);

            int[] refhids = new int[hids.Length + 2];
            int[] reflabels = new int[hids.Length + 2];

            hids.CopyTo(refhids, 1);
            for (int i = 0; i < reflabels.Length; ++i)
            {
                reflabels[i] = i - 1;
            }

            int[] refchdcnt = new int[hids.Length + 2];

            for (int i = 0; i < hids.Length; ++i)
            {
                refchdcnt[hids[i]]++;
            }

            List<ParserAction> palist = new List<ParserAction>();
            List<string> lblist = new List<string>();

            while (!fakeConfig.IsEnd)
            {
                ParserAction tp;
                int lablid;
                fakeConfig.InferAction(refhids, reflabels, refchdcnt, out tp, out lablid);
                palist.Add(tp);
                if (lablid < 0)
                {
                    lblist.Add("");
                }
                else
                {
                    lblist.Add(labels[lablid]);
                }
                if (!fakeConfig.ApplyParsingAction(tp, lablid))
                {
                    throw new Exception();
                }
            }

            pa = palist.ToArray();
            xlabels = lblist.ToArray();
            return true;
        }


        static public bool InferAction(int[] hids, out ParserAction[] pa)
        {
            string[] dummylabels = new string[hids.Length];
            for (int i = 0; i < dummylabels.Length; ++i)
            {
                dummylabels[i] = "dummy";
            }

            string[] xlabels;

            return InferAction(hids, dummylabels, out pa, out xlabels);
        }

        static public bool InferAction(int[] hids, out ParserAction[] pa, out int[] LabelIdx)
        {
            string[] dummylabels = new string[hids.Length];
            for (int i = 0; i < dummylabels.Length; ++i)
            {
                dummylabels[i] = i.ToString();
            }

            string[] xlabels;

            InferAction(hids, dummylabels, out pa, out xlabels);

            LabelIdx = new int[xlabels.Length];

            for (int i = 0; i < LabelIdx.Length; ++i)
            {
                if (pa[i] == ParserAction.LA || pa[i] == ParserAction.RA)
                {
                    LabelIdx[i] = int.Parse(xlabels[i]);
                }
                else
                {
                    LabelIdx[i] = -1;
                }
            }

            return true;
        }

        public void InferAction(int[] refhid, int[] reflabl, int[] refchdcnt, out ParserAction pa, out int labl)
        {
            labl = -1;
            pa = ParserAction.NIL;
            //if (refTree == null || refTree.root == null || refTree.Length != tree.Length)
            //{
            //    return;
            //}
            // trivil case
            if (IsStackEmpty && IsBufferEmpty)
            {
                return;
            }
            if (IsStackEmpty)
            {
                pa = ParserAction.SHIFT;
                return;
            }
            if (IsBufferEmpty)
            {
                pa = ParserAction.REDUCE;
                return;
            }
            // now the workingStack and incoming sequence both not empty
            int s0 = stack.Peek();
            int n0 = next;
            //DepNode refSNode = refTree[stkid];
            //DepNode refINode = refTree[bufid];
            int chdcnts0 = lchdcnt[s0] + rchdcnt[s0];
            int chdcntn0 = lchdcnt[n0] + rchdcnt[n0];

            if (refchdcnt[s0] == chdcnts0 && (hid[s0] >= 0 || refhid[s0] == 0))
            {
                pa = ParserAction.REDUCE;
            }
            else if (refhid[s0] != n0 && refhid[n0] != s0)
            {
                pa = ParserAction.SHIFT;
            }
            else if (refhid[s0] == n0)
            {
                pa = ParserAction.LA;
                labl = reflabl[s0];
            }
            else
            {
                pa = ParserAction.RA;
                //depType = refINode.depType;
                labl = reflabl[n0];
            }
        }

    }

    public class GParserConfig
    {
        public float score;
        private int[] tok;
        private int[] pos;
        private int[] wcluster;
        private int[] hid;
        private int[] label;

        public int[] state { get; private set; }

        private int next { get; set; }

        private SimpleStack<int> stack;

        // some auxiliary to help keep track of the parser state;
        private int[] lchdcnt;
        private int[] rchdcnt;
        private int[] lchdid;
        private int[] rchdid;
        private int[] llchdid;
        private int[] rrchdid;

        public List<int> CommandTrace;

        public bool Compare(GParserConfig other)
        {
            return //CompareArr(tok, other.tok)
                //&& CompareArr(pos, other.pos)
                CompareArr(hid, other.hid)
                && CompareArr(label, other.label)
                // && CompareArr(state, other.state)
                && next == other.next
                && CompareArr(lchdcnt, other.lchdcnt)
                && CompareArr(rchdcnt, other.rchdcnt)
                && CompareArr(lchdid, other.lchdid)
                && CompareArr(rchdid, other.rchdid)
                && CompareArr(llchdid, other.llchdid)
                && CompareArr(rrchdid, other.rrchdid)
                && CompareStack(stack, other.stack);

        }

        static private bool CompareArr(int[] a, int[] b)
        {
            if (a == null ^ b == null)
            {
                return false;
            }
            if (a == null && b == null)
            {
                return true;
            }

            if (a.Length != b.Length)
            {
                return false;
            }

            for (int i = 0; i < a.Length; ++i)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }

            return true;
        }

        static private bool CompareStack(SimpleStack<int> a, SimpleStack<int> b)
        {
            if (a == null ^ b == null)
            {
                return false;
            }
            if (a == null && b == null)
            {
                return true;
            }

            if (a.Cap != b.Cap)
            {
                return false;
            }

            if (a.Count != b.Count)
            {
                return false;
            }

            for (int i = 0; i < a.Count; ++i)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }

            return true;
        }

        public int[] HeadIDs()
        {
            int[] r = new int[hid.Length - 2];
            for (int i = 0; i < r.Length; ++i)
            {
                r[i] = hid[i + 1];
            }
            return r;
        }

        public int[] ArcLabels()
        {
            int[] r = new int[label.Length - 2];
            for (int i = 0; i < r.Length; ++i)
            {
                r[i] = label[i + 1];
            }
            return r;
        }

        public bool IsEnd { get { return IsBufferEmpty; } }

        public bool IsStackEmpty { get { return stack.IsEmpty; } }

        public bool IsBufferEmpty { get { return next >= tok.Length - 1; } }

        public GParserConfig(int[] tok, int[] pos, int[] wcluster)
        {
            int len = tok.Length;
            this.tok = (int[])tok.Clone();
            this.pos = (int[])pos.Clone();
            this.wcluster = wcluster;
            if (wcluster == null)
            {
                this.wcluster = new int[this.tok.Length];

                for (int i = 0; i < this.wcluster.Length; ++i)
                {
                    this.wcluster[i] = -1;
                }
            }
            hid = new int[len];
            label = new int[len];
            lchdcnt = new int[len];
            lchdid = new int[len];
            rchdcnt = new int[len];
            rchdid = new int[len];
            llchdid = new int[len];
            rrchdid = new int[len];

            for (int i = 0; i < len; ++i)
            {
                hid[i] = -1;
            }

            for (int i = 0; i < len; ++i)
            {
                label[i] = -1;
            }

            for (int i = 0; i < len; ++i)
            {
                lchdid[i] = -1;
            }

            for (int i = 0; i < len; ++i)
            {
                rchdid[i] = -1;
            }

            for (int i = 0; i < len; ++i)
            {
                llchdid[i] = -1;
            }

            for (int i = 0; i < len; ++i)
            {
                rrchdid[i] = -1;
            }

            stack = new SimpleStack<int>(len);

            state = new int[ParserStateDescriptor.Count];

            stack.Push(0);

            next = 1;

            CommandTrace = new List<int>();

            SetKernals();
        }

        public GParserConfig Clone()
        {
            GParserConfig clone = new GParserConfig();

            clone.tok = (int[])tok.Clone();
            clone.pos = (int[])pos.Clone();
            clone.hid = (int[])hid.Clone();
            clone.wcluster = (int[])wcluster.Clone();
            clone.label = (int[])label.Clone();
            clone.lchdcnt = (int[])lchdcnt.Clone();
            clone.lchdid = (int[])lchdid.Clone();
            clone.rchdcnt = (int[])rchdcnt.Clone();
            clone.rchdid = (int[])rchdid.Clone();
            clone.llchdid = (int[])llchdid.Clone();
            clone.rrchdid = (int[])rrchdid.Clone();
            clone.stack = stack.Clone();
            clone.next = next;
            clone.score = score;
            clone.state = (int[])state.Clone();

            clone.CommandTrace = new List<int>();

            foreach (var c in CommandTrace)
            {
                clone.CommandTrace.Add(c);
            }

            return clone;
        }

        /// <summary>
        /// no safety check whatever
        /// </summary>
        /// <param name="backup"></param>
        public void CopyTo(GParserConfig backup)
        {
            hid.CopyTo(backup.hid, 0);
            label.CopyTo(backup.label, 0);
            lchdcnt.CopyTo(backup.lchdcnt, 0);
            rchdcnt.CopyTo(backup.rchdcnt, 0);
            lchdid.CopyTo(backup.lchdid, 0);
            rchdid.CopyTo(backup.rchdid, 0);
            llchdid.CopyTo(backup.llchdid, 0);
            rrchdid.CopyTo(backup.rrchdid, 0);
            state.CopyTo(backup.state, 0);
            backup.next = next;
            backup.score = score;
            stack.CopyTo(backup.stack);
            backup.CommandTrace.Clear();

            foreach (var c in CommandTrace)
            {
                backup.CommandTrace.Add(c);
            }

        }

        private GParserConfig()
        {
        }

        private void SetKernals()
        {
            // inremental update is not used;
            // clear all state;
            for (int i = 0; i < state.Length; ++i)
            {
                state[i] = -1;
            }
            // feature on s0
            if (!IsStackEmpty)
            {
                int s0id = stack[0];
                state[ParserStateDescriptor.s0ID] = s0id;
                state[ParserStateDescriptor.s0w] = tok[s0id];
                state[ParserStateDescriptor.s0p] = pos[s0id];
                state[ParserStateDescriptor.s0arc] = hid[s0id] == -1 ? -1 : label[s0id];
                state[ParserStateDescriptor.s0vl] = QuantValence(lchdcnt[s0id]);
                state[ParserStateDescriptor.s0vr] = QuantValence(rchdcnt[s0id]);
                state[ParserStateDescriptor.s0c] = wcluster[s0id];
            }

            // feature on s0h
            if (!IsStackEmpty && hid[stack[0]] >= 0)
            {
                int id = hid[stack[0]];
                state[ParserStateDescriptor.s0hID] = id;
                state[ParserStateDescriptor.s0hw] = tok[id];
                state[ParserStateDescriptor.s0hp] = pos[id];
                state[ParserStateDescriptor.s0harc] = hid[id] == -1 ? -1 : label[id];
                state[ParserStateDescriptor.s0hc] = wcluster[id];
            }

            // feature on s0h2
            if (!IsStackEmpty && hid[stack[0]] >= 0 && hid[hid[stack[0]]] >= 0)
            {
                int id = hid[hid[stack[0]]];
                state[ParserStateDescriptor.s0h2ID] = id;
                state[ParserStateDescriptor.s0h2w] = tok[id];
                state[ParserStateDescriptor.s0h2p] = pos[id];
                state[ParserStateDescriptor.s0h2c] = wcluster[id];
            }

            // feature on s0l
            if (!IsStackEmpty && lchdid[stack[0]] >= 0)
            {
                int id = lchdid[stack[0]];
                state[ParserStateDescriptor.s0lID] = id;
                state[ParserStateDescriptor.s0lw] = tok[id];
                state[ParserStateDescriptor.s0lp] = pos[id];
                state[ParserStateDescriptor.s0larc] = hid[id] == -1 ? -1 : label[id];
                state[ParserStateDescriptor.s0lc] = wcluster[id];
            }

            // feature on s0r
            if (!IsStackEmpty && rchdid[stack[0]] >= 0)
            {
                int id = rchdid[stack[0]];
                state[ParserStateDescriptor.s0rID] = id;
                state[ParserStateDescriptor.s0rw] = tok[id];
                state[ParserStateDescriptor.s0rp] = pos[id];
                state[ParserStateDescriptor.s0rarc] = hid[id] == -1 ? -1 : label[id];
                state[ParserStateDescriptor.s0rc] = wcluster[id];
            }

            // feature on s0l2
            if (!IsStackEmpty && llchdid[stack[0]] >= 0)
            {
                int id = llchdid[stack[0]];
                state[ParserStateDescriptor.s0l2ID] = id;
                state[ParserStateDescriptor.s0l2w] = tok[id];
                state[ParserStateDescriptor.s0l2p] = pos[id];
                state[ParserStateDescriptor.s0l2arc] = hid[id] == -1 ? -1 : label[id];
            }

            // feature on s0r2
            if (!IsStackEmpty && rrchdid[stack[0]] >= 0)
            {
                int id = rrchdid[stack[0]];
                state[ParserStateDescriptor.s0r2ID] = id;
                state[ParserStateDescriptor.s0r2w] = tok[id];
                state[ParserStateDescriptor.s0r2p] = pos[id];
                state[ParserStateDescriptor.s0r2arc] = hid[id] == -1 ? -1 : label[id];
            }

            // feature on n0
            if (next < tok.Length)
            {
                int id = next;
                state[ParserStateDescriptor.n0ID] = id;
                state[ParserStateDescriptor.n0w] = tok[id];
                state[ParserStateDescriptor.n0p] = pos[id];
                //state[KernalFeature.n0arc] = hid[s0id] == -1 ? -1 : label[s0id];
                state[ParserStateDescriptor.n0vl] = QuantValence(lchdcnt[id]);
                state[ParserStateDescriptor.n0c] = wcluster[id];
                //state[KernalFeature.s0vr] = QuantValence(rchdcnt[s0id]);
            }

            // feature on n0l
            if (next < tok.Length && lchdid[next] >= 0)
            {
                int id = lchdid[next];
                state[ParserStateDescriptor.n0lID] = id;
                state[ParserStateDescriptor.n0lw] = tok[id];
                state[ParserStateDescriptor.n0lp] = pos[id];
                state[ParserStateDescriptor.n0larc] = hid[id] == -1 ? -1 : label[id];
                state[ParserStateDescriptor.n0lc] = wcluster[id];
            }

            // feature on n0l2
            if (next < tok.Length && llchdid[next] >= 0)
            {
                int id = llchdid[next];
                state[ParserStateDescriptor.n0l2ID] = id;
                state[ParserStateDescriptor.n0l2w] = tok[id];
                state[ParserStateDescriptor.n0l2p] = pos[id];
                state[ParserStateDescriptor.n0l2arc] = hid[id] == -1 ? -1 : label[id];
            }

            // feature on n1
            if (next + 1 < tok.Length)
            {
                int id = next + 1;
                state[ParserStateDescriptor.n1p] = pos[id];
                state[ParserStateDescriptor.n1w] = tok[id];
                state[ParserStateDescriptor.n1c] = wcluster[id];
            }

            // feature on n1
            if (next + 2 < tok.Length)
            {
                int id = next + 2;
                state[ParserStateDescriptor.n2p] = pos[id];
                state[ParserStateDescriptor.n2w] = tok[id];
                state[ParserStateDescriptor.n2c] = wcluster[id];
            }

            // misc
            state[ParserStateDescriptor.dst] = (IsBufferEmpty || IsStackEmpty) ? -1 : QuantDistance(next - stack[0]);
        }

        private int QuantValence(int v)
        {
            if (v < 5)
            {
                return v;
            }
            else if (v < 8)
            {
                return 5;
            }
            else
            {
                return 6;
            }
        }

        private int QuantDistance(int d)
        {
            if (d < 4)
            {
                return d;
            }
            else if (d < 5)
            {
                return 4;
            }
            else if (d < 10)
            {
                return 5;
            }
            else
            {
                return 6;
            }
        }

        public bool IsApplicableAction(ParserAction pa)
        {
            switch (pa)
            {
                case ParserAction.LA:
                    return !IsStackEmpty && stack[0] != 0 && !IsBufferEmpty && hid[stack.Peek()] == -1;
                case ParserAction.RA:
                    return !IsStackEmpty && !IsBufferEmpty && hid[next] == -1;
                case ParserAction.REDUCE:
                    return !IsStackEmpty && hid[stack[0]] != -1;
                case ParserAction.SHIFT:
                    return !IsBufferEmpty;
                default:
                    return false;
            }
        }

        public bool IsApplicableAction(ParserAction pa, int labl, ParserForcedConstraints constraints)
        {
            if (constraints == null)
            {
                return IsApplicableAction(pa);
            }

            throw new Exception("not implemented yet!");
            bool canReduce = true;
            bool canLA = true;
            bool canRA = true;
            bool canSH = true;

            if (!IsStackEmpty && !IsBufferEmpty && constraints != null)
            {
                int s0 = stack.Peek();
                int b0 = next;

                if (constraints.HaveForcedChildOnBuffer(s0, b0))
                {
                    canLA = false;
                    canReduce = false;
                }

                if (constraints.IsForcedHead(s0))
                {
                    int forceHid = constraints.ForcedHeadId(s0);
                    if (forceHid == 0)
                    {
                        canLA = false;
                        //canSH = false;
                    }
                    else if (forceHid > b0)
                    {
                        canReduce = false;
                        canLA = false;
                    }
                    else if (forceHid == b0)
                    {
                        canReduce = false;
                        canRA = false;
                        canSH = false;
                        if (constraints.IsForcedDepArc(s0))
                        {
                            canLA = labl == constraints.ForcedLabel(s0);
                        }
                    }
                }

                if (constraints.HaveForcedChildrenOnStack(b0, s0))
                {
                    canRA = false;
                    canSH = false;
                }

                if (constraints.IsForcedHead(b0))
                {
                    int fid = constraints.ForcedHeadId(b0);

                    if (fid == 0)
                    {
                        canRA = false;
                    }
                    else if (fid < s0)
                    {
                        canRA = false;
                        canSH = false;
                    }
                    else if (fid == s0)
                    {
                        canLA = false;
                        canReduce = false;
                        canSH = false;
                        if (constraints.IsForcedDepArc(b0))
                        {
                            canRA = labl == constraints.ForcedLabel(b0);
                        }
                    }
                    else if (fid > b0)
                    {
                        canRA = false;
                    }
                }
            }

            switch (pa)
            {
                case ParserAction.LA:
                    return canLA && IsApplicableAction(pa);
                case ParserAction.RA:
                    return canRA && IsApplicableAction(pa);
                case ParserAction.SHIFT:
                    return canSH && IsApplicableAction(pa);
                case ParserAction.REDUCE:
                    return canReduce && IsApplicableAction(pa);
                default:
                    return false;
            }
        }

        public bool ApplyParsingAction(ParserAction pa, int labl)
        {
            switch (pa)
            {
                case ParserAction.LA:
                    return LeftArc(labl);
                case ParserAction.RA:
                    return RightArc(labl);
                case ParserAction.REDUCE:
                    return Reduce();
                case ParserAction.SHIFT:
                    return Shift();
                default:
                    break;
            }
            return false;
        }

        public bool Shift()
        {
            if (IsApplicableAction(ParserAction.SHIFT))
            {
                stack.Push(next++);
                // FinishOff();
                SetKernals();
                return true;
            }
            else
            {
                return false;
            }
        }

        //private void FinishOff()
        //{
        //    if (next >= tok.Length - 1)
        //    {
        //        if (stack.Count == 1)
        //        {
        //            int s0 = stack.Pop();
        //            if (hid[s0] < 0)
        //            {
        //                hid[s0] = 0;
        //            }
        //        }
        //    }
        //    else if (next == tok.Length - 2 && IsStackEmpty)
        //    {
        //        hid[next] = 0;
        //        next++;
        //    }
        //}

        public bool Reduce()
        {
            if (IsApplicableAction(ParserAction.REDUCE))
            {
                int s0 = stack.Pop();
                //if (hid[s0] < 0)
                //{
                //    //hid[s0] = 0;
                //    throw new Exception("cannot reduce!");
                //}
                //FinishOff();
                SetKernals();

                return true;
            }
            else
            {
                return false;
            }
        }

        public bool LeftArc(int labl)
        {
            if (IsApplicableAction(ParserAction.LA))
            {
                int s0 = stack.Pop();
                hid[s0] = next;
                label[s0] = labl;
                lchdcnt[next]++;
                llchdid[next] = lchdid[next];
                lchdid[next] = s0;
                //FinishOff();
                SetKernals();
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool RightArc(int labl)
        {
            if (IsApplicableAction(ParserAction.RA))
            {
                int s0 = stack[0];
                hid[next] = s0;
                label[next] = labl;
                rchdcnt[s0]++;
                rrchdid[s0] = rchdid[s0];
                rchdid[s0] = next;
                stack.Push(next);
                next++;
                //FinishOff();
                SetKernals();
                return true;
            }
            else
            {
                return false;
            }
        }

        static public bool InferAction(int[] hids, string[] labels, out ParserAction[] pa, out string[] xlabels)
        {
            int[] fakeTok = new int[hids.Length + 2];
            int[] fakePos = new int[hids.Length + 2];

            ParserConfig fakeConfig = new ParserConfig(fakeTok, fakePos, null);

            int[] refhids = new int[hids.Length + 2];
            int[] reflabels = new int[hids.Length + 2];

            hids.CopyTo(refhids, 1);
            for (int i = 0; i < reflabels.Length; ++i)
            {
                reflabels[i] = i - 1;
            }

            int[] refchdcnt = new int[hids.Length + 2];

            for (int i = 0; i < hids.Length; ++i)
            {
                refchdcnt[hids[i]]++;
            }

            List<ParserAction> palist = new List<ParserAction>();
            List<string> lblist = new List<string>();

            while (!fakeConfig.IsEnd)
            {
                ParserAction tp;
                int lablid;
                fakeConfig.InferAction(refhids, reflabels, refchdcnt, out tp, out lablid);
                palist.Add(tp);
                if (lablid < 0)
                {
                    lblist.Add("");
                }
                else
                {
                    lblist.Add(labels[lablid]);
                }
                if (!fakeConfig.ApplyParsingAction(tp, lablid))
                {
                    throw new Exception();
                }
            }

            pa = palist.ToArray();
            xlabels = lblist.ToArray();
            return true;
        }

        static public bool InferAction(int[] hids, out ParserAction[] pa)
        {
            string[] dummylabels = new string[hids.Length];
            for (int i = 0; i < dummylabels.Length; ++i)
            {
                dummylabels[i] = "dummy";
            }

            string[] xlabels;

            return InferAction(hids, dummylabels, out pa, out xlabels);
        }

        static public bool InferAction(int[] hids, out ParserAction[] pa, out int[] LabelIdx)
        {
            string[] dummylabels = new string[hids.Length];
            for (int i = 0; i < dummylabels.Length; ++i)
            {
                dummylabels[i] = i.ToString();
            }

            string[] xlabels;

            InferAction(hids, dummylabels, out pa, out xlabels);

            LabelIdx = new int[xlabels.Length];

            for (int i = 0; i < LabelIdx.Length; ++i)
            {
                if (pa[i] == ParserAction.LA || pa[i] == ParserAction.RA)
                {
                    LabelIdx[i] = int.Parse(xlabels[i]);
                }
                else
                {
                    LabelIdx[i] = -1;
                }
            }

            return true;
        }

        public void InferAction(int[] refhid, int[] reflabl, int[] refchdcnt, out ParserAction pa, out int labl)
        {
            labl = -1;
            pa = ParserAction.NIL;
            //if (refTree == null || refTree.root == null || refTree.Length != tree.Length)
            //{
            //    return;
            //}
            // trivil case
            if (IsStackEmpty && IsBufferEmpty)
            {
                return;
            }
            if (IsStackEmpty)
            {
                pa = ParserAction.SHIFT;
                return;
            }
            if (IsBufferEmpty)
            {
                pa = ParserAction.REDUCE;
                return;
            }
            // now the workingStack and incoming sequence both not empty
            int s0 = stack.Peek();
            int n0 = next;
            //DepNode refSNode = refTree[stkid];
            //DepNode refINode = refTree[bufid];
            int chdcnts0 = lchdcnt[s0] + rchdcnt[s0];
            int chdcntn0 = lchdcnt[n0] + rchdcnt[n0];

            if (refchdcnt[s0] == chdcnts0 && (hid[s0] >= 0 || refhid[s0] == 0))
            {
                pa = ParserAction.REDUCE;
            }
            else if (refhid[s0] != n0 && refhid[n0] != s0)
            {
                pa = ParserAction.SHIFT;
            }
            else if (refhid[s0] == n0)
            {
                pa = ParserAction.LA;
                labl = reflabl[s0];
            }
            else
            {
                pa = ParserAction.RA;
                //depType = refINode.depType;
                labl = reflabl[n0];
            }
        }

        public bool IsZeroCostAction(ParserAction pa, int labl, int[] refhid, int[] reflabl)
        {
            if (IsEnd)
            {
                return false;
            }

            if (!IsApplicableAction(pa))
            {
                return false;
            }

            switch (pa)
            {
                case ParserAction.LA:
                    if (refhid[stack[0]] > next)
                    {
                        return false;
                    }
                    for (int k = next + 1; k < tok.Length - 1; ++k)
                    {
                        if (refhid[k] == stack[0])
                        {
                            return false;
                        }
                    }
                    if (refhid[stack[0]] == next && reflabl[stack[0]] != labl)
                    {
                        return false;
                    }
                    return true;
                case ParserAction.RA:
                    if (refhid[next] < stack[0])
                    {
                        for (int k = 1; k < stack.Count; ++k)
                        {
                            if (refhid[next] == stack[k])
                            {
                                return false;
                            }
                        }
                    }
                    else if (refhid[next] > next)
                    {
                        return false;
                    }
                    for (int k = 1; k < stack.Count; ++k)
                    {
                        if (refhid[stack[k]] == next)
                        {
                            return false;
                        }
                    }
                    if (refhid[next] == stack[0] && reflabl[next] != labl && refhid[next] != 0)
                    {
                        return false;
                    }
                    return true;
                case ParserAction.REDUCE:
                    for (int k = next; k < tok.Length - 1; ++k)
                    {
                        if (refhid[k] == stack[0])
                        {
                            return false;
                        }
                    }
                    return true;
                case ParserAction.SHIFT:
                    for (int k = 0; k < stack.Count; ++k)
                    {
                        if (refhid[next] == stack[k])
                        {
                            return false;
                        }

                        if (refhid[stack[k]] == next)
                        {
                            return false;
                        }
                    }
                    return true;
                default:
                    return false;
            }
        }
    }
}
