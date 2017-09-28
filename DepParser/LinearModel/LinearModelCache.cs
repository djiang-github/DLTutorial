using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinearFunction
{
    public class LinearModelCache
    {
        public void Clear()
        {
            foreach (Dictionary<int, float[]> cache in scoreCache)
            {
                if (cache != null)
                {
                    cache.Clear();
                }
            }
        }

        public void GetScores(int[] state, float[] scores)
        {
            for (int i = 0; i < scores.Length; ++i)
            {
                scores[i] = 0;
            }
            for (int i = 0; i < featureCache.Length; ++i)
            {
                LinearFeatureGroup fg = featureCache[i].featureGroup;

                if (fg.IsZeroValueState(state))
                {
                    continue;
                }

                int hash;

                if (fg.GetDetHashCode(state, out hash))
                {
                    float[] xscore;
                    if (!scoreCache[i].TryGetValue(hash, out xscore))
                    {
                        xscore = new float[scores.Length];
                        fg.GetFeatures(state, featureCache[i].featureCache);
                        Model.GetScores(featureCache[i].featureCache, xscore);
                        scoreCache[i][hash] = xscore;
                        AddScore(scores, xscore);
                    }
                    else
                    {
                        AddScore(scores, xscore);
                    }

                }
                else
                {
                    fg.GetFeatures(state, featureCache[i].featureCache);
                    Model.AddScores(featureCache[i].featureCache, scores);
                }
            }
        }

        public void GetScores(int[] state, int[] replaceId, int[][] replacement, float[] scores)
        {
            // for efficiency reason, assuming replacement is independent of each other

            for (int i = 0; i < scores.Length; ++i)
            {
                scores[i] = 0;
            }
            for (int i = 0; i < featureCache.Length; ++i)
            {
                LinearFeatureGroup fg = featureCache[i].featureGroup;

                if (fg.IsZeroValueState(state))
                {
                    continue;
                }

                int hash;

                if (fg.GetDetHashCode(state, out hash))
                {
                    float[] xscore;
                    if (!scoreCache[i].TryGetValue(hash, out xscore))
                    {
                        xscore = new float[scores.Length];
                        fg.GetFeatures(state, featureCache[i].featureCache);
                        Model.GetScores(featureCache[i].featureCache, xscore);
                        scoreCache[i][hash] = xscore;
                        AddScore(scores, xscore);
                    }
                    else
                    {
                        AddScore(scores, xscore);
                    }

                }
                else
                {
                    fg.GetFeatures(state, featureCache[i].featureCache);
                    Model.AddScores(featureCache[i].featureCache, scores);
                }
            }

            // deal with replacements

            for (int i = 0; i < replaceId.Length; ++i)
            {
                int rid = replaceId[i];
                if (rid >= featuresRelatedToStateId.Length
                    || featuresRelatedToStateId[rid] == null)
                {
                    continue;
                }

                LinearFeatureTemplate[] templates = featuresRelatedToStateId[rid];
                LinearModelFeature[] features = featureCacheRelatedToStateId[rid];

                int originalState = state[rid];

                for (int j = 0; j < templates.Length; ++j)
                {
                    foreach (int rplcm in replacement[i])
                    {
                        if (rplcm < 0)
                        {
                            break;
                        }

                        state[rid] = rplcm;

                        templates[j].GetFeature(state, features[j]);
                        Model.AddScores(features[j], scores);
                    }
                }

                state[rid] = originalState;
            }

        }

        public void AddScores(int[] state, float[] scores, int gid)
        {
            LinearFeatureGroup fg = featureCache[gid].featureGroup;

            if (fg.IsZeroValueState(state))
            {
                return;
            }

            int hash;

            if (fg.GetDetHashCode(state, out hash))
            {
                float[] xscore;
                if (!scoreCache[gid].TryGetValue(hash, out xscore))
                {
                    xscore = new float[scores.Length];
                    fg.GetFeatures(state, featureCache[gid].featureCache);
                    Model.GetScores(featureCache[gid].featureCache, xscore);
                    scoreCache[gid][hash] = xscore;
                    AddScore(scores, xscore);
                }
                else
                {
                    AddScore(scores, xscore);
                }

            }
            else
            {
                fg.GetFeatures(state, featureCache[gid].featureCache);
                Model.AddScores(featureCache[gid].featureCache, scores);
            }
        }

        public void AddScores(int[] state, int[] replaceId, int[][] replacement, float[] scores, int gid)
        {
            LinearFeatureGroup fg = featureCache[gid].featureGroup;

            if (fg.IsZeroValueState(state))
            {
                return;
            }

            int hash;

            if (fg.GetDetHashCode(state, out hash))
            {
                float[] xscore;
                if (!scoreCache[gid].TryGetValue(hash, out xscore))
                {
                    xscore = new float[scores.Length];
                    fg.GetFeatures(state, featureCache[gid].featureCache);
                    Model.GetScores(featureCache[gid].featureCache, xscore);
                    scoreCache[gid][hash] = xscore;
                    AddScore(scores, xscore);
                }
                else
                {
                    AddScore(scores, xscore);
                }

            }
            else
            {
                fg.GetFeatures(state, featureCache[gid].featureCache);
                Model.AddScores(featureCache[gid].featureCache, scores);
            }

            // deal with replacements

            for (int i = 0; i < replaceId.Length; ++i)
            {
                int rid = replaceId[i];
                if (rid >= featuresRelatedToStateId.Length
                    || featuresRelatedToStateId[rid] == null)
                {
                    continue;
                }

                LinearFeatureTemplate[] templates = featureRelatedToStateIdInGroup[gid][rid];
                LinearModelFeature[] features = featureCacheRelatedToStateIdInGroup[gid][rid];

                if (templates == null || features == null)
                {
                    continue;
                }

                int originalState = state[rid];

                for (int j = 0; j < templates.Length; ++j)
                {
                    foreach (int rplcm in replacement[i])
                    {
                        if (rplcm < 0)
                        {
                            break;
                        }

                        state[rid] = rplcm;

                        templates[j].GetFeature(state, features[j]);
                        Model.AddScores(features[j], scores);
                    }
                }

                state[rid] = originalState;
            }
        }

        private void AddScore(float[] dst, float[] src)
        {
            for (int i = 0; i < dst.Length; ++i)
            {
                dst[i] += src[i];
            }
        }

        public void GetFeatures(int[] state, List<LinearModelFeature> featureList)
        {
            //featureList = new List<LinearModelFeature>();

            foreach (LinearFeatureGroupCache pfgc in featureCache)
            {
                pfgc.GenerateFeatureToCache(state);
                for (int i = 0; i < pfgc.featureCache.Length; ++i)
                {
                    featureList.Add(new LinearModelFeature(pfgc.featureCache[i]));
                }
            }
        }

        public void GetFeatures(int[] state, int[] replaceId, int[][] replacement, List<LinearModelFeature> featureList)
        {
            //featureList = new List<LinearModelFeature>();

            foreach (LinearFeatureGroupCache pfgc in featureCache)
            {
                pfgc.GenerateFeatureToCache(state);
                for (int i = 0; i < pfgc.featureCache.Length; ++i)
                {
                    featureList.Add(new LinearModelFeature(pfgc.featureCache[i]));
                }
            }

            for (int i = 0; i < replaceId.Length; ++i)
            {
                int rid = replaceId[i];
                if (rid >= featuresRelatedToStateId.Length
                    || featuresRelatedToStateId[rid] == null)
                {
                    continue;
                }

                LinearFeatureTemplate[] templates = featuresRelatedToStateId[rid];
                LinearModelFeature[] features = featureCacheRelatedToStateId[rid];

                int originalState = state[rid];

                for (int j = 0; j < templates.Length; ++j)
                {
                    foreach (int rplcm in replacement[i])
                    {
                        if (rplcm < 0)
                        {
                            break;
                        }

                        state[rid] = rplcm;

                        LinearModelFeature f = templates[j].GetFeature(state);

                        bool isValid = true;
                        if (f == null)
                        {
                            isValid = false;
                        }
                        else
                        {
                            foreach (int s in f.ElemArr)
                            {
                                if (s < 0)
                                {
                                    isValid = false;
                                    break;
                                }
                            }
                        }

                        if (isValid)
                        {
                            featureList.Add(f);
                        }
                    }
                }

                state[rid] = originalState;
            }

        }

        public void GetScoresNoCache(int[] state, int[] replaceId, int[][] replacement, float[] scores)
        {
            //featureList = new List<LinearModelFeature>();

            Array.Clear(scores, 0, scores.Length);
            foreach (LinearFeatureGroupCache pfgc in featureCache)
            {
                pfgc.GenerateFeatureToCache(state);
                for (int i = 0; i < pfgc.featureCache.Length; ++i)
                {
                    if (pfgc.featureCache[i].IsValid)
                    {
                        Model.AddScores(pfgc.featureCache[i], scores);
                    }
                    //featureList.Add(new LinearModelFeature(pfgc.featureCache[i]));
                }
            }

            for (int i = 0; i < replaceId.Length; ++i)
            {
                int rid = replaceId[i];
                if (rid >= featuresRelatedToStateId.Length
                    || featuresRelatedToStateId[rid] == null)
                {
                    continue;
                }

                LinearFeatureTemplate[] templates = featuresRelatedToStateId[rid];
                LinearModelFeature[] features = featureCacheRelatedToStateId[rid];

                int originalState = state[rid];

                for (int j = 0; j < templates.Length; ++j)
                {

                    foreach (int rplcm in replacement[i])
                    {
                        if (rplcm < 0)
                        {
                            break;
                        }

                        state[rid] = rplcm;

                        //LinearModelFeature f = templates[j].GetFeature(state);

                        templates[j].GetFeature(state, features[j]);

                        if (features[j].IsValid)
                        {
                            Model.AddScores(features[j], scores);
                        }
                    }
                }

                state[rid] = originalState;
            }

        }

        public LinearModelCache(ILinearFunction model, List<LinearFeatureTemplate> templates)
        {
            this.Model = model;

            pfg = GroupingFeatures(templates);

            featureCache = new LinearFeatureGroupCache[pfg.Length];

            for (int i = 0; i < pfg.Length; ++i)
            {
                featureCache[i] = new LinearFeatureGroupCache(pfg[i]);
            }

            scoreCache = new Dictionary<int, float[]>[featureCache.Length];
            for (int i = 0; i < featureCache.Length; ++i)
            {
                scoreCache[i] = new Dictionary<int, float[]>();
            }

            Dictionary<int, List<LinearFeatureTemplate>> StateIdToFeatureDict = new Dictionary<int, List<LinearFeatureTemplate>>();

            int minId = int.MaxValue;
            int maxId = int.MinValue;

            featureRelatedToStateIdInGroup = new LinearFeatureTemplate[pfg.Length][][];
            featureCacheRelatedToStateIdInGroup = new LinearModelFeature[pfg.Length][][];

            

            foreach (LinearFeatureGroup fg in pfg)
            {
                foreach (LinearFeatureTemplate ft in fg.features)
                {
                    foreach (int stateid in ft.stateIds)
                    {
                        minId = Math.Min(minId, stateid);
                        maxId = Math.Max(maxId, stateid);
                        List<LinearFeatureTemplate> templateList;
                        if (!StateIdToFeatureDict.TryGetValue(stateid, out templateList))
                        {
                            templateList = new List<LinearFeatureTemplate>();
                            StateIdToFeatureDict[stateid] = templateList;
                        }

                        templateList.Add(ft);
                    }
                }
            }

            int nextG = 0;
            foreach (LinearFeatureGroup fg in pfg)
            {
                featureRelatedToStateIdInGroup[nextG] = new LinearFeatureTemplate[maxId + 1][];
                featureCacheRelatedToStateIdInGroup[nextG] = new LinearModelFeature[maxId + 1][];
                var s2fdict = new Dictionary<int, List<LinearFeatureTemplate>>();

                foreach (LinearFeatureTemplate ft in fg.features)
                {
                    foreach (int stateid in ft.stateIds)
                    {
                        List<LinearFeatureTemplate> templateList;
                        if (!s2fdict.TryGetValue(stateid, out templateList))
                        {
                            templateList = new List<LinearFeatureTemplate>();
                            s2fdict[stateid] = templateList;
                        }

                        templateList.Add(ft);
                    }
                }

                foreach (int stateId in s2fdict.Keys)
                {
                    featureRelatedToStateIdInGroup[nextG][stateId] = s2fdict[stateId].ToArray();
                    featureCacheRelatedToStateIdInGroup[nextG][stateId] = new LinearModelFeature[featureRelatedToStateIdInGroup[nextG][stateId].Length];
                    for (int i = 0; i < featureRelatedToStateIdInGroup[nextG][stateId].Length; ++i)
                    {
                        featureCacheRelatedToStateIdInGroup[nextG][stateId][i] = featureRelatedToStateIdInGroup[nextG][stateId][i].MakeCache();
                    }
                }
                nextG += 1;
            }

            featuresRelatedToStateId = new LinearFeatureTemplate[maxId + 1][];
            featureCacheRelatedToStateId = new LinearModelFeature[maxId + 1][];

            foreach (int stateId in StateIdToFeatureDict.Keys)
            {
                featuresRelatedToStateId[stateId] = StateIdToFeatureDict[stateId].ToArray();
                featureCacheRelatedToStateId[stateId] = new LinearModelFeature[featuresRelatedToStateId[stateId].Length];
                for (int i = 0; i < featuresRelatedToStateId[stateId].Length; ++i)
                {
                    featureCacheRelatedToStateId[stateId][i] = featuresRelatedToStateId[stateId][i].MakeCache();
                }
            }
        }

        public LinearFeatureGroup[] FeatureGroups { get { return pfg; } }

        private LinearFeatureGroup[] pfg;

        private LinearFeatureGroup[] GroupingFeatures(List<LinearFeatureTemplate> featList)
        {
            List<List<LinearFeatureTemplate>> featureGroupList = new List<List<LinearFeatureTemplate>>();
            List<LinearFeatureTemplate> highOrderFeatures = new List<LinearFeatureTemplate>();
            foreach (LinearFeatureTemplate feat in featList)
            {
                if (feat.determinate.Length > 2)
                {
                    highOrderFeatures.Add(feat);
                    continue;
                }

                bool isMerged = false;
                foreach (List<LinearFeatureTemplate> groupFeats in featureGroupList)
                {
                    if (groupFeats[0].IsSameFeatureGroup(feat))
                    {
                        groupFeats.Add(feat);
                        isMerged = true;
                        break;
                    }
                }

                if (!isMerged)
                {
                    List<LinearFeatureTemplate> featl = new List<LinearFeatureTemplate>();
                    featl.Add(feat);
                    featureGroupList.Add(featl);
                }
            }

            List<LinearFeatureGroup> fgl = new List<LinearFeatureGroup>();
            foreach (List<LinearFeatureTemplate> fg in featureGroupList)
            {
                fgl.Add(new LinearFeatureGroup(fg.ToArray()));
            }

            if (highOrderFeatures.Count > 0)
            {
                fgl.Add(new LinearFeatureGroup(highOrderFeatures.ToArray()));
            }

            return fgl.ToArray();
        }

        private LinearFeatureGroupCache[] featureCache;

        private Dictionary<int, float[]>[] scoreCache;

        // ONLY GOD will know what these things means!!! HAHAHAHA...
        // I just cannot figure out a better way to write this shit.
        private LinearFeatureTemplate[][][] featureRelatedToStateIdInGroup;

        private LinearModelFeature[][][] featureCacheRelatedToStateIdInGroup;

        private LinearFeatureTemplate[][] featuresRelatedToStateId;

        private LinearModelFeature[][] featureCacheRelatedToStateId;

        public ILinearFunction Model { get; private set; }
    }
}
