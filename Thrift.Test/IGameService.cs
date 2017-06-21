using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thrift.IDLHelp;
using Thrift.Test.Entity;


namespace Thrift.Test
{
    public interface IGameService
    {
        string GetGuid(string guid);

        void SetGuid(string guid);

        GameInfoThrift GetGameInfo();
    }
}
