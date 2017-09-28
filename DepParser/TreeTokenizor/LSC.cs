using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreeTokenizor
{
    class LSC
    {
        static public List<int> RecoverRawEngInputMap_LCS(string rawInput, string[] segArr)
        {
            List<int> newMap = new List<int>();
            List<string> rawWords = new List<string>();
            rawWords.Add("<s>");
            string[] raw = rawInput.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            rawWords.AddRange(raw);

            List<string> segWords = new List<string>();
            segWords.Add("<s>");
            segWords.AddRange(segArr);

            int max_dim = Math.Max(rawWords.Count, segWords.Count);
            int[,] LCS = new int[max_dim, max_dim];
            int[,] BackTrace = new int[max_dim, max_dim];

            int[,] I = new int[max_dim, max_dim];
            int[,] J = new int[max_dim, max_dim];

            for (int i = 0; i < max_dim; i++)
            {
                LCS[0, i] = LCS[i, 0] = 0;
            }

            //Minimal edit distance construction
            for (int i = 1; i < rawWords.Count; i++)
            {
                for (int j = 1; j < segWords.Count; j++)
                {
                    int tmp_i = i;
                    int tmp_j = j;
                    if (rawWords[i].Equals(segWords[j]))
                    {
                        LCS[i, j] = LCS[i - 1, j - 1] + 1;
                        BackTrace[i, j] = 2;
                        I[i, j] = i - 1;
                        J[i, j] = j - 1;
                    }
                    else if (rawWords[i].StartsWith(segWords[j]) || segWords[j].StartsWith(rawWords[i]))
                    {
                        StringBuilder sb_raw = new StringBuilder();
                        StringBuilder sb_seg = new StringBuilder();
                        sb_raw.Append(rawWords[i]);
                        sb_seg.Append(segWords[j]);
                        while (true)
                        {
                            if (sb_seg.ToString().Equals(sb_raw.ToString()))
                            {
                                LCS[i, j] = LCS[tmp_i - 1, tmp_j - 1] + 1;
                                BackTrace[i, j] = 2;
                                I[i, j] = tmp_i - 1;
                                J[i, j] = tmp_j - 1;
                                break;
                            }
                            else if (sb_raw.ToString().StartsWith(sb_seg.ToString()) && j + 1 < segWords.Count)
                            {
                                sb_seg.Append(segWords[++j]);
                            }
                            else if (sb_seg.ToString().StartsWith(sb_raw.ToString()) && i + 1 < rawWords.Count)
                            {
                                sb_raw.Append(rawWords[++i]);
                            }
                            else
                            {
                                i = tmp_i;
                                j = tmp_j;
                                if (LCS[i, j] < LCS[i - 1, j] || LCS[i, j] < LCS[i, j - 1])
                                {
                                    if (LCS[i - 1, j] >= LCS[i, j - 1])
                                    {
                                        LCS[i, j] = LCS[i - 1, j];
                                        BackTrace[i, j] = 1;
                                        I[i, j] = i - 1;
                                        J[i, j] = j;
                                    }
                                    else
                                    {
                                        LCS[i, j] = LCS[i, j - 1];
                                        BackTrace[i, j] = -1;
                                        I[i, j] = i;
                                        J[i, j] = j - 1;
                                    }
                                }
                                break;
                            }
                        }
                    }
                    else
                    {
                        if (LCS[i, j] < LCS[i - 1, j] || LCS[i, j] < LCS[i, j - 1])
                        {
                            if (LCS[i - 1, j] >= LCS[i, j - 1])
                            {
                                LCS[i, j] = LCS[i - 1, j];
                                BackTrace[i, j] = 1;
                                I[i, j] = i - 1;
                                J[i, j] = j;
                            }
                            else
                            {
                                LCS[i, j] = LCS[i, j - 1];
                                BackTrace[i, j] = -1;
                                I[i, j] = i;
                                J[i, j] = j - 1;
                            }
                        }
                    }
                    i = tmp_i;
                    j = tmp_j;
                }
            }

            //get the best path
            int rawIndex = rawWords.Count - 1;
            int segIndex = segWords.Count - 1;
            //bool[] rawList = new bool[rawWords.Count - 1];
            //bool[] segList = new bool[segWords.Count - 1];
            List<string> rawPos = new List<string>();
            List<string> segPos = new List<string>();
            while (rawIndex > 0 && segIndex > 0)
            {
                if (BackTrace[rawIndex, segIndex] == 2)
                {
                    //rawList[rawIndex - 1] = true;
                    //segList[segIndex - 1] = true;
                    StringBuilder sb_raw = new StringBuilder();
                    StringBuilder sb_seg = new StringBuilder();
                    for (int i = I[rawIndex, segIndex] + 1; i <= rawIndex; i++)
                    {
                        sb_raw.Append(rawWords[i]);
                    }
                    for (int i = J[rawIndex, segIndex] + 1; i <= segIndex; i++)
                    {
                        sb_seg.Append(segWords[i]);
                    }
                    if (sb_raw.ToString().Equals(sb_seg.ToString()))
                    {
                        rawPos.Add((I[rawIndex, segIndex] + 1) + "-" + rawIndex);
                        segPos.Add((J[rawIndex, segIndex] + 1) + "-" + segIndex);
                    }

                }
                int tmp_i = I[rawIndex, segIndex];
                int tmp_j = J[rawIndex, segIndex];
                rawIndex = tmp_i;
                segIndex = tmp_j;
            }

            if (rawPos.Count == 0 || segPos.Count == 0)
            {
                StringBuilder sb_raw = new StringBuilder();
                for (int i = 1; i < rawWords.Count; i++)
                {
                    if (!String.IsNullOrEmpty(sb_raw.ToString()))
                        sb_raw.Append(" ");
                    sb_raw.Append(rawWords[i]);
                }
                StringBuilder sb_seg = new StringBuilder();
                List<int> segP = new List<int>();
                for (int i = 1; i < segWords.Count; i++)
                {
                    sb_seg.Append(segWords[i]);
                    segP.Add(sb_seg.Length);
                }
                if (segWords.Count <= 2)
                {
                    newMap.Add(sb_raw.Length);
                }
                else
                {
                    List<int> nMap = GetLCS(sb_raw.ToString(), sb_seg.ToString(), segP);
                    foreach (int j in nMap)
                    {
                        newMap.Add(j);
                    }
                }
                return newMap;
            }

            rawPos.Reverse();
            segPos.Reverse();

            //produce character-based alignment
            StringBuilder sb = new StringBuilder();

            if (rawPos[0].StartsWith("1") == false || segPos[0].StartsWith("1") == false)
            {
                string[] r_items = rawPos[0].Split(new char[] { '-' });
                int start = Int32.Parse(r_items[0]);
                for (int pos = 1; pos < start; pos++)
                {
                    if (!String.IsNullOrEmpty(sb.ToString()))
                        sb.Append(" ");
                    sb.Append(rawWords[pos]);
                }
                string[] s_items = segPos[0].Split(new char[] { '-' });
                start = Int32.Parse(s_items[0]);

                if (start == 2)
                {
                    newMap.Add(sb.Length);
                }
                else
                {
                    int sb_end = sb.Length;
                    StringBuilder sb_seg = new StringBuilder();
                    List<int> segP = new List<int>();
                    for (int pos = 1; pos < start; pos++)
                    {
                        sb_seg.Append(segWords[pos]);
                        //newMap.Add(sb.Length);
                        segP.Add(sb_seg.Length);
                    }
                    List<int> nMap = GetLCS(sb.ToString(), sb_seg.ToString(), segP);
                    foreach (int i in nMap)
                    {
                        newMap.Add(i);
                    }
                }
            }

            int pre_r_end = 0;
            int pre_s_end = 0;

            for (int i = 0; i < rawPos.Count; i++)
            {
                string[] r_items = rawPos[i].Split(new char[] { '-' });
                string[] s_items = segPos[i].Split(new char[] { '-' });
                int r_start = Int32.Parse(r_items[0]);
                int r_end = Int32.Parse(r_items[1]);
                int s_start = Int32.Parse(s_items[0]);
                int s_end = Int32.Parse(s_items[1]);

                if (i > 0)
                {
                    if (r_start - pre_r_end > 1 || s_start - pre_s_end > 1)
                    {
                        int sb_end = sb.Length;

                        if (!String.IsNullOrEmpty(sb.ToString()))
                            sb_end++;

                        int tmp_r_start = pre_r_end + 1;
                        int tmp_r_end = r_start - 1;
                        int tmp_s_start = pre_s_end + 1;
                        int tmp_s_end = s_start - 1;
                        StringBuilder sb_raw = new StringBuilder();
                        for (int pos = tmp_r_start; pos <= tmp_r_end; pos++)
                        {
                            if (!String.IsNullOrEmpty(sb.ToString()))
                            {
                                sb.Append(" ");
                            }
                            sb.Append(rawWords[pos]);
                            if (!String.IsNullOrEmpty(sb_raw.ToString()))
                            {
                                sb_raw.Append(" ");
                            }
                            sb_raw.Append(rawWords[pos]);
                        }

                        if (tmp_s_end == tmp_s_start)
                        {
                            newMap.Add(sb.Length);
                        }

                        else
                        {

                            StringBuilder sb_seg = new StringBuilder();
                            List<int> segP = new List<int>();
                            for (int pos = tmp_s_start; pos <= tmp_s_end; pos++)
                            {
                                sb_seg.Append(segWords[pos]);
                                //newMap.Add(sb.Length);
                                segP.Add(sb_seg.Length);
                            }
                            List<int> nMap = GetLCS(sb_raw.ToString(), sb_seg.ToString(), segP);
                            foreach (int j in nMap)
                            {
                                newMap.Add(sb_end + j);
                            }
                        }

                        //for (int pos = tmp_s_start; pos <= tmp_s_start; pos++)
                        //{
                        //    newMap.Add(sb.Length);
                        //}
                    }
                }

                StringBuilder tmp_sb = new StringBuilder();
                List<int> spacePos = new List<int>();
                for (int pos = r_start; pos <= r_end; pos++)
                {
                    if (!String.IsNullOrEmpty(tmp_sb.ToString()))
                    {
                        //tmp_sb.Append(" ");
                        spacePos.Add(tmp_sb.Length);
                    }
                    tmp_sb.Append(rawWords[pos]);
                }

                int pre_length = 0;
                if (sb.Length == 0)
                    pre_length = 0;
                else
                    pre_length = sb.Length + 1;

                int index = 0;
                for (int pos = s_start; pos <= s_end; pos++)
                {
                    index = tmp_sb.ToString().IndexOf(segWords[pos], index, StringComparison.Ordinal);
                    int tmp_length = index + segWords[pos].Length;

                    int numOfspace = 0;

                    foreach (int space in spacePos)
                    {
                        if (tmp_length > space)
                            numOfspace++;
                    }
                    newMap.Add(pre_length + tmp_length + numOfspace);
                    index += segWords[pos].Length;
                }

                for (int pos = r_start; pos <= r_end; pos++)
                {
                    if (!String.IsNullOrEmpty(sb.ToString()))
                        sb.Append(" ");
                    sb.Append(rawWords[pos]);
                }

                pre_r_end = r_end;
                pre_s_end = s_end;
            }

            if (pre_r_end + 1 < rawWords.Count || pre_s_end + 1 < segWords.Count)
            {
                int sb_end = sb.Length;
                if (!String.IsNullOrEmpty(sb.ToString()))
                    sb_end++;
                StringBuilder sb_raw = new StringBuilder();
                for (int pos = pre_r_end + 1; pos < rawWords.Count; pos++)
                {
                    if (!String.IsNullOrEmpty(sb.ToString()))
                        sb.Append(" ");
                    sb.Append(rawWords[pos]);

                    if (!String.IsNullOrEmpty(sb_raw.ToString()))
                        sb_raw.Append(" ");
                    sb_raw.Append(rawWords[pos]);
                }

                if (segWords.Count - pre_s_end == 2)
                {
                    newMap.Add(sb.Length);
                }
                else
                {
                    StringBuilder sb_seg = new StringBuilder();
                    List<int> segP = new List<int>();
                    for (int pos = pre_s_end + 1; pos < segWords.Count; pos++)
                    {
                        sb_seg.Append(segWords[pos]);
                        //newMap.Add(sb.Length);
                        segP.Add(sb_seg.Length);
                    }
                    List<int> nMap = GetLCS(sb_raw.ToString(), sb_seg.ToString(), segP);
                    foreach (int j in nMap)
                    {
                        newMap.Add(sb_end + j);
                    }
                    //for (int pos = pre_s_end + 1; pos < pre_s_end + 1; pos++)
                    //{
                    //    newMap.Add(sb.Length);
                    //}
                }
            }

            return newMap;
        }

        static List<int> GetLCS(string rawStr, string segStr, List<int> segPos)
        {
            List<int> res = new List<int>();

            List<Char> rawChars = new List<Char>();
            rawChars.Add('^');
            rawChars.AddRange(rawStr.ToCharArray());

            List<Char> segChars = new List<Char>();
            segChars.Add('^');
            segChars.AddRange(segStr.ToCharArray());

            int max_dim = Math.Max(rawChars.Count, segChars.Count);
            int[,] LCS = new int[max_dim, max_dim];
            int[,] BackTrace = new int[max_dim, max_dim];

            for (int i = 1; i < rawChars.Count; i++)
            {
                for (int j = 1; j < segChars.Count; j++)
                {
                    if (rawChars[i] == segChars[j])
                    {
                        LCS[i, j] = LCS[i - 1, j - 1] + 1;
                        BackTrace[i, j] = 2;
                    }
                    else
                    {
                        if (LCS[i, j] < LCS[i - 1, j] || LCS[i, j] < LCS[i, j - 1])
                        {
                            if (LCS[i - 1, j] >= LCS[i, j - 1])
                            {
                                LCS[i, j] = LCS[i - 1, j];
                                BackTrace[i, j] = 1;
                            }
                            else
                            {
                                LCS[i, j] = LCS[i, j - 1];
                                BackTrace[i, j] = -1;
                            }
                        }
                    }
                }
            }

            int rawIndex = rawChars.Count - 1;
            int segIndex = segChars.Count - 1;

            Dictionary<int, int> segMap = new Dictionary<int, int>();

            while (rawIndex > 0 && segIndex > 0)
            {
                if (BackTrace[rawIndex, segIndex] == 2)
                {
                    segMap[segIndex] = rawIndex;
                    rawIndex--;
                    segIndex--;
                }
                else if (BackTrace[rawIndex, segIndex] == 1)
                {
                    rawIndex--;
                }
                else if (BackTrace[rawIndex, segIndex] == -1)
                {
                    segIndex--;
                }
                else
                {
                    break;
                }
            }

            if (segMap.Count == 0 && rawChars.Count >= segPos.Count + 1)
            {
                for (int i = 0; i < segPos.Count - 1; i++)
                {
                    res.Add(i + 1);
                }
                res.Add(rawChars.Count - 1);
                return res;
            }

            foreach (int i in segPos)
            {
                if (segMap.ContainsKey(i))
                {
                    res.Add(segMap[i]);
                }
                else
                {
                    bool flag = false;
                    int pos = -1;
                    for (int j = i + 1; j < segChars.Count; j++)
                    {
                        if (segMap.ContainsKey(j))
                        {
                            //res.Add(segMap[j] - 1);
                            pos = segMap[j] - 1;
                            flag = true;
                            break;
                        }
                    }
                    for (int j = i - 1; j >= 0; j--)
                    {
                        if (segMap.ContainsKey(j) && ((flag && pos > segMap[j] + 1) || !flag))
                        {
                            pos = segMap[j] + 1;
                            flag = true;
                            break;
                        }
                    }
                    if (flag)
                    {
                        res.Add(pos);
                        segMap.Add(i, pos);
                    }
                    else
                    {
                        res.Add(rawChars.Count - 1);
                    }
                }
            }

            return res;
        }

    }
}
