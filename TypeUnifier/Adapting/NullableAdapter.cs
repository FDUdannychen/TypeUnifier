using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.DynamicProxy;

namespace TypeUnifier.Adapting
{
    class NullableAdapter : ITypeAdapter
    {
        public static bool IsAdaptable(Type from, Type to)
        {
            return from.IsGenericType
                && from.GetGenericTypeDefinition().Equals(typeof(Nullable<>))
                && to.IsGenericType
                && to.GetGenericTypeDefinition().Equals(typeof(Nullable<>));
        }

        public Type From { get; }
        public Type To { get; }

        private readonly ITypeAdapter _inner;

        public NullableAdapter(Type from, Type to, ITypeAdapter inner)
        {
            this.From = from;
            this.To = to;
            _inner = inner;
        }

        public object Adapt(object value, IInterceptor interceptor)
        {
            if (value == null) return null;

            var fromUnderlyingType = Nullable.GetUnderlyingType(this.From);
            var fromUnderlyingValue = Convert.ChangeType(value, fromUnderlyingType);
            var toUnderlyingType = Nullable.GetUnderlyingType(this.To);
            var toUnderlyingValue = _inner.Adapt(fromUnderlyingValue, interceptor);
            return Activator.CreateInstance(this.To, toUnderlyingValue);
        }
    }
}
