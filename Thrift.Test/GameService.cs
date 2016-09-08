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
        public void Ping()
        { }

        public GameInfo Get(int gameId)
        {
            return new GameInfo() { GameID = gameId, GameName = "双扣" };
        }

        public async Task<List<GameInfo>> GetALL()
        {
            return await Task.Run<List<GameInfo>>(() =>
            {
                return new List<Entity.GameInfo>() {
                    new GameInfo() { GameID = 11, GameName = "双扣" } ,
                new GameInfo() { GameID = 22, GameName = "斗地主" }
                };
            });

        }
    }
}
