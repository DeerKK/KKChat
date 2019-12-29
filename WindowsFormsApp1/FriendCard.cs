using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

//add
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;

namespace WindowsFormsApp1
{
    public partial class FriendCard : UserControl
    {
        public FriendCard(string big, string small)
        {
            InitializeComponent();

            if(big.Length<20 && big.Length >0 && small.Length < 20 && small.Length > 0)
            {
                this.label2.Text = big;
                this.label1.Text = small;
            }
            

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void FriendCard_Load(object sender, EventArgs e)
        {

        }

        // click
        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("发起与用户" + this.label2.Text + "的TCP通话？", "TCP单人会话", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

            if (dr == DialogResult.Yes)
            {
                //set server ip
                IPAddress ServerIP = IPAddress.Parse(Global.ServerIP);
                int ServerPort = Global.ServerPort;

                // set local ip
                Socket Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress ipaddress = IPAddress.Parse(Global.ServerIP);
                IPEndPoint endpoint = new IPEndPoint(ipaddress, ServerPort);
                Client.Connect(endpoint);

                Global.CurFriendName = this.label2.Text;

                string FriendName = Global.CurFriendName;
                string SendMsg = 'q' + FriendName;
                byte[] SendByte = Encoding.UTF8.GetBytes(SendMsg);
                Client.Send(SendByte);

                byte[] RevMsg = new byte[50];
                string RevString = Encoding.UTF8.GetString(RevMsg, 0, Client.Receive(RevMsg));

                if (RevString == "n")
                {
                    System.Windows.Forms.MessageBox.Show("该用户不在线", "通知", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                else if (RevString == "Incorrect No.")
                {
                    System.Windows.Forms.MessageBox.Show("该用户不存在", "通知", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                else
                {                    
                    //get friend ip                    
                    Global.CurFriendIP = RevString;
                    string FriendIP = Global.CurFriendIP;
                    int FriendPort = int.Parse(FriendName.Substring(5)) + 10000;

                    //debug
                    //System.Windows.Forms.MessageBox.Show(RevString, "通知", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                    Socket socket_client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    IPAddress aa = IPAddress.Parse(FriendIP);
                    IPEndPoint bb = new IPEndPoint(aa, FriendPort);

                    try
                    {
                        socket_client.Connect(bb);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("TCP连接失败", "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return;
                    }                 

                    //send signal
                    string HelloMsg = "Hello/" + "TCPSingle/" + Global.User + "/" + Global.UserIP;
                    byte[] HelloByte = Encoding.UTF8.GetBytes(HelloMsg);
                    socket_client.Send(HelloByte);

                    //receive ans
                    byte[] AnsMsg = new byte[50];
                    string AnsString = Encoding.UTF8.GetString(AnsMsg, 0, socket_client.Receive(AnsMsg));

                    //read ans
                    if (AnsString == "Accept")
                    {
                        SingleChat NewChat = new SingleChat(FriendName, FriendIP, Global.User, socket_client);
                        NewChat.ShowDialog();
                    }
                    else if (AnsString == "Decline")
                    {
                        MessageBox.Show("会话被拒绝", "通知", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
                
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Global.CurFriendName = this.label1.Text;
            MessageBox.Show("Start(TCP) chatting with " + Global.CurFriendName, "Start", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
