using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.DynamicProxy;

namespace TypeUnifier.Adapting
{
    class EnumAdapter : ITypeAdapter
    {
        public static bool IsAdaptable(Type from, Type to)
        {
            return from.IsEnum
                && to.IsEnum
                && from.GetEnumUnderlyingType().Equals(to.GetEnumUnderlyingType());
        }

        public Type From { get; }
        public Type To { get; }

        public EnumAdapter(Type from, Type to)
        {
            this.From = from;
            this.To = to;
        }

        public object Adapt(object value, IInterceptor interceptor)
        {
            var underlyingType = Enum.GetUnderlyingType(value.GetType());
            var underlyingValue = Convert.ChangeType(value, underlyingType);

            if (!Enum.IsDefined(this.To, underlyingValue))
                throw new InvalidCastException($"Can't cast value {value} to {this.To.FullName}: value is not defined in target enum");

            return Enum.ToObject(this.To, underlyingValue);
        }
    }
}
