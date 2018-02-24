using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace TypeUnifier.Test
{
    public class ComplexAdaptingTest
    {
        public interface IData
        {
            [Constructor] IData New(object value);
            object Value { get; }
        }

        public class Data
        {
            public object Value { get; }

            public Data(object value)
            {
                this.Value = value;
            }
        }

        public enum EnumDataAbs
        {
            Exist = 0,
            Gone = 1,
        }

        public enum EnumDataImpl
        {
            Exist = 0
        }

        public interface IDataService
        {
            [Constructor] IDataService New(IData[] data1, EnumDataAbs? data2, EnumDataAbs[] data3);
            [Constructor] IDataService New(IEnumerable<IData> data1, IEnumerable<EnumDataAbs> data2);
            [Constructor] IDataService New(ICollection<EnumDataAbs?[]> data);
            [Constructor] IDataService New(IList<IData[]> data);

            EnumDataAbs? Test(IData[] data1, EnumDataAbs? data2, EnumDataAbs[] data3);
            IData[] Test(IEnumerable<IData> data1, IEnumerable<EnumDataAbs> data2);
            EnumDataAbs?[] Test(ICollection<EnumDataAbs?[]> data);
            IEnumerable<IData> Test(IList<IEnumerable<IData>> data);

            [Static] EnumDataAbs? TestStatic(IData[] data1, EnumDataAbs? data2, EnumDataAbs[] data3);
            [Static] IData[] TestStatic(IEnumerable<IData> data1, IEnumerable<EnumDataAbs> data2);
            [Static] EnumDataAbs?[] TestStatic(ICollection<EnumDataAbs?[]> data);
            [Static] IEnumerable<IData> TestStatic(IList<IEnumerable<IData>> data);
        }

        public class DataService
        {
            public DataService(Data[] data1, EnumDataImpl? data2, EnumDataImpl[] data3) { }
            public DataService(IEnumerable<Data> data1, IEnumerable<EnumDataImpl> data2) { }
            public DataService(ICollection<EnumDataImpl?[]> data) { }
            public DataService(IList<Data[]> data) { }

            public EnumDataImpl? Test(Data[] data1, EnumDataImpl? data2, EnumDataImpl[] data3) => data2;
            public Data[] Test(IEnumerable<Data> data1, IEnumerable<EnumDataImpl> data2) => data1.ToArray();
            EnumDataImpl?[] Test(ICollection<EnumDataImpl?[]> data) => data.FirstOrDefault();
            public IEnumerable<Data> Test(IList<IEnumerable<Data>> data) => data.FirstOrDefault();

            public static EnumDataImpl? TestStatic(Data[] data1, EnumDataImpl? data2, EnumDataImpl[] data3) => data2;
            static Data[] TestStatic(IEnumerable<Data> data1, IEnumerable<EnumDataImpl> data2) => data1.ToArray();
            public static EnumDataImpl?[] TestStatic(ICollection<EnumDataImpl?[]> data) => data.FirstOrDefault();
            public static IEnumerable<Data> TestStatic(IList<IEnumerable<Data>> data) => data.FirstOrDefault();
        }

        static INode _node = GetNode();
        static IDispatcher _dispatcher = new Dispatcher(_node);
        static INode GetNode()
        {
            var node = new Node("v1");
            node.AddMapping<IData, Data>();
            node.AddMapping<EnumDataAbs, EnumDataImpl>();
            node.AddMapping<IDataService, DataService>();
            return node;
        }

        [Fact]
        public void Constructor_Adapting()
        {
            IData data = _dispatcher.For<IData>(_node.Id).New(123);
            EnumDataAbs enumExist = EnumDataAbs.Exist;
            EnumDataAbs enumGone = EnumDataAbs.Gone;

            IDataService service = _dispatcher.For<IDataService>(_node.Id);

            service.New(new[] { data }, null, new[] { enumExist });
            service.New(new[] { data }, enumExist, new[] { enumExist });
            Assert.Throws<InvalidCastException>(() => service.New(new[] { data }, enumGone, new[] { enumExist }));

            service.New(new[] { data }, new[] { enumExist });
            Assert.Throws<InvalidCastException>(() => service.New(new[] { data }, new[] { enumGone }));

            service.New(new List<EnumDataAbs?[]> { new EnumDataAbs?[] { enumExist } });
            Assert.Throws<InvalidCastException>(() => service.New(new List<EnumDataAbs?[]> { new EnumDataAbs?[] { enumGone } }));

            service.New(new List<IData[]> { new[] { data } });
            service.New(new List<IData[]> { null });
        }

        [Fact]
        public void Instance_Method_Adapting()
        {
            IData data = _dispatcher.For<IData>(_node.Id).New(123);
            EnumDataAbs enumExist = EnumDataAbs.Exist;
            EnumDataAbs enumGone = EnumDataAbs.Gone;

            IDataService service = _dispatcher.For<IDataService>(_node.Id).New(null, null);

            Assert.Null(service.Test(new[] { data }, null, new[] { enumExist }));
            Assert.Equal(enumExist, service.Test(new[] { data }, enumExist, new[] { enumExist }));
            Assert.Throws<InvalidCastException>(() => service.Test(new[] { data }, enumGone, new[] { enumExist }));

            Assert.Equal(data.Value, service.Test(new[] { data }, new[] { enumExist }).First().Value);
            Assert.Throws<InvalidCastException>(() => service.Test(new[] { data }, new[] { enumGone }));

            Assert.Throws<MethodNotImplementedException>(() => service.Test(new List<EnumDataAbs?[]> { new EnumDataAbs?[] { enumExist } }));

            service.Test(new List<IEnumerable<IData>> { new[] { data } });
            service.Test(new List<IEnumerable<IData>> { null });
        }

        [Fact]
        public void Static_Method_Adapting()
        {
            IData data = _dispatcher.For<IData>(_node.Id).New(123);
            EnumDataAbs enumExist = EnumDataAbs.Exist;
            EnumDataAbs enumGone = EnumDataAbs.Gone;

            IDataService service = _dispatcher.For<IDataService>(_node.Id);

            Assert.Null(service.TestStatic(new[] { data }, null, new[] { enumExist }));
            Assert.Equal(enumExist, service.TestStatic(new[] { data }, enumExist, new[] { enumExist }));
            Assert.Throws<InvalidCastException>(() => service.TestStatic(new[] { data }, enumGone, new[] { enumExist }));

            Assert.Throws<MethodNotImplementedException>(() => service.TestStatic(new[] { data }, new[] { enumExist }));
            
            service.TestStatic(new List<EnumDataAbs?[]> { new EnumDataAbs?[] { enumExist } });

            service.TestStatic(new List<IEnumerable<IData>> { new[] { data } });
            service.TestStatic(new List<IEnumerable<IData>> { null });
        }
    }
}
