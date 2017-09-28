using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NanYUtilityLib
{
    public class EnglishTextNormalizer
    {
        public static char NormalizeLetter(char c, bool preserveChineseFullStop)
        {
            if (c == ' ' || c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z')
                return c;

            if ((c >= 'ａ') && (c <= 'ｚ'))
                return (char)('a' + c - 'ａ');
            if ((c >= 'Ａ') && (c <= 'Ｚ'))
                return (char)('A' + c - 'Ａ');
            if ((c >= '０') && (c <= '９'))
                return (char)('0' + c - '０');

            switch (c)
            {
                //case '“': return '"';
                //case '”': return '"';
                //case '‘': return '\'';
                //case '’': return '\'';
                case '＠': return '@';
                case '﹫': return '@';
                case '。': return preserveChineseFullStop ? '。' : '.';
                case '．': return '.';
                //case '·': return '.';
                case '，': return ',';
                case '﹐': return ',';//by zdd
                case '：': return ':';
                case '∶': return ':';
                case '；': return ';';
                case '？': return '?';
                case '［': return '[';
                case '］': return ']';
                case '｛': return '{';
                case '｝': return '}';
                case '（': return '(';
                case '）': return ')';
                case '〔': return '(';
                case '〕': return ')';
                case '＜': return '<';
                case '＞': return '>';
                case '／': return '/';
                case '□': return '-';
                case '—': return '-';
                case '－': return '-';
                case '＋': return '+';
                case '＝': return '=';
                case '！': return '!';
                case '＃': return '#';
                case '％': return '%';
                case '＊': return '*';
                //case '、': return ',';// do not convert it for rule matching
                case '｜': return '|';
                case '「': return '"';
                case '『': return '"';
                case '』': return '"';
                case '」': return '"';
                case '【': return '"';
                case '】': return '"';
                case '○': return '0';
                case '　': return ' ';//by zdd
                case '﹕': return ':';
                case '﹔': return ';';
                case '﹖': return '?';
                case '︰': return ':';
                case '﹗': return '!';
                case '〞': return '"';
                case '﹁': return '"';
                case '﹂': return '"';
                case '﹃': return '"';
                case '﹄': return '"';

                case '◆': return ' ';
                case '▼': return ' ';
                case '△': return ' ';
                case '◎': return ' ';
                case '■': return ' ';
                case '●': return ' ';

                default: return c;
            }
        }//end NormalizeLetter

        public static string NormalizeLetter(string str, bool preserveChineseFullStop)
        {
            StringBuilder sb = new StringBuilder(str);

            for (int i = 0; i < sb.Length; ++i)
            {
                sb[i] = NormalizeLetter(sb[i], preserveChineseFullStop);
            }

            return sb.ToString();
        }
    }
}
