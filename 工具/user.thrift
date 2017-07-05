//引用

//命名空间
namespace csharp Thrift.Common.UserService
namespace java Thrift.Common.UserService
namespace cpp Thrift.Common.UserService

//**************************************************************************//

 //如果required标识的域没有赋值，thrift将报错
 //如果optional标识的域没有赋值，该域将不会被序列化传输
 //optional字段，需要将它的__isset值设为true(默认为true)，这样才能序列化并传输或者存储

//用户信息
struct UserInfo {
  1: required i64 UserID;
  2: string UserName;
  3: optional i32 Age;
  4: UserTypeEnum UserType;
  5: list<UserGameInfo> Games;
}

//用户类型
enum UserTypeEnum {
 ANDROID=1,    
 IOS = 2
 }   

 //游戏信息
struct UserGameInfo{
  1: required i32 GameID;
  2: string GameName;
}

//由于thrift不能返回为空，所以包装了一下
struct ResultUserInfo {
1:bool Status;
2:i32 Code;
3:string Message;
4:UserInfo Data;
}

//**************************************************************************//

//用户服务
service UserService {

//添加用户
bool AddUser(1:UserInfo user)                          
 
//获取用户信息
UserInfo GetUserInfo(1:i64 userID)

//获取用户信息2
ResultUserInfo GetUserInfo2(1:i64 userID)

// "oneway"标识符表示client发出请求后不必等待回复（非阻塞）直接进行下面的操作，
 // "oneway"方法的返回值必须是void
 oneway void Test()
 
 //加法
i32 Add(1:i32 x,2:i32 y)                                 
}

