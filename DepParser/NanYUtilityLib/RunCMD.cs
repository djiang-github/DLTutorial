using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Diagnostics;

namespace NanYUtilityLib
{
    static public class RunCMD
    {
        public static void Run(String command, bool UseShellExecute, bool CreateNoWindow)
        {
            Process p = new Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.Arguments = "/c " + command;
            p.StartInfo.UseShellExecute = UseShellExecute;
            p.StartInfo.CreateNoWindow = CreateNoWindow;
            p.Start();
            p.WaitForExit();
        }

        public static void Run(string command)
        {
            Run(command, false, false);
        }

        public static void Run(string[] command, bool UseShellExecute, bool CreateNoWindow)
        {
            int N = command.Length;
            Process[] p = new Process[N];
            for (int i = 0; i < p.Length; ++i)
            {
                p[i] = new Process();
                p[i].StartInfo.FileName = "cmd.exe";
                p[i].StartInfo.Arguments = "/c " + command[i];
                p[i].StartInfo.UseShellExecute = UseShellExecute;
                p[i].StartInfo.CreateNoWindow = CreateNoWindow;
                p[i].Start();
            }

            for (int i = 0; i < p.Length; ++i)
            {
                p[i].WaitForExit();
            }
        }

        public static void Run(string[] command)
        {
            Run(command, false, false);
        }
    }
}
