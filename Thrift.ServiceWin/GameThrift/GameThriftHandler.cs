using GameThrift;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thrift.ServiceWin
{
    public class GameThriftHandler : GameService.Iface
    {
        public GameInfo GetGameInfo(int GameId)
        {
            return new GameInfo() { GameId = GameId };
        }
    }
}
