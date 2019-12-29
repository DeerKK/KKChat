using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

//add pkg
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;

using System.Resources;
using System.Runtime.InteropServices;
using System.IO;

using System.Text.RegularExpressions;

namespace WindowsFormsApp1
{
    public partial class Login : Form
    {
        // Function 1: initial set
        public Login()
        {
            InitializeComponent();

            //Center window
            this.StartPosition = FormStartPosition.CenterScreen;
            
            //Ser IP
            Global.ServerIP = "166.111.140.57";
            Global.ServerPort = 8000;
            this.DialogResult = DialogResult.Retry;
            Global.Login_cls = false;

            //set for debug
            this.UserBox.Text = "2016011438";
            this.PasswordBox.Text = "net2019";
        }

        // Function 2: check
        public bool UserConfirm()
        {
            string user = UserBox.Text;
            string password = PasswordBox.Text;
            int acc_length = UserBox.TextLength;

            bool flag = false;

            // judge the logical user first
            if ((user == null || user.Length == 0) || (password == null || password.Length == 0))
            {
                MessageBox.Show("用户名/密码不能为空", "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return flag;
            }
            else
            {
                if (user.Length < 10)
                {
                    MessageBox.Show("用户名长度不正确", "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    UserBox.Clear();
                    PasswordBox.Clear();
                    return flag;
                }

                // Check port
                int Port = int.Parse(user.Substring(5)) + 10000;

                bool port_flag = false;
                IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
                IPEndPoint[] ipEndPoints = ipProperties.GetActiveTcpListeners();

                foreach (IPEndPoint endpoint in ipEndPoints)
                {
                    if (endpoint.Port == Port)
                    {
                        port_flag = true;
                        break;
                    }
                }

                if (port_flag)
                {
                    MessageBox.Show("该用户已登录", "通知", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    PasswordBox.Clear();
                    UserBox.Clear();
                    return flag;
                }

                flag = true;
                return flag;
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void login_Load(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        // Function 3: Login
        private void button1_Click(object sender, EventArgs e)
        {
            if (UserConfirm())
            {
                TcpClient client = new TcpClient();
                // try to connet the client and check if we lose it
                try
                {
                    client.Connect(Global.ServerIP, Global.ServerPort);
                }
                catch
                {
                    MessageBox.Show("TCP连接失败", "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                if (client.Connected)
                {
                    // stream send to the server and with it we can login
                    NetworkStream ClientStream = client.GetStream();

                    string SendMsg = UserBox.Text + "_" + PasswordBox.Text;
                    byte[] msg = Encoding.Default.GetBytes(SendMsg);
                    ClientStream.Write(msg, 0, msg.Length); // write into the stream

                    byte[] RevMsg = new byte[50];
                    int bytes_length = 0;
                    ClientStream.ReadTimeout = 1000;
                    bool istimeout = false;

                    try
                    {
                        bytes_length = ClientStream.Read(RevMsg, 0, 50);
                    }
                    catch
                    {
                        istimeout = true;
                        MessageBox.Show("TCP超时", "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        ClientStream.Close();
                        client.Close();
                    }

                    string succ_flag = Encoding.Default.GetString(RevMsg, 0, bytes_length);
                    Regex r = new Regex(@"^lol");

                    if (r.IsMatch(@succ_flag) && istimeout == false)
                    {
                        int UserSpecialPort = int.Parse(UserBox.Text.Substring(5)) + 10000;

                        Global.User = UserBox.Text;
                        Global.CurFriendName = UserBox.Text;
                        Global.CurFriendPort = UserSpecialPort; 
                        Global.Login_cls = true;

                        this.DialogResult = DialogResult.OK;  
                        
                        ClientStream.Close();
                        client.Close();
                    }
                    else
                    {
                        MessageBox.Show("TCP超时/TCP连接成功, 用户名/密码错误", "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        PasswordBox.Clear();
                    }
                }
                else
                {
                    client.Close();
                }
            }
            
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {

        }

        private void label1_Click_1(object sender, EventArgs e)
        {

        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Global.Login_cls = true;
            this.DialogResult = DialogResult.No;
        }
    }
}
