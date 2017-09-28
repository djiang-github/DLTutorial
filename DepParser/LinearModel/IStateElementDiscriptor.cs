using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinearFunction
{
    public interface IStateElementDiscriptor
    {
        bool GetElementId(string ElementName, out int ElementId, out int Determinate);
        bool GetElememtDet(int Id, out int det);
        bool GetElementName(int ElementId, out string ElementName);
    }
}
