using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TypeUnifier.Adapting
{
    class MethodInfoAdapter
    {
        public MethodInfoAdapter(bool isConstructor, bool isStatic, IEnumerable<string> aliasNames)
        {
            this.IsConstructor = isConstructor;
            this.IsStatic = isStatic;
            this.AliasNames = aliasNames;
        }

        public bool IsConstructor { get; }

        public bool IsStatic { get; }

        public IEnumerable<string> AliasNames { get; }
    }
}
