using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thrift.Test.Entity;

namespace Thrift.Test
{
    public class GameService : IGameService
    {

        public string ss()
        {
            return null;
        }

        public List<int> aa()
        {
            return null;
        }

        public bool bb()
        {
            return false;
        }
        public int cc()
        {
            return 0;
        }


        public void Ping()
        { }

        public GameInfo Get(int gameId)
        {
            return new GameInfo() { GameID = gameId, GameName = "双扣" };
        }

        public List<GameInfo> GetALL()
        {
                return new List<Entity.GameInfo>() {
                    new GameInfo() { GameID = 11, GameName = "双扣" } ,
                new GameInfo() { GameID = 22, GameName = "斗地主" }
                };

        }
    }
}
