using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.DynamicProxy;

namespace TypeUnifier.Adapting
{
    class AssignableAdapter : ITypeAdapter
    {
        public Type From { get; }
        public Type To { get; }

        public AssignableAdapter(Type from, Type to)
        {
            this.From = from;
            this.To = to;
        }

        public static bool IsAdaptable(Type from, Type to)
        {
            return !from.IsByRef && to.IsAssignableFrom(from);
        }

        public object Adapt(object value, IInterceptor interceptor)
        {
            return value;
        }
    }
}
