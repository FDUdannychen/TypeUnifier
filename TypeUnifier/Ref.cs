using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using TypeUnifier.Adapting;

namespace TypeUnifier
{
    static class Ref
    {
        public static readonly MethodInfo ITypeAdapter_Adapt
            = typeof(ITypeAdapter).GetMethod(nameof(ITypeAdapter.Adapt));

        public static readonly MethodInfo ImplementationToAbstractionAdapter_ForceAdapt
            = typeof(ImplementationToAbstractionAdapter).GetMethod(nameof(ImplementationToAbstractionAdapter.ForceAdapt));

        public static readonly PropertyInfo IInvocation_Proxy
            = typeof(IInvocation).GetProperty(nameof(IInvocation.Proxy));

        public static readonly PropertyInfo IInvocation_Arguments
            = typeof(IInvocation).GetProperty(nameof(IInvocation.Arguments));

        public static readonly PropertyInfo IInvocation_ReturnValue
            = typeof(IInvocation).GetProperty(nameof(IInvocation.ReturnValue));

        public static readonly PropertyInfo DispatchedObject_Implementation
            = typeof(DispatchedObject).GetProperty(nameof(DispatchedObject.Implementation));
    }
}
