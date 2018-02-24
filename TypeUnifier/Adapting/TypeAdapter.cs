using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TypeUnifier.Adapting
{
    class TypeAdapter
    {
        public static ITypeAdapter Build(IReadOnlyDictionary<Type, Type> mappings, Type from, Type to)
        {
            if (AssignableAdapter.IsAdaptable(from, to))
            {
                return new AssignableAdapter(from, to);
            }

            if (mappings.ContainsKey(from) && mappings[from].Equals(to))
            {
                if (EnumAdapter.IsAdaptable(from, to))
                {
                    return new EnumAdapter(from, to);
                }

                if (AbstractionToImplementationAdapter.IsAdaptable(from, to))
                {
                    return new AbstractionToImplementationAdapter(from, to);
                }
            }

            if (mappings.ContainsKey(to) && mappings[to].Equals(from))
            {
                if (EnumAdapter.IsAdaptable(from, to))
                {
                    return new EnumAdapter(from, to);
                }

                if (ImplementationToAbstractionAdapter.IsAdaptable(from, to))
                {
                    return new ImplementationToAbstractionAdapter(from, to);
                }
            }

            if (ByRefAdapter.IsAdaptable(from, to))
            {
                var fromElementType = from.GetElementType();
                var toElementType = to.GetElementType();

                var inner = Build(mappings, fromElementType, toElementType);
                if (inner != null)
                {
                    return new ByRefAdapter(from, to, inner);
                }
            }

            if (NullableAdapter.IsAdaptable(from, to))
            {
                var fromUnderlyingType = Nullable.GetUnderlyingType(from);
                var toUnderlyingType = Nullable.GetUnderlyingType(to);

                var inner = Build(mappings, fromUnderlyingType, toUnderlyingType);
                if (inner != null)
                {
                    return new NullableAdapter(from, to, inner);
                }
            }

            if (ArrayAdapter.IsAdaptable(from, to))
            {
                var fromElementType = from.GetElementType();
                var toElementType = to.GetElementType();

                var inner = Build(mappings, fromElementType, toElementType);
                if (inner != null)
                {
                    return new ArrayAdapter(from, to, inner);
                }
            }

            if (ListAdapter.IsAdaptable(from, to))
            {
                var fromItemType = from.GetGenericArguments().Single();
                var toItemType = to.GetGenericArguments().Single();

                var inner = Build(mappings, fromItemType, toItemType);
                if (inner != null)
                {
                    return new ListAdapter(from, to, inner);
                }
            }

            return null;
        }
    }
}
