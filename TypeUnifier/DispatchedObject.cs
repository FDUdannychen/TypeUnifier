using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeUnifier
{
    abstract class DispatchedObject
    {
        public string NodeId { get; set; }

        public Type AbstractionType { get; set; }

        public object Implementation { get; set; }
    }
}
