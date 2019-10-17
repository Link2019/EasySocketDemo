using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EasySocketDemo
{
    public partial class Form1 : Form
    {

        Socket socketSend;
        Socket socketWatch = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        Dictionary<string, Socket> dicSocket = new Dictionary<string, Socket>();
        public Form1()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            IPAddress ip = IPAddress.Any;
            IPEndPoint point = new IPEndPoint(ip, Convert.ToInt32(txtPort.Text));
            socketWatch.Bind(point);
            socketWatch.Listen(10);
            ShowMsg("监听成功");


            Thread th = new Thread(Listen);
            th.IsBackground = false;
            th.Start();
        }

        private void Listen()
        {
            while (true)
            {
                socketSend = socketWatch.Accept();
                dicSocket.Add(socketSend.RemoteEndPoint.ToString(), socketSend);
                cboUsers.Items.Add(socketSend.RemoteEndPoint.ToString());
                ShowMsg(socketSend.RemoteEndPoint.ToString() + ":" + "连接成功");

                Thread th = new Thread(Receive);
                th.IsBackground = true;
                th.Start(socketSend);
            }
        }

        private void Receive(object o)
        {
            Socket socketSend = o as Socket;
            while (true)
            {
                try
                {
                    byte[] buffer = new byte[1024 * 1024 * 2];
                    int r = socketSend.Receive(buffer);
                    if (r == 0)
                    {
                        break;
                    }
                    string str = Encoding.UTF8.GetString(buffer, 0, r);
                    ShowMsg(socketSend.RemoteEndPoint.ToString() + ":" + str);
                }
                catch { }

            }
        }

        private void ShowMsg(string v)
        {
            txtLog.AppendText(v + "\r\n");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        private void btnSendMsg_Click(object sender, EventArgs e)
        {
            try
            {
                string str = txtMsg.Text;
                byte[] buffer = Encoding.UTF8.GetBytes(str);
                List<byte> list = new List<byte>();
                list.Add(0);
                list.AddRange(buffer);
                byte[] newBuffer = list.ToArray();
                string ip = cboUsers.SelectedItem.ToString();
                dicSocket[ip].Send(newBuffer);
            }
            catch { }
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "请选择要发送的文件";
            ofd.Filter = "文本文件|*.txt|所有文件|*.*";
            ofd.InitialDirectory = @"C:\Users\Administrator.USER-20190915QG\Desktop";
            ofd.Multiselect = false;
            ofd.ShowDialog();
            txtFile.Text = ofd.FileName;

        }

        private void btnSendFile_Click(object sender, EventArgs e)
        {
            string path = txtFile.Text;
            using (FileStream fsRead = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                byte[] buffer = new byte[1024 * 1024 * 5];
                int r = fsRead.Read(buffer, 0, buffer.Length);
                List<byte> list = new List<byte>();
                list.Add(1);
                list.AddRange(buffer);
                byte[] newBuffer = list.ToArray();
                string ip = cboUsers.SelectedItem.ToString();
                dicSocket[ip].Send(newBuffer, 0, r + 1, SocketFlags.None);
            }

        }

        private void btnZD_Click(object sender, EventArgs e)
        {
            byte[] buffer = new byte[1];
            buffer[0] = 2;
            string ip = cboUsers.SelectedItem.ToString();
            dicSocket[ip].Send(buffer);
        }
    }
}
