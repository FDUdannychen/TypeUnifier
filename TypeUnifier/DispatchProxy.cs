using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.DynamicProxy;

namespace TypeUnifier
{
    class DispatchProxy
    {
        private static readonly ProxyGenerator _generator = new ProxyGenerator();
        private static readonly ProxyGenerationOptions _options = new ProxyGenerationOptions { BaseTypeForInterfaceProxy = typeof(DispatchedObject) };

        public static object Create(Type abstraction, IInterceptor interceptor)
        {
            var instance = (DispatchedObject)_generator.CreateInterfaceProxyWithoutTarget(abstraction, _options, interceptor);
            instance.AbstractionType = abstraction;            
            return instance;
        }
    }
}
