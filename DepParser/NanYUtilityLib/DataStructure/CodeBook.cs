using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace NanYUtilityLib
{
    public class CodeBook
    {
        Dictionary<string, Int64> encoder;
        Dictionary<Int64, string> decoder;
        Int64 nextCode;
        
        public CodeBook()
        {
            encoder = new Dictionary<string, long>();
            decoder = new Dictionary<long, string>();
            nextCode = 0;
            MinCode = 0;
        }

        public CodeBook(int start)
        {
            encoder = new Dictionary<string, long>();
            decoder = new Dictionary<long, string>();
            nextCode = start;
            MinCode = start;
        }

        public void Clear()
        {
            encoder.Clear();
            decoder.Clear();
            nextCode = MinCode;
        }

        public string this[Int64 id]
        {
            get { return decoder[id]; }
        }

        public Int64 this[string txt]
        {
            get { return encoder[txt]; }
        }

        public Int64 GetId(string txt, Int64 defaultValue)
        {
            Int64 id;

            if (txt != null && encoder.TryGetValue(txt, out id))
            {
                return id;
            }

            return defaultValue;
        }

        public string GetTxt(Int64 id, string defaultValue)
        {
            string txt;

            if (decoder.TryGetValue(id, out txt))
            {
                return txt;
            }

            return defaultValue;
        }

        public bool TryGetId(string txt, out Int64 id)
        {
            return encoder.TryGetValue(txt, out id);
        }

        public bool TryGetTxt(Int64 id, out string txt)
        {
            return decoder.TryGetValue(id, out txt);
        }

        public Int64 Add(string txt)
        {
            if (encoder.ContainsKey(txt))
            {
                return encoder[txt];
            }
            else
            {
                encoder[txt] = nextCode;
                decoder[nextCode] = txt;
                return nextCode++;
            }
        }

        public IEnumerable<string> Texts
        {
            get { return encoder.Keys; }
        }

        public Int64 Count
        {
            get { return nextCode - MinCode; }
        }

        public Int64 MaxCode
        {
            get { return nextCode - 1; }
        }

        public Int64 MinCode { get; private set; }

        public void Dump(string fn)
        {
            StreamWriter sw = new StreamWriter(fn, false);
            for (long i = this.MinCode; i <= this.MaxCode; ++i)
            {
                sw.WriteLine("{0} {1}", this[i], i);
            }
            sw.Close();
        }

        public void Dump(string fn, string enc)
        {
            StreamWriter sw = new StreamWriter(fn, false, Encoding.GetEncoding(enc));
            for (long i = this.MinCode; i <= this.MaxCode; ++i)
            {
                sw.WriteLine("{0} {1}", this[i], i);
            }
            sw.Close();
        }
        public bool Load(string fn, string enc)
        {
            Clear();
            StreamReader sr = new StreamReader(fn, Encoding.GetEncoding(enc));
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                string[] parts = line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2)
                {
                    continue;
                }
                long id;
                if (!long.TryParse(parts[1], out id))
                {
                    continue;
                }
                if (id != this.Add(parts[0]))
                {
                    Clear();
                    sr.Close();
                    Console.Error.WriteLine("Error in codebook file!");
                    return false;
                }
            }
            return true;
        }

        public bool Load(string fn)
        {
            Clear();
            StreamReader sr = new StreamReader(fn);
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                string[] parts = line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2)
                {
                    continue;
                }
                long id;
                if (!long.TryParse(parts[1], out id))
                {
                    continue;
                }
                if (id != this.Add(parts[0]))
                {
                    Clear();
                    sr.Close();
                    Console.Error.WriteLine("Error in codebook file!");
                    return false;
                }
            }
            return true;
        }
    }

    public class CodeBook32
    {
        Dictionary<string, int> encoder;
        Dictionary<int, string> decoder;
        int nextCode;

        public void DumpToStream(TextModelWriter sw)
        {
            sw.Write(typeof(CodeBook32).FullName);
            sw.WriteOption("MinCode", MinCode);
            sw.WriteOption("NextCode", nextCode);
            sw.NestLevel += 1;
            for (int id = MinCode; id <= MaxCode; ++id)
            {
                sw.Write(decoder[id]);
            }
            sw.NestLevel -= 1;
            sw.Write(typeof(CodeBook32).FullName);
        }

        static public CodeBook32 LoadFromStream(TextModelReader sr)
        {
            string name = typeof(CodeBook32).FullName;

            string xname = sr.Read();
            if (xname != name)
            {
                throw new Exception("model name does not match");
            }
            int startlvl = sr.NestLevel;

            int minCode = sr.ReadOptionInt("MinCode");
            int nextCode = sr.ReadOptionInt("NextCode");
            var cb = new CodeBook32(minCode);
            while (true)
            {
                string line = sr.Read();

                int lvl = sr.NestLevel;

                if (lvl == startlvl)
                {
                    if (line != name)
                    {
                        throw new Exception("model name does not match");
                    }
                    break;
                }
                else
                {
                    cb.Add(line);
                }
            }

            if (cb.nextCode != nextCode)
            {
                throw new Exception("codebook size does not match");
            }

            return cb;
        }

        public CodeBook32()
        {
            encoder = new Dictionary<string, int>();
            decoder = new Dictionary<int, string>();
            nextCode = 0;
            MinCode = 0;
        }

        public CodeBook32(int start)
        {
            encoder = new Dictionary<string, int>();
            decoder = new Dictionary<int, string>();
            nextCode = start;
            MinCode = start;
        }

        public void Clear()
        {
            encoder.Clear();
            decoder.Clear();
            nextCode = MinCode;
        }

        public string this[int id]
        {
            get { return decoder[id]; }
        }

        public int this[string txt]
        {
            get { return encoder[txt]; }
        }

        public int GetId(string txt, int defaultValue)
        {
            int id;

            if (txt != null && encoder.TryGetValue(txt, out id))
            {
                return id;
            }

            return defaultValue;
        }

        public string GetTxt(int id, string defaultValue)
        {
            string txt;

            if (decoder.TryGetValue(id, out txt))
            {
                return txt;
            }

            return defaultValue;
        }

        public bool TryGetId(string txt, out int id)
        {
            return encoder.TryGetValue(txt, out id);
        }

        public bool TryGetTxt(int id, out string txt)
        {
            return decoder.TryGetValue(id, out txt);
        }

        public int Add(string txt)
        {
            if (encoder.ContainsKey(txt))
            {
                return encoder[txt];
            }
            else
            {
                encoder[txt] = nextCode;
                decoder[nextCode] = txt;
                return nextCode++;
            }
        }

        public IEnumerable<string> Texts
        {
            get { return encoder.Keys; }
        }

        public int Count
        {
            get { return nextCode - MinCode; }
        }

        public int MaxCode
        {
            get { return nextCode - 1; }
        }

        public int MinCode { get; private set; }

        public void Dump(string fn)
        {
            StreamWriter sw = new StreamWriter(fn, false);
            for (int i = this.MinCode; i <= this.MaxCode; ++i)
            {
                sw.WriteLine("{0} {1}", this[i], i);
            }
            sw.Close();
        }

        public void Dump(string fn, string enc)
        {
            StreamWriter sw = new StreamWriter(fn, false, Encoding.GetEncoding(enc));
            for (int i = this.MinCode; i <= this.MaxCode; ++i)
            {
                sw.WriteLine("{0} {1}", this[i], i);
            }
            sw.Close();
        }
        public bool Load(string fn, string enc)
        {
            Clear();
            StreamReader sr = new StreamReader(fn, Encoding.GetEncoding(enc));
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                string[] parts = line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2)
                {
                    continue;
                }
                int id;
                if (!int.TryParse(parts[1], out id))
                {
                    continue;
                }
                if (id != this.Add(parts[0]))
                {
                    Clear();
                    sr.Close();
                    Console.Error.WriteLine("Error in codebook file!");
                    return false;
                }
            }
            return true;
        }

        public bool Load(string fn)
        {
            Clear();
            StreamReader sr = new StreamReader(fn);
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                string[] parts = line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2)
                {
                    continue;
                }
                int id;
                if (!int.TryParse(parts[1], out id))
                {
                    continue;
                }
                if (id != this.Add(parts[0]))
                {
                    Clear();
                    sr.Close();
                    Console.Error.WriteLine("Error in codebook file!");
                    return false;
                }
            }
            return true;
        }
    }

}
