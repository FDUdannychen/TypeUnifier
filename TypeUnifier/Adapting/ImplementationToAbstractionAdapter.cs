using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.DynamicProxy;

namespace TypeUnifier.Adapting
{
    class ImplementationToAbstractionAdapter : ITypeAdapter
    {
        public static bool IsAdaptable(Type from, Type to)
        {
            return !from.IsInterface && to.IsInterface;
        }

        public Type From { get; }
        public Type To { get; }

        public ImplementationToAbstractionAdapter(Type from, Type to)
        {
            this.From = from;
            this.To = to;
        }

        public object Adapt(object value, IInterceptor interceptor)
        {
            return ForceAdapt(value, this.From, this.To, interceptor);
        }

        public static object ForceAdapt(object value, Type from, Type to, IInterceptor interceptor)
        {
            var dispatched = (DispatchedObject)DispatchProxy.Create(to, interceptor);
            dispatched.Implementation = value;
            return dispatched;
        }
    }
}
