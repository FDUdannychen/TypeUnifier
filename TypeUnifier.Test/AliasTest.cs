using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace TypeUnifier.Test
{
    public class AliasTest
    {
        public interface IData
        {
            [Constructor] IData New();
            [Alias("Process", "Processing")] void Process();
            [Static] [Alias("ConvertFromBytes")] IData Convert(byte[] data);
        }

        public class DataV1
        {
            public void Process() { }
            public static DataV1 Convert(byte[] data) => null;
        }

        public class DataV2
        {
            public void Processing() { }
            public static DataV2 ConvertFromBytes(byte[] data) => null;
        }

        [Fact]
        public void Alias_Names_Matching()
        {
            var node1 = new Node("v1");
            node1.AddMapping<IData, DataV1>();
                        
            var node2 = new Node("v2");
            node2.AddMapping<IData, DataV2>();

            var dispatcher = new Dispatcher(new[] { node1, node2 });

            var data1 = dispatcher.For<IData>(node1.Id).New();
            data1.Process();
            Assert.Throws<MethodNotImplementedException>(() => data1.Convert(null));

            var data2 = dispatcher.For<IData>(node2.Id).New();
            data2.Process();
            data2.Convert(null);
        }
    }
}
