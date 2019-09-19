using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Net;
using System.Net.Sockets;


namespace Client14
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            string ip = this.textBoxIP.Text.Trim();
            string portS = this.textBoxPort.Text.Trim();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            string ip= this.textBoxIP.Text.Trim();
            string portS= this.textBoxPort.Text.Trim();
            if (string.IsNullOrWhiteSpace(ip) || string.IsNullOrWhiteSpace(portS))
            {
                MessageBox.Show("请输入端口和Ip","提示");
                return;
            }
            else 
            {
               SocketError socketError=  Request.Connect(ip, int.Parse(portS));
               if (socketError == SocketError.Success)
               {
                   
                   this.btnConnect.Enabled = false;
                   this.textBoxRece.AppendText("已连接到主机 \r\n");
                   Request.OnReceiveData += Request_OnReceiveData;
                   Request.OnServerClosed += Request_OnServerClosed;
                   Request.StartHeartbeat();
               }
            }

           // Request.Connect()
        }
        public delegate SocketError del();
        /// <summary>
        /// server 断开
        /// </summary>
        private void Request_OnServerClosed()
        {
            this.BeginInvoke(new MessageHandle(UpdateRece), "server 已断开" + "\r\n");
            
           Request.Disconnect();

       //SocketError socketError= Request.TryConnect();
       //if (socketError == SocketError.Success) 
       //{
       //    this.BeginInvoke(new MessageHandle(UpdateRece), "已再次连接到server " + "\r\n");
       //}

           del del3 = new del(Request.TryConnect);

           IAsyncResult iar2 = del3.BeginInvoke(Connect2Server, del3);
           
        }

        private void Connect2Server(IAsyncResult ar)
        {
          
            this.BeginInvoke(new MessageHandle(UpdateRece), "已连接服务器");

        }
        public delegate void MessageHandle(string msg);
        /// <summary>
        /// 收到数据
        /// </summary>
        /// <param name="message"></param>
        private void Request_OnReceiveData(byte[] message)
        {
            string msg= Encoding.UTF8.GetString(message);
            this.BeginInvoke(new MessageHandle(UpdateRece), msg);
        }

        private void UpdateRece(string msg)
        {
            this.textBoxRece.AppendText("Rece:"+msg + "\r\n");
        }

 

        private void btnSend_Click(object sender, EventArgs e)
        {
            string message=this.textBoxMsg.Text.Trim();
            if (string.IsNullOrWhiteSpace(message))
            {
                MessageBox.Show("请输入信息");
                return;
            }
            else 
            {
              bool isSent=  Request.Send(message);
              if (isSent) 
              {
                  this.textBoxSend.AppendText("send:"+message + "\r\n");
              }
                
            }
            
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void btnStop_Click(object sender, EventArgs e)
        {
        }
    }
}
