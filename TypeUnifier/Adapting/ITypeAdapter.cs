using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.DynamicProxy;

namespace TypeUnifier.Adapting
{
    public interface ITypeAdapter
    {
        Type From { get; }
        Type To { get; }
        object Adapt(object value, IInterceptor interceptor);
    }
}
