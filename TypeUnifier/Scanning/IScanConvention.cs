using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeUnifier.Adapting;

namespace TypeUnifier.Scanning
{
    public interface IScanConvention
    {
        bool IsValidPair(Type abstraction, Type implementation);
    }
}
