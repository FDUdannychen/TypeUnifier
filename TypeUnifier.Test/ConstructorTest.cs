using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace TypeUnifier.Test
{
    public class ConstructorTest
    {
        public interface IAddress
        {
            [Constructor] IAddress New(string city);
            [Constructor] Address IncorrectReturn(string city);

            string City { get; }
        }

        public class Address
        {
            public Address(string city)
            {
                this.City = city;
            }

            public string City { get; set; }
        }

        public class Address2
        {
            public Address2(string city)
            {
                this.City = city;
            }

            public string City { get; set; }
        }

        public interface IPerson
        {
            [Constructor] IPerson New();
            [Constructor] IPerson New(string name);
            [Constructor] IPerson New(string name, IAddress address);

            string Name { get; }
            IAddress Address { get; }
        }

        class Person
        {
            protected internal Person(string name)
                : this(name, null)
            { }

            public Person(string name, Address address)
            {
                this.Name = name;
                this.Address = address;
            }

            public string Name { get; }
            public Address Address { get; }
        }

        [Fact]
        public void Constructor_Not_Exist_Or_Non_Public()
        {
            var node = new Node("test");
            node.AddMapping<IPerson, Person>();
            node.AddMapping<IAddress, Address>();
            var dispatcher = new Dispatcher(node);

            Assert.Throws<MethodNotImplementedException>(() => dispatcher.For<IPerson>(node.Id).New());
            Assert.Throws<MethodNotImplementedException>(() => dispatcher.For<IPerson>(node.Id).New("Tom"));
        }

        [Fact]
        public void Constructor_Return_Wrong_Type()
        {
            var node = new Node("test");
            node.AddMapping<IAddress, Address>();
            var dispatcher = new Dispatcher(node);
            Assert.Throws<ConstructorReturnTypeException>(() => dispatcher.For<IAddress>(node.Id).IncorrectReturn("Shanghai"));
        }

        [Fact]
        public void Constructor_Called_Once()
        {
            var node = new Node("test");
            node.AddMapping<IPerson, Person>();
            node.AddMapping<IAddress, Address>();
            var dispatcher = new Dispatcher(node);

            Assert.Throws<NotConstructedException>(() => dispatcher.For<IPerson>(node.Id).Name);
            Assert.Throws<MultipleConstructionException>(() => dispatcher.For<IPerson>(node.Id).New("test", null).New("test", null));
        }

        [Fact]
        public void Constructor_With_Adapted_Type()
        {
            var node = new Node("test");
            node.AddMapping<IPerson, Person>();
            node.AddMapping<IAddress, Address>();
            var dispatcher = new Dispatcher(node);

            var city = "Shanghai";
            IAddress address = dispatcher.For<IAddress>(node.Id).New(city);
            Assert.Equal(city, address.City);

            IPerson person = dispatcher.For<IPerson>(node.Id).New("Tom", address);
            Assert.Equal(address.City, person.Address.City);
        }

        [Fact]
        public void Constructor_With_Wrong_Adapted_Type()
        {
            var node1 = new Node("v1");
            node1.AddMapping<IPerson, Person>();
            node1.AddMapping<IAddress, Address>();
            
            var node2 = new Node("v2");
            node2.AddMapping<IPerson, Person>();
            node2.AddMapping<IAddress, Address2>();

            var dispatcher = new Dispatcher(new[] { node1, node2 });
            var city = "Shanghai";
            IAddress address = dispatcher.For<IAddress>(node2.Id).New(city);
            Assert.Throws<InvalidCastException>(() => dispatcher.For<IPerson>(node1.Id).New("Tom", address));
        }
    }
}
