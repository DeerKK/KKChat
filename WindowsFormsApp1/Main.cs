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
    public partial class Main : Form
    {
        string UserName;
        static IPAddress ServerIP;
        static int ServerPort;

        Socket Client = null;
        IPAddress UserIP;
        Socket SocketListen = null;
        Socket ServerConnect = null;
        Thread ThreadListen = null;

        Dictionary<string, int> UserToNum;

        string mode = "SingleChat";

        string FriendPath = ".\\" + Global.User + "\\Friends\\";

        int UserSpecialPort;

        public Main()
        {
            //Center window
            this.StartPosition = FormStartPosition.CenterScreen;
           
            InitializeComponent();

            this.FriendListView.Hide();
            this.UDPChatButton.Hide();
            this.TCPChatButton.Hide();
            this.button1.Hide();
            this.button2.Hide();

            //set p2p file
            UserToNum = new Dictionary<string, int>();

            // show recent friend, TODO
            FriendsShowFunction();

            //set server ip
            ServerIP = IPAddress.Parse(Global.ServerIP);
            ServerPort = Global.ServerPort;

            // set local ip
            Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ipaddress = IPAddress.Parse(Global.ServerIP);
            IPEndPoint endpoint = new IPEndPoint(ipaddress, ServerPort);
            Client.Connect(endpoint);

            //MessageBox.Show("local ip set", "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

            string HostName = Dns.GetHostName();
            IPAddress[] addressList = Dns.GetHostAddresses(HostName);
            foreach (IPAddress ip in addressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork) UserIP = ip;
            }

            // TCP listen
            SocketListen = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            ipaddress = UserIP;
            Global.UserIP = UserIP.ToString();

            //set windows text
            UserName = Global.User;
            this.UserNameLabel.Text = UserName;

            // Lock to endpoint
            UserSpecialPort = int.Parse(UserName.Substring(5)) + 10000; //11438~11442

            try
            {
                endpoint = new IPEndPoint(ipaddress, UserSpecialPort);
                SocketListen.Bind(endpoint);
                SocketListen.Listen(20);
                ThreadListen = new Thread(ListenConnecting);
                ThreadListen.IsBackground = true;
                ThreadListen.Start();

                //MessageBox.Show("Peer监听打开" + ipaddress.ToString() + ":" + UserSpecialPort.ToString(), "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            catch
            {
                MessageBox.Show("正常监听打开失败", "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
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
                    MessageBox.Show("该用户在线，IP为"+FriendIP, "通知", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                                string dataasstring = "unknown ip"; //your data
                                byte[] info = new UTF8Encoding(true).GetBytes(dataasstring);
                                fs.Write(info, 0, info.Length);

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

        // Function 2: create friend path, show recent friends and set friend name in global
        private void FriendsShowFunction()
        {
            FriendsFlow.Controls.Clear();           
            if (Directory.Exists(FriendPath))
            {
                Console.WriteLine(FriendPath); //debug
                
                // TODO: should write a recent friends record

                DirectoryInfo folder = new DirectoryInfo(FriendPath);
                foreach (FileInfo file in folder.GetFiles("*.txt"))
                {
                    string filename = file.FullName;
                    string FriendName = filename.Substring(filename.LastIndexOf(".")-10, 10);
                    if (FriendName == Global.User) { continue; }
                    string [] all_lines = File.ReadAllLines(filename);
                    string current_truename = all_lines[0];
                    FriendCard show_recent = new FriendCard(FriendName, current_truename);
                    FriendsFlow.Controls.Add(show_recent);
                    show_recent.Parent = FriendsFlow;
                    show_recent.Show();
                }
            }
            else
            {
                Directory.CreateDirectory(FriendPath);
            }
        }

        // Function 3: show all friend in the center
        private void ShowAllFriendList()
        {
            FriendListView.Clear();
            FriendListView.Columns.AddRange(new ColumnHeader[] { testColumnHead });
            FriendListView.View = View.Details;
         
            IPEndPoint endpoint = new IPEndPoint(ServerIP, ServerPort);
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            try
            {
                sock.Connect(endpoint);
            }
            catch
            {
                MessageBox.Show(this, "TCP连接失败",
                    "网络错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DirectoryInfo folder = new DirectoryInfo(FriendPath);
            int tmp_index = 0;
            foreach (FileInfo file in folder.GetFiles("*.txt"))
            {
                string filename = file.FullName;
                string FriendName = filename.Substring(filename.LastIndexOf(".") - 10, 10);
                if (FriendName == Global.User) { continue; }

                string SendMsg = "q" + FriendName;
                byte[] RevByte = new byte[50];
                byte[] SendByte = Encoding.ASCII.GetBytes(SendMsg);

                sock.Send(SendByte);
                string RevString = Encoding.ASCII.GetString(RevByte, 0, sock.Receive(RevByte));

                if (RevString != "n" && RevString != "Incorrect No.")
                {
                    ListViewItem listviewItem = new ListViewItem(FriendName, 0);//创建列表项
                    FriendListView.Items.Insert(0, listviewItem);
                }
                else
                {
                    ListViewItem listviewItem = new ListViewItem(FriendName, 0);//创建列表项
                    listviewItem.ForeColor = Color.Gray;                   
                    FriendListView.Items.Insert(0, listviewItem);
                }
            }

            UDPChatButton.Show();
            TCPChatButton.Show();
            this.button1.Show();
            FriendListView.Show();            
        }

        // Function 3: Listen TCP
        private void ListenConnecting()
        {
            while (true)
            {
                ServerConnect = SocketListen.Accept();
                
                // Receive Msg
                byte[] RevMsg = new byte[1024 * 1024];
                string[] RevString = Encoding.UTF8.GetString(RevMsg, 0, ServerConnect.Receive(RevMsg)).Split('/');

                string signal = RevString[0];
                string type = RevString[1];
                string FriendName = RevString[2];
                string FriendIP = RevString[3];

                // Single chat
                if (signal == "Hello" && type == "TCPSingle")
                {
                    DialogResult dr = MessageBox.Show("用户" + FriendName + "请求会话","TCP单人会话", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                    if (dr == DialogResult.Yes)
                    {
                        byte[] AcceptMsg = Encoding.UTF8.GetBytes("Accept");
                        ServerConnect.Send(AcceptMsg);

                        //MessageBox.Show( FriendName + " "+UserName, "TCP单人会话", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                        SingleChat NewChat = new SingleChat(FriendName, FriendIP, UserName, ServerConnect);
                        NewChat.ShowDialog();
                    }
                    else
                    {
                        byte[] RefuseMsg = Encoding.UTF8.GetBytes("Decline");
                        ServerConnect.Send(RefuseMsg);
                    }
                }

                else if(signal == "Hello" && type == "UDPSingle")
                {
                    DialogResult dr = MessageBox.Show("用户" + FriendName + "请求会话", "UDP单人会话", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                    if (dr == DialogResult.Yes)
                    {
                        byte[] AcceptMsg = Encoding.UTF8.GetBytes("Accept");
                        ServerConnect.Send(AcceptMsg);

                        SingleChatUDP NewChat = new SingleChatUDP(FriendName, FriendIP, UserName, UserIP.ToString());
                        NewChat.ShowDialog();
                    }
                    else
                    {
                        byte[] RefuseMsg = Encoding.UTF8.GetBytes("Decline");
                        ServerConnect.Send(RefuseMsg);
                    }
                    
                }

                //Group chat
                //TODO
                else if (signal == "Hello" && type == "TCPGroup")
                {
                    DialogResult result = System.Windows.Forms.MessageBox.Show("用户" + FriendName + "请求群聊", "多人会话", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        // begin group
                        Dictionary<int, Socket> SocketList = new Dictionary<int, Socket>();
                        Dictionary<int, string> NameList = new Dictionary<int, string>();
                        Dictionary<int, string> IPList = new Dictionary<int, string>();

                        //note: string SendMsg = "Hello/TCPGroup/" + UserName+"/"+UserIP+"/"+(selectNum+1).ToString();
                        //for (int j = 0; j < selectNum; j++)
                        //{
                        //   SendMsg += "/" + NameList[j];
                        //}

                        int FriendsNum = RevString.Length - 5;
                        int GroupNum = FriendsNum+1;

                        //get friends names
                        NameList.Add(0, RevString[2]);
                        int j = 1;

                        for (int i = 0; i < FriendsNum; i++)
                        {
                            if(RevString[i+5] != UserName)
                            {
                                NameList.Add(j, RevString[i + 5]);
                                j++;
                            }                           
                        }
                        NameList.Add(FriendsNum, UserName);

                        SocketList.Add(0, ServerConnect);

                        IPList.Add(0, UserIP.ToString());
                        //TODO
                        GroupChat NewMchat = new GroupChat(SocketList, NameList, IPList,0, UserName);

                        NewMchat.ShowDialog();
                    }
                }

                else if (signal == "Hello" && type == "P2PFileGroup")
                {
                    //DialogResult result = System.Windows.Forms.MessageBox.Show("用户" + FriendName + "请求P2P多人文件传输", "多人会话"+UserName, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    //force open!
                    if(true)//if (result == DialogResult.Yes)
                    {
                        // begin group
                        Dictionary<int, Socket> SocketList = new Dictionary<int, Socket>();
                        Dictionary<int, string> NameList = new Dictionary<int, string>();
                        Dictionary<int, string> IPList = new Dictionary<int, string>();

                        //note: string SendMsg = "Hello/TCPGroup/" + UserName+"/"+UserIP+"/"+(selectNum+1).ToString();
                        //for (int j = 0; j < selectNum; j++)
                        //{
                        //   SendMsg += "/" + NameList[j];
                        //}

                        int FriendsNum = RevString.Length - 5;
                        int GroupNum = FriendsNum + 1;

                        //before get friends, set the order bin
                        UserToNum.Clear();

                        for (int i = 0; i < FriendsNum; i++)
                        {
                            UserToNum.Add(RevString[i + 5], i + 1);                     
                        }
                        UserToNum.Add(FriendName, 0); //this friend is host

                        //get friends names
                        NameList.Add(0, RevString[2]);
                        int j = 1;

                        for (int i = 0; i < FriendsNum; i++)
                        {
                            if (RevString[i + 5] != UserName)
                            {
                                NameList.Add(j, RevString[i + 5]);
                                j++;
                            }
                        }
                        NameList.Add(FriendsNum, UserName);

                        SocketList.Add(0, ServerConnect);

                        IPList.Add(0, UserIP.ToString());
                        //TODO
                        //MessageBox.Show("准备创建窗口", "多人会话" + UserName, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                        GroupFile NewMchat = new GroupFile(SocketList, NameList, IPList, 0, UserName, UserToNum);

                        NewMchat.ShowDialog();
                    }
                }
            }
        }

        // Function 4: Start TCP Single Chat
        private void StartSingleChat()
        {
            //-------------------------------------------------------------------//

            if (Global.CurFriendName == Global.User)
            {
                MessageBox.Show("未设定聊天对象/聊天对象为自己", "错误", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            // neglet offline
            else
            {
                // get friend info
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

                    Socket socket_client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    IPAddress ipaddress = IPAddress.Parse(FriendIP);
                    IPEndPoint endpoint = new IPEndPoint(ipaddress, FriendPort);

                    try
                    {
                        socket_client.Connect(endpoint);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("TCP连接失败", "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return;
                    }

                    //send signal
                    string HelloMsg = "Hello/" + "TCPSingle/" + UserName + "/" + UserIP;
                    byte[] HelloByte = Encoding.UTF8.GetBytes(HelloMsg);
                    socket_client.Send(HelloByte);

                    //receive ans
                    byte[] AnsMsg = new byte[50];
                    string AnsString = Encoding.UTF8.GetString(AnsMsg, 0, socket_client.Receive(AnsMsg));

                    //read ans
                    if (AnsString == "Accept")
                    {
                        SingleChat NewChat = new SingleChat(FriendName, FriendIP, UserName, socket_client);
                        NewChat.ShowDialog();
                    }
                    else if (AnsString == "Decline")
                    {
                        MessageBox.Show("会话被拒绝", "通知", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                              
            }
        }

        // Function 5: Start TCP Group Chat
        private void StartGroupChat()
        {         
            // check num of people
            int selectNum = FriendListView.CheckedItems.Count;
            for(int i =0; i<selectNum;i++)
            {
                if (FriendListView.CheckedItems[i].ForeColor == Color.Gray)
                {
                    selectNum -= 1;
                }
            }          

            if (selectNum < 2)
            {
                MessageBox.Show("群聊至少需要在线的3人", "通知", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            else
            {
                Dictionary<int, Socket> SocketList = new Dictionary<int, Socket>();
                Dictionary<int, string> NameList = new Dictionary<int, string>();
                Dictionary<int, string> IPList = new Dictionary<int, string>();

                bool safe_flag = true;

                //int GroupPort = UserSpecialPort;

                //get friends names
                for (int i = 0; i < selectNum; i++)
                {
                    NameList.Add(i, FriendListView.CheckedItems[i].Text);
                }
                NameList.Add(selectNum, UserName);

                // get Friends IP
                IPEndPoint endpoint0 = new IPEndPoint(ServerIP, ServerPort);
                Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                try
                {
                    sock.Connect(endpoint0);
                }
                catch
                {
                    MessageBox.Show(this, "TCP连接失败","网络错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                for (int i = 0; i < selectNum; i++)
                {
                    string SendMsg = "q" + NameList[i];
                    byte[] RevByte = new byte[50];
                    byte[] SendByte = Encoding.ASCII.GetBytes(SendMsg);

                    sock.Send(SendByte);
                    string RevString = Encoding.ASCII.GetString(RevByte, 0, sock.Receive(RevByte));

                    if (RevString == "n")
                    {
                        MessageBox.Show("用户"+ NameList[i]+"下线，连接失败", "通知", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        safe_flag = false;
                    }
                    else if (RevString == "Incorrect No.")
                    {
                        MessageBox.Show("存在不正确的用户名" + NameList[i] + "，连接失败", "通知", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        safe_flag = false;
                    }
                    else
                    {
                        IPList.Add(i, RevString);
                    }
                }
                IPList.Add(selectNum, UserIP.ToString());

                //socket for every one
                for (int i = 0; i < selectNum; i++)
                {
                    //MessageBox.Show("into send socket", "警告", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    int thisFriendPort =int.Parse(NameList[i].Substring(5)) + 10000;

                    Socket socket_client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    IPAddress ipaddress = IPAddress.Parse(IPList[i]);
                    IPEndPoint endpoint = new IPEndPoint(ipaddress, thisFriendPort);
                    try
                    {
                        socket_client.Connect(endpoint);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "警告", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return;
                    }

                    //send msg = head+host+num+guest
                    //MessageBox.Show("准备发送群组信息", "警告", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                    string SendMsg = "Hello/TCPGroup/" + UserName+"/"+UserIP+"/"+(selectNum+1).ToString();

                    for (int j = 0; j < selectNum; j++)
                    {
                        SendMsg += "/" + NameList[j];
                    }

                    byte[] Msg = Encoding.UTF8.GetBytes(SendMsg);
                    socket_client.Send(Msg);

                    //add to socketlist
                    SocketList.Add(i, socket_client);                   
                }

                //MessageBox.Show("邀请已发送", "警告", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                int isHost = 1;
                GroupChat Newchat = new GroupChat(SocketList, NameList, IPList, isHost, UserName);

                //MessageBox.Show("socket num"+ SocketList.Count().ToString(), "警告", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                //note: ipnum = namenum, socketnum = namenum-1
                Newchat.ShowDialog();
            }
        }

        // Function 6: P2P Group File
        private void P2PGroupFile()
        {
            // check num of people
            int selectNum = FriendListView.CheckedItems.Count;
            for (int i = 0; i < selectNum; i++)
            {
                if (FriendListView.CheckedItems[i].ForeColor == Color.Gray)
                {
                    selectNum -= 1;
                }
            }

            if (selectNum < 1)
            {
                MessageBox.Show("群发文件至少需要在线的3人", "通知", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            else
            {
                Dictionary<int, Socket> SocketList = new Dictionary<int, Socket>();
                Dictionary<int, string> NameList = new Dictionary<int, string>();
                Dictionary<int, string> IPList = new Dictionary<int, string>();

                bool safe_flag = true;

                //int GroupPort = UserSpecialPort;

                //get friends names
                for (int i = 0; i < selectNum; i++)
                {
                    NameList.Add(i, FriendListView.CheckedItems[i].Text);
                }
                NameList.Add(selectNum, UserName);

                // get Friends IP
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

                for (int i = 0; i < selectNum; i++)
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
                        MessageBox.Show("存在不正确的用户名" + NameList[i] + "，连接失败", "通知", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        safe_flag = false;
                    }
                    else
                    {
                        IPList.Add(i, RevString);
                    }
                }
                IPList.Add(selectNum, UserIP.ToString());

                //----------------------------------------------//
                //socket for every one
                for (int i = 0; i < selectNum; i++)
                {
                    //MessageBox.Show("into send welcom msg socket", "警告", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    int thisFriendPort = int.Parse(NameList[i].Substring(5)) + 10000;

                    Socket socket_client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    IPAddress ipaddress = IPAddress.Parse(IPList[i]);
                    IPEndPoint endpoint = new IPEndPoint(ipaddress, thisFriendPort);
                    try
                    {
                        socket_client.Connect(endpoint);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "警告", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return;
                    }

                    //send msg = head+host+num+guest
                    //MessageBox.Show("准备发送群组信息", "警告", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                    string SendMsg = "Hello/P2PFileGroup/" + UserName + "/" + UserIP + "/" + (selectNum + 1).ToString();

                    for (int j = 0; j < selectNum; j++)
                    {
                        SendMsg += "/" + NameList[j];
                    }

                    byte[] Msg = Encoding.UTF8.GetBytes(SendMsg);
                    socket_client.Send(Msg);

                    //MessageBox.Show("邀请已发送到"+ NameList[i], "警告", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                    //add to socketlist
                    SocketList.Add(i, socket_client);
                }
                
                int isHost = 1;

                //set name to num bin
                UserToNum.Clear();

                for (int j = 0; j < selectNum; j++)
                {
                    UserToNum.Add(NameList[j], j+1);
                }
                UserToNum.Add(NameList[selectNum], 0); //host is 0

                GroupFile Newchat = new GroupFile(SocketList, NameList, IPList, isHost, UserName, UserToNum);

                //MessageBox.Show("socket num"+ SocketList.Count().ToString(), "警告", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                //note: ipnum = namenum, socketnum = namenum-1
                Newchat.ShowDialog();

            }
           
        }

        // Function 7: Start UDP Single Chat
        private void StartSingleChatUDP()
        {
            if (Global.CurFriendName == Global.User)
            {
                MessageBox.Show("未设定聊天对象/聊天对象为自己", "错误", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            // neglet offline
            else
            {
                // get friend info
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

                    //use TCP to get ack
                    Socket socket_client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    IPAddress ipaddress = IPAddress.Parse(FriendIP);
                    IPEndPoint endpoint = new IPEndPoint(ipaddress, FriendPort);

                    try
                    {
                        socket_client.Connect(endpoint);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("连接失败(尝试请求对方开启UDP服务失败)", "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return;
                    }

                    //send signal
                    string HelloMsg = "Hello/" + "UDPSingle/" + UserName + "/" + UserIP;
                    byte[] HelloByte = Encoding.UTF8.GetBytes(HelloMsg);
                    socket_client.Send(HelloByte);

                    //receive ans
                    byte[] AnsMsg = new byte[50];
                    string AnsString = Encoding.UTF8.GetString(AnsMsg, 0, socket_client.Receive(AnsMsg));

                    //read ans
                    if (AnsString == "Accept")
                    {
                        SingleChatUDP NewChat = new SingleChatUDP(FriendName, FriendIP, UserName, UserIP.ToString());
                        NewChat.ShowDialog();
                    }
                    else if (AnsString == "Decline")
                    {
                        MessageBox.Show("UDP会话被拒绝", "通知", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }

            }
            
        }

        // Function 0: Log out
        private void LogOut()
        {
            IPEndPoint endpoint = new IPEndPoint(ServerIP, 8000);
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);

            string SendMsg = "logout" + UserName;
            byte[] RecByte = new byte[50];
            byte[] SendByte = Encoding.ASCII.GetBytes(SendMsg);

            string RecString;

            try
            {
                sock.Connect(endpoint);
            }
            catch
            {
                MessageBox.Show(this, "TCP连接失败","错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            sock.Send(SendByte);
            RecString = Encoding.ASCII.GetString(RecByte, 0, sock.Receive(RecByte));

            if (RecString == "loo")
            {
                MessageBox.Show(this, "已从服务器登出", "通知", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            else
                MessageBox.Show("未知错误");
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        // F1 Button
        private void button4_Click(object sender, EventArgs e)
        {
            SearchFunctionAdd();
        }

        // F0 Button
        private void button9_Click(object sender, EventArgs e)
        {
            LogOut();
        }

        // F4 Button
        private void button7_Click(object sender, EventArgs e)
        {
            mode = "SingleChat";
            ShowAllFriendList();
            //StartSingleChat();
        }

        // F5 Button
        private void button8_Click(object sender, EventArgs e)
        {
            mode = "GroupChat";
            ShowAllFriendList();
        }

        private void FriendListView_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        // TCP Chat Button
        private void TCPChatButton_Click(object sender, EventArgs e)
        {
            if(mode == "SingleChat")
            {
                if (FriendListView.CheckedItems.Count == 1)
                {
                    Global.CurFriendName = FriendListView.CheckedItems[0].SubItems[0].Text;
                    StartSingleChat();
                }
                else
                {
                    ;
                }
            }           
            else if(mode == "GroupChat")
            {
                StartGroupChat();
            }
            else if(mode == "P2PGroupFile")
            {
                P2PGroupFile();
            }
            else {; }
           
        }

        private void UserNameLabel_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.FriendListView.Hide();
            this.UDPChatButton.Hide();
            this.TCPChatButton.Hide();
            this.button1.Hide();
            this.button2.Hide();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            mode = "P2PGroupFile";
            ShowAllFriendList();
        }

        private void UDPChatButton_Click(object sender, EventArgs e)
        {
            if (mode == "SingleChat")
            {
                if (FriendListView.CheckedItems.Count == 1)
                {
                    Global.CurFriendName = FriendListView.CheckedItems[0].SubItems[0].Text;
                    StartSingleChatUDP();
                }
                else
                {
                    ;
                }
            }

            else
                ;//sorry, need to done after
        }

        private void button5_Click(object sender, EventArgs e)
        {
            
            this.button2.Show();            
            ShowAllFriendList();
            this.TCPChatButton.Hide();
            this.UDPChatButton.Hide();
        }

        // delete
        private void button2_Click(object sender, EventArgs e)
        {
            int selectNum = FriendListView.CheckedItems.Count;

            if (selectNum < 1)
            {
                MessageBox.Show("未选择好友", "通知", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            else
            {
                Dictionary<int, Socket> SocketList = new Dictionary<int, Socket>();
                Dictionary<int, string> NameList = new Dictionary<int, string>();
                Dictionary<int, string> IPList = new Dictionary<int, string>();

                //get friends names
                for (int i = 0; i < selectNum; i++)
                {
                    NameList.Add(i, FriendListView.CheckedItems[i].Text);
                }

                //delete
                for(int i = 0; i < selectNum; i++)
                {
                    string thisFriendPath = FriendPath + NameList[i].ToString() + ".txt";

                    System.IO.File.Delete(thisFriendPath);
                }                
            }

            this.button2.Show();
            ShowAllFriendList();
            this.TCPChatButton.Hide();
            this.UDPChatButton.Hide();
        }

        private void button3_Click(object sender, EventArgs e)
        {

        }

        private void SendBotton_Click(object sender, EventArgs e)
        {

        }

        private void FriendNameLabel_Click(object sender, EventArgs e)
        {

        }
    }
}
