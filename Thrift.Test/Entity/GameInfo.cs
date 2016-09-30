using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thrift.IDLHelp;

namespace Thrift.Test.Entity
{
    public class GameInfo : ThriftBase
    {
        public int GameID { get; set; }
        public string GameName { get; set; }
    }
}
