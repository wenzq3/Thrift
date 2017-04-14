using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thrift.Server;
using Thrift.Test;
using Thrift.Test.My;
using Thrift.Test.Thrift;

namespace Thrift.Service
{
    public class GameServiceHandler : ThriftTestThrift.Iface
    {

        public GameServiceHandler()
        {
            Console.WriteLine("GameServiceHandler()");
        }

        private GameService2 gameService = new Thrift.Test.My.GameService2();

        public string  get2(string msg)
        {
            System.Threading.Thread.Sleep(100);
            return msg;
        }
    }
}
