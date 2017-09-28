using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserUtil
{
    public class LRParseWord : IComparable<LRParseWord>
    {
        public string Tok;
        public string PoS;
        public string Label;
        public int id;

        public int CompareTo(LRParseWord other)
        {
            return this.id.CompareTo(other.id);
        }
    }
}
