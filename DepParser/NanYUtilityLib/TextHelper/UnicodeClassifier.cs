using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NanYUtilityLib
{
    public class UnicodeClassifier
    {
        // Check whether a unicode char is a CJK script.
        // Including CJK unified Ideographs and its Extension A,
        // and CJK Compatibility Ideographs.
        // Japanese katakana and fullwidth latin chars are not included.
        public static bool IsCJKIdeograph(char c)
        {
            // CJK Unified Ideographs:
            // 4E00-62FF, 6300-77FF, 7800-8CFF, 8D00-9FFF
            // CJK Unified Ideographs Extension A:
            // 3400-4DBF
            // CJK Compatibility Ideographs:
            // F900–FAFF
            return (c >= (char)0x4E00 && c <= (char)0x9FFF)
                || (c >= (char)0x3400 && c <= (char)0x4DBF)
                || (c >= (char)0xF900 && c <= (char)0xFAFF);
        }

        public static bool IsJapaneseKana(char c)
        {
            return (c >= (char)0x30A1 && c <= (char)0x30FA)
                || (c >= (char)0xFF66 && c <= (char)0xFF9D)
                || (c >= (char)0x31F0 && c <= (char)0x31FF)
                || (c >= (char)0x3041 && c <= (char)0x3096);
        }
    }
}
