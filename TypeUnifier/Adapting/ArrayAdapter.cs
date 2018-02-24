using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.DynamicProxy;

namespace TypeUnifier.Adapting
{
    class ArrayAdapter : ITypeAdapter
    {
        public static bool IsAdaptable(Type from, Type to)
        {
            return from.IsArray
                && to.IsArray
                && from.GetArrayRank() == 1
                && to.GetArrayRank() == 1;
        }

        public Type From { get; }
        public Type To { get; }

        private readonly ITypeAdapter _inner;

        public ArrayAdapter(Type from, Type to, ITypeAdapter inner)
        {
            this.From = from;
            this.To = to;
            _inner = inner;
        }

        public object Adapt(object value, IInterceptor interceptor)
        {
            if (value == null) return null;

            var fromArray = (Array)value;
            var toArray = Array.CreateInstance(this.To.GetElementType(), fromArray.Length);

            for (var i = 0; i < fromArray.Length; i++)
            {
                var fromItem = fromArray.GetValue(i);
                var toItem = _inner.Adapt(fromItem, interceptor);
                toArray.SetValue(toItem, i);
            }

            return toArray;
        }
    }
}
