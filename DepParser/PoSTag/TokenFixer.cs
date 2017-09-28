using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralPosTag
{
    public class TokenFixer
    {
        public static string[] FixQuote(string[] words)
        {
            string[] tokens = new string[words.Length];

            bool waitR = false;

            for (int i = 0; i < tokens.Length; ++i)
            {
                string prevW = i > 0 ? words[i - 1] : null;
                string nextW = i < tokens.Length - 1 ? words[i + 1] : null;

                if (words[i] == "\"")
                {
                    if (waitR
                        || (i > 0 && i == tokens.Length - 1)
                        || (i > 0 && (nextW == "says" || nextW == "said" || nextW == "say"))
                        )
                    {
                        tokens[i] = "\'\'";
                        waitR = false;
                    }
                    else
                    {
                        tokens[i] = "``";
                        waitR = true;
                    }
                }
                else
                {
                    tokens[i] = words[i];
                }
            }

            return tokens;
        }
    }
}
