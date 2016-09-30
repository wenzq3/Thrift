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

        public List<int> aa()
        {
            return default(List<int>);
        }
        public bool bb()
        {
            return default(bool);
        }
        public int cc()
        {
            return default(int);
        }

        public string ss()
        {
            return default(string);
        }

        public Thrift_Test_Entity_GameInfo Get(int gameId)
        {

            var gameInfo = gameService.Get(gameId);


            if (gameInfo == null)
                return new Thrift_Test_Entity_GameInfo() { ThriftSuccessed = false, ThriftMessage = "为空" };

            return gameInfo.MapTo<Thrift_Test_Entity_GameInfo>();
        }

        public List<Thrift_Test_Entity_GameInfo> GetALL()
        {
            List<Thrift_Test_Entity_GameInfo> result = new List<Thrift_Test_Entity_GameInfo>();


            var list = gameService.GetALL();
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
