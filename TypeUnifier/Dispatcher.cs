using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeUnifier
{
    public class Dispatcher : IDispatcher
    {
        private readonly Dictionary<string, DispatchInterceptor> _interceptors;

        public Dispatcher(params INode[] nodes)
        {
            _interceptors = nodes.ToDictionary(n => n.Id, n => new DispatchInterceptor(n));
        }

        public TAbstraction For<TAbstraction>(string nodeId) where TAbstraction : class
        {
            if (!_interceptors.ContainsKey(nodeId))
                throw new NodeNotFoundException(nodeId);

            if (!_interceptors[nodeId].Node.Mappings.ContainsKey(typeof(TAbstraction)))
                throw new NodeNotImplementedException(nodeId, typeof(TAbstraction));

            return (TAbstraction)DispatchProxy.Create(typeof(TAbstraction), _interceptors[nodeId]);
        }
    }
}
