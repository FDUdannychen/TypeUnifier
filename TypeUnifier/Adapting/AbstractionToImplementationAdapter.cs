using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.DynamicProxy;

namespace TypeUnifier.Adapting
{
    class AbstractionToImplementationAdapter : ITypeAdapter
    {
        public Type From { get; }
        public Type To { get; }

        public AbstractionToImplementationAdapter(Type from, Type to)
        {
            this.From = from;
            this.To = to;
        }

        public static bool IsAdaptable(Type from, Type to)
        {
            return from.IsInterface && !to.IsInterface;
        }

        public object Adapt(object value, IInterceptor interceptor)
        {
            if (value == null) return null;
            return ((DispatchedObject)value).Implementation;
        }
    }
}
