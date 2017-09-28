using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinearDepParser
{
    public class ParserForcedConstraints
    {
        public bool IsForcedHead(int id)
        {
            return isForcedHid[id];
        }

        public int ForcedHeadId(int id)
        {
            return forcedHid[id];
        }

        public int ForcedLabel(int id)
        {
            return forcedLabel[id];
        }

        public bool IsForcedDepArc(int id)
        {
            return isForcedArcLabel[id] && forcedLabel[id] != -1;
        }

        public bool HaveForcedChildOnBuffer(int id, int bufferTop)
        {
            if (childrenList[id].Count == 0)
            {
                return false;
            }
            else
            {
                return childrenList[id][childrenList[id].Count - 1] >= bufferTop;
            }
        }

        public bool HaveForcedChildrenOnStack(int id, int StackTop)
        {
            if (childrenList[id].Count == 0)
            {
                return false;
            }
            else
            {
                return childrenList[id][0] <= StackTop;
            }
        }

        public ParserForcedConstraints(ParserVocab vocab, int[] forcedHid, string[] forcedArcLabel)
        {
            int N = forcedHid.Length + 1;
            this.isForcedHid = new bool[N];
            this.isForcedArcLabel = new bool[N];
            this.forcedHid = new int[N];
            this.forcedArcLabel = new string[N];
            this.vocab = vocab;

            //isForcedHead.CopyTo(this.isForcedHid, 1);
            for (int i = 0; i < forcedHid.Length; ++i)
            {
                isForcedHid[i + 1] = forcedHid[i] >= 0;
            }
            forcedHid.CopyTo(this.forcedHid, 1);
            forcedArcLabel.CopyTo(this.forcedArcLabel, 1);

            forcedLabel = new int[N];

            Init();
        }

        public void Init()
        {
            for (int i = 0; i < isForcedHid.Length; ++i)
            {
                if (!isForcedHid[i])
                {
                    isForcedArcLabel[i] = false;
                    forcedHid[i] = -1;
                    forcedArcLabel[i] = null;
                    forcedLabel[i] = -1;
                }
                else if (forcedArcLabel[i] != null)
                {
                    isForcedArcLabel[i] = true;
                    forcedLabel[i] = vocab.GetLabelId(forcedArcLabel[i]);
                }
            }

            childrenList = new List<int>[isForcedHid.Length];
            for (int i = 0; i < isForcedHid.Length; ++i)
            {
                childrenList[i] = new List<int>();
            }

            for (int i = 0; i < isForcedHid.Length; ++i)
            {
                if (isForcedHid[i])
                {
                    childrenList[forcedHid[i]].Add(i);
                }
            }
        }

        public void AddConstraint(int id, int headid, string label)
        {
            isForcedHid[id] = true;
            forcedHid[id] = headid;
            forcedArcLabel[id] = label;
            forcedLabel[id] = vocab.GetLabelId(label);
            Init();
        }

        List<int>[] childrenList;

        bool[] isForcedHid;
        bool[] isForcedArcLabel;
        int[] forcedHid;
        string[] forcedArcLabel;
        int[] forcedLabel;
        ParserVocab vocab;
    }
}
