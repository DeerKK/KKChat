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
    public partial class GroupChat : Form
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

        //add
        int GroupNum = 0;
        int FriendsNum = 0;
        Dictionary<int, Socket> SocketList;
        Dictionary<int, string> NameList;
        Dictionary<int, string> IPList;
        Dictionary<int, Thread> ThreadList = new Dictionary<int, Thread>();
        int isHost;

        public GroupChat(Dictionary<int, Socket> socketlist, Dictionary<int, string> namelist, 
             Dictionary<int, string> iplist, int ishost, string username)
        {
            InitializeComponent();

            if(ishost == 1)
            {
                GroupNum = namelist.Count();
                FriendsNum = GroupNum - 1;

                SocketList = socketlist;
                NameList = namelist;
                isHost = ishost;
                IPList = iplist;
                UserName = username;
                UserIP = IPList[FriendsNum];

                //FriendName = friendname;
                //FriendIP = friendip;

                this.FriendNameLabel.Text = "群聊（" + GroupNum + "人）";

                // show friends
                UpdateFriendList();

                CheckForIllegalCrossThreadCalls = false;

                this.UserNameLabel.Text = username;

                //MessageBox.Show("开始建立线程", "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                for (int i = 0; i < FriendsNum; i++)
                {
                    //MessageBox.Show("建立线程"+i.ToString(), "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    //open thread
                    ParameterizedThreadStart pts = new ParameterizedThreadStart(ReceiveMsg);
                    Thread thread = new Thread(pts);
                    thread.IsBackground = true;
                    thread.Start(SocketList[i]);
                    ThreadList.Add(i, thread);
                }
            }
            else if(ishost == 0)
            {
                GroupNum = namelist.Count();
                FriendsNum = GroupNum - 1;

                SocketList = socketlist;
                NameList = namelist;
                isHost = ishost;
                IPList = iplist;
                UserName = username;
                //note
                UserIP = IPList[0];

                this.FriendNameLabel.Text = "群聊（" + GroupNum + "人）";

                // show friends
                UpdateFriendList();

                CheckForIllegalCrossThreadCalls = false;

                this.UserNameLabel.Text = username;

                //MessageBox.Show("开始建立线程", "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                ParameterizedThreadStart pts = new ParameterizedThreadStart(ReceiveMsg);
                Thread thread = new Thread(pts);
                thread.IsBackground = true;
                thread.Start(SocketList[0]);
                ThreadList.Add(0, thread);               
            }
            else
            {
                MessageBox.Show("未设置ishost", "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }           

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
            for(int i=0;i<GroupNum;i++)
            {
                if(NameList[i] == UserName)
                {
                    FriendCard this_friend = new FriendCard(NameList[i]+"（我）", IPList[i]);
                    FriendsFlow.Controls.Add(this_friend);
                    this_friend.Parent = FriendsFlow;
                    this_friend.Show();
                }
                else
                {
                    FriendCard this_friend = new FriendCard(NameList[i], IPList[i]);
                    FriendsFlow.Controls.Add(this_friend);
                    this_friend.Parent = FriendsFlow;
                    this_friend.Show();
                }
                
            }
            
        }

        // Function 2.1: Update Friend status
        private void UpdateFriendList()
        {
            bool safe_flag = true;

            
            //clear
            IPList.Clear();

            // get friend ip
            IPAddress ServerIP = IPAddress.Parse(Global.ServerIP);
            int ServerPort = Global.ServerPort;

            IPEndPoint endpoint0 = new IPEndPoint(ServerIP, ServerPort);
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            try
            {
                sock.Connect(endpoint0);
            }
            catch
            {
                MessageBox.Show(this, "TCP连接失败", "网络错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            for (int i = 0; i < FriendsNum; i++)
            {
                string SendMsg = "q" + NameList[i];
                byte[] RevByte = new byte[50];
                byte[] SendByte = Encoding.ASCII.GetBytes(SendMsg);

                sock.Send(SendByte);
                string RevString = Encoding.ASCII.GetString(RevByte, 0, sock.Receive(RevByte));

                if (RevString == "n")
                {
                    MessageBox.Show("用户" + NameList[i] + "下线，连接失败", "通知", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    safe_flag = false;
                }
                else if (RevString == "Incorrect No.")
                {
                    //MessageBox.Show("存在不正确的用户名" + NameList[i] + "，连接失败", "通知", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    safe_flag = false;
                }
                else
                {
                    IPList.Add(i, RevString);
                }
            }

            //get local ip
            IPList.Add(FriendsNum, UserIP);

            // show agian
            FriendsShowFunction();
        }

        // Function 3: receive message
        private void ReceiveMsg(object SocketThread)
        {
            string MsgHead;
            int LengthMsg = 0;           
            string ReceiveString; 

            string TextString="unkonwn text";
            string FileHeadString;
            string filename="unknown file";
            string sender;
            long fileLength=0;
            string isHostSig;

            byte[] buffer = new byte[ReceiveBufferSize];

            Socket SocketPt = SocketThread as Socket;

            while (true)
            {
                try
                {
                    LengthMsg = SocketPt.Receive(buffer);
                    ReceiveString = Encoding.UTF8.GetString(buffer, 0, LengthMsg);

                    MsgHead = Encoding.UTF8.GetString(buffer, 0, 6);
                    sender = Encoding.UTF8.GetString(buffer, 6, 10);
                    isHostSig = Encoding.UTF8.GetString(buffer, 10, 11);


                    if (LengthMsg > 0 && sender != UserName)
                    {
                        //Text
                        if (MsgHead == "/Text/") //Text
                        {   
                            if(isHost ==1)
                            {
                                TextString = Encoding.UTF8.GetString(buffer, 18, LengthMsg - 18);

                                ChatBox.SelectionAlignment = HorizontalAlignment.Left;
                                ChatBox.SelectionColor = Color.Blue;
                                ChatBox.AppendText(sender + " " + GetCurrentTime() + " " + "\r\n");

                                ChatBox.SelectionAlignment = HorizontalAlignment.Left;
                                ChatBox.SelectionColor = Color.Black;
                                ChatBox.AppendText(TextString + "\r\n");

                                byte[] SendMsg = Encoding.UTF8.GetBytes("/Text/" + sender + "00" + TextString);
                                
                                //help send but no himself
                                for (int i = 0; i < FriendsNum && NameList[i]!=sender; i++)
                                {

                                    SocketList[i].Send(SendMsg);
                                }

                            }
                            else if(isHost ==0)
                            {
                                TextString = Encoding.UTF8.GetString(buffer, 18, LengthMsg - 18);

                                ChatBox.SelectionAlignment = HorizontalAlignment.Left;
                                ChatBox.SelectionColor = Color.Blue;
                                ChatBox.AppendText(sender);
                                ChatBox.AppendText(" "+GetCurrentTime() + " " + "\r\n");

                                ChatBox.SelectionAlignment = HorizontalAlignment.Left;
                                ChatBox.SelectionColor = Color.Black;
                                ChatBox.AppendText(TextString + "\r\n");
                            }
                            else
                            {
                                ;
                            }                            
                        }
                     
                        //quit info
                        else if (MsgHead == "/Quit/")
                        {
                            if(isHost==1)
                            {
                                MessageBox.Show("成员" + sender + "已经退出，对他的连接将关闭", "通知", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                ChatBox.SelectionAlignment = HorizontalAlignment.Center;
                                ChatBox.SelectionColor = Color.Gray;
                                ChatBox.AppendText(GetCurrentTime() + " " + sender + "退出群聊\r\n");

                                SocketPt.Close();//关闭这个TCP
                            }
                            else
                            {
                                if(isHostSig=="11")
                                {
                                    MessageBox.Show("群主" + sender + "群聊解散", "通知", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    this.Close();
                                }
                                else
                                {
                                    MessageBox.Show("成员" + sender + "已经退出，对他的连接将关闭", "通知", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                    ChatBox.SelectionAlignment = HorizontalAlignment.Center;
                                    ChatBox.SelectionColor = Color.Gray;
                                    ChatBox.AppendText(GetCurrentTime() + " " + sender + "退出群聊\r\n");

                                    SocketPt.Close();//关闭这个TCP
                                }
                            }                            
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
            if(MsgType == "Text")
            {
                if (SendBox.Text.Length > 0)
                {
                    if(isHost == 1)
                    {
                        byte[] SendMsg = Encoding.UTF8.GetBytes("/Text/" + UserName+"11"+SendBox.Text.Trim());
                        for (int i = 0; i < FriendsNum; i++)
                        {
                            SocketList[i].Send(SendMsg);
                        }

                        ChatBox.SelectionAlignment = HorizontalAlignment.Right;
                        ChatBox.SelectionColor = Color.Green;
                        ChatBox.AppendText(UserName + "(我)" + " " + GetCurrentTime() + "\r\n");
                        ChatBox.SelectionAlignment = HorizontalAlignment.Right;
                        ChatBox.SelectionColor = Color.Black;
                        ChatBox.AppendText(SendBox.Text.Trim() + "\r\n");

                        ChatBox.SelectionAlignment = HorizontalAlignment.Left;

                        SendBox.Clear();
                    }
                    else if(isHost ==0)
                    {
                        byte[] SendMsg = Encoding.UTF8.GetBytes("/Text/" + UserName + "00" + SendBox.Text.Trim());
                        SocketList[0].Send(SendMsg);

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
                        ;
                    }                             
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
            try
            {
                if(isHost==1)
                {
                    byte[] SendExit = Encoding.UTF8.GetBytes("/Quit/" + UserName +"11");
                    for (int i = 0; i < FriendsNum; i++)
                    {
                        SocketList[i].Send(SendExit);
                    }
                }
                else
                {
                    byte[] SendExit = Encoding.UTF8.GetBytes("/Quit/" + UserName + "00");
                    SocketList[0].Send(SendExit);
                }
                
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
            MessageBox.Show("请返回主界面使用群发文件（基于P2P网络）", "通知", MessageBoxButtons.OK, MessageBoxIcon.Error);          
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
