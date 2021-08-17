using System;

namespace Csharp_RPC_Client
{
    class Program
    {
        static void Main(string[] args)
        {
            MyUserInfoProxy myUserInfoProxy = new MyUserInfoProxy();
            bool loginSucc = myUserInfoProxy.Login("abc", "123");
            if (loginSucc)
            {
                Console.WriteLine("Login Succ");
            }
            else
            {
                Console.WriteLine("Login Failed");
            }

            int calculateRes = myUserInfoProxy.Add(1, 2);
            Console.WriteLine("1+2=" + calculateRes);

            Console.ReadLine();
        }
    }
}
