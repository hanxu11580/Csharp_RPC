namespace Csharp_RPC_Server
{
    interface IUserInfo
    {
        bool Login(string account, string pwd);
    }

    public class MyUserInfo : IUserInfo
    {
        public bool Login(string account, string pwd)
        {
            if (account == "abc" && pwd == "123")
            {
                return true;
            }
            return false;
        }
    }
}
