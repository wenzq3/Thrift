//����

//�����ռ�
namespace csharp Thrift.Common.UserService
namespace java Thrift.Common.UserService
namespace cpp Thrift.Common.UserService

//**************************************************************************//

 //���required��ʶ����û�и�ֵ��thrift������
 //���optional��ʶ����û�и�ֵ�����򽫲��ᱻ���л�����
 //optional�ֶΣ���Ҫ������__issetֵ��Ϊtrue(Ĭ��Ϊtrue)�������������л���������ߴ洢

//�û���Ϣ
struct UserInfo {
  1: required i64 UserID;
  2: string UserName;
  3: optional i32 Age;
  4: UserTypeEnum UserType;
  5: list<UserGameInfo> Games;
}

//�û�����
enum UserTypeEnum {
 ANDROID=1,    
 IOS = 2
 }   

 //��Ϸ��Ϣ
struct UserGameInfo{
  1: required i32 GameID;
  2: string GameName;
}

//����thrift���ܷ���Ϊ�գ����԰�װ��һ��
struct ResultUserInfo {
1:bool Status;
2:i32 Code;
3:string Message;
4:UserInfo Data;
}

//**************************************************************************//

//�û�����
service UserService {

//����û�
bool AddUser(1:UserInfo user)                          
 
//��ȡ�û���Ϣ
UserInfo GetUserInfo(1:i64 userID)

//��ȡ�û���Ϣ2
ResultUserInfo GetUserInfo2(1:i64 userID)

// "oneway"��ʶ����ʾclient��������󲻱صȴ��ظ�����������ֱ�ӽ�������Ĳ�����
 // "oneway"�����ķ���ֵ������void
 oneway void Test()
 
 //�ӷ�
i32 Add(1:i32 x,2:i32 y)                                 
}

