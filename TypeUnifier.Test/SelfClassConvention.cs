using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeUnifier.Scanning;

namespace TypeUnifier.Test
{
    class SelfClassConvention<TSelf> : DefaultScanConvention
    {
        public override bool IsValidPair(Type abstraction, Type implementation)
        {
            return abstraction.DeclaringType == typeof(TSelf)
                && implementation.DeclaringType == typeof(TSelf)
                && base.IsValidPair(abstraction, implementation);
        }
    }
}
