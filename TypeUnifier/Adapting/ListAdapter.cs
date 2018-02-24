using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.DynamicProxy;

namespace TypeUnifier.Adapting
{
    class ListAdapter : ITypeAdapter
    {
        private static HashSet<Type> _collectionTypes = new HashSet<Type>(
            typeof(List<>).GetInterfaces()
                .Where(t => t.IsGenericType)
                .Select(t => t.GetGenericTypeDefinition()));

        public static bool IsAdaptable(Type from, Type to)
        {
            return from.IsGenericType
                && to.IsGenericType
                && from.GetGenericTypeDefinition().Equals(to.GetGenericTypeDefinition())
                && _collectionTypes.Contains(from.GetGenericTypeDefinition());
        }

        public Type From { get; }
        public Type To { get; }

        private readonly ITypeAdapter _inner;

        public ListAdapter(Type from, Type to, ITypeAdapter inner)
        {
            this.From = from;
            this.To = to;
            _inner = inner;
        }

        public object Adapt(object value, IInterceptor interceptor)
        {
            if (value == null) return null;
            
            var toItemType = this.To.GetGenericArguments().Single();

            var toValue = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(toItemType));

            foreach (var fromItem in (IEnumerable)value)
            {
                var toItem = _inner.Adapt(fromItem, interceptor);
                toValue.Add(toItem);
            }

            return toValue;
        }
    }
}
