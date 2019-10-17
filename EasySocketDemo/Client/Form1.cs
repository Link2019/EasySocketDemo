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

namespace Client
{
    public partial class Form1 : Form
    {
        Socket socketSend = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        public Form1()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                IPAddress ip = IPAddress.Parse(txtIP.Text);
                IPEndPoint point = new IPEndPoint(ip, Convert.ToInt32(txtPort.Text));
                socketSend.Connect(point);
                ShowMsg("连接成功");

                Thread th = new Thread(Receive);
                th.IsBackground = true;
                th.Start();
            }
            catch { }

        }

        private void Receive()
        {
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
                    //buffer[0]为标记位,0判断是文本,1判断是文件,2判断是震动
                    if (buffer[0] == 0)
                    {
                        string str = Encoding.UTF8.GetString(buffer, 1, r - 1);
                        ShowMsg(socketSend.RemoteEndPoint.ToString() + ":" + str);
                    }
                    else if (buffer[0] == 1)
                    {
                        SaveFileDialog sfd = new SaveFileDialog();
                        sfd.InitialDirectory = @"C:\Users\Administrator.USER-20190915QG\Desktop";
                        sfd.Title = "请选择要保存的文件";
                        sfd.Filter = "文本文件|*.txt|所有文件|*.*";
                        sfd.ShowDialog(this);
                        string path = sfd.FileName;
                        using (FileStream fsWrite = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write))
                        {
                            fsWrite.Write(buffer, 1, r - 1);
                        }
                        MessageBox.Show("保存成功");

                    }
                    else if (buffer[0] == 2)
                    {
                        ZD();
                    }
                }
                catch { }


            }
        }

        private void ZD()
        {
            int currentX = this.Location.X;
            int currentY = this.Location.Y;
            for(int i=0;i<1000;i++)
            {
                this.Location = new Point(currentX + 10, currentY + 10);
                this.Location = new Point(currentX - 10, currentY - 10);
            }
            this.Location = new Point(currentX, currentY);
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
                string str = txtMsg.Text.Trim();
                byte[] buffer = Encoding.UTF8.GetBytes(str);
                socketSend.Send(buffer);
            }
            catch { }
        }
    }
}
