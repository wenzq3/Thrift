using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thrift.Test.Entity
{
    public class GameInfoThrift : ThriftResultBase
    {
        public GameInfo GameID { get; set; }
    }
}
