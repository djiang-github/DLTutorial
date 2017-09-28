using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace ParserUtil
{
    static public class GlobalVariables
    {
        static public string defaultArcType
        {
            get
            {
                return "dep";
            }
        }
        static public string nullArcType
        {
            get
            {
                return "null";
            }
        }
        static public string rootArcType
        {
            get
            {
                return "root";
            }
        }
        static public bool IsPuncPoS(string pos)
        {
            return pos == ":" || pos == "." || pos == "HYPEN" || pos == "\'"
                || pos == "," || pos == "\"" || pos == "LRB" || pos == "RRB";
              
        }

        static public string FakeArcLabel
        {
            get
            {
                return "FAKE";
            }
        }
    }

    static public class Logger
    {
        static string logfn = "log2";
        [Conditional("DEBUG")]
        static public void log(string msg)
        {
            System.IO.StreamWriter sw = new System.IO.StreamWriter(logfn, true);
            sw.WriteLine(msg);
            sw.Close();
        }
        [Conditional("DEBUG")]
        static public void log()
        {
            System.IO.StreamWriter sw = new System.IO.StreamWriter(logfn, true);
            sw.WriteLine();
            sw.Close();
        }
    }
}
