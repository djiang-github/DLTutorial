using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LinearFunction;

namespace LinearDepParser
{
    public class ParserStateDescriptor : IStateElementDiscriptor
    {
        public bool GetElementId(string ElementName, out int ElementId, out int Determinate)
        {
            ElementId = NameToId(ElementName);
            if (ElementId < n0ID)
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

        public const int n0ID = 0;
        public const int s0ID = 1;
        public const int s0hID = 2;
        public const int s0h2ID = 3;
        public const int s0lID = 4;
        public const int s0rID = 5;
        public const int s0l2ID = 6;
        public const int s0r2ID = 7;
        public const int n0lID = 8;
        public const int n0l2ID = 9;
        public const int s0w = 10;
        public const int s0p = 11;
        public const int n0w = 12;
        public const int n0p = 13;
        public const int n1w = 14;
        public const int n1p = 15;
        public const int n2w = 16;
        public const int n2p = 17;
        public const int s0hp = 18;
        public const int s0lp = 19;
        public const int s0rp = 20;
        public const int n0lp = 21;
        public const int dst = 22;
        public const int s0vl = 23;
        public const int s0vr = 24;
        public const int n0vl = 25;
        public const int s0hw = 26;
        public const int s0arc = 27;
        public const int s0lw = 28;
        public const int s0larc = 29;
        public const int s0rw = 30;
        public const int s0rarc = 31;
        public const int s0h2w = 32;
        public const int s0h2p = 33;
        public const int s0harc = 34;
        public const int s0l2w = 35;
        public const int s0l2p = 36;
        public const int s0l2arc = 37;
        public const int s0r2w = 38;
        public const int s0r2p = 39;
        public const int s0r2arc = 40;
        public const int n0l2w = 41;
        public const int n0l2p = 42;
        public const int n0l2arc = 43;
        public const int n0lw = 44;
        public const int n0larc = 45;
        public const int s0c = 46; // stack 0 cluster
        public const int s0hc = 47; // stack 0 head cluster
        public const int s0lc = 48; // stack 0 left child cluster
        public const int s0rc = 49; // stack 0 right child cluster
        public const int n0c = 50; // buffer 0 cluster
        public const int n1c = 51; // buffer 1 cluster
        public const int n2c = 52; // buffer 2 cluster
        public const int n0lc = 53; // buffer 3 cluster
        public const int s0h2c = 54; // stack 0 head head cluster
        public const int Count = 55;

        private static int[] determinates = 
        {
             n0ID,//b0ID = 0;
             s0ID,//s0ID = 1;
             s0hID,//s0hID = 2;
             s0h2ID,//s0h2ID = 3;
             s0lID,//s0lID = 4;
             s0rID,//s0rID = 5;
             s0l2ID,//s0l2ID = 6;
             s0r2ID,//s0r2ID = 7;
             n0lID,//b0lID = 8;
             n0l2ID,//b0l2ID = 9;
             s0ID,//s0w = 10;
             s0ID,//s0p = 11;
             n0ID,//n0w = 12;
             n0ID,//n0p = 13;
             n0ID,//n1w = 14;
             n0ID,//n1p = 15;
             n0ID,//n2w = 16;
             n0ID,//n2p = 17;
             s0hID,//s0hp = 18;
             s0lID,//s0lp = 19;
             s0rID,//s0rp = 20;
             n0lID,//n0lp = 21;
             dst,//dst = 22;
             s0vl,//s0vl = 23;
             s0vr,//s0vr = 24;
             n0vl,//n0vl = 25;
             s0hID,//s0hw = 26;
             s0arc,//s0arc = 27;
             s0lID,//s0lw = 28;
             s0larc,//s0larc = 29;
             s0rID,//s0rw = 30;
             s0rarc,//s0rarc = 31;
             s0h2ID,//s0h2w = 32;
             s0h2ID,//s0h2p = 33;
             s0harc,//s0harc = 34;
             s0l2ID,//s0l2w = 35;
             s0l2ID,//s0l2p = 36;
             s0l2arc,//s0l2arc = 37;
             s0r2ID,//s0r2w = 38;
             s0r2ID,//s0r2p = 39;
             s0r2arc,//s0r2arc = 40;
             n0l2ID,//n0l2w = 41;
             n0l2ID,//n0l2p = 42;
             n0l2arc,//n0l2arc = 43;
             n0lID,//n0lw = 44;
             n0larc,//n0larc = 45;
             s0ID, //s0c = 46; // stack 0 cluster
             s0hID, //s0hc = 47; // stack 0 head cluster
             s0lID, //s0lc = 48; // stack 0 left child cluster
             s0rID, //s0rc = 49; // stack 0 right child cluster
             n0ID, //n0c = 50; // buffer 0 cluster
             n0ID, //n1c = 51; // buffer 1 cluster
             n0ID, //n2c = 52; // buffer 2 cluster
             n0lID, //n0lc = 53; // buffer 3 cluster
             s0h2ID //s0h2c = 54; // stack 0 head head cluster
        };

        private static string[] Names = 
        {
            "b0ID", //0;
            "s0ID", //1;
            "s0hID", //2;
            "s0h2ID", //3;
            "s0lID", //4;
            "s0rID", //5;
            "s0l2ID", //6;
            "s0r2ID", //7;
            "b0lID", //8;
            "b0l2ID", //9;
            "s0w", //10;
            "s0p", //11;
            "n0w", //12;
            "n0p", //13;
            "n1w", //14;
            "n1p", //15;
            "n2w", //16;
            "n2p", //17;
            "s0hp", //18;
            "s0lp", //19;
            "s0rp", //20;
            "n0lp", //21;
            "dst", //22;
            "s0vl", //23;
            "s0vr", //24;
            "n0vl", //25;
            "s0hw", //26;
            "s0arc", //27;
            "s0lw", //28;
            "s0larc", //29;
            "s0rw", //30;
            "s0rarc", //31;
            "s0h2w", //32;
            "s0h2p", //33;
            "s0harc", //34;
            "s0l2w", //35;
            "s0l2p", //36;
            "s0l2arc", //37;
            "s0r2w", //38;
            "s0r2p", //39;
            "s0r2arc", //40;
            "n0l2w", //41;
            "n0l2p", //42;
            "n0l2arc", //43;
            "n0lw", //44;
            "n0larc", //45;
            "s0c", //46; // stack 0 cluster
            "s0hc", //47; // stack 0 head cluster
            "s0lc", //48; // stack 0 left child cluster
            "s0rc", //49; // stack 0 right child cluster
            "n0c", //50; // buffer 0 cluster
            "n1c", //51; // buffer 1 cluster
            "n2c", //52; // buffer 2 cluster
            "n0lc", //53; // buffer 3 cluster
            "s0h2c" //54; // stack 0 head head cluster
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
