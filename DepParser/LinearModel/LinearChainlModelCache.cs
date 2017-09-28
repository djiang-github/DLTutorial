using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinearFunction
{
    public class LinearChainModelCache
    {
        public void Clear()
        {
            staticScoreCache = null;
            prevYScoreCache = null;
            succYScoreCache = null;
        }

        public LinearChainModelCache(ILinearFunction model, int TagCount, IEnumerable<LinearChainFeatureTemplate> TemplateList)
        {
            this.TagCount = TagCount;
            this.model = model;

            List<LinearChainFeatureTemplate> statFtList = new List<LinearChainFeatureTemplate>();
            List<LinearChainFeatureTemplate> prevFtList = new List<LinearChainFeatureTemplate>();
            List<LinearChainFeatureTemplate> succFtList = new List<LinearChainFeatureTemplate>();
            List<LinearChainFeatureTemplate> highFtList = new List<LinearChainFeatureTemplate>();

            int prevLen = 0;
            int succLen = 0;

            foreach (LinearChainFeatureTemplate ft in TemplateList)
            {
                if (ft.IsStatFeature)
                {
                    statFtList.Add(ft);
                }
                else if (ft.IsUniTagFeature)
                {
                    int yoff;
                    if (ft.GetYOffset(out yoff))
                    {
                        if (yoff > 0)
                        {
                            succFtList.Add(ft);
                            succLen = Math.Max(succLen, yoff);
                        }
                        else if (yoff < 0)
                        {
                            prevFtList.Add(ft);
                            prevLen = Math.Max(prevLen, -yoff);
                        }
                        else
                        {
                            throw new Exception("Wrong Feature Templates!!!");
                        }
                    }
                    else
                    {
                        throw new Exception("Wrong Feature Templates!!!");
                    }
                }
                else
                {
                    highFtList.Add(ft);
                }
            }

            if (statFtList.Count > 0)
            {
                staticFeatureTemplates = statFtList.ToArray();
                staticFeatures = new LinearModelFeature[staticFeatureTemplates.Length];

                for (int i = 0; i < staticFeatureTemplates.Length; ++i)
                {
                    staticFeatures[i] = staticFeatureTemplates[i].MakeCache();
                }
            }

            if (prevFtList.Count > 0 && prevLen > 0)
            {
                uniPrevYFeatureTemplates = new LinearChainFeatureTemplate[prevLen][];
                uniPrevYFeatures = new LinearModelFeature[prevLen][];
                List<LinearChainFeatureTemplate>[] sepList = new List<LinearChainFeatureTemplate>[prevLen];

                foreach (LinearChainFeatureTemplate ft in prevFtList)
                {
                    int yid;
                    ft.GetYOffset(out yid);
                    int id = -yid - 1;
                    if (sepList[id] == null)
                    {
                        sepList[id] = new List<LinearChainFeatureTemplate>();
                    }
                    sepList[id].Add(ft);
                }

                for (int i = 0; i < sepList.Length; ++i)
                {
                    if (sepList[i] != null)
                    {
                        uniPrevYFeatureTemplates[i] = sepList[i].ToArray();
                        uniPrevYFeatures[i] = new LinearModelFeature[sepList[i].Count];
                        for (int j = 0; j < uniPrevYFeatureTemplates[i].Length; ++j)
                        {
                            uniPrevYFeatures[i][j] = uniPrevYFeatureTemplates[i][j].MakeCache();
                        }
                    }
                }
            }

            if (succFtList.Count > 0 && succLen > 0)
            {
                uniSuccYFeatureTemplates = new LinearChainFeatureTemplate[succLen][];
                uniSuccYFeatures = new LinearModelFeature[succLen][];
                List<LinearChainFeatureTemplate>[] sepList = new List<LinearChainFeatureTemplate>[succLen];

                foreach (LinearChainFeatureTemplate ft in succFtList)
                {
                    int yid;
                    ft.GetYOffset(out yid);
                    int id = yid - 1;
                    if (sepList[id] == null)
                    {
                        sepList[id] = new List<LinearChainFeatureTemplate>();
                    }
                    sepList[id].Add(ft);
                }

                for (int i = 0; i < sepList.Length; ++i)
                {
                    if (sepList[i] != null)
                    {
                        uniSuccYFeatureTemplates[i] = sepList[i].ToArray();
                        uniSuccYFeatures[i] = new LinearModelFeature[sepList[i].Count];
                        for (int j = 0; j < uniSuccYFeatureTemplates[i].Length; ++j)
                        {
                            uniSuccYFeatures[i][j] = uniSuccYFeatureTemplates[i][j].MakeCache();
                        }
                    }
                }
            }

            if (highFtList.Count > 0)
            {
                HighOrderFeatureTemplates = highFtList.ToArray();
                HighOrderFeatures = new LinearModelFeature[highFtList.Count];
                for (int i = 0; i < HighOrderFeatures.Length; ++i)
                {
                    HighOrderFeatures[i] = HighOrderFeatureTemplates[i].MakeCache();
                }
            }
        }

        public void StartNextInstance(int Length)
        {
            staticScoreCache = new float[Length][];
            if (uniPrevYFeatureTemplates != null)
            {
                prevYScoreCache = new float[uniPrevYFeatureTemplates.Length][][][];
                for (int i = 0; i < prevYScoreCache.Length; ++i)
                {
                    prevYScoreCache[i] = new float[Length][][];
                }
            }
            if (uniSuccYFeatureTemplates != null)
            {
                succYScoreCache = new float[uniSuccYFeatureTemplates.Length][][][];
                for (int i = 0; i < succYScoreCache.Length; ++i)
                {
                    succYScoreCache[i] = new float[Length][][];
                }
            }
        }

        public void GetScore(int[][] observ, int[] tags, int id, float[] score)
        {
            if (staticScoreCache[id] == null)
            {
                GetStaticScoreToCache(observ, tags, id);
            }
            staticScoreCache[id].CopyTo(score, 0);
            
            if (uniPrevYFeatureTemplates != null)
            {
                GetPrevYScore(observ, tags, id, score);
            }

            if (uniSuccYFeatureTemplates != null)
            {
                GetSuccYScore(observ, tags, id, score);
            }

            if (HighOrderFeatureTemplates != null)
            {
                GetHighOrderScore(observ, tags, id, score);
            }
        }

        public List<LinearModelFeature> GetAllFeatures(int[][] observ, int[] tags, int id)
        {
            List<LinearModelFeature> features = new List<LinearModelFeature>();

            if (staticFeatures != null)
            {
                for(int i = 0; i < staticFeatures.Length; ++i)
                {
                    LinearModelFeature ftCopy = new LinearModelFeature(staticFeatures[i]);
                    LinearChainFeatureTemplate template = staticFeatureTemplates[i];
                    if (template.GetFeature(tags, observ, id, ftCopy))
                    {
                        features.Add(ftCopy);
                    }
                }
            }

            if (HighOrderFeatures != null)
            {
                for (int i = 0; i < HighOrderFeatures.Length; ++i)
                {
                    LinearModelFeature ftCopy = new LinearModelFeature(HighOrderFeatures[i]);
                    LinearChainFeatureTemplate template = HighOrderFeatureTemplates[i];
                    if (template.GetFeature(tags, observ, id, ftCopy))
                    {
                        features.Add(ftCopy);
                    }
                }
            }

            if (uniPrevYFeatures != null)
            {
                for (int i = 0; i < uniPrevYFeatures.Length; ++i)
                {
                    if (uniPrevYFeatures[i] != null)
                    {
                        for (int j = 0; j < uniPrevYFeatures[i].Length; ++j)
                        {
                            LinearModelFeature ftCopy = new LinearModelFeature(uniPrevYFeatures[i][j]);
                            LinearChainFeatureTemplate template = uniPrevYFeatureTemplates[i][j];
                            if (template.GetFeature(tags, observ, id, ftCopy))
                            {
                                features.Add(ftCopy);
                            }
                        }
                    }
                }
            }

            if (uniSuccYFeatures != null)
            {
                for (int i = 0; i < uniSuccYFeatures.Length; ++i)
                {
                    if (uniSuccYFeatures[i] != null)
                    {
                        for (int j = 0; j < uniSuccYFeatures[i].Length; ++j)
                        {
                            LinearModelFeature ftCopy = new LinearModelFeature(uniSuccYFeatures[i][j]);
                            LinearChainFeatureTemplate template = uniSuccYFeatureTemplates[i][j];
                            if (template.GetFeature(tags, observ, id, ftCopy))
                            {
                                features.Add(ftCopy);
                            }
                        }
                    }
                }
            }

            return features;
        }

        private void GetHighOrderScore(int[][] observ, int[] tags, int id, float[] score)
        {
            for (int i = 0; i < HighOrderFeatureTemplates.Length; ++i)
            {
                HighOrderFeatureTemplates[i].GetFeature(
                    tags, observ, id, HighOrderFeatures[i]);
                model.AddScores(HighOrderFeatures[i], score);
            }
        }

        private void GetPrevYScore(int[][] observ, int[] tags, int id, float[] score)
        {
            for (int i = 0; i < uniPrevYFeatureTemplates.Length; ++i)
            {
                int yid = id - 1 - i;

                if (yid < 0 || tags[yid] < 0)
                {
                    continue;
                }
                int tag = tags[yid];
                if (uniPrevYFeatureTemplates[i] != null)
                {
                    if (prevYScoreCache[i][id] == null)
                    {
                        prevYScoreCache[i][id] = new float[TagCount + 1][];
                    }

                    if (prevYScoreCache[i][id][tag] == null)
                    {
                        prevYScoreCache[i][id][tag] = new float[TagCount];
                        for (int j = 0; j < uniPrevYFeatureTemplates[i].Length; ++j)
                        {
                            uniPrevYFeatureTemplates[i][j].GetFeature(
                                tags, observ, id, uniPrevYFeatures[i][j]);
                            model.AddScores(uniPrevYFeatures[i][j], prevYScoreCache[i][id][tag]);
                        }
                    }
                    AddFloatArr(score, prevYScoreCache[i][id][tag]);
                }
            }
        }

        private void GetSuccYScore(int[][] observ, int[] tags, int id, float[] score)
        {
            for (int i = 0; i < uniSuccYFeatureTemplates.Length; ++i)
            {
                int yid = id + 1 + i;
                
                if (yid >= tags.Length || tags[yid] < 0)
                {
                    continue;
                }

                int tag = tags[yid];

                if (uniSuccYFeatureTemplates[i] != null)
                {
                    if (succYScoreCache[i][id] == null)
                    {
                        succYScoreCache[i][id] = new float[TagCount + 1][];
                    }

                    if (succYScoreCache[i][id][tag] == null)
                    {
                        succYScoreCache[i][id][tag] = new float[TagCount];
                        for (int j = 0; j < uniSuccYFeatureTemplates[i].Length; ++j)
                        {
                            uniSuccYFeatureTemplates[i][j].GetFeature(
                                tags, observ, id, uniSuccYFeatures[i][j]);
                            model.AddScores(uniSuccYFeatures[i][j], succYScoreCache[i][id][tag]);
                        }
                    }
                    AddFloatArr(score, succYScoreCache[i][id][tag]);
                }
            }
        }

        private void AddFloatArr(float[] dest, float[] src)
        {
            for (int i = 0; i < dest.Length; ++i)
            {
                dest[i] += src[i];
            }
        }

        private void GetStaticScoreToCache(int[][] observ, int[] tags, int id)
        {
            staticScoreCache[id] = new float[TagCount];
            for (int i = 0; i < staticFeatureTemplates.Length; ++i)
            {
                staticFeatureTemplates[i].GetFeature(tags, observ, id, staticFeatures[i]);
                model.AddScores(staticFeatures[i], staticScoreCache[id]);
            }
        }

        LinearChainFeatureTemplate[] staticFeatureTemplates;
        LinearChainFeatureTemplate[][] uniPrevYFeatureTemplates;
        LinearChainFeatureTemplate[][] uniSuccYFeatureTemplates;
        LinearChainFeatureTemplate[] HighOrderFeatureTemplates;

        LinearModelFeature[] staticFeatures;
        LinearModelFeature[][] uniPrevYFeatures;
        LinearModelFeature[][] uniSuccYFeatures;
        LinearModelFeature[] HighOrderFeatures;

        // first (left most) dimension:     word index
        private float[][] staticScoreCache;

        // first (left most) dimension:     Y offset
        // second dimension:                word index
        // third dimension:                 tag
        private float[][][][] prevYScoreCache;

        // first (left most) dimension:     Y offset
        // second dimension:                word index
        // third dimension:                 tag
        private float[][][][] succYScoreCache;

        public int TagCount { get; private set; }

        ILinearFunction model;
    }
}
