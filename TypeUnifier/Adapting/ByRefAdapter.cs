using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.DynamicProxy;

namespace TypeUnifier.Adapting
{
    class ByRefAdapter : ITypeAdapter
    {
        public static bool IsAdaptable(Type from, Type to)
        {
            return from.IsByRef && to.IsByRef;
        }

        public Type From { get; }
        public Type To { get; }

        private readonly ITypeAdapter _inner;

        public ByRefAdapter(Type from, Type to, ITypeAdapter inner)
        {
            this.From = from;
            this.To = to;
            _inner = inner;
        }

        public object Adapt(object value, IInterceptor interceptor)
        {
            return _inner.Adapt(value, interceptor);
        }
    }
}
