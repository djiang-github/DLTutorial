using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinearFunction
{
    public class LinearChainFeatureTemplate
    {
        public LinearChainFeatureTemplate(int templateId, int dictId, int groupId, int[] offsets, int[] observId)
        {
            this.offsets = offsets;
            this.observId = observId;
            this.DictId = dictId;
            this.TemplateId = templateId;
            this.GroupId = groupId;
            Length = offsets.Length;
            IsStatFeature = false;
            IsUniTagFeature = false;
            int ynum = 0;
            int yoff = 0;
            for (int i = 0; i < observId.Length; ++i)
            {
                if (observId[i] < 0)
                {
                    ynum++;
                    yoff = offsets[i];
                }
            }
            if (ynum == 0)
            {
                IsStatFeature = true;
            }
            else if (ynum == 1)
            {
                IsUniTagFeature = true;
                yOffset = yoff;
            }
        }

        public bool GetFeature(int[] TagSequence, int[][] Observation, int Id, LinearModelFeature feature)
        {
            int[] feat = feature.ElemArr;

            for (int i = 0; i < Length; ++i)
            {
                int x = Id + offsets[i];
                if (x < 0 || x >= Observation.Length)
                {
                    feat[0] = -1;
                    return false;
                }
                if (observId[i] < 0)
                {
                    // tag feature;
                    feat[i] = TagSequence[x];
                    if (feat[i] < 0)
                    {
                        feat[0] = -1;
                        return false;
                    }
                }
                else
                {
                    if ((feat[i] = Observation[x][observId[i]]) < 0)
                    {
                        feat[0] = -1;
                        return false;
                    }
                }
            }
            return true;
        }

        public int Length { get; private set; }

        public bool IsStatFeature { get; private set; }

        public bool IsUniTagFeature { get; private set; }

        public bool GetYOffset(out int offset)
        {
            offset = yOffset;
            if (!IsUniTagFeature)
            {
                return false;
            }
            return true;
        }

        public string Name
        {
            get
            {
                List<string> namefrags = new List<string>();

                if (GroupId >= 0)
                {
                    namefrags.Add("b");
                    namefrags.Add(GroupId.ToString());
                }

                for (int i = 0; i < Length; ++i)
                {
                    if (observId[i] < 0)
                    {
                        namefrags.Add("y");
                        namefrags.Add(offsets[i].ToString());
                    }
                    else
                    {
                        namefrags.Add("x");
                        namefrags.Add(offsets[i].ToString());
                        namefrags.Add(observId[i].ToString());
                    }
                }

                return string.Join(" ", namefrags);
            }
        }

        private int yOffset;

        private int[] offsets;

        // observId[i] < 0 means it is a Tag features;
        private int[] observId;

        public int DictId { get; private set; }
        public int TemplateId { get; private set; }
        public int GroupId { get; private set; }

        public LinearModelFeature MakeCache()
        {
            return new LinearModelFeature(DictId, Length);
        }

    }
}
