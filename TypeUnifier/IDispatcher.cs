using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeUnifier
{
    public interface IDispatcher
    {
        TAbstraction For<TAbstraction>(string nodeId) where TAbstraction : class;
    }
}
