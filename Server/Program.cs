using System;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using NLog.Fluent;

namespace Server
{
    class Program
    {
        private static TcpServer serverHandler;
        
        static void Main(string[] args)
        {
            serverHandler = new TcpServer(300, 1024);
            
            serverHandler.Init();
            serverHandler.ServerStopedEvent += m_socket_ServerStopedEvent;


            int port = 50000;
            bool isStart = serverHandler.Start(new IPEndPoint(IPAddress.Any, port));
            if (isStart)
            {
                Console.WriteLine("服务器已启动 \r\n");
                serverHandler.ClientNumberChange += m_socket_ClientNumberChange;
                serverHandler.ReceiveClientData += m_socket_ReceiveClientData;
            }


            Log.Info("Hello World!");
        }

        /// <summary>
        /// server停止
        /// </summary>
        static void m_socket_ServerStopedEvent()
        {
            Console.WriteLine("服务器已停止" + "\r\n");
        }

        /// <summary>
        /// client 连接或断开时
        /// </summary>
        /// <param name="num"></param>
        /// <param name="token"></param>
        static void m_socket_ClientNumberChange(int num, AsyncUserToken token)
        {
            string msg = "";
            if (num > 0)
            {
                msg = string.Format("{0},{1}已连接", num, token.Remote.ToString());
                Log.Info(msg);
            }

            if (num < 0)
            {
                msg = string.Format("{0},{1}断开连接", num, token.Remote.ToString());
            }

            Log.Info(msg);
        }

        /// <summary>
        /// 收到来自client的消息
        /// </summary>
        /// <param name="socketAsyncEventarfs"></param>
        /// <param name="token"></param>
        /// <param name="buff"></param>
        static void m_socket_ReceiveClientData(AsyncUserToken token, byte[] buff)
        {
            // Console.WriteLine("收到client来的数据");
            string msg = Encoding.UTF8.GetString(buff);
            //   Console.WriteLine("Rece:{0}", msg);
            Log.Info(msg);
            ///发送消息
            byte[] message = Encoding.UTF8.GetBytes("success:" + msg);
            bool issent = serverHandler.SendMessage(token, message);
            if (issent)
            {
                Log.Info("success:" + msg);
            }

            //转发到另外一个client上,如果收到心跳包heartbeat,则不转发，直接serverhui
            if (msg == "heartbeat")
            {
                return;
            }

            //将下面代码封装一下。
            if (serverHandler.ClientList.Count > 1)
            {
                AsyncUserToken ntoken = serverHandler.ClientList.Find(s => s.IPAddress != token.IPAddress);
                if (ntoken != null)
                {
                    serverHandler.SendMessage(ntoken, buff);
                }
            }
        }
    }
}