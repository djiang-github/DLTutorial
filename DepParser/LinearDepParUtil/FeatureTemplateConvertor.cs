using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LinearDepParser;

namespace LinearDepParUtil
{
    static class FeatureTemplateConvertor
    {
        static public bool Convert(string oldTemplates, out int[] newTemplates)
        {
            string[] parts = oldTemplates.Split(new string[] { ".", ";", " " }, StringSplitOptions.RemoveEmptyEntries);
            //throw new Exception("This has been commented out!!!");
            List<int> tempIdList = new List<int>();
            foreach (string p in parts)
            {
                int x = ParserStateDescriptor.NameToId(p);
                if (x < 0)
                {
                    newTemplates = null;
                    return false;
                }
                tempIdList.Add(x);
            }
            if (tempIdList.Count == 0)
            {
                newTemplates = null;
                return false;
            }
            newTemplates = tempIdList.ToArray();
            return true;
        }
    }
}
