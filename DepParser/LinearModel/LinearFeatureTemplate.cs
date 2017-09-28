using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinearFunction
{


    public class LinearFeatureTemplate
    {
        public int TemplateID { get; private set; }
        public int DictID { get; private set; }
        public int GroupID { get; private set; }
        public int[] stateIds;
        public int[] determinate;

        public int Length { get { return stateIds.Length; } }

        

        public void GetFeature(int[] state, LinearModelFeature feature)
        {
            int[] ElemArr = feature.ElemArr;
            for (int i = 0; i < stateIds.Length; ++i)
            {
                if (state[stateIds[i]] < 0)
                {
                    ElemArr[i] = -1;
                    ElemArr[0] = -1;
                    return;
                }
                ElemArr[i] = state[stateIds[i]];
            }
        }

        public LinearModelFeature GetFeature(int[] state)
        {
            LinearModelFeature feature = MakeCache();

            for (int i = 0; i < stateIds.Length; ++i)
            {
                if (state[stateIds[i]] < 0)
                {
                    return null;
                }
                feature.ElemArr[i] = state[stateIds[i]];
            }

            return feature;
        }

        public LinearFeatureTemplate(int TemplateID, int DictId, int GroupId, int[] stateIds, int[] determinate)
        {
            this.TemplateID = TemplateID;
            this.DictID = DictId;
            this.GroupID = GroupId;

            this.determinate = (int[])determinate.Clone();
            this.stateIds = (int[])stateIds.Clone();
            Array.Sort<int>(this.determinate);
            List<int> detlist = new List<int>();
            int last = this.determinate[0];
            detlist.Add(last);

            for (int i = 1; i < this.determinate.Length; ++i)
            {
                if (last != this.determinate[i])
                {
                    detlist.Add(this.determinate[i]);
                    last = this.determinate[i];
                }
            }
            this.determinate = detlist.ToArray();
        }

        public LinearModelFeature MakeCache()
        {
            return new LinearModelFeature(DictID, Length);
        }

        public string Name
        {
            get
            {
                if (GroupID >= 0)
                {
                    return string.Format("b {0} {1}",
                        GroupID, string.Join(" ", stateIds));

                }
                else
                {
                    return string.Join(" ", stateIds);
                }
            }
        }

        public bool IsSameFeatureGroup(LinearFeatureTemplate other)
        {
            if (other == null)
            {
                return false;
            }
            if (other.determinate.Length != determinate.Length)
            {
                return false;
            }
            for (int i = 0; i < determinate.Length; ++i)
            {
                if (determinate[i] != other.determinate[i])
                {
                    return false;
                }
            }
            return true;
        }
    }

    public class LinearFeatureGroup
    {
        public enum GroupType
        {
            Unigram,
            Bigram,
            Other
        }

        public LinearFeatureTemplate[] features;
        
        //public int[] featureIDs;
        //public int[] dictIDs;

        public GroupType type { get; private set; }

        public bool GetDetHashCode(int[] state, out int hash)
        {
            switch (type)
            {
                case GroupType.Unigram:
                    hash = state[det0];
                    return true;
                case GroupType.Bigram:
                    if (((uint)state[det0]) >> 16 != 0 || ((uint)state[det1]) >> 16 != 0)
                    {
                        throw new Exception("Error generating hashcode!");
                    }
                    hash = (int)((((uint)state[det0]) << 16) | (uint)state[det1]);
                    return true;
                default:
                    hash = 0;
                    return false;
            }
        }

        public bool IsZeroValueState(int[] state)
        {
            switch (type)
            {
                case GroupType.Unigram:
                    return state[det0] < 0;
                case GroupType.Bigram:
                    return state[det0] < 0 || state[det1] < 0;
                default:
                    return false;
            }
        }

        private readonly int det0;
        private readonly int det1;

        public int Count { get { return features.Length; } }
        public LinearFeatureGroup(LinearFeatureTemplate[] features)
        {
            this.features = (LinearFeatureTemplate[])features.Clone();
            //featureIDs = new int[features.Length];
            //for (int i = 0; i < featureIDs.Length; ++i)
            //{
            //    featureIDs[i] = features[i].TemplateID;
            //}

            bool isPureGroup = true;
            for (int i = 1; i < features.Length; ++i)
            {
                if (!features[0].IsSameFeatureGroup(features[i]))
                {
                    isPureGroup = false;
                }
            }

            if (isPureGroup)
            {
                if (features[0].determinate.Length == 1)
                {
                    type = GroupType.Unigram;
                    det0 = features[0].determinate[0];
                }
                else if (features[0].determinate.Length == 2)
                {
                    type = GroupType.Bigram;
                    det0 = features[0].determinate[0];
                    det1 = features[0].determinate[1];
                }
                else
                {
                    type = GroupType.Other;
                }
            }
            else
            {
                type = GroupType.Other;
            }
        }

        public void GetFeatures(int[] state, LinearModelFeature[] cache)
        {
            for (int i = 0; i < features.Length; ++i)
            {
                features[i].GetFeature(state, cache[i]);
            }
        }
    }

    public class LinearFeatureGroupCache
    {
        public LinearFeatureGroup featureGroup { get; private set; }
        public LinearModelFeature[] featureCache;
        public LinearFeatureGroupCache(LinearFeatureGroup featureGroup)
        {
            this.featureGroup = featureGroup;
            featureCache = new LinearModelFeature[featureGroup.Count];
            for (int i = 0; i < featureGroup.Count; ++i)
            {
                featureCache[i] = featureGroup.features[i].MakeCache();
            }
        }

        public void GenerateFeatureToCache(int[] state)
        {
            featureGroup.GetFeatures(state, featureCache);
        }
    }

    public class LinearFeatureTemplateSet
    {
        public LinearFeatureTemplate[] Templates;

        public LinearFeatureGroup[] TemplateGroups;
    }
}
