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
    public class GameServiceHandler : MarshalByRefObject, ThriftTestThrift.Iface
    {

        public GameServiceHandler()
        {
            Console.WriteLine("GameServiceHandler()");
        }

        private GameService2 gameService = new Thrift.Test.My.GameService2();

        public string get2(string msg)
        {
            //   if (msg == "2") throw new Exception("error");
            System.Threading.Thread.Sleep(10);
            return msg;
        }

        public string gettime()
        {
            throw new Exception("error");
            System.Threading.Thread.Sleep(10);
            return DateTime.Now.ToString();
        }
    }
}
