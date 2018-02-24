using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TypeUnifier.Adapting;
using TypeUnifier.Scanning;

namespace TypeUnifier
{
    public class Node : INode
    {
        public string Id { get; }

        private readonly Dictionary<Type, Type> _mappings = new Dictionary<Type, Type>();
        public IReadOnlyDictionary<Type, Type> Mappings => _mappings;

        public Node(string id) => this.Id = id;

        public void AddMapping<TAbstraction, TImplementation>()
        {
            this.AddMapping(typeof(TAbstraction), typeof(TImplementation));
        }

        public void AddMapping(Type abstraction, Type implementation)
        {
            if (abstraction.Equals(implementation))
                throw new ArgumentException("Can't add mapping to the original type");

            if (!EnumAdapter.IsAdaptable(abstraction, implementation) && !AbstractionToImplementationAdapter.IsAdaptable(abstraction, implementation))
                throw new ArgumentException($"No adapters found between {abstraction.FullName} and {implementation.FullName}");

            _mappings[abstraction] = implementation;
        }

        public void Scan(Assembly abstractionAssembly, Assembly implementationAssembly, IScanConvention convention = null)
        {
            convention = convention ?? new DefaultScanConvention();
            var abstractions = abstractionAssembly.GetTypes();
            var implementations = implementationAssembly.GetTypes();

            foreach (var abstraction in abstractions)
            {
                if (_mappings.ContainsKey(abstraction)) continue;

                var possibleImplementations = implementations
                    .Where(i => convention.IsValidPair(abstraction, i))
                    .ToList();

                if (possibleImplementations.Count > 1)
                    throw new AmbiguousImplementationException(abstraction, possibleImplementations);

                if (possibleImplementations.Count == 1)
                    _mappings.Add(abstraction, possibleImplementations.Single());
            }
        }
    }
}
