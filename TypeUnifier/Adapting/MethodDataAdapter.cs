using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TypeUnifier.Adapting
{
    class MethodDataAdapter
    {
        public MethodDataAdapter(MethodBase method, IEnumerable<ITypeAdapter> parameterAdapters, ITypeAdapter returnAdapter)
        {
            this.Method = method;
            this.ParameterAdapters = parameterAdapters.ToArray();
            this.ReturnAdapter = returnAdapter;
        }

        public MethodBase Method { get; }

        public ITypeAdapter[] ParameterAdapters { get; }

        public ITypeAdapter ReturnAdapter { get; }
    }
}
