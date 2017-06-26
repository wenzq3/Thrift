using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thrift.Server;
using Thrift.Test;
using Thrift.Test.Thrift;

namespace Thrift.Service
{
    public class TestServiceHandler : MarshalByRefObject, ThriftTestThrift.Iface
    {

        public TestServiceHandler()
        {
        }

        public TestInfoThrift GetTestInfo()
        {
            return new TestInfoThrift() { };
        }

        public string GetGuid(string guid)
        {
            System.Threading.Thread.Sleep(10);
            return guid;
        }

        public void SetGuid(string guid)
        {
            throw new Exception("error");
        }
    }
}
