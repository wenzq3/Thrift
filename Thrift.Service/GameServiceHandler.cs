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
    public class GameServiceHandler : ThriftTestThrift.Iface
    {

        public GameServiceHandler()
        {
            Console.WriteLine("GameServiceHandler()");
        }

        private GameService2 gameService = new Thrift.Test.GameService2();

        public Thrift_Test_Game2 get2()
        {
            //        Console.WriteLine(ServerEventHandler.GetClientIP());
            var data = gameService.get2();

            Thrift_Test_Game2 gg;

            if (data != null)
            {
                var result = data.MapTo<Thrift_Test_Game2>();
                result.Successed = true;
                return result;
            }
            else
                return new Thrift_Test_Game2() { Successed = false };
        }
    }
}
