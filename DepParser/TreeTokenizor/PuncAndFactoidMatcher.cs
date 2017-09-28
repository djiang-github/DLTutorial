using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConvertTreeTokenization
{
    static class PuncAndFactoidMatcher
    {
        public static bool TryMatchFactoid(string token, out string PoS)
        {
            switch (token)
            {
                case "$number":
                case "$ord":
                case "$ipver":
                    PoS = "CD";
                    return true;
                case "$day":
                case "$time":
                case "$date":
                case "$literal":
                case "$person":
                case "$loc":
                case "$org":
                case "$dateline":
                case "$ne":
                case "$url":
                case "$email":
                case "$modeltype":
                case "$unit":
                    PoS = "NN";
                    return true;
                default:
                    PoS = null;
                    return false;
            }
        }

        public static bool TryMatchPunc(string token, out string PoS)
        {
            switch (token)
            {
                case ".":
                case "!":
                case "?":
                case ";":
                    PoS = ".";
                    return true;
                case ",":
                    PoS = ",";
                    return true;
                case "\"":
                case "\''":
                case "``":
                    PoS = "\"";
                    return true;
                case "\'":
                    PoS = "POS";
                    return true;
                case "(":
                case "[":
                case "{":
                    PoS = "LRB";
                    return true;
                case ")":
                case "]":
                case "}":
                    PoS = "RRB";
                    return true;
                case "`":
                    PoS = "\'";
                    return true;
                case "-":
                    PoS = "-";
                    return true;
                case ":":
                case "\\":
                case "/":
                case "...":
                case "--":
                    PoS = ":";
                    return true;
                default:
                    PoS = null;
                    return false;
            }
        }

        public static bool IsPuncPoS(string PoS)
        {
            const string punPosStr = ". , \" POS \' LRB RRB HYPEN : $ -";

            return punPosStr.IndexOf(PoS) >= 0;
        }

        public static bool IsPuncToken(string token)
        {
            const string puncTokenStr = ". , ! ? - -- ... \" \' \'\' ` `` ( [ { ) ] } : ; $ % @ & * ^ _ # / \\ ";
            return puncTokenStr.IndexOf(token + " ") >= 0;
        }
    }

}
