using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreeTokenizor
{
    class CharConvertor
    {
        public static char FulltoHalf(char c)
        {
            if ((c >= 'ａ') && (c <= 'ｚ'))
                return (char)('a' + c - 'ａ');
            if ((c >= 'Ａ') && (c <= 'Ｚ'))
                return (char)('A' + c - 'Ａ');
            if ((c >= '０') && (c <= '９'))
                return (char)('0' + c - '０');

            switch (c)
            {
                case '“': return '"';
                case '”': return '"';
                case '‘': return '\'';
                case '’': return '\'';
                case '＠': return '@';
                case '﹫': return '@';
                case '。': return '.';
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
        }//end FulltoHarf

    }
}
