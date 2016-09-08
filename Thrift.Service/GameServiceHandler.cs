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
        private IGameService gameService = new Thrift.Test.GameService();

        public void Ping()
        { }

        public Thrift_Test_Entity_GameInfo Get(int gameId)
        {
            return gameService.Get(gameId).MapTo<Thrift_Test_Entity_GameInfo>();
        }

        public List<Thrift_Test_Entity_GameInfo> GetALL()
        {

            List<Thrift_Test_Entity_GameInfo> result = new List<Thrift_Test_Entity_GameInfo>();


            var list = gameService.GetALL().Result;
            if (list == null) return result;

            //  return list.MapTo<List<Thrift_BLL_Entity_GameInfo>>();

            result.AddRange(list.Select(q => new Thrift_Test_Entity_GameInfo
            {
                GameID = q.GameID,
                GameName = q.GameName
            }));


            return result;
        }
    }
}
