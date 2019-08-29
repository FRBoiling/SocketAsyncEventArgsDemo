using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;

namespace Lister13
{
    public partial class Form1 : Form
    {
        private Server m_socket;

        public Form1()
        {
            InitializeComponent();
            this.btnStart.Enabled = true;
            this.btnStop.Enabled = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            m_socket = new Server(300, 1024);
            m_socket.Init();
            m_socket.ServerStopedEvent += m_socket_ServerStopedEvent;

        }

        private void btnStart_Click(object sender, EventArgs e)
        {
           
            string portStr = this.textBoxPort.Text;
            if (string.IsNullOrEmpty(portStr))
            {
                MessageBox.Show("请输入监听端口", "提示", MessageBoxButtons.OK);
                return;
            }
            else
            {

                int port = int.Parse(portStr);
                bool isStart = m_socket.Start(new IPEndPoint(IPAddress.Any, port));
                if (isStart)
                {
                    this.btnStart.Enabled = false;
                    this.btnStop.Enabled = true;
                    this.textBoxRece.AppendText("服务器已启动 \r\n");
                    m_socket.ClientNumberChange += m_socket_ClientNumberChange;
                    m_socket.ReceiveClientData += m_socket_ReceiveClientData;
                }
            }

        }
        /// <summary>
        /// server停止
        /// </summary>
        void m_socket_ServerStopedEvent()
        {
            this.textBoxRece.AppendText("服务器已停止" + "\r\n");
            this.btnStop.Enabled = false;
            this.btnStart.Enabled = true;

           

        }
        public delegate void MessageHandle(string msg);
        /// <summary>
        /// 收到来自client的消息
        /// </summary>
        /// <param name="socketAsyncEventarfs"></param>
        /// <param name="token"></param>
        /// <param name="buff"></param>
        void m_socket_ReceiveClientData(AsyncUserToken token, byte[] buff)
        {
            // Console.WriteLine("收到client来的数据");
            string msg = Encoding.UTF8.GetString(buff);
            //   Console.WriteLine("Rece:{0}", msg);
            this.BeginInvoke(new MessageHandle(UpdateRece), msg);
            ///发送消息
            byte[] message = Encoding.UTF8.GetBytes("success:" + msg);
            bool issent = m_socket.SendMessage(token, message);
            if (issent)
            {
                this.BeginInvoke(new MessageHandle(UpdateSend), "success:"+msg);
            }

            //转发到另外一个client上,如果收到心跳包heartbeat,则不转发，直接serverhui
            if (msg =="heartbeat")
            {
                return;
            }
            //将下面代码封装一下。
            if (m_socket.ClientList.Count > 1)
            {
                AsyncUserToken ntoken = m_socket.ClientList.Find(s => s.IPAddress != token.IPAddress);
                if (ntoken != null)
                {
                    m_socket.SendMessage(ntoken, buff);
                }
            }
        }

        private void UpdateSend(string msg)
        {
            this.textBoxSend.AppendText("Send: " + msg + "\r\n");
        }

        private void UpdateRece(string msg)
        {
            this.textBoxRece.AppendText("Rece:" + msg + "\r\n");
        }
        /// <summary>
        /// client 连接或断开时
        /// </summary>
        /// <param name="num"></param>
        /// <param name="token"></param>
        void m_socket_ClientNumberChange(int num, AsyncUserToken token)
        {
            if (num > 0)
            {
                string msg = string.Format("{0},{1}已连接", num, token.Remote.ToString());
                this.BeginInvoke(new MessageHandle(UpdateRece), msg);

            }
            if (num < 0)
            {
                string msg = string.Format("{0},{1}断开连接", num, token.Remote.ToString());
                this.BeginInvoke(new MessageHandle(UpdateRece), msg);
            }

        }

        private void btnStop_Click(object sender, EventArgs e)
        {

            this.m_socket.Stop();
        }

        private void btnSend_Click(object sender, EventArgs e)
        {

            if (m_socket.ClientList.Count > 0)
            {
                AsyncUserToken token = null;
                // 查找要发送的client的socket对象。zhx
                token = m_socket.ClientList.Find(x => x.IPAddress.ToString() == "127.0.0.1");
                //foreach (AsyncUserToken usertoken in m_socket.ClientList)
                //{
                //    if (usertoken.IPAddress.ToString() == "192.168.1.158")
                //    {
                //        token = usertoken;
                //        break;
                //    }
                //}
                string message = this.send.Text.Trim();
                if (string.IsNullOrWhiteSpace(message))
                {

                    MessageBox.Show("请输入要发送的内容", "提示", MessageBoxButtons.OK);
                    return;
                }
                else
                {
                    if (token != null)
                    {
                        byte[] messageBuffer = Encoding.UTF8.GetBytes(this.send.Text);
                        bool issent = m_socket.SendMessage(token, messageBuffer);
                        if (issent)
                        {
                            this.textBoxSend.AppendText("Send: " + message + "\r\n");
                        }
                    }
                }


            }
            else
            {
                MessageBox.Show("未监听到连接，无法发送", "提示", MessageBoxButtons.OK);
                return;
            }

        }
    }
}
