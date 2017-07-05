using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thrift.IDLHelp;

namespace Thrift.Test.Entity
{
    public class TestInfo
    {
        public int TestId { get; set; }
        public string TestName { get; set; }
        public List<int> Icon { get; set; }
    }
}

