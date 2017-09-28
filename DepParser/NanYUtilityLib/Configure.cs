using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.IO;

namespace NanYUtilityLib
{
    public class Configure
    {
        static string[] args;
        public static void SetArgs(string[] args)
        {
            Configure.args = args;
        }
        public static string GetCmdOpt(string opt)
        {
            if (args == null)
                return null;
            string nopt = opt.ToLower();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Length <= 1 || args[i][0] != '-')
                    continue;
                if (args[i].Substring(1).ToLower() == nopt && i + 1 < args.Length)
                    return args[i + 1];
            }
            return null;
        }

        public static string GetOpt(string opt)
        {
            string v = GetCmdOpt(opt);
            if (v != null)
                return v;
            return System.Configuration.ConfigurationManager.AppSettings[opt];
        }

        public static void Load(string fn)
        {
        }

        public static string GetOptionString(string key)
        {
            string v = GetOpt(key);
            return v == null ? null : v.Trim();
        }

        public static string[] GetOptionStringList(string key)
        {
            return GetOpt(key).Trim().Split(' ');
        }

        public static int GetOptionInt(string key, int defaultValue)
        {
            string v = GetOpt(key);
            return v == null ? defaultValue : int.Parse(v);
        }
        public static long GetOptionLong(string key, long dafaultValue)
        {
            string v = GetOpt(key);
            return v == null ? dafaultValue : long.Parse(v);
        }
        public static int[] GetOptionIntList(string key)
        {
            string v = GetOpt(key);
            if (v == null)
                return null;
            string[] t = v.Trim().Split(' ');
            int[] ret = new int[t.Length];
            for (int i = 0; i < t.Length; i++)
                ret[i] = int.Parse(t[i]);
            return ret;
        }

        public static bool GetOptionBool(string key, bool defaultValue)
        {
            string v = GetOpt(key);
            return v == null ? defaultValue : v.Equals("1") || v.Equals("true");
        }

        public static float GetOptionFloat(string key, float defaultValue)
        {
            string v = GetOpt(key);
            return v == null ? defaultValue : float.Parse(v);
        }

        public static double[] GetOptionFloatList(string key)
        {
            string v = GetOpt(key);
            if (v == null)
                return null;
            string[] t = v.Trim().Split(' ');
            double[] ret = new double[t.Length];
            for (int i = 0; i < t.Length; i++)
                ret[i] = double.Parse(t[i]);
            return ret;
        }
        //public static double[] LoadParams()
        //{
        //    double[] featWeight;
        //    string fwgroup = Configure.GetOptionString("lambda");
        //    if (fwgroup == null)
        //        UtilFunc.Exit("Feature weight group not specified.");
        //    featWeight = Configure.GetOptionFloatList(fwgroup);
        //    if (featWeight == null)
        //        UtilFunc.Exit("Feature weight group [" + fwgroup + "] not found.");
        //    Console.Error.WriteLine("Loading feature weights [{0}]", fwgroup);
        //    return featWeight;
        //}
    }
}
