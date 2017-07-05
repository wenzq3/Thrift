//命令空间
namespace csharp GameThrift 

//结构体
struct GameInfo
{
1: optional i32 GameId
2: optional string  GameName
}
//服务类名
service GameService
{
//一个获取方法
GameInfo GetGameInfo(1:i32 GameId)
}
