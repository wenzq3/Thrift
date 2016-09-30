using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thrift.IDLHelp;
using Thrift.Test.Entity;

namespace Thrift.Test
{

    public class BaseGame {
        /// <summary>
        /// 操作是否成功
        /// </summary>
        public bool Successed { get; set; }

        /// <summary>
        /// 操作失败或异常返回消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 状态码：0：表示操作成功
        /// </summary>
        public int StatusCode { get; set; }
    }

    public class Game
    {
        public int GameID { get; set; }
    }

    public class Game2: BaseGame
    {
        public int GameID { get; set; }
    }

    public interface IGameService2
    {
        Result<Game> get1();
        Game2 get2();
    }

    public class GameService2 : IGameService2
    {
        public Result<Game> get1()
        {
            return null;
        }

        public Game2 get2()
        {
            return null;
        }
    }

    /// <summary>
    /// 测试thrift服务接口定义
    /// </summary>
    public interface IGameService
    {
        string ss();
        List<int> aa();
        bool bb();
        int cc();
        GameInfo Get(int gameId);

        void Ping();


        List<GameInfo> GetALL();

    }
}
