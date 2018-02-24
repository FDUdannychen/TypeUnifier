using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TypeUnifier
{
    public abstract class DispatchAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class ConstructorAttribute : DispatchAttribute { }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class StaticAttribute : DispatchAttribute { }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class AliasAttribute : DispatchAttribute
    {
        public IEnumerable<string> Names { get; }
        public AliasAttribute(params string[] names) => this.Names = names;
    }
}
