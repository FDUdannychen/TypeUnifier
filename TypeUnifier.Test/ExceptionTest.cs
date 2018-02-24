using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace TypeUnifier.Test
{
    public class ExceptionTest
    {
        public interface IServiceException : IException
        {
            string Description { get; }
        }

        public interface IPermissionDeniedException : IServiceException
        {
            int Code { get; }
        }

        public class ServiceException : Exception
        {
            public ServiceException(string message) : base(message) { }
            public string Description { get; set; }
        }

        public class PermissionDeniedException : ServiceException
        {
            public PermissionDeniedException(string message) : base(message) { }
            public int Code { get; set; }
        }

        public interface ISomeService
        {
            [Constructor] ISomeService New(bool willThrowException);
            void Work();
            [Static] void StartAll();
        }

        public class SomeService
        {
            public SomeService(bool willThrowException)
            {
                if (willThrowException)
                    throw new ServiceException(nameof(ServiceException)) { Description = nameof(ServiceException) };
            }

            public void Work()
            {
                throw new PermissionDeniedException(nameof(PermissionDeniedException))
                {
                    Code = typeof(PermissionDeniedException).GetHashCode(),
                    Description = nameof(PermissionDeniedException)
                };
            }

            public static void StartAll()
            {
                throw new ArgumentException();
            }
        }

        static INode _node = GetNode();
        static IDispatcher _dispatcher = new Dispatcher(_node);
        static INode GetNode()
        {
            var node = new Node("test");
            node.AddMapping<ISomeService, SomeService>();
            node.AddMapping<IServiceException, ServiceException>();
            node.AddMapping<IPermissionDeniedException, PermissionDeniedException>();
            return node;
        }

        [Fact]
        public void Exception_Should_Be_Wrapped()
        {
            var service = _dispatcher.For<ISomeService>(_node.Id);
            Assert.Throws<ArgumentException>(() => service.StartAll());

            var ex1 = Assert.Throws<Exception<IServiceException>>(() => service.New(true));
            Assert.Equal(nameof(ServiceException), ex1.Abstraction.Description);
            Assert.Equal(nameof(ServiceException), ex1.Abstraction.Message);
            Assert.Equal(nameof(ServiceException), ex1.Message);

            service = service.New(false);
            var ex2 = Assert.Throws<Exception<IPermissionDeniedException>>(() => service.Work());
            Assert.Equal(typeof(PermissionDeniedException).GetHashCode(), ex2.Abstraction.Code);
            Assert.Equal(nameof(PermissionDeniedException), ex2.Abstraction.Description);
            Assert.Equal(nameof(PermissionDeniedException), ex2.Abstraction.Message);
            Assert.Equal(nameof(PermissionDeniedException), ex2.Message);
        }
    }
}
