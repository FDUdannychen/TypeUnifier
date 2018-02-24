using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeUnifier.Adapting;

namespace TypeUnifier.Scanning
{
    public class DefaultScanConvention : IScanConvention
    {
        public virtual bool IsValidPair(Type abstraction, Type implementation)
        {
            return EnumAdapter.IsAdaptable(abstraction, implementation)
                && abstraction.Name.Equals(implementation.Name)
                || AbstractionToImplementationAdapter.IsAdaptable(abstraction, implementation)
                && abstraction.Name.Length == implementation.Name.Length + 1
                && abstraction.Name[0] == 'I'
                && abstraction.Name.Skip(1).SequenceEqual(implementation.Name);
        }
    }
}
