using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace TypeUnifier.Test
{
    public class StaticTest
    {
        public interface IUser { }
        public interface IUserSearchCriteria { }
        public class User { }
        public class UserSearchCriteria { }

        public interface IUserService
        {
            [Static] IUser GetUser(int id);
            [Static] IUser GetUser(string name);
            [Static] IUser GetUser(byte[] hash);
            [Static] IUser GetUser(IUserSearchCriteria criteria);
            [method: Static] event EventHandler OnUserNotFound;
            string ServiceName { [Static]get; [Static]set; }
            [Static] void AddUser(IUser user);
        }

        public class UserService
        {
            public static User GetUser(int id)
            {
                if (id > 0) return new User();
                OnUserNotFound(null, EventArgs.Empty);
                return null;
            }

            protected internal static User GetUser(byte[] hash) => new User();
            public static User GetUser(UserSearchCriteria criteria) => new User();
            public static event EventHandler OnUserNotFound;
            public static string ServiceName { get; set; }
            public static void AddUser(User user) { }
            public static void AddUser(IUser user) { }
        }

        [Fact]
        public void Not_Implemented_Or_Non_Public_Static_Method()
        {
            var node = new Node("test");
            node.AddMapping<IUser, User>();
            node.AddMapping<IUserSearchCriteria, UserSearchCriteria>();
            node.AddMapping<IUserService, UserService>();
            var dispatcher = new Dispatcher(node);

            Assert.Throws<MethodNotImplementedException>(() => dispatcher.For<IUserService>(node.Id).GetUser(string.Empty));
            Assert.Throws<MethodNotImplementedException>(() => dispatcher.For<IUserService>(node.Id).GetUser(Array.Empty<byte>()));
        }

        [Fact]
        public void Not_Implemented_Before_Add_Mapping()
        {
            var node = new Node("test");
            Func<IDispatcher, IUser> getUserByCriteria = d => d.For<IUserService>(node.Id).GetUser((IUserSearchCriteria)null);

            var dispatcher = new Dispatcher(node);
            Assert.Throws<NodeNotImplementedException>(() => getUserByCriteria(dispatcher));

            node.AddMapping<IUserService, UserService>();
            dispatcher = new Dispatcher(node);
            Assert.Throws<MethodNotImplementedException>(() => getUserByCriteria(dispatcher));

            node.AddMapping<IUserSearchCriteria, UserSearchCriteria>();
            dispatcher = new Dispatcher(node);
            Assert.Throws<MethodNotImplementedException>(() => getUserByCriteria(dispatcher));

            node.AddMapping<IUser, User>();
            dispatcher = new Dispatcher(node);
            IUser user = getUserByCriteria(dispatcher);
            Assert.NotNull(user);
        }

        [Fact]
        public void Ambiguous_Method_Call()
        {
            var node = new Node("test");
            node.AddMapping<IUserService, UserService>();
            var dispatcher = new Dispatcher(node);
            dispatcher.For<IUserService>(node.Id).AddUser(null);

            node.AddMapping<IUser, User>();
            dispatcher = new Dispatcher(node);
            Assert.Throws<AmbiguousMethodException>(() => dispatcher.For<IUserService>(node.Id).AddUser(null));
        }

        [Fact]
        public void Static_Event_And_Property()
        {
            var node = new Node("test");
            node.AddMapping<IUser, User>();
            node.AddMapping<IUserService, UserService>();
            var dispatcher = new Dispatcher(node);

            var staticEventHit = false;
            dispatcher.For<IUserService>(node.Id).OnUserNotFound += delegate { staticEventHit = true; };
            IUser user = dispatcher.For<IUserService>(node.Id).GetUser(-1);
            Assert.True(staticEventHit);

            var serviceName = "TestUserSerivce";
            dispatcher.For<IUserService>(node.Id).ServiceName = serviceName;
            Assert.Equal(serviceName, UserService.ServiceName);
        }
    }
}
