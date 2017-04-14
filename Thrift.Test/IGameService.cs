using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thrift.IDLHelp;
using Thrift.Test.Entity;

namespace Thrift.Test.My2
{
    public class Game1
    {
        public string abc { get; set; }
    }

    public class Game22
    {
        public string abc { get; set; }
    }
}
namespace Thrift.Test.My
{

    public class BaseGame
    {
        /// <summary>
        /// 操作是否成功
        /// </summary>
        public bool Status { get; set; }

        /// <summary>
        /// 操作失败或异常返回消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 状态码：0：表示操作成功
        /// </summary>
        public int Code { get; set; }
    }

    public class Game1 : BaseGame
    {
        public int GameID { get; set; }
    }

    public class Game2 : BaseGame
    {
        public int GameID { get; set; }

        public Game3 game3 { get; set; }

        public List<Game4> game4 { get; set; }

        public Dictionary<Game4, Game5> gg { get; set; }

        public Game1 g1 { get; set; }

     //   public Thrift.Test.My2.Game1 g11 { get; set; }

        public Thrift.Test.My2.Game22 g22 { get; set; }

        public string aaa { get; set; }
    }

    public class Game3 : BaseGame
    {
        public string GameName { get; set; }
    }

    public class Game4 : BaseGame
    {
        public string GameName { get; set; }
    }

    public class Game5 : BaseGame
    {
        public string GameName { get; set; }
    }

    public interface IGameService2
    {
        string get2(string guid);

        //bool get1(Game1 game);

        //List<Game5> get4();


        //List<int> getlist();
    }

    public class GameService2 : IGameService2
    {
       public  string get2(string guid)
        {
            System.Threading.Thread.Sleep(10);
            return guid;
        }

        public Game2 get2()
        {
            return null;
        }
        public bool get1(Game1 game)
        {
            return false;
        }

        public List<Game5> get4()
        {
            return null;
        }

        public List<int> getlist()
        {
            return null;
        }
    }
}
