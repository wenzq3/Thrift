using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thrift.Test.Entity;

namespace Thrift.Test
{
     /// <summary>
    /// 测试thrift服务接口定义
    /// </summary>
    public interface IGameService
    {
        void Ping();
        GameInfo Get(int gameId);

       Task<List<GameInfo>> GetALL();

    }
}
