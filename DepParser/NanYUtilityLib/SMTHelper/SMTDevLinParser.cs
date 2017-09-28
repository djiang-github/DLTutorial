using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NanYUtilityLib
{
    public class SMTDevLine
    {
        public string text;
        public List<SMTFactoid> factoid = new List<SMTFactoid>();

        private SMTDevLine()
        {
        }

        public SMTDevLine(string devline)
        {
            string[] parts = devline.Split(new string[] { " |||| " }, StringSplitOptions.RemoveEmptyEntries);
            text = parts[0];

            if (parts.Length >= 2)
            {
                string[] factdescriptors = parts[1].Split(new string[] { "{", "}" }, StringSplitOptions.RemoveEmptyEntries);
                if (factdescriptors.Length > 0)
                {
                    foreach (var f in factdescriptors)
                    {
                        factoid.Add(new SMTFactoid(f));
                    }
                }
            }
        }

        static public SMTDevLine CreateDevLineNoThrow(string devline)
        {
            if (string.IsNullOrWhiteSpace(devline))
            {
                return null;
            }
            string[] parts = devline.Split(new string[] { " |||| " }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 2 && parts.Length != 1)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(parts[0]))
            {
                return null;
            }

            var dline = new SMTDevLine();

            dline.text = parts[0];

            if (parts.Length == 2)
            {
                string[] factdescriptors = parts[1].Split(new string[] { "{", "}" }, StringSplitOptions.RemoveEmptyEntries);
                if (factdescriptors.Length > 0)
                {
                    foreach (var f in factdescriptors)
                    {
                        var fd = SMTFactoid.CreateFactoidNoThrow(f);
                        if (fd != null)
                        {
                            dline.factoid.Add(fd);
                        }
                    }
                }
            }

            return dline;
        }

        public string Description
        {
            get
            {
                StringBuilder sb = new StringBuilder(text);
                if (factoid != null && factoid.Count > 0)
                {
                    sb.Append(" |||| ");

                    foreach (var f in factoid)
                    {
                        sb.Append(f.Description);
                    }
                }
                return sb.ToString();
            }
        }
    }

    public class SMTFactoid
    {
        public int start;
        public int endInclusive;
        public string symbol;
        public string source;
        public string translation;

        public string Description
        {
            get
            {
                return "{" + string.Format("{0} ||| {1} ||| {2} ||| {3} ||| {4}",
                    start, endInclusive, translation, symbol, source) + "}";
            }

        }

        public SMTFactoid(string description)
        {
            string[] parts = description.Split(new string[] { "{", "}", "|||" }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 5)
            {
                throw new Exception("Bad factoid");
            }

            start = int.Parse(parts[0]);
            endInclusive = int.Parse(parts[1]);
            translation = parts[2];
            symbol = parts[3];
            source = parts[4];
        }

        static public SMTFactoid CreateFactoidNoThrow(string description)
        {
            if (description == null)
            {
                return null;
            }
            string[] parts = description.Split(new string[] { "{", "}", "|||" }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 5)
            {
                return null;
            }

            var factoid = new SMTFactoid();

            if(!int.TryParse(parts[0], out factoid.start)
                || !int.TryParse(parts[1], out factoid.endInclusive))
            {
                return null;
            }
            factoid.translation = parts[2];
            factoid.symbol = parts[3];
            factoid.source = parts[4];

            return factoid;
        }

        private SMTFactoid()
        {
        }
    }
}
