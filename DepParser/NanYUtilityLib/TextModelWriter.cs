using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

namespace NanYUtilityLib
{
    public class TextModelWriter : IDisposable
    {
        private bool disposed = false;
        private bool ownStream = false;
        private StreamWriter sw;
        private int nestLevel = 0;

        private const UInt64 SIG = 0x2333233323332333UL;
        private const UInt64 VER = 0UL;
        public int NestLevel {
            get
            {
                return nestLevel;
            }
            set
            {
                if(value < 0)
                {
                    throw new Exception("nest level cannot be negative!");
                }
                else
                {
                    nestLevel = value;
                }
            }
        }
        public TextModelWriter(string file)
            :this(new StreamWriter(file), true)
        {
            WriteComment("Model Created by TextModelWriter");
            WriteOption("SIG", SIG);
            WriteOption("VER", VER);
        }

        public TextModelWriter(StreamWriter sw, bool ownStream = false)
        {
            this.ownStream = ownStream;
            this.sw = sw;
        }

        public void WriteOption(string key, object value)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < nestLevel; ++i)
            {
                sb.Append('\t');
            }

            
            var line = string.Format("{0}\t{1}", key, value);

            Escape(sb, line);

            sw.WriteLine(sb.ToString());
        }

        public void Write(string line)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < nestLevel; ++i)
            {
                sb.Append('\t');
            }

            Escape(sb, line);

            sw.WriteLine(sb.ToString());
        }

        public void WriteComment(string line)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var c in line)
            {
                if (c == '\r' || c == '\n')
                {
                    if (sb.Length > 0)
                    {
                        sw.WriteLine("// {0}", sb.ToString());
                        sb.Clear();
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }

            if (sb.Length > 0)
            {
                sw.WriteLine("// {0}", sb.ToString());
            }
        }

        public void WriteEmptyLine()
        {
            sw.WriteLine();
        }

        public void Close()
        {
            sw.Close();
        }
        private static void Escape(StringBuilder sb, string line)
        {
            if (line == null)
            {
                return;
            }


            for (int i = 0; i < line.Length; ++i)
            {
                char c = line[i];
                if (c == '\t')
                {
                    if (i == 0)
                    {
                        sb.Append("\\t");
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
                else if (c == '\r')
                {
                    sb.Append("\\r");
                }
                else if (c == '\n')
                {
                    sb.Append("\\n");
                }
                else if (c == '\\')
                {
                    sb.Append("\\\\");
                }
                else
                {
                    sb.Append(c);
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called. 
            if(!this.disposed)
            {
                // If disposing equals true, dispose all managed 
                // and unmanaged resources. 
                if(disposing)
                {
                    // Dispose managed resources.
                    //component.Dispose();
                    if(ownStream)
                    {
                        sw.Dispose();
                    }
                }

                // Call the appropriate methods to clean up 
                // unmanaged resources here. 
                // If disposing is false, 
                // only the following code is executed.
                if(ownStream)
                {
                    sw.Close();
                }

                // Note disposing has been done.
                disposed = true;
            }
        }

    }
}
