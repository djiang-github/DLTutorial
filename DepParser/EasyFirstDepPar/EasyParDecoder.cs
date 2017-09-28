using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LinearFunction;

namespace EasyFirstDepPar
{
    public class EasyParDecoder
    {
        public EasyParDecoder(EasyFirstParserModelInfo parserModelInfo, ILinearFunction model)
        {
            this.parserModelInfo = parserModelInfo;
            this.model = model;
        }

        public bool Run(string[] tok, string[] pos, out int[] hid, out string[] label)
        {
            int[] btok = parserModelInfo.vocab.BinarizeWithPadding(tok);
            int[] bpos = parserModelInfo.vocab.BinarizeWithPadding(pos);

            cache = new LinearModelCache(model, parserModelInfo.lmInfo.TemplateSet);

            EasyFirstParserConfig config = new EasyFirstParserConfig(btok, bpos, cache, parserModelInfo, model);

            while (!config.IsEnd)
            {
                if (!config.ApplyNextBest())
                {
                    hid = null;
                    label = null;
                    return false;
                }
            }

            hid = new int[tok.Length];
            label = new string[tok.Length];

            for (int i = 0; i < hid.Length; ++i)
            {
                hid[i] = config.hid[i + 1];
                if (hid[i] <= 0)
                {
                    hid[i] = 0;
                    label[i] = "root";
                }
                else
                {
                    label[i] = parserModelInfo.vocab.LabelId2Name[config.label[i + 1]];
                }
            }

            return true;
        }

        public bool RunWithOrder(string[] tok, string[] pos, out int[] hid, out string[] label)
        {
            int[] btok = parserModelInfo.vocab.BinarizeWithPadding(tok);
            int[] bpos = parserModelInfo.vocab.BinarizeWithPadding(pos);

            cache = new LinearModelCache(model, parserModelInfo.lmInfo.TemplateSet);

            EasyFirstParserConfig config = new EasyFirstParserConfig(btok, bpos, cache, parserModelInfo, model);

            int[] order = new int[tok.Length];

            for (int i = 0; i < order.Length; ++i)
            {
                order[i] = -1;
            }

            int next = 0;

            int maxRR = Math.Max(1, tok.Length / 3 * 2);

            while (!config.IsEnd)
            {
                int cid;
                int xhid;
                if (!config.ApplyNextBest(out cid, out xhid))
                {
                    hid = null;
                    label = null;
                    return false;
                }

                order[cid - 1] = next;

                next += 1;

                if (next >= maxRR)
                {
                    break;
                }
            }

            hid = new int[tok.Length];
            label = new string[tok.Length];

            for (int i = 0; i < hid.Length; ++i)
            {
                hid[i] = config.hid[i + 1];
                if (hid[i] <= 0)
                {
                    hid[i] = 0;
                    label[i] = "root";
                }
                else
                {
                    label[i] = parserModelInfo.vocab.LabelId2Name[config.label[i + 1]] + "_" + order[i].ToString();
                }
            }

            return true;
        }

        public int[] CalculateSpan(int[] hids)
        {
            int[] begin = new int[hids.Length];
            int[] end = new int[hids.Length];
            for(int i= 0; i < hids.Length; ++i)
            {
                begin[i] = i;
                end[i] = i + 1;
            }
            
            for (int i = 1; i < hids.Length - 1; ++i)
            {
                int p = hids[i];
                while (p > 0)
                {
                    begin[p] = Math.Min(begin[p], i);
                    end[p] = Math.Max(end[p], i + 1);
                    p = hids[p];
                }
            }
            int[] span = new int[hids.Length];
            for (int i = 0; i < hids.Length; ++i)
            {
                span[i] = end[i] - begin[i];
            }
            return span;
        }

        public bool Train(string[] tok, string[] pos, int[] hid, string[] label, out bool passed, out List<FeatureUpdatePackage> updates)
        {
            int[] btok = parserModelInfo.vocab.BinarizeWithPadding(tok);
            int[] bpos = parserModelInfo.vocab.BinarizeWithPadding(pos);

            int[] bhid = new int[hid.Length + 2];
            for (int i = 0; i < hid.Length; ++i)
            {
                bhid[i + 1] = hid[i];
            }

            int[] blabel = parserModelInfo.vocab.BinarizeLabelWithPadding(hid, label);

            int[] bspan = CalculateSpan(bhid);
            cache = new LinearModelCache(model, parserModelInfo.lmInfo.TemplateSet);

            EasyFirstParserConfig config = new EasyFirstParserConfig(btok, bpos, cache, parserModelInfo, model);

            while (!config.IsEnd)
            {
                if (!config.ApplyNextBest(bhid, blabel, bspan))
                {
                    if (!config.GetUpdate(bhid, blabel, bspan, out updates))
                    {
                        passed = false;
                        return false;
                    }
                    passed = false;
                    return true;
                }
            }

            updates = null;
            passed = true;
            return true;
        }

        LinearModelCache cache;
        EasyFirstParserModelInfo parserModelInfo;
        ILinearFunction model;
    }
}
