using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace TypeUnifier.Test
{
    public class NodeTest
    {
        public interface IFoo { }
        public class Foo { }
        public interface IBar { }
        public class Bar { }
        public enum Enum1 { }
        public enum Enum2 { }

        [Fact]
        public void Add_Valid_Mappings()
        {
            var node = new Node("test");
            node.AddMapping<IFoo, Foo>();
            node.AddMapping(typeof(IBar), typeof(Bar));
            node.AddMapping<Enum1, Enum2>();
            node.AddMapping(typeof(Enum2), typeof(Enum1));
            Assert.Equal(4, node.Mappings.Count);
        }

        [Theory]
        [InlineData(typeof(IFoo), typeof(IBar))]
        [InlineData(typeof(Foo), typeof(Foo))]
        [InlineData(typeof(Foo), typeof(IFoo))]
        [InlineData(typeof(Enum1), typeof(Enum1))]
        public void Add_Invalid_Mappings(Type abstraction, Type implementation)
        {
            var node = new Node("test");
            Assert.Throws<ArgumentException>(() => node.AddMapping(abstraction, implementation));
        }

        [Fact]
        public void Add_Mapping_Override()
        {
            var node = new Node("test");

            node.AddMapping(typeof(IFoo), typeof(Bar));
            node.Scan(typeof(IFoo).Assembly, typeof(Foo).Assembly, new SelfClassConvention<NodeTest>());
            Assert.Equal(typeof(Bar), node.Mappings[typeof(IFoo)]);
            Assert.Equal(typeof(Bar), node.Mappings[typeof(IBar)]);

            node.AddMapping(typeof(IFoo), typeof(Foo));
            node.AddMapping(typeof(IBar), typeof(Foo));
            Assert.Equal(typeof(Foo), node.Mappings[typeof(IFoo)]);
            Assert.Equal(typeof(Foo), node.Mappings[typeof(IBar)]);
        }

        public interface IClient
        {
            [Constructor] IClient New();
            int Version { get; }
        }

        public class ClientV1
        {
            public int Version => 1;
        }

        public class ClientV2
        {
            public int Version => 2;
        }


        [Fact]
        public void Dispatching_Between_Nodes()
        {
            var node1 = new Node("v1");
            node1.AddMapping<IClient, ClientV1>();
            node1.AddMapping<IFoo, Foo>();

            var node2 = new Node("v2");
            node2.AddMapping<IClient, ClientV2>();

            var dispatcher = new Dispatcher(new[] { node1, node2 });
            Assert.Throws<NodeNotFoundException>(() => dispatcher.For<IClient>("v3"));
            Assert.Throws<NodeNotImplementedException>(() => dispatcher.For<IFoo>(node2.Id));

            var clientV1 = dispatcher.For<IClient>(node1.Id).New();
            var clientV2 = dispatcher.For<IClient>(node2.Id).New();
            Assert.Equal(1, clientV1.Version);
            Assert.Equal(2, clientV2.Version);
        }
    }
}
