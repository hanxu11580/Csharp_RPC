namespace Csharp_RPC_Server
{
    interface IUserInfo
    {
        bool Login(string account, string pwd);

        int Add(int a, int b);
    }

    public class MyUserInfo : IUserInfo
    {
        public int Add(int a, int b)
        {
            return a + b;
        }

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
