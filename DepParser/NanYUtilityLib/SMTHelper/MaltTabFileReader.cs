using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace NanYUtilityLib.DepParUtil
{
    
    public class MaltTabFileReader
    {
        private StreamReader sr;
        private bool ifOpen = false;

        public bool SingleLineMode { get; set; }

        public static List<ParserSentence> ReadAll(string filename)
        {
            return ReadAll(filename, true);
        }

        public static List<ParserSentence> ReadAll(string filename, bool isSingleLine)
        {
            MaltTabFileReader mtfr = new MaltTabFileReader(filename);
            mtfr.SingleLineMode = isSingleLine;

            var sentences = new List<ParserSentence>();

            while (!mtfr.EndOfStream)
            {
                ParserSentence ps;

                if (mtfr.GetNextSent(out ps))
                {
                    sentences.Add(ps);
                }
            }

            return sentences;
        }

        public MaltTabFileReader(string fn)
        {
            sr = new StreamReader(fn);
            ifOpen = true;
            SingleLineMode = true;
        }

        public MaltTabFileReader(string fn, Encoding enc)
        {
            sr = new StreamReader(fn, enc);
            ifOpen = true;
            SingleLineMode = true;
        }

        public bool EndOfStream
        {
            get { return !ifOpen || sr.EndOfStream; }
        }

        public void Close()
        {
            if (ifOpen)
            {
                sr.Close();
                ifOpen = false;
            }
        }
        ~MaltTabFileReader()
        {
            if (ifOpen)
            {
                sr.Close();
                ifOpen = false;
            }
        }

        static public ParserSentence Parse(string parseline)
        {
            var lineList = new List<string>();

            string[] lines = parseline.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            if (lines.Length == 0)
            {
                lineList = null;
                return null;
            }

            foreach (string line in lines)
            {
                lineList.Add(line);
            }

            var token = new string[lineList.Count];
            var POS = new string[lineList.Count];
            var hID = new int[lineList.Count];
            var depType = new string[lineList.Count];
            try
            {
                for (int i = 0; i < lineList.Count; ++i)
                {
                    string[] parts = lineList[i].Split('\t');
                    token[i] = parts[0];
                    POS[i] = parts[1];
                    hID[i] = int.Parse(parts[2]);
                    depType[i] = parts[3];
                }
            }
            catch
            {
                return null;
            }

            return new ParserSentence(token, POS, hID, depType);
        }

        public bool GetRawLineList(out List<string> lineList)
        {
            if (SingleLineMode)
            {
                return GetRawLineListSingleLine(out lineList);
            }

            lineList = null;
            if (ifOpen == false || sr.EndOfStream)
            {
                return false;
            }
            lineList = new List<string>();
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine().Trim();
                if (string.IsNullOrEmpty(line))
                {
                    break;
                }
                lineList.Add(line);
            }
            if (lineList.Count == 0)
            {
                lineList = null;
                return false;
            }
            return true;
        }

        private bool GetRawLineListSingleLine(out List<string> lineList)
        {
            lineList = null;
            if (ifOpen == false || sr.EndOfStream)
            {
                return false;
            }

            lineList = new List<string>();

            string multiline = sr.ReadLine();

            string[] lines = multiline.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            if (lines.Length == 0)
            {
                lineList = null;
                return false;
            }

            foreach (string line in lines)
            {
                lineList.Add(line);
            }

            return true;
        }

        public bool GetNextSent(out string[] token, out string[] POS, out int[] hID, out string[] depType)
        {
            token = null;
            POS = null;
            hID = null;
            depType = null;
            List<string> lineList;
            if (!GetRawLineList(out lineList) || lineList.Count == 0)
            {
                return false;
            }
            token = new string[lineList.Count];
            POS = new string[lineList.Count];
            hID = new int[lineList.Count];
            depType = new string[lineList.Count];
            try
            {
                for (int i = 0; i < lineList.Count; ++i)
                {
                    string[] parts = lineList[i].Split('\t');
                    token[i] = parts[0];
                    POS[i] = parts[1];
                    hID[i] = int.Parse(parts[2]);
                    depType[i] = parts[3];
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        public bool GetNextSent(out string[] token, out string[] pos, out int[] hid, out string[] arclabel, out string[] chunktag)
        {
            token = null;
            pos = null;
            hid = null;
            arclabel = null;
            chunktag = null;
            List<string> lineList;
            if (!GetRawLineList(out lineList) || lineList.Count == 0)
            {
                return false;
            }
            token = new string[lineList.Count];
            pos = new string[lineList.Count];
            hid = new int[lineList.Count];
            arclabel = new string[lineList.Count];
            chunktag = new string[lineList.Count];

            for (int i = 0; i < lineList.Count; ++i)
            {
                string[] parts = lineList[i].Split('\t');
                if (parts.Length != 5)
                {
                    return false;
                }
                token[i] = parts[0];
                pos[i] = parts[1];
                hid[i] = int.Parse(parts[2]);
                if (!int.TryParse(parts[2], out hid[i]))
                {
                    return false;
                }
                arclabel[i] = parts[3];
                chunktag[i] = parts[4];
            }

            return true;
        }

        public bool GetNextSent(out ParserSentence snt)
        {
            snt = null;
            string[] tok;
            string[] pos;
            int[] hid;
            string[] label;

            if (GetNextSent(out tok, out pos, out hid, out label))
            {
                snt = new ParserSentence(tok, pos, hid, label);
                return true;
            }
            else
            {
                return false;
            }
        }
    }


}

