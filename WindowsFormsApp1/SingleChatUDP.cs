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

using System.Threading;

namespace WindowsFormsApp1
{
    public partial class SingleChatUDP : Form
    {
        string FriendPath = ".\\" + Global.User + "\\Friends\\";

        Thread ThreadListen = null;
        Socket ChatSocket = null;

        Thread ThreadFile;
        Thread ThreadFile2;
        DialogResult SaveDialogResult;
        SaveFileDialog SaveDialog = null;
        DialogResult OpenDialogResult;
        OpenFileDialog OpenDialog = null;

        string UserName;
        string UserIP;
        string FriendName;
        string FriendIP;

        //file transfer set
        int SendBufferSize = 1024 * 1024;
        int ReceiveBufferSize = 1024 * 1024;

        public SingleChatUDP(string friendname, string friendip, string username, string userip)
        {
            InitializeComponent();

            FriendName = friendname;
            FriendIP = friendip;
            this.FriendNameLabel.Text = FriendName;

            // show this friend
            FriendsShowFunction();

            CheckForIllegalCrossThreadCalls = false;
            UserName = username;
            this.UserNameLabel.Text = username;

            ChatSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            int UDP_Port = int.Parse(UserName.Substring(5)) + 15000;

            ChatSocket.Bind(new IPEndPoint(IPAddress.Parse(userip), UDP_Port));//绑定端口号和IP

            //MessageBox.Show("UDP socket set", "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

            // listen
            Thread ThreadListen = new Thread(ReceiveMsg);//开启接收消息线程
            ThreadListen.IsBackground = true;
            ThreadListen.Start();



        }

        // Function 1: search online friend
        private string SearchFunction(string FriendName)
        {
            string Result = "None";

            TcpClient client = new TcpClient();
            try
            {
                client.Connect(Global.ServerIP, Global.ServerPort);
            }
            catch
            {
                MessageBox.Show("TCP连接失败", "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                client.Close();
                Result = "Fail";
                return Result;
            }

            if (client.Connected)
            {
                NetworkStream SendStream = client.GetStream();
                string ask = "q" + FriendName;
                byte[] msg = Encoding.Default.GetBytes(ask);
                SendStream.Write(msg, 0, msg.Length);

                byte[] msg_get = new byte[50];
                int bytesRead = 0;
                SendStream.ReadTimeout = 1000;
                bool istimeout = false;
                try
                {
                    bytesRead = SendStream.Read(msg_get, 0, 50);
                }
                catch
                {
                    istimeout = true;
                    MessageBox.Show("TCP连接失败", "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    SendStream.Close();
                    client.Close();
                    Result = "Fail";
                    return Result;
                }

                if (msg_get[0] == 'n')//offline
                {
                    MessageBox.Show("该用户不在线", "通知", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    SendStream.Close();
                    client.Close();
                    return Result;
                }
                else if (msg_get[0] == 'I')//incorrect
                {
                    MessageBox.Show("用户名不存在", "通知", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    SendStream.Close();
                    client.Close();
                    Result = "Fail";
                    return Result;
                }
                else if (istimeout == false)// right!
                {
                    string FriendIP = Encoding.Default.GetString(msg_get, 0, bytesRead);
                    MessageBox.Show("该用户在线，IP为" + FriendIP, "通知", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    SendStream.Close();
                    client.Close();
                    return FriendIP;
                }
                else
                {
                    SendStream.Close();
                    client.Close();
                    Result = "Fail";
                    return Result;
                }
            }
            else
            {
                client.Close();
                return Result;
            }
        }

        private void SearchFunctionAdd()
        {
            if (SearchBar.Text.Length != 10)
            {
                MessageBox.Show("用户名不存在", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SearchBar.Clear();
            }
            else
            {
                // self search
                if (this.SearchBar.Text == Global.User)
                {
                    MessageBox.Show("您已在线", "通知", MessageBoxButtons.OK, MessageBoxIcon.Question);
                    SearchBar.Clear();
                }
                else
                {
                    string SearchFriendName = this.SearchBar.Text;
                    string SearchFriendIP = SearchFunction(SearchFriendName);
                    if (SearchFriendIP == "Fail")
                    {
                        SearchBar.Clear();
                    }
                    else
                    {
                        // check if is friend
                        bool isFriendFlag = false;
                        DirectoryInfo folder = new DirectoryInfo(FriendPath);
                        foreach (FileInfo file in folder.GetFiles("*.txt"))
                        {
                            string filename = file.FullName;
                            string FriendName = filename.Substring(filename.LastIndexOf(".") - 10, 10);
                            if (FriendName == Global.User) { continue; }
                            if (FriendName == SearchFriendName)
                            { isFriendFlag = true; }
                        }

                        if (isFriendFlag)
                        {
                            DialogResult Dr = MessageBox.Show("该用户已是您的好友，发起会话？", "询问", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                            if (Dr == DialogResult.OK)
                            {
                                Global.CurFriendName = SearchFriendName;
                                Global.CurFriendIP = SearchFriendIP;
                                StartSingleChat();
                            }
                        }
                        else
                        {
                            DialogResult Dr = MessageBox.Show("是否添加为好友？", "询问", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                            if (Dr == DialogResult.OK)
                            {
                                FileStream fs = new FileStream(FriendPath + SearchFriendName + ".txt", FileMode.Create);
                                fs.Close();

                                DialogResult Dr2 = MessageBox.Show("该用户已是您的好友，发起会话？", "询问", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                                if (Dr2 == DialogResult.OK)
                                {
                                    Global.CurFriendName = SearchFriendName;
                                    Global.CurFriendIP = SearchFriendIP;
                                    StartSingleChat();
                                }
                            }
                        }
                    }
                }
            }
        }

        // Function 2: show chatting friend
        private void FriendsShowFunction()
        {
            FriendsFlow.Controls.Clear();
            FriendCard this_friend = new FriendCard(FriendName, FriendIP);
            FriendsFlow.Controls.Add(this_friend);
            this_friend.Parent = FriendsFlow;
            this_friend.Show();
        }

        // Function 3: receive message
        private void ReceiveMsg()
        {
            string MsgHead;
            int LengthMsg = 0;           
            string ReceiveString; 

            string TextString;
            string FileHeadString;
            string filename="unknow_file";
            long fileLength=0;

            byte[] buffer = new byte[ReceiveBufferSize];
            EndPoint point = new IPEndPoint(IPAddress.Any, 0);//用来保存发送方的ip和端口号
           
            while (true)
            {
                try
                {
                    LengthMsg = ChatSocket.ReceiveFrom(buffer, ref point);
                    ReceiveString = Encoding.UTF8.GetString(buffer, 0, LengthMsg);
                    MsgHead = Encoding.UTF8.GetString(buffer, 0, 6);

                    if (LengthMsg > 0)
                    {
                        //Text
                        if (MsgHead == "/Text/") //Text
                        {
                            TextString = Encoding.UTF8.GetString(buffer, 6, LengthMsg - 6);

                            ChatBox.SelectionAlignment = HorizontalAlignment.Left;
                            ChatBox.SelectionColor = Color.Blue;
                            ChatBox.AppendText(FriendName + " " + GetCurrentTime() + " " + "\r\n");

                            ChatBox.SelectionAlignment = HorizontalAlignment.Left;
                            ChatBox.SelectionColor = Color.Black;
                            ChatBox.AppendText(TextString + "\r\n");
                        }
                        // File head
                        else if (MsgHead == "/FlHd/")
                        {
                            FileHeadString = Encoding.UTF8.GetString(buffer, 6, LengthMsg - 6);
                            filename = FileHeadString.Split('/').First(); 
                            fileLength = Convert.ToInt64(FileHeadString.Split('/').Last());
                        }
                        // File
                        else if (MsgHead == "/File/")//Main file
                        {
                            ;                           
                        }

                        //acc
                        else if (ReceiveString == "/FlAc/")
                        {
                            ChatBox.SelectionAlignment = HorizontalAlignment.Center;
                            ChatBox.SelectionColor = Color.Gray;
                            ChatBox.AppendText(GetCurrentTime() + "\r\n" + "对方成功接收文件\r\n");
                        }

                        //fail
                        else if (ReceiveString == "/FlRj/")
                        {
                            ChatBox.SelectionAlignment = HorizontalAlignment.Center;
                            ChatBox.SelectionColor = Color.Gray;
                            ChatBox.AppendText(GetCurrentTime() + "\r\n" + "对方拒绝接收文件\r\n");
                        }

                        //quit info
                        else if (ReceiveString == "/Quit/")
                        {
                            MessageBox.Show("对方已退出当前聊天, 本连接将关闭", "通知", MessageBoxButtons.OK, MessageBoxIcon.Information);                           
                            this.Close();                           
                        }
                    }
                }
                catch (Exception ex)
                {
                    break;
                }

            }

        }

        // Function 4: send message
        private void SendMsg(string MsgType)
        {
            // for UDP
            int FriendPort = int.Parse(FriendName.Substring(5)) + 15000;
            EndPoint point = new IPEndPoint(IPAddress.Parse(FriendIP), FriendPort);

            if (MsgType == "Text")
            {
                if (SendBox.Text.Length > 0)
                {
                    byte[] SendMsg = Encoding.UTF8.GetBytes("/Text/" + SendBox.Text.Trim());
                    //server.SendTo(Encoding.UTF8.GetBytes(msg), point);
                    ChatSocket.SendTo(SendMsg, point);

                    ChatBox.SelectionAlignment = HorizontalAlignment.Right;
                    ChatBox.SelectionColor = Color.Green;                   
                    ChatBox.AppendText(UserName + "(我)" + " " + GetCurrentTime() + "\r\n");
                    ChatBox.SelectionAlignment = HorizontalAlignment.Right;
                    ChatBox.SelectionColor = Color.Black;
                    ChatBox.AppendText(SendBox.Text.Trim() + "\r\n");

                    ChatBox.SelectionAlignment = HorizontalAlignment.Left;

                    SendBox.Clear();                    
                }
                else
                {
                    MessageBox.Show("不能发生空白信息", "通知", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }

            else if(MsgType == "File")
            {
                ;
            }

            else
            {
                MessageBox.Show("未知的消息类型", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        // Function 5: Stop TCP connection
        private void Quit()
        {
            int FriendPort = int.Parse(FriendName.Substring(5)) + 15000;
            EndPoint point = new IPEndPoint(IPAddress.Parse(FriendIP), FriendPort);
            byte[] SendExit = Encoding.UTF8.GetBytes("/Quit/");
            try
            {
                ChatSocket.SendTo(SendExit, point);
            }
            catch
            {
                MessageBox.Show("TCP连接故障，退出失败", "通知", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            ThreadListen.Abort();
            this.Close();
        }

        //TODO:
        private void StartSingleChat()
        {

        }

        private DateTime GetCurrentTime()
        {
            DateTime currentTime = new DateTime();
            currentTime = DateTime.Now;
            return currentTime;
        }

        private void SaveDialogCall()
        {
            SaveDialogResult = SaveDialog.ShowDialog();
        }

        private void OpenDialogCall()
        {
            OpenDialogResult = OpenDialog.ShowDialog();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        // Function 2 Button
        private void button4_Click(object sender, EventArgs e)
        {
            SearchFunctionAdd();
        }

        private void SingleChat_Load(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            MessageBox.Show("为保证质量，请使用TCP下的文件发送", "通知", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
        }

        private void SendBotton_Click(object sender, EventArgs e)
        {
            SendMsg("Text");
        }

        private void button9_Click(object sender, EventArgs e)
        {
            Quit();
        }
    }
}
