using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace NanYUtilityLib.DepParUtil
{
    public class MaltFileWriter
    {
        private StreamWriter sw;
        private bool ifOpen = false;
        private bool bExplicitID = false;

        public bool IsSingleLine = true;
        public MaltFileWriter(string fn, bool bExplicitID)
        {
            this.bExplicitID = false;
            sw = new StreamWriter(fn, false);
            ifOpen = true;
            this.bExplicitID = bExplicitID;
        }
        public MaltFileWriter(string fn)
            :this(fn, false)
        {
        }

        public MaltFileWriter(Stream fn)
        {
            sw = new StreamWriter(fn);
        }

        public MaltFileWriter(string fn, Encoding enc, bool bExplicitID)
        {
            sw = new StreamWriter(fn, false, enc);
            ifOpen = true;
            this.bExplicitID = bExplicitID;
        }

        public MaltFileWriter(string fn, Encoding enc)
            :this(fn, enc, false)
        {
        }

        public void Flush()
        {
            if (ifOpen)
            {
                sw.Flush();
            }
        }

        public void Close()
        {
            if (ifOpen)
            {
                sw.Close();
                ifOpen = false;
            }
        }
        ~MaltFileWriter()
        {
            if (ifOpen)
            {
                sw.Close();
                ifOpen = false;
            }
        }

        public StreamWriter Base { get { return sw; } }
        //public void Write(DepTree tree)
        //{
        //    if (tree == null || tree.root == null)
        //    {
        //        sw.WriteLine();
        //        return;
        //    }
        //    for (int i = 1; i < tree.Length; ++i)
        //    {
        //        if (bExplicitID)
        //        {
        //            sw.Write("{0}\t", i);
        //        }
        //        sw.WriteLine("{0}\t{1}\t{2}\t{3}", tree[i].token, tree[i].POS, tree[i].head == null ? 0 : tree[i].head.id, tree[i].head == null ? "ROOT" : tree[i].depType);
        //    }
        //    sw.WriteLine();
        //}

        public void Write(string[] token, string[] pos, int[] hid, string[] deptype)
        {
            if (token == null || pos == null || hid == null || deptype == null)
            {
                sw.WriteLine();
                return;
            }
            if (token.Length != pos.Length || token.Length != hid.Length || token.Length != deptype.Length)
            {
                sw.WriteLine();
                return;
            }
            for (int i = 0; i < token.Length; ++i)
            {
                if (bExplicitID)
                {
                    sw.Write("{0}\t", i + 1);
                }
                if (IsSingleLine)
                {
                    sw.Write("{0}\t{1}\t{2}\t{3} ", token[i], pos[i], hid[i], deptype[i]);
                }
                else
                {
                    sw.WriteLine("{0}\t{1}\t{2}\t{3}", token[i], pos[i], hid[i], deptype[i]);
                }
            }
            sw.WriteLine();
        }

        static public string GetParseLine(string[] token, string[] pos, int[] hid, string[] deptype)
        {
            StringBuilder sb = new StringBuilder();
            if (token == null || pos == null || hid == null || deptype == null)
            {
                return "";
            }
            if (token.Length != pos.Length || token.Length != hid.Length || token.Length != deptype.Length)
            {
                return "";
            }
            for (int i = 0; i < token.Length; ++i)
            {
                
                sb.AppendFormat("{0}\t{1}\t{2}\t{3} ", token[i], pos[i], hid[i], deptype[i]);
                
            }
            return sb.ToString();
        }


        public void Write(string[] token, string[] pos, int[] hid, string[] deptype, string[] chunktag)
        {
            if (token == null || pos == null || hid == null || deptype == null || chunktag == null)
            {
                sw.WriteLine();
                return;
            }
            if (token.Length != pos.Length || token.Length != hid.Length || token.Length != deptype.Length
                || token.Length != chunktag.Length)
            {
                sw.WriteLine();
                return;
            }
            for (int i = 0; i < token.Length; ++i)
            {
                if (bExplicitID)
                {
                    sw.Write("{0}\t", i + 1);
                }
                if (IsSingleLine)
                {
                    sw.Write("{0}\t{1}\t{2}\t{3}\t{4} ", token[i], pos[i], hid[i], deptype[i], chunktag[i]);
                }
                else
                {
                    sw.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", token[i], pos[i], hid[i], deptype[i], chunktag[i]);
                }
            }
            sw.WriteLine();
        }


        public void Write()
        {
            sw.WriteLine();
        }
    }
}
