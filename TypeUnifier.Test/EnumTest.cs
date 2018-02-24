using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace TypeUnifier.Test
{
    public class EnumTest
    {
        public enum EmployeeType
        {
            FullTime = 0,
            Contract = 2
        }

        public enum EmployeeTypeV1
        {
            FullTime = 0,
            PartTime
        }

        public interface IEmployee
        {
            [Constructor] IEmployee New(EmployeeType type);
            EmployeeType? Type { get; }
            void ChangeEmployeeType(EmployeeType? type);
            void ChangeToPartTime();
        }

        public class EmployeeV1
        {
            public EmployeeTypeV1? Type { get; private set; }

            public EmployeeV1(EmployeeTypeV1 type)
            {
                this.Type = type;
            }

            public void ChangeEmployeeType(EmployeeTypeV1? type)
            {
                this.Type = type;
            }

            public void ChangeToPartTime()
            {
                this.Type = EmployeeTypeV1.PartTime;
            }
        }
                
        static INode _node = GetNode();
        static IDispatcher _dispatcher = new Dispatcher(_node);
        static INode GetNode()
        {
            var node = new Node("v1");
            node.AddMapping<IEmployee, EmployeeV1>();
            node.AddMapping<EmployeeType, EmployeeTypeV1>();
            return node;
        }

        [Fact]
        public void Enum_Mapping()
        {
            var employeeDef = _dispatcher.For<IEmployee>(_node.Id);
            Assert.Throws<InvalidCastException>(() => employeeDef.New(EmployeeType.Contract));

            var employee = employeeDef.New(EmployeeType.FullTime);
            Assert.Equal(EmployeeType.FullTime, employee.Type);

            employee.ChangeEmployeeType(null);
            Assert.Null(employee.Type);

            employee.ChangeToPartTime();
            Assert.Throws<InvalidCastException>(() => employee.Type);
        }
    }
}
