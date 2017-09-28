using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NanYUtilityLib.DepParUtil
{
    public class ParseWord
    {
        public string Token { get; set; }
        public string PoS { get; set; }
        public int HeadId { get; set; }
        public string Label { get; set; }
    }

    public class ParserSentence
    {
        public int Length { get { return tok.Length; } }
        public string[] tok;
        public string[] pos;
        public int[] hid;
        public string[] label;

        public ParserSentence(string[] tok, string[] pos, int[] hid, string[] label)
        {
            this.tok = tok;
            this.pos = pos;
            this.hid = hid;
            this.label = label;
        }

        public ParserSentence Clone()
        {
            return new ParserSentence
                    (
                        (string[])tok.Clone(),
                        (string[])pos.Clone(),
                        (int[])hid.Clone(),
                        (string[])label.Clone()
                    );
        }

        public ParserSentence Reverse()
        {
            string[] rtok = tok.Reverse<string>().ToArray<string>();
            string[] rpos = pos.Reverse<string>().ToArray<string>();

            int[] rhid = ReverseHid(hid);

            string[] rlabel = label.Reverse<string>().ToArray<string>();

            return new ParserSentence(rtok, rpos, rhid, rlabel);

        }

        private int[] ReverseHid(int[] hid)
        {
            int[] rhid = new int[hid.Length];
            int L = rhid.Length;
            for (int i = 0; i < hid.Length; ++i)
            {
                if (hid[i] == 0)
                {
                    rhid[L - 1 - i] = 0;
                }
                else
                {
                    rhid[L - 1 - i] = L - 1 - (hid[i] - 1) + 1;
                }
            }

            return rhid;
        }

        public bool DeleteWord(int ID)
        {
            if (ID < 0 || ID >= Length)
            {
                return false;
            }

            // cannot delete root word
            // cannot delete word with children
            if (hid[ID] == 0 || HaveChild(ID))
            {
                return false;
            }

            string[] newTok = new string[Length - 1];
            string[] newPos = new string[Length - 1];
            int[] newHid = new int[Length - 1];
            string[] newLabel = new string[Length - 1];

            for (int i = 0; i < ID; ++i)
            {
                newTok[i] = tok[i];
                newPos[i] = pos[i];
                if (hid[i] + 1 > ID + 1)
                {
                    newHid[i] = hid[i] - 1;
                }
                else
                {
                    newHid[i] = hid[i];
                }
                newLabel[i] = label[i];
            }

            for (int i = ID + 1; i < Length; ++i)
            {
                newTok[i - 1] = tok[i];
                newPos[i - 1] = pos[i];
                if (hid[i] + 1 > ID + 1)
                {
                    newHid[i - 1] = hid[i] - 1;
                }
                else
                {
                    newHid[i - 1] = hid[i];
                }

                newLabel[i - 1] = label[i];
            }
            tok = newTok;
            pos = newPos;
            hid = newHid;
            label = newLabel;
            return true;
        }

        public bool InsertWordAtIndex(int ID, int HeadID, string Token, string PoS, string Label)
        {
            if (ID < 0 || ID >= Length)
            {
                return false;
            }

            if (HeadID <= -1 || HeadID > Length)
            {
                return false;
            }

            string[] newTok = new string[Length + 1];
            string[] newPos = new string[Length + 1];
            int[] newHid = new int[Length + 1];
            string[] newLabel = new string[Length + 1];

            for (int i = 0; i < ID; ++i)
            {
                newTok[i] = tok[i];
                newPos[i] = pos[i];
                if (hid[i] + 1 > ID + 1)
                {
                    newHid[i] = hid[i] + 1;
                }
                else
                {
                    newHid[i] = hid[i];
                }
                newLabel[i] = label[i];
            }

            newTok[ID] = Token;
            newPos[ID] = PoS;
            newLabel[ID] = Label;
            if (HeadID + 1 > ID + 1)
            {
                newHid[ID] = hid[ID] + 1;
            }
            else
            {
                newHid[ID] = hid[ID];
            }

            for (int i = ID + 1; i < Length; ++i)
            {
                newTok[i + 1] = tok[i];
                newPos[i + 1] = pos[i];
                if (hid[i] + 1 > ID + 1)
                {
                    newHid[i + 1] = hid[i] + 1;
                }
                else
                {
                    newHid[i + 1] = hid[i];
                }

                newLabel[i + 1] = label[i];
            }
            tok = newTok;
            pos = newPos;
            hid = newHid;
            label = newLabel;
            return true;
        }

        public bool HaveChild(int ID)
        {
            if (ID < 0 || ID >= Length)
            {
                return false;
            }

            return Array.IndexOf<int>(hid, ID + 1) >= 0;
        }
        
    }

}
