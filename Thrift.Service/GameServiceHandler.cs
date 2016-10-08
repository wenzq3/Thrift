using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thrift.Test;
using ThriftTest;

namespace Thrift.Service
{
    public class GameServiceHandler : GameThriftService.Iface
    {

        private GameService2 gameService = new Thrift.Test.GameService2();

        public Thrift_Test_Game2 get2()
        {
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
