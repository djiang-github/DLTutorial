using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LinearFunction;

namespace EasyFirstDepPar
{
    public class EasyFirstParserStateDescriptor : IStateElementDiscriptor
    {
        public bool GetElementId(string ElementName, out int ElementId, out int Determinate)
        {
            ElementId = NameToId(ElementName);
            if (ElementId < l0id)
            {
                Determinate = -1;
                return false;
            }
            else
            {
                Determinate = determinates[ElementId];
                return true;
            }
        }
        public bool GetElememtDet(int Id, out int det)
        {
            if (Id < 0 || Id >= Count)
            {
                det = -1;
                return false;
            }
            else
            {
                det = determinates[Id];
                return true;
            }
        }

        public bool GetElementName(int ElementId, out string ElementName)
        {
            if (ElementId < 0 || ElementId >= Count)
            {
                ElementName = null;
                return false;
            }
            else
            {
                ElementName = Names[ElementId];
                return true;
            }
        }

        public const int l0id = 0;
        public const int l1id = 1;
        public const int l2id = 2;
        public const int r0id = 3;
        public const int r1id = 4;
        public const int r2id = 5;
        public const int l0len = 6;
        public const int l1len = 7;
        public const int l2len = 8;
        public const int r0len = 9;
        public const int r1len = 10;
        public const int r2len = 11;
        public const int l0hc = 12;
        public const int l1hc = 13;
        public const int l2hc = 14;
        public const int r0hc = 15;
        public const int r1hc = 16;
        public const int r2hc = 17;
        public const int r3hc = 18;
        public const int l2l1dst = 19;
        public const int l1l0dst = 20;
        public const int l0r0dst = 21;
        public const int r0r1dst = 22;
        public const int r1r2dst = 23;
        public const int l0t = 24;
        public const int l1t = 25;
        public const int l2t = 26;
        public const int r0t = 27;
        public const int r1t = 28;
        public const int r2t = 29;
        public const int l0w = 30;
        public const int l1w = 31;
        public const int l2w = 32;
        public const int r0w = 33;
        public const int r1w = 34;
        public const int r2w = 35;
        public const int l0lct = 36;
        public const int l1lct = 37;
        public const int l2lct = 38;
        public const int r0lct = 39;
        public const int r1lct = 40;
        public const int r2lct = 41;
        public const int l0rct = 42;
        public const int l1rct = 43;
        public const int l2rct = 44;
        public const int r0rct = 45;
        public const int r1rct = 46;
        public const int r2rct = 47;

        public const int l0lcw = 48;
        public const int l1lcw = 49;
        public const int l2lcw = 50;
        public const int r0lcw = 51;
        public const int r1lcw = 52;
        public const int r2lcw = 53;
        

        public const int l0rcw = 54;
        public const int l1rcw = 55;
        public const int l2rcw = 56;
        public const int r0rcw = 57;
        public const int r1rcw = 58;
        public const int r2rcw = 59;

        public const int l0prep = 60;
        public const int l1prep = 61;
        public const int l2prep = 62;
        public const int r0prep = 63;
        public const int r1prep = 64;
        public const int r2prep = 65;

        public const int l0larc = 66;
        public const int l1larc = 67;
        public const int l2larc = 68;
        public const int r0larc = 69;
        public const int r1larc = 70;
        public const int r2larc = 71;

        public const int l0rarc = 72;
        public const int l1rarc = 73;
        public const int l2rarc = 74;
        public const int r0rarc = 75;
        public const int r1rarc = 76;
        public const int r2rarc = 77;

        public const int Count = 78;


        //public int ValenceCount { get; private set; }
        //public int PoSTagCount { get; private set; }
        //public int ArcLabelCount { get; private set; }

        private static int[] determinates = 
        {
             l0id, //0;
            l1id, //1;
            l2id, //2;
            r0id, //3;
            r1id, //4;
            r2id, //5;
            l0len, //6;
            l1len, //7;
            l2len, //8;
            r0len, //9;
            r1len, //10;
            r2len, //11;
            l0hc, //12;
            l1hc, //13;
            l2hc, //14;
            r0hc, //15;
            r1hc, //16;
            r2hc, //17;
            r3hc, //18;
            l2l1dst, //19;
            l1l0dst, //20;
            l0r0dst, //21;
            r0r1dst, //22;
            r1r2dst, //23;
            l0id, //24;
            l1id, //25;
            l2id, //26;
            r0id, //27;
            r1id, //28;
            r2id, //29;
            l0id, //30;
            l1id, //31;
            l2id, //32;
            r0id, //33;
            r1id, //34;
            r2id, //35;
            l0lct, //36;
            l1lct, //37;
            l2lct, //38;
            r0lct, //39;
            r1lct, //40;
            r2lct, //41;
            l0rct, //42;
            l1rct, //43;
            l2rct, //44;
            r0rct, //45;
            r1rct, //46;
            r2rct, //47;

            l0lcw, //48;
            l1lcw, //49;
            l2lcw, //50;
            r0lcw, //51;
            r1lcw, //52;
            r2lcw, //53;
        

            l0rcw, //54;
            l1rcw, //55;
            l2rcw, //56;
            r0rcw, //57;
            r1rcw, //58;
            r2rcw, //59;

            l0id, //60;
            l1id, //61;
            l2id, //62;
            r0id, //63;
            r1id, //64;
            r2id, //65;

            l0larc,// = 66;
            l1larc,// = 67;
            l2larc,// = 68;
            r0larc,// = 69;
            r1larc,// = 70;
            r2larc,// = 71;

            l0rarc,// = 66;
            l1rarc,// = 67;
            l2rarc,// = 68;
            r0rarc,// = 69;
            r1rarc,// = 70;
            r2rarc,// = 71;
        };

        private static string[] Names = 
        {
            "l0id", //0;
        "l1id", //1;
        "l2id", //2;
        "r0id", //3;
        "r1id", //4;
        "r2id", //5;
        "l0len", //6;
        "l1len", //7;
        "l2len", //8;
        "r0len", //9;
        "r1len", //10;
        "r2len", //11;
        "l0hc", //12;
        "l1hc", //13;
        "l2hc", //14;
        "r0hc", //15;
        "r1hc", //16;
        "r2hc", //17;
        "r3hc", //18;
        "l2l1dst", //19;
        "l1l0dst", //20;
        "l0r0dst", //21;
        "r0r1dst", //22;
        "r1r2dst", //23;
        "l0t", //24;
        "l1t", //25;
        "l2t", //26;
        "r0t", //27;
        "r1t", //28;
        "r2t", //29;
        "l0w", //30;
        "l1w", //31;
        "l2w", //32;
        "r0w", //33;
        "r1w", //34;
        "r2w", //35;
        "l0lct", //36;
        "l1lct", //37;
        "l2lct", //38;
        "r0lct", //39;
        "r1lct", //40;
        "r2lct", //41;
        "l0rct", //42;
        "l1rct", //43;
        "l2rct", //44;
        "r0rct", //45;
        "r1rct", //46;
        "r2rct", //47;

        "l0lcw", //48;
        "l1lcw", //49;
        "l2lcw", //50;
        "r0lcw", //51;
        "r1lcw", //52;
        "r2lcw", //53;
        

        "l0rcw", //54;
        "l1rcw", //55;
        "l2rcw", //56;
        "r0rcw", //57;
        "r1rcw", //58;
        "r2rcw", //59;

        "l0prep", //60;
        "l1prep", //61;
        "l2prep", //62;
        "r0prep", //63;
        "r1prep", //64;
        "r2prep", //65;

        "l0larc",// = 66;
        "l1larc",// = 67;
        "l2larc",// = 68;
        "r0larc",// = 69;
        "r1larc",// = 70;
        "r2larc",// = 71;

        "l0rarc",// = 66;
        "l1rarc",// = 67;
        "l2rarc",// = 68;
        "r0rarc",// = 69;
        "r1rarc",// = 70;
        "r2rarc",// = 71;
        };

        public static int GetDet(int featId)
        {
            if (featId < 0 || featId >= Count)
            {
                return -1;
            }
            else
            {
                return determinates[featId];
            }
        }

        static public int NameToId(string name)
        {
            return Array.FindIndex<string>(Names, i => (i == name));
        }
    }
}
