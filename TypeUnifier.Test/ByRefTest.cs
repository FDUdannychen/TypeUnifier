using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace TypeUnifier.Test
{
    public class ByRefTest
    {
        public interface IService
        {
            [Constructor] IService New();
            [Static] void CreateUser(string name, out IUser user);
            void ModifyUser(string newName, ref IUser user, out string oldName);
        }

        public class Service
        {
            public static void CreateUser(string name, out User user)
            {
                user = new User(name);
            }

            public void ModifyUser(string newName, ref User user, out string oldName)
            {
                oldName = user.Name;
                user.Name = newName;
            }
        }

        public interface IUser
        {
            string Name { get; }
        }

        public class User
        {
            public string Name { get; set; }
            public User(string name) => this.Name = name;
        }

        [Fact]
        public void Argument_By_Ref()
        {
            var node = new Node("test");
            node.AddMapping<IService, Service>();
            node.AddMapping<IUser, User>();
            var dispatcher = new Dispatcher(node);

            var name = "OldName";
            dispatcher.For<IService>(node.Id).CreateUser(name, out IUser user);
            Assert.Equal(name, user.Name);

            var newName = "NewName";
            var service = dispatcher.For<IService>(node.Id).New();
            service.ModifyUser(newName, ref user, out var oldName);
            Assert.Equal(name, oldName);
            Assert.Equal(newName, user.Name);
        }
    }
}
