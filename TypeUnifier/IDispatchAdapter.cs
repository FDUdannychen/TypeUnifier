using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeUnifier
{
    public interface IDispatchAdapter
    {
        bool IsAdaptable(Type from, Type to);

        object Adapt(object from, Type toType);
    }
}
