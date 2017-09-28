using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MSRA.NLC.Lingo.NLP;

using System.Text.RegularExpressions;
using Microsoft.MT.Common.Tokenization;


namespace RecoverWBAlignment
{
    public interface ITokenizorWrapper
    {
        string[] Tokenize(string sent);
    }

    public class QATokenizor : ITokenizorWrapper
    {
        string[] ITokenizorWrapper.Tokenize(string sent)
        {
            sent = Regex.Replace(sent, @"\'ll", "DUMMMY_LL");
            sent = Regex.Replace(sent, @"\'m", "DUMMMY_AM");
            sent = Regex.Replace(sent, @"\'s", "DUMMMY_S");
            sent = Regex.Replace(sent, @"\'re", "DUMMMY_RE");
            sent = Regex.Replace(sent, @"\'d", "DUMMMY_D");
            string[] tok = tokenizer.Tokenize(sent);

            sent = string.Join(" ", tok);

            sent = Regex.Replace(sent, "DUMMMY_LL", "\'ll");
            sent = Regex.Replace(sent, "DUMMMY_AM", "\'m");
            sent = Regex.Replace(sent, "DUMMMY_S", "\'s");
            sent = Regex.Replace(sent, "DUMMMY_RE", "\'re");
            sent = Regex.Replace(sent, "DUMMMY_D", "\'d");
            return sent.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
        }

        ITokenizer tokenizer = new EnglishTokenizerBasedOnRules();
    }

    public class MTTokenizor : ITokenizorWrapper
    {
        public MTTokenizor(string[] args)
        {
            tokenizor = new Microsoft.MT.Common.Tokenization.SegRT(args);
        }

        public string[] Tokenize(string sent)
        {
            return tokenizor.preSegSent(sent).Split(new string[] { "||||" }, StringSplitOptions.RemoveEmptyEntries)[0].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
        }

        Microsoft.MT.Common.Tokenization.SegRT tokenizor;
    }

    public class MTCWBreaker : ITokenizorWrapper
    {
        public MTCWBreaker(string datapath)
        {
            cwb = new MSRA.NLC.Lingo.NLP.ChineseWordBreaker(datapath);
        }

        public string[] Tokenize(string sent)
        {
            return cwb.Tokenize(sent);
        }

        MSRA.NLC.Lingo.NLP.ChineseWordBreaker cwb;
        
    }

    public class RecoverEngWBAlign
    {
        // This class is used to recover word alignment between English sentences before and after tokenization.

        // Note0:
        // It won't work for languages other than English.
        // Furthermore, the tokenizor should hold the following independence assuption:
        // Consider one sentence consisting of two parts A, B,
        // if T(AB) == T(A) + T(B), then the tokenizor must guarentee that
        // T(A|AB) == T(A) && T(B|AB) == T(B)

        // Note1:
        // The tokenizor can treat end-sentence punctuation differently.
        // In fact, if you define "sentence" to be a normal sentence with a dummy token
        // appended to the end, then the independency assuption still holds.

        // Note2:
        // It is so inefficient that makes me puke.
        // But it works on the interface of Microsoft.MT.Common.Tokenization.SegRT,
        // so I don't have to touch the inside code...
        
        // Note3:
        // It is sensitive to the choice of end-of-sentence dummy token.
        // You must choose a dummy token that the tokenizor will never change or merge
        // with previous word.

        public bool RecoverAlign(
            string[] oriSent,
            string[] wbSent,
            out string alignStr)
        {
            alignStr = null;
            int oriID = 0;
            int wbID = 0;
            StringBuilder sb = new StringBuilder();
            
            while (oriID < oriSent.Length && wbID < wbSent.Length)
            {
                if (oriSent[oriID] == wbSent[wbID])
                {
                    sb.AppendFormat("{0}-{1} ", oriID, wbID);
                    oriID++;
                    wbID++;
                }
                else
                {
                    int oriEnd = oriID;
                    int wbEnd = wbID;
                    
                    bool bSuccess = MatchNextToken(oriSent, wbSent, oriID, out oriEnd, out wbEnd);
                    if (!bSuccess)
                    {
                        alignStr = sb.ToString().Trim();
                        return false;
                    }
                    else
                    {
                        for (int i = oriID; i < oriEnd; ++i)
                        {
                            for (int j = wbID; j < wbEnd; ++j)
                            {
                                sb.AppendFormat("{0}-{1} ", i, j);
                            }
                        }
                        oriID = oriEnd;
                        wbID = wbEnd;
                    }
                }
            }

            if (oriID != oriSent.Length || wbID != wbSent.Length)
            {
                alignStr = sb.ToString().Trim();
                return false;
            }

            alignStr = sb.ToString().Trim();

            return true;
        }

        private bool MatchNextToken(
            string[] oriSent,
            string[] wbSent,
            int oriID,
            out int oriEnd,
            out int wbEnd)
        {
            oriEnd = oriID + 1;
            wbEnd = 0;
            bool bSuccess = false;
            while (oriEnd <= oriSent.Length)
            {
                string[] firstPart = GetWBSent(oriSent, 0, oriEnd);
                string[] secondPart = GetWBSent(oriSent, oriEnd);
                if (MatchStringArr(wbSent, 0, firstPart, secondPart))
                {
                    bSuccess = true;
                    wbEnd = firstPart.Length;
                    break;
                }
                oriEnd++;
            }
            if (bSuccess)
            {
                while (oriEnd > oriID + 1)
                {
                    if (oriSent[oriEnd - 1] == wbSent[wbEnd - 1])
                    {
                        oriEnd--;
                        wbEnd--;
                    }
                    break;
                }
            }
            return bSuccess;
        }

        public bool WBwithAlign(
            string[] oriSent,
            out string[] wbSent,
            out string alignStr)
        {
            wbSent = GetWBSent(oriSent, 0);

            return RecoverAlign(oriSent, wbSent, out alignStr);
        }

        public bool WBwithAlign(
            string oriSent,
            out string wbSent,
            out string alignStr)
        {
            string[] oriArr = oriSent.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            string[] wbArr;
            bool bSuccess = WBwithAlign(oriArr, out wbArr, out alignStr);
            wbSent = string.Join(" ", wbArr);
            return bSuccess;
        }

        public string[] WB(string[] oriSent)
        {
            return GetWBSent(oriSent, 0);
        }

        public RecoverEngWBAlign(ITokenizorWrapper wbreaker)
        {
            this.wbreaker = wbreaker;
        }

        public RecoverEngWBAlign(ITokenizorWrapper wbreaker, string sentEndDummy)
        {
            this.wbreaker = wbreaker;
            this.sentEndDummy = sentEndDummy;
        }

        private string[] GetWBSent(string[] oriSent, int oriBeg, int oriEnd)
        {
            if (oriSent == null || oriBeg >= oriEnd || oriBeg < 0 || oriEnd > oriSent.Length)
            {
                return null;
            }

            string sentFrag = string.Join(" ", oriSent, oriBeg, oriEnd - oriBeg);
            if (oriEnd != oriSent.Length)
            {
                // If the fragment does not include the end of the sentence,
                // I add a pseudo token at the end of the fragment
                // because the Eng-WB treat end-punc differently.
                sentFrag += " " + this.sentEndDummy;
            }

            //string wbRawString = this.wbreaker.preSegSent(sentFrag);

            //string[] wbWordArr;

            //string[] wbParts = wbRawString.Split(new string[] { "||||" }, StringSplitOptions.RemoveEmptyEntries);
            //if (wbParts == null || wbParts.Length == 0)
            //{
            //    wbWordArr = null;
            //}
            string[] wbWordArr = wbreaker.Tokenize(sentFrag);//wbParts[0].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            
            if (wbWordArr == null || wbWordArr.Length == 0)
            {
                return null;
            }

            if (oriEnd != oriSent.Length)
            {
                wbWordArr = TruncateSentEndDummy(wbWordArr);
            }

            return wbWordArr;
        }

        private string[] TruncateSentEndDummy(string[] wbWordArr)
        {
            if (wbWordArr == null || wbWordArr.Length == 0)
            {
                return null;
            }
            if (wbWordArr[wbWordArr.Length - 1] != this.sentEndDummy)
            {
                if (wbWordArr[wbWordArr.Length - 1].EndsWith(this.sentEndDummy))
                {
                    wbWordArr[wbWordArr.Length - 1] = 
                        wbWordArr[wbWordArr.Length - 1].Substring(
                        0, wbWordArr[wbWordArr.Length - 1].Length - this.sentEndDummy.Length
                        );
                    return wbWordArr;
                }
                else
                {
                    throw new Exception("Word breaker changes sentence-end-Dummy-Token!");
                }
            }
            if (wbWordArr.Length == 1)
            {
                wbWordArr = null;
            }
            string[] tmpWbWordArr = new string[wbWordArr.Length - 1];
            for (int i = 0; i < tmpWbWordArr.Length; ++i)
            {
                tmpWbWordArr[i] = wbWordArr[i];
            }
            wbWordArr = tmpWbWordArr;
            return wbWordArr;
        }

        private string[] GetWBSent(string[] oriSent, int oriBeg)
        {
            if (oriSent == null)
            {
                return null;
            }

            return GetWBSent(oriSent, oriBeg, oriSent.Length);
        }

        private bool MatchStringArr(string[] wordArr, int beg, string[] firstPart, string[] secondPart)
        {
            if (wordArr == null || beg > wordArr.Length || beg < 0)
            {
                return false;
            }

            int firstLen = firstPart == null ? 0 : firstPart.Length;
            int secondLen = secondPart == null ? 0 : secondPart.Length;

            if (beg == wordArr.Length)
            {
                // special case
                return firstLen == 0 && secondLen == 0;
            }         

            if (firstLen + secondLen != wordArr.Length - beg)
            {
                return false;
            }

            for (int i = beg; i < firstLen + beg; ++i)
            {
                if (wordArr[i] != firstPart[i - beg])
                {
                    return false;
                }
            }

            for (int i = beg + firstLen; i < beg + firstLen + secondLen; ++i)
            {
                if (wordArr[i] != secondPart[i - (beg + firstLen)])
                {
                    return false;
                }
            }

            return true;
        }

        //private Microsoft.MT.Common.Tokenization.SegRT wbreaker;

        private ITokenizorWrapper wbreaker;

        private readonly string sentEndDummy = "$number";

    }
}
