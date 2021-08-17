namespace Csharp_RPC_Client
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Net.Sockets;
    using System.Net;

    interface IUserInfo
    {
        bool Login(string account, string pwd);
    }

    public class MyUserInfoProxy : IUserInfo
    {
        // 默认小端

        private static byte[] Int2Bytes(int val)
        {
            byte[] res = new byte[4];
            res[0] = (byte)(val >> 0);
            res[1] = (byte)(val >> 8);
            res[2] = (byte)(val >> 16);
            res[3] = (byte)(val >> 24);
            return res;
        }

        private static int Bytes2Int(byte[] bytes)
        {
            int res = (bytes[0] | (bytes[1] << 8) | (bytes[2] << 16) | (bytes[3] << 24));
            return res;
        }


        public bool Login(string account, string pwd)
        {
            byte[] sendBytes = EncodeSendPackage(nameof(IUserInfo), "Login", 2, TypeEnum.Bool, new List<ArgTypeInfo>() {
                new ArgTypeInfo()
                {
                    argType = TypeEnum.String,value = account
                },
                new ArgTypeInfo()
                {
                    argType = TypeEnum.String,value = pwd
                }
            });

            Socket sk = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sk.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8888));
            sk.Send(sendBytes);
            Console.WriteLine("Send字节数" + sendBytes.Length);

            byte[] rBuffer = new byte[1024];
            int rCount = sk.Receive(rBuffer);
            if (rCount > 0)
            {
                return rBuffer[0] == 1;
            }
            throw new Exception();
        }

        private static byte[] EncodeSendPackage(string interfaceName, string methodName, int argLen, TypeEnum returnType, List<ArgTypeInfo> argTypeInfos
            )
        {
            // 接口名称
            List<byte> byteList = new List<byte>();
            byte[] interfaceBytes = Encoding.UTF8.GetBytes(interfaceName);
            byteList.Add((byte)interfaceBytes.Length);
            byteList.AddRange(interfaceBytes);
            // 方法名称
            byte[] methodNameBytes = Encoding.UTF8.GetBytes(methodName);
            byteList.Add((byte)methodNameBytes.Length);
            byteList.AddRange(methodNameBytes);

            // 参数个数
            byteList.Add((byte)argLen);

            // 返回类型
            byteList.Add((byte)returnType);

            // 参数列表
            foreach (ArgTypeInfo ati in argTypeInfos)
            {
                byteList.Add((byte)ati.argType);
                if (ati.argType == TypeEnum.String)
                {
                    string value = ati.value as string;
                    byte[] stringBytes = Encoding.UTF8.GetBytes(value);
                    byteList.Add((byte)stringBytes.Length);
                    byteList.AddRange(stringBytes);
                }
                else if (ati.argType == TypeEnum.Int)
                {
                    int value = Convert.ToInt32(ati.value);
                    byte[] intBytes = Int2Bytes(value);
                    byteList.AddRange(intBytes);
                }
                else if (ati.argType == TypeEnum.Bool)
                {
                    bool value = Convert.ToBoolean(ati.value);
                    byte boolBytes = value ? (byte)1 : (byte)0;
                    byteList.Add(boolBytes);
                }
            }

            return byteList.ToArray();
        }
    }


    public class ArgTypeInfo
    {
        public TypeEnum argType { get; set; }

        public object value { get; set; }
    }

    public enum TypeEnum
    {
        Void = 0,
        Int,
        Bool,
        String
    }
}
