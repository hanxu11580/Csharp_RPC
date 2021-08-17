using System;

namespace Csharp_RPC_Client
{
    class Program
    {
        static void Main(string[] args)
        {

            MyUserInfoProxy myUserInfoProxy = new MyUserInfoProxy();
            bool loginSucc = myUserInfoProxy.Login("ccc", "123");
            if (loginSucc)
            {
                Console.WriteLine("Login Succ");
            }
            else
            {
                Console.WriteLine("Login Failed");
            }


            Console.ReadLine();
        }
    }
}
