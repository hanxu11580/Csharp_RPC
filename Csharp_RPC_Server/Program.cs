using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Csharp_RPC_Server
{
    class Program
    {

        private static Socket listenSocket;

        static void Main(string[] args)
        {
            listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ipadr = IPAddress.Parse("0.0.0.0");
            IPEndPoint iPEnd = new IPEndPoint(ipadr, 8888);
            listenSocket.Bind(iPEnd);
            listenSocket.Listen(0);

            Thread t = new Thread(Execute);
            // 后台线程随着主线程结束而结束
            t.IsBackground = true;
            t.Start();

            Console.WriteLine("Server Lauch");
            Console.Read();
        }

        private static void Execute()
        {
            while (true)
            {
                Socket client = listenSocket.Accept();

                Thread cThread = new Thread(SingleClientExecute);
                cThread.IsBackground = true;
                cThread.Start(client);
            }
        }


        private static void SingleClientExecute(object obj)
        {
            Socket client = obj as Socket;

            byte[] rBuffer = new byte[1024];
            int rCount = client.Receive(rBuffer);
            if (rCount > 0)
            {
                MemoryStream ms = new MemoryStream(rBuffer);
                BinaryReader br = new BinaryReader(ms);

                // RPC协议格式
                // 接口名长度(1字节)、接口名， 方法名长度(1字节)，方法名，参数长度(1字节)，返回类型，
                // 参数序列(string类型前面有字符串长度)，int类型是32位
                // 接口名
                int interfaceLen = br.ReadByte();
                byte[] interfaceBytes = br.ReadBytes(interfaceLen);
                string interfaceName = Encoding.UTF8.GetString(interfaceBytes);
                // 方法名
                int methodNameLen = br.ReadByte();
                byte[] methodNameBytes = br.ReadBytes(methodNameLen);
                string methodName = Encoding.UTF8.GetString(methodNameBytes);
                // 参数长度
                int argsLen = br.ReadByte();
                // 返回类型
                int returnType = br.ReadByte();
                // 解析argsLen个参数
                List<object> argsList = new List<object>();
                for (int i = 0; i < argsLen; i++)
                {
                    int singleArgType = br.ReadByte();
                    if (singleArgType == 1)
                    { // int
                        byte[] intBytes = br.ReadBytes(4);
                        int value = Bytes2Int(intBytes);
                        argsList.Add(value); //涉及装箱
                    }
                    else if (singleArgType == 2)
                    { // bool
                        bool value = br.ReadByte() == 1;
                        argsList.Add(value);
                    }
                    else if (singleArgType == 3)
                    { // string
                        int stringBytesLen = br.ReadByte();
                        byte[] stringBytes = br.ReadBytes(stringBytesLen);
                        string value = Encoding.UTF8.GetString(stringBytes);
                        argsList.Add(value);
                    }
                }


                // 下面通过得到的信息，去调用相关方法
                Type interfaceType = Type.GetType(MethodBase.GetCurrentMethod().DeclaringType.Namespace + "." + interfaceName);
                if (interfaceType == null)
                { //没有这个接口
                    throw new Exception();
                }

                Type subClassType = null;
                var types = Assembly.GetExecutingAssembly().GetTypes();
                foreach (var type in types)
                {
                    if (interfaceType.IsAssignableFrom(type) && type != interfaceType)
                    {
                        subClassType = type;
                        break;
                    }
                }

                if (subClassType == null) throw new Exception();

                MethodInfo[] methodInfos = subClassType.GetMethods();
                MethodInfo method = null;
                foreach (var mi in methodInfos)
                {
                    if (mi.Name == methodName)
                    {
                        method = mi;
                        break;
                    }
                }

                if (method == null) throw new Exception();

                // 执行
                object instance = Activator.CreateInstance(subClassType);

                // 得到结果 用于发送
                object res = method.Invoke(instance, argsList.ToArray());
                if (returnType == 1)
                { // int
                    int value = Convert.ToInt32(res);
                    byte[] resBytes = Int2Bytes(value);
                    client.Send(resBytes);
                    return;
                }
                else if (returnType == 2)
                { // bool
                    bool value = Convert.ToBoolean(res);
                    byte[] boolBytes = new byte[1] { value ? (byte)1 : (byte)0 };
                    client.Send(boolBytes);
                    return;
                }
                else if (returnType == 3)
                { // string
                    List<byte> sendBytes = new List<byte>();
                    byte[] strBytes = Encoding.UTF8.GetBytes(res.ToString());
                    sendBytes.Add((byte)strBytes.Length);
                    sendBytes.AddRange(strBytes);
                    client.Send(sendBytes.ToArray());
                    return;
                }
            }
        }


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
    }
}
