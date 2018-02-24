using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TypeUnifier
{
    public class NodeNotFoundException : Exception
    {
        public NodeNotFoundException(string id)
            : base($"Node {id} is not found")
        { }
    }

    public class NodeNotImplementedException : Exception
    {
        public NodeNotImplementedException(string id, Type abstraction)
            : base($"Can't find any implementation of {abstraction.Name} in node {id}")
        { }
    }

    public class AmbiguousImplementationException : Exception
    {
        public AmbiguousImplementationException(Type abstraction, IEnumerable<Type> implementations)
            : base($"Ambiguous implementations of {abstraction.Name}: {string.Join(",", implementations.Select(i => i.FullName))}")
        { }
    }

    public class ConstructorReturnTypeException : Exception
    {
        public ConstructorReturnTypeException(MethodInfo method, Type expectedReturnType)
            : base($"Invalid dispatching constructor return type in {method}, expected {expectedReturnType.FullName}")
        { }
    }

    public class NotConstructedException : Exception
    {
        public NotConstructedException(MethodInfo method)
            : base($"Can't invoke {method}: object is not constructed")
        { }
    }

    public class MultipleConstructionException : Exception
    {
        public MultipleConstructionException(MethodInfo constructor)
            : base($"Can't invoke {constructor}: object is already constructed")
        { }
    }

    public class MethodNotImplementedException : NotImplementedException
    {
        public MethodNotImplementedException(MethodInfo abstraction, Type implementation)
            : base($"The method {abstraction} is not implemented in {implementation.FullName}")
        { }
    }

    public class AmbiguousMethodException : Exception
    {
        public AmbiguousMethodException(IEnumerable<MethodBase> methods)
            : base($"Ambiguous access: {string.Join("/", methods)}")
        { }
    }
}
