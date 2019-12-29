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
    public partial class GroupFile : Form
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
        Thread ThreadFileCat;

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

        //add p2p group file
        string P2PGroupFilePath;
        Dictionary<string, int> UserToNum;
        int MyGroupNum;
        Socket Client=null;
        Dictionary<int, Socket> PeersSockerList;
        Dictionary<int, Socket> PeersSockerList_Rev;
        string[] PeersNameList;
        string[] PeersNameList_Rev;

        int NumInPeerSocker = 0;
        int NumInPeerSocker_Rev = 0;

        Socket SocketListen; // only listen to peer.
        Socket ServerConnect = null;

        long MyPartLength = 0;
        string MyPartName = "";
        string MyPartPath = "";

        string[] PartHeadString;
        string[] PartFileName;
        long[] PartFileLength;

        //string P2PGroupFilePath = ".\\" + Global.User + "\\P2PGroupFile\\";

        public GroupFile(Dictionary<int, Socket> socketlist, Dictionary<int, string> namelist, 
             Dictionary<int, string> iplist, int ishost, string username, Dictionary<string, int> usertonum)
        {
            InitializeComponent();

            //creat file folder            
            P2PGroupFilePath = ".\\" + username + "\\P2PGroupFile\\";
            SetupP2PGroupFileDic(P2PGroupFilePath);

            //all need
            UserToNum = usertonum;

            // link to server for search
            IPAddress ServerIP = IPAddress.Parse(Global.ServerIP);
            int ServerPort = Global.ServerPort;

            Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ipaddress = IPAddress.Parse(Global.ServerIP);
            IPEndPoint endpoint = new IPEndPoint(ipaddress, ServerPort);
            Client.Connect(endpoint);

            //prepare to receive, for client
            string[] PartHeadString = new string[FriendsNum];
            string[] PartFileName = new string[FriendsNum];
            long[] PartFileLength = new long[FriendsNum];

            if (ishost == 1)
            {
                //set
                MyGroupNum = 0;

                GroupNum = namelist.Count();
                FriendsNum = GroupNum - 1;

                SocketList = socketlist;
                NameList = namelist;
                isHost = ishost;
                IPList = iplist;
                UserName = username;
                UserIP = IPList[FriendsNum];              

                this.FriendNameLabel.Text = "P2P文件传输-源文件端（" + GroupNum + "人）";

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

                //wait
                System.Threading.Thread.Sleep(2000);


                //TODO should ack all in, but no time
                for (int i = 0; i < FriendsNum; i++)
                {
                    byte[] SendMsg = Encoding.UTF8.GetBytes("/NetB/");
                    SocketList[i].Send(SendMsg);
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
                PeersSockerList = new Dictionary<int, Socket>();
                PeersSockerList_Rev = new Dictionary<int, Socket>();
                PeersNameList = new string[FriendsNum -1];
                PeersNameList_Rev = new string[FriendsNum - 1];

                //find my num, this num is the open socket number.
                MyGroupNum = UserToNum[UserName];

                this.FriendNameLabel.Text = "P2P文件传输-客户端" + MyGroupNum.ToString()+"（" + GroupNum + "人）";

                // show friends
                UpdateFriendList();

                CheckForIllegalCrossThreadCalls = false;

                this.UserNameLabel.Text = username;
                               
                // all need to listen host
                ParameterizedThreadStart pts = new ParameterizedThreadStart(ReceiveMsg);
                Thread thread = new Thread(pts);
                thread.IsBackground = true;
                thread.Start(SocketList[0]);
                ThreadList.Add(0, thread);

                // listen to peers
                SocketListen = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                // Lock to endpoint
                int UserSpecialPort = int.Parse(UserName.Substring(5)) + 5000; //11438~11442+500
                try
                {
                    IPAddress UserIP_tmp = IPAddress.Parse(UserIP);

                    endpoint = new IPEndPoint(UserIP_tmp, UserSpecialPort);
                    SocketListen.Bind(endpoint);
                    SocketListen.Listen(20);
                    ThreadListen = new Thread(ListenConnecting);
                    ThreadListen.IsBackground = true;
                    ThreadListen.Start();

                    //MessageBox.Show("Peer监听打开"+ ipaddress.ToString() + ":"+ UserSpecialPort.ToString(), "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); 
                }
                catch
                {
                    MessageBox.Show("Peer监听打开失败"+ UserIP+":"+ UserSpecialPort.ToString(), "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); 
                }

                ThreadFileCat = new Thread(ConcatFile);
                ThreadFileCat.IsBackground = true;
                ThreadFileCat.Start();

                //System.Threading.Thread.Sleep(2000); //wait 2s for all socket open               
            }
            else
            {
                MessageBox.Show("未设置ishost", "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }           

        }

        //Function 12: cat file
        private void ConcatFile()
        {
            DirectoryInfo folder = new DirectoryInfo(P2PGroupFilePath);
            string real_filename = "";
            int partNum = 1;
            string[] partfilename_bin = new string[FriendsNum];
            int index = 0;
            string tmp_filename;
            string[] filenamepart;

            string savePath;

            while (true)
            {
                Thread.Sleep(500);
                //list all part file
                
                foreach (FileInfo file in folder.GetFiles())
                {
                    tmp_filename = file.FullName;
                    filenamepart = tmp_filename.Split('.');
                    partNum = (int)Convert.ToInt64(filenamepart[2]);

                    partfilename_bin[partNum-1] = tmp_filename;
                    real_filename = filenamepart[0] + "." + filenamepart[1];

                    index++;
                }

                if(index == FriendsNum)
                {
                    Thread.Sleep(2000);

                    savePath = real_filename;

                    //MessageBox.Show(real_filename+" "+ partfilename_bin[0]+ partfilename_bin[1], 
                    //    "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                    FileStream fs = new FileStream(savePath, FileMode.Create, FileAccess.Write);

                    for (int i = 0; i < FriendsNum; i++)
                    {
                        byte[] buffer = new byte[1024*1024];
                        string filePath = partfilename_bin[i];

                        //long fileLength = 0;
                        int readLength = 0;
                        long sentFileLength = 0;

                        FileStream FS = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                        while ((readLength = FS.Read(buffer, 0, buffer.Length)) > 0)// && sentFileLength < fileLength)
                        {
                            //MessageBox.Show(readLength.ToString(), "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                            sentFileLength += readLength;

                            fs.Write(buffer, 0, readLength);
                            fs.Flush();
                        }
                        FS.Close();
                    }

                    fs.Close();

                    break;
                }
                else
                {
                    index = 0;
                }
            }         
        }

        //Function 11: build net
        private void P2PNetBuild()
        {
            // get link socket               
            for (int i = 1; i < (FriendsNum+1); i++)
            {
                // get peer name
                if (i != MyGroupNum)
                {
                    string PeerName = GetPeerName(i);
                    //MessageBox.Show(UserName + "into get peer name", "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                    while (GetPeerSocket(PeerName) == 0) { System.Threading.Thread.Sleep(100); }
                }               
            }

            System.Threading.Thread.Sleep(1000); //wait for all socket create

            // listen to all peer socket
            
            for (int i = 1; i < FriendsNum; i++)
            {
                ParameterizedThreadStart pts0 = new ParameterizedThreadStart(ReceiveMsg);
                Thread thread0 = new Thread(pts0);
                thread0.IsBackground = true;
                thread0.Start(PeersSockerList[i - 1]);
                ThreadList.Add(i, thread0);
            }

            for (int i = 1; i < FriendsNum; i++)
            {
                ParameterizedThreadStart pts0 = new ParameterizedThreadStart(ReceiveMsg);
                Thread thread0 = new Thread(pts0);
                thread0.IsBackground = true;
                thread0.Start(PeersSockerList_Rev[i - 1]);
                ThreadList.Add(i+ FriendsNum-1, thread0);
            }

            //TODO!!

        }

        // Function 10: Peer to Peer
        private void P2PNetBegin()
        {
            for(int i=0;i<PeersSockerList.Count;i++)
            {
                //MessageBox.Show(UserName+"send to" + PeersNameList[i], "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                P2PSent(MyPartPath, PeersSockerList[i], PeersNameList[i]);
            }
        }

        private void P2PSent(string mypartfilepath, Socket tmp_socket, string tmp_friendname)
        {
            //Send File Head
            long fileLength = new FileInfo(mypartfilepath).Length;
            string fileHead = MyPartName + "/" + MyPartLength;
            byte[] SendfileHead = Encoding.UTF8.GetBytes("/PPHd/" + fileHead);
            tmp_socket.Send(SendfileHead);


            //Send main file
            byte[] buffer = new byte[SendBufferSize]; // 1024 1024

            using (FileStream FS = new FileStream(mypartfilepath, FileMode.Open, FileAccess.Read))
            {
                int readLength = 0;
                int bufferNum = 0;
                long sentFileLength = 0;
                byte[] MainFileHead = Encoding.UTF8.GetBytes("/PPMa/");

                while ((readLength = FS.Read(buffer, 0, buffer.Length)) > 0 && sentFileLength < fileLength)
                {
                    sentFileLength += readLength;
                    if (bufferNum == 0) // add head
                    {
                        byte[] buffer_with_head = new byte[readLength + 6];
                        Buffer.BlockCopy(MainFileHead, 0, buffer_with_head, 0, 6);
                        Buffer.BlockCopy(buffer, 0, buffer_with_head, 6, readLength);
                        tmp_socket.Send(buffer_with_head, 0, readLength + 6, SocketFlags.None);
                    }
                    else
                    {
                        tmp_socket.Send(buffer, 0, readLength, SocketFlags.None);
                    }
                    bufferNum += 1;

                    //show
                    progressBar2.Value = 100 * (int)sentFileLength / (int)fileLength;
                }
                FS.Close();
            }

            progressBar2.Value = 0; //send bar

            ChatBox.SelectionAlignment = HorizontalAlignment.Center;
            ChatBox.SelectionColor = Color.Gray;
            ChatBox.AppendText(GetCurrentTime() + "\r\n" + UserName + "(我)发起了文件传输： " + MyPartName + "到"+tmp_friendname+"\r\n");
        }

        //Function 9: see how much peer is link
        private void ShowPeerLinkNum()
        {
            MessageBox.Show(ThreadList.Count().ToString(), "通知", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        // Function 7: for peer get socket
        private int GetPeerSocket(string FriendName)
        {
            int flag = 0;

            // get friend info
            string SendMsg = 'q' + FriendName;
            byte[] SendByte = Encoding.UTF8.GetBytes(SendMsg);
            Client.Send(SendByte);

            byte[] RevMsg = new byte[50];
            string RevString = Encoding.UTF8.GetString(RevMsg, 0, Client.Receive(RevMsg));

            if (RevString == "n")
            {
                System.Windows.Forms.MessageBox.Show("用户"+FriendName+"不在线", "通知", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else if (RevString == "Incorrect No.")
            {
                System.Windows.Forms.MessageBox.Show("用户" + FriendName + "不存在", "通知", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
            {
                //get friend ip                    
                Global.CurFriendIP = RevString;
                string FriendIP = Global.CurFriendIP;
                int FriendPort = int.Parse(FriendName.Substring(5)) + 5000;

                Socket socket_client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress ipaddress = IPAddress.Parse(FriendIP);
                IPEndPoint endpoint = new IPEndPoint(ipaddress, FriendPort);

                try
                {
                    socket_client.Connect(endpoint);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(UserName+"连接"+FriendName+"失败", "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return flag;
                }

                //send signal
                string HelloMsg = "Hello/" + "P2PGroupPeer/" + UserName + "/" + UserIP;
                byte[] HelloByte = Encoding.UTF8.GetBytes(HelloMsg);

                //MessageBox.Show(UserName + "连接" + FriendName + ":send hello", "通知", MessageBoxButtons.OK, MessageBoxIcon.Information);
                socket_client.Send(HelloByte);

                //receive ans
                byte[] AnsMsg = new byte[50];
                string AnsString = Encoding.UTF8.GetString(AnsMsg, 0, socket_client.Receive(AnsMsg));

                //read ans
                if (AnsString == "Accept")
                {
                    PeersSockerList.Add(NumInPeerSocker, socket_client);
                    PeersNameList[NumInPeerSocker] = FriendName;

                    NumInPeerSocker++;
                    flag = 1;

                    // send to host
                    string LinkMsg = "/PeLk/" + UserName + "link to" + FriendName;
                    byte[] LinkByte = Encoding.UTF8.GetBytes(LinkMsg);
                    //MessageBox.Show(UserName + "连接" + FriendName + "成功", "通知", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    SocketList[0].Send(LinkByte);
                }
                else if (AnsString == "Decline")
                {
                    //MessageBox.Show(UserName + "连接" + FriendName + "被拒绝", "通知", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }

            return flag;

            
        }

        // Function 8: listen to peer
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
                if (signal == "Hello" && type == "P2PGroupPeer")
                {
                    byte[] AcceptMsg = Encoding.UTF8.GetBytes("Accept");
                    ServerConnect.Send(AcceptMsg);

                    PeersSockerList_Rev.Add(NumInPeerSocker_Rev, ServerConnect);
                    PeersNameList_Rev[NumInPeerSocker_Rev] = FriendName;

                    NumInPeerSocker_Rev++;
                }
                else
                {
                    ;
                }
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

            string TextString="unkonwn_text";
            string FileHeadString="unknown_head";
            string filename="unknown_file.part";
            string sender;
            long fileLength=0;
            string isHostSig;

            byte[] buffer = new byte[ReceiveBufferSize];

            Socket SocketPt = SocketThread as Socket;

            //add for peer
            string Peername = "";

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

                        else if(MsgHead == "/FPHd/")
                        {
                            FileHeadString = Encoding.UTF8.GetString(buffer, 6, LengthMsg - 6);
                            filename = FileHeadString.Split('/').First();
                            fileLength = Convert.ToInt64(FileHeadString.Split('/').Last());

                            MyPartLength = fileLength;
                            MyPartName = filename;
                        }

                        else if(MsgHead == "/FPMa/")                          
                        {
                            //MessageBox.Show("文件接受,长度"+fileLength, "通知",MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                            //string fileNameSuffix = filename.Substring(filename.LastIndexOf('.'));//suffix

                            string savePath = P2PGroupFilePath+ filename;
                            int received = 0;
                            long RevFileLength = 0;
                            int bufferNum = 0;

                            MyPartPath = savePath;
                            //MessageBox.Show("开始写入", "通知", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            //directly write
                            using (FileStream fs = new FileStream(savePath, FileMode.Create, FileAccess.Write))
                            {
                                while (RevFileLength < fileLength)
                                {
                                    if (bufferNum == 0)
                                    {
                                        fs.Write(buffer, 6, LengthMsg - 6); //rm head: /File/
                                        fs.Flush();
                                        RevFileLength += LengthMsg - 6;
                                    }
                                    received = SocketPt.Receive(buffer); // direct
                                    fs.Write(buffer, 0, received);
                                    fs.Flush();

                                    RevFileLength += received;
                                    bufferNum += 1;

                                    //debug
                                    //MessageBox.Show("写入"+ buffer.Length.ToString(), "通知", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                    //show
                                    progressBar1.Value = (int)(100 * RevFileLength / fileLength);
                                }
                                fs.Close();

                                //MessageBox.Show("写入完成", "通知", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }

                            progressBar1.Value = 0; //reveive bar

                            string save_filename = savePath.Substring(savePath.LastIndexOf("\\") + 1);
                            string save_filepath = savePath.Substring(0, savePath.LastIndexOf("\\"));

                            //byte[] SendMsg = Encoding.UTF8.GetBytes("/FlAc/");
                            //ChatSocket.Send(SendMsg);

                            ChatBox.SelectionAlignment = HorizontalAlignment.Center;
                            ChatBox.SelectionColor = Color.Gray;
                            ChatBox.AppendText(GetCurrentTime() + "\r\n(我)成功接受了来自" + FriendName + "的文件" + save_filename + "\r\n");
                                                        
                        }

                        else if(MsgHead == "/PeLk/")
                        {
                            if (isHost == 1)
                            {
                                TextString = Encoding.UTF8.GetString(buffer, 6, LengthMsg - 6);

                                ChatBox.SelectionAlignment = HorizontalAlignment.Left;
                                ChatBox.SelectionColor = Color.Blue;
                                ChatBox.AppendText("P2P Net Information" + GetCurrentTime() + " " + "\r\n");

                                ChatBox.SelectionAlignment = HorizontalAlignment.Left;
                                ChatBox.SelectionColor = Color.Black;
                                ChatBox.AppendText(TextString + "\r\n");

                                /* TODO
                                byte[] SendMsg = Encoding.UTF8.GetBytes("/Text/" + sender + "00" + TextString);

                                //help send but no himself
                                for (int i = 0; i < FriendsNum && NameList[i] != sender; i++)
                                {

                                    SocketList[i].Send(SendMsg);
                                }
                                */

                            }
                        }

                        else if(MsgHead == "/PPHd/")
                        {
                            //continue;
                            // find the order
                            /*
                            string tmp_PartHeadString = Encoding.UTF8.GetString(buffer, 6, LengthMsg - 6);
                            string tmp_file_name = FileHeadString.Split('/').First();
                            long tmp_file_length = Convert.ToInt64(FileHeadString.Split('/').Last());

                            string tmp_num = tmp_file_name.Split('.').Last();
                            int tmp_num_int = (int)Convert.ToInt64(tmp_file_name.Split('.').Last());

                            PartFileName[tmp_num_int] = tmp_file_name;
                            PartFileLength[tmp_num_int] = tmp_file_length;
                            */

                            // maybe it will work. in the same thread, the filename should be the same.                           
                            string tmp_PartHeadString = Encoding.UTF8.GetString(buffer, 6, LengthMsg - 6);
                            FileHeadString = tmp_PartHeadString;

                            string tmp_file_name = FileHeadString.Split('/').First();

                            string tmp_num = tmp_file_name.Split('.').Last();
                            int tmp_num_int = (int)Convert.ToInt64(tmp_file_name.Split('.').Last());

                            //MessageBox.Show("PeerHead received, part="+tmp_num_int.ToString(), "通知", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            Peername = GetPeerName(tmp_num_int);

                            filename = FileHeadString.Split('/').First();
                            fileLength = Convert.ToInt64(FileHeadString.Split('/').Last());
                        }

                        else if (MsgHead == "/PPMa/")
                        {
                            string savePath = P2PGroupFilePath + filename;
                            int received = 0;
                            long RevFileLength = 0;
                            int bufferNum = 0;

                            MyPartPath = savePath;

                            //MessageBox.Show("开始写入", "通知", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            //directly write
                            using (FileStream fs = new FileStream(savePath, FileMode.Create, FileAccess.Write))
                            {
                                while (RevFileLength < fileLength)
                                {
                                    if (bufferNum == 0)
                                    {
                                        fs.Write(buffer, 6, LengthMsg - 6); //rm head: /File/
                                        fs.Flush();
                                        RevFileLength += LengthMsg - 6;
                                    }
                                    received = SocketPt.Receive(buffer); // direct
                                    fs.Write(buffer, 0, received);
                                    fs.Flush();

                                    RevFileLength += received;
                                    bufferNum += 1;

                                    //debug
                                    //MessageBox.Show("写入"+ buffer.Length.ToString(), "通知", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                    //show
                                    progressBar1.Value = (int)(100 * RevFileLength / fileLength);
                                }
                                fs.Close();

                                //MessageBox.Show("写入完成", "通知", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }

                            progressBar1.Value = 0; //reveive bar

                            string save_filename = savePath.Substring(savePath.LastIndexOf("\\") + 1);
                            string save_filepath = savePath.Substring(0, savePath.LastIndexOf("\\"));

                            //byte[] SendMsg = Encoding.UTF8.GetBytes("/FlAc/");
                            //ChatSocket.Send(SendMsg);

                            ChatBox.SelectionAlignment = HorizontalAlignment.Center;
                            ChatBox.SelectionColor = Color.Gray;
                            ChatBox.AppendText(GetCurrentTime() + "\r\n(我)成功接受了来自" + Peername + "的文件" + save_filename + "\r\n");
                        }

                        else if (MsgHead == "/PPSt/")
                        {
                            P2PNetBegin();
                        }

                        else if (MsgHead == "/NetB/")
                        {
                            P2PNetBuild();
                        }
                       
                        //quit info
                        else if (MsgHead == "/Quit/")
                        {
                            MessageBox.Show("P2P网络成员"+sender+"已经退出，全网络关闭", "通知", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            if(MsgType == "Text")
            {
                ;
            }

            else if(MsgType == "FileHost")
            {
                string fileName = null;
                string filePath = null;

                OpenDialog = new OpenFileDialog();

                ThreadFile2 = new Thread(OpenDialogCall);
                ThreadFile2.SetApartmentState(ApartmentState.STA);
                ThreadFile2.Start();
                ThreadFile2.Join();

                if (OpenDialogResult == DialogResult.OK)
                {
                    if (OpenDialog.FileName != "")
                    {
                        fileName = OpenDialog.SafeFileName;
                        filePath = OpenDialog.FileName;

                        //Send File Head
                        long fileLength = new FileInfo(filePath).Length;

                        // set part size
                        long[] filePartEndLength = new long[FriendsNum];
                        long[] filePartLength = new long[FriendsNum];

                        for (int i=0; i<FriendsNum;i++)
                        {
                            filePartEndLength[i] = fileLength / (long)FriendsNum * (i+1);
                        }
                        //set final part
                        filePartEndLength[FriendsNum-1] = fileLength;

                        for (int i = 0; i < FriendsNum; i++)
                        {
                            filePartLength[i] = filePartEndLength[0];
                        }

                        //for debug

                        if (FriendsNum > 1)
                            filePartLength[FriendsNum - 1] = fileLength - filePartEndLength[FriendsNum - 2];
                        else
                            filePartLength[FriendsNum - 1] = fileLength;

                        //MessageBox.Show("set part length finish, "+"part:"+FriendsNum.ToString(), "通知", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        //Send main file, set n part
                        byte[] buffer = new byte[SendBufferSize]; // 1024 1024

                        // set part size
                        using (FileStream FS = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                        {
                            for (int i = 0; i < FriendsNum; i++)
                            {                              
                                long thisPartLength = filePartLength[i];

                                //MessageBox.Show("part to" + NameList[i] + " "+"length: "+thisPartLength.ToString(), "通知", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                //Send File Head
                                string partfileName = fileName + "."+ (i+1).ToString();
                                string partfileHead = partfileName + "/" + thisPartLength;

                                byte[] SendfileHead = Encoding.UTF8.GetBytes("/FPHd/" + partfileHead);
                                SocketList[i].Send(SendfileHead);

                                // send main
                                if (thisPartLength > buffer.Length)
                                {
                                    int cur_part_readLength = 0;
                                    long cur_part_sentFileLength = 0;
                                    int cur_can_read_length = buffer.Length;
                                    int cur_bufferNum = 0;
                                    byte[] PartFileHead = Encoding.UTF8.GetBytes("/FPMa/");

                                    while ((cur_part_readLength = FS.Read(buffer, 0, cur_can_read_length)) > 0 && cur_part_sentFileLength < thisPartLength)
                                    {
                                        //MessageBox.Show("read buffer from file:" + cur_part_readLength.ToString(), "通知", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                        cur_part_sentFileLength += cur_part_readLength;
                                        if (cur_bufferNum == 0) // add head
                                        {
                                            //MessageBox.Show("into " + cur_part_readLength.ToString(), "通知", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                            byte[] buffer_with_head = new byte[cur_part_readLength + 6];
                                            Buffer.BlockCopy(PartFileHead, 0, buffer_with_head, 0, 6);

                                            Buffer.BlockCopy(buffer, 0, buffer_with_head, 6, cur_part_readLength);
                                            SocketList[i].Send(buffer_with_head, 0, cur_part_readLength + 6, SocketFlags.None);

                                            //MessageBox.Show("buffer sent" + cur_part_readLength.ToString(), "通知", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                        }
                                        else
                                        {
                                            //MessageBox.Show("buffer pre to send", "通知", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                            //byte[] buffer_no_head = new byte[cur_part_readLength];
                                            //Buffer.BlockCopy(buffer, 0, buffer_no_head, 0, cur_part_readLength);

                                            SocketList[i].Send(buffer, 0, cur_part_readLength, SocketFlags.None);

                                            //MessageBox.Show("buffer sent" + cur_part_readLength.ToString(), "通知", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                        }
                                        cur_bufferNum += 1;

                                        if (thisPartLength - cur_part_sentFileLength > buffer.Length)
                                            cur_can_read_length = buffer.Length;
                                        else
                                            cur_can_read_length = (int)(thisPartLength - cur_part_sentFileLength);

                                        //MessageBox.Show("next can read buffer " + cur_can_read_length.ToString(), "通知", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                        //show
                                        progressBar2.Value = (int)(100 * cur_part_sentFileLength / thisPartLength);

                                    }
                                }
                                else
                                {
                                    int cur_part_readLength = 0;
                                    long cur_part_sentFileLength = 0;
                                    int cur_can_read_length = (int)thisPartLength;
                                    int cur_bufferNum = 0;
                                    byte[] PartFileHead = Encoding.UTF8.GetBytes("/FPHd/");

                                    while ((cur_part_readLength = FS.Read(buffer, 0, cur_can_read_length)) > 0 && cur_part_sentFileLength < thisPartLength)
                                    {
                                        //MessageBox.Show("read buffer from file:" + cur_part_readLength.ToString(), "通知", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                        cur_part_sentFileLength += cur_part_readLength;
                                        if (cur_bufferNum == 0) // add head
                                        {
                                            byte[] buffer_with_head = new byte[cur_part_readLength + 6];
                                            Buffer.BlockCopy(PartFileHead, 0, buffer_with_head, 0, 6);

                                            Buffer.BlockCopy(buffer, 0, buffer_with_head, 6, cur_part_readLength);
                                            SocketList[i].Send(buffer_with_head, 0, cur_part_readLength + 6, SocketFlags.None);
                                        }
                                        else
                                        {
                                            SocketList[i].Send(buffer, 0, cur_part_readLength, SocketFlags.None);
                                            //MessageBox.Show("buffer sent" + cur_part_readLength.ToString(), "通知", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                        }
                                        cur_bufferNum += 1;

                                        cur_can_read_length = (int)(thisPartLength - cur_part_sentFileLength);

                                        //show
                                        progressBar2.Value = (int)(100 * cur_part_sentFileLength / thisPartLength);
                                    }
                                }

                                //MessageBox.Show("对"+NameList[i]+"的部分上传成功", "通知", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }                           
                            FS.Close();
                        }

                        MessageBox.Show("所有部分上传成功", "通知", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        progressBar2.Value = 0; //send bar

                        for (int i=0;i<FriendsNum;i++)
                        {
                            byte[] startCmd = Encoding.UTF8.GetBytes("/PPSt/");
                            SocketList[i].Send(startCmd);
                        }

                        ChatBox.SelectionAlignment = HorizontalAlignment.Center;
                        ChatBox.SelectionColor = Color.Gray;
                        ChatBox.AppendText(GetCurrentTime() + "\r\n" + UserName + "(我)发起了P2P多人文件传输： " + fileName + "\r\n");                                               
                    }
                }
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
                if (isHost == 1)
                {
                    byte[] SendExit = Encoding.UTF8.GetBytes("/Quit/" + UserName + "11");
                    for (int i = 0; i < FriendsNum; i++)
                    {
                        SocketList[i].Send(SendExit);
                    }
                }
                else
                {
                    byte[] SendExit = Encoding.UTF8.GetBytes("/Quit/" + UserName + "00");
                    SocketList[0].Send(SendExit);
                    for(int i =0;i<FriendsNum-1;i++)
                    {
                        PeersSockerList[i].Send(SendExit);
                    }                  
                }

            }
            catch
            {
                MessageBox.Show("TCP连接故障，退出失败", "通知", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            ThreadListen.Abort();
            this.Close();
        }

        //Funciton 6: setup file dictionary
        private void SetupP2PGroupFileDic(string path)
        {
            if (Directory.Exists(path))
            {
                DirectoryInfo di = new DirectoryInfo(path);
                di.Delete(true);
                Directory.CreateDirectory(path);
            }
            else
            {
                Directory.CreateDirectory(path);
            }
        }

        //Assist Function:
        private string GetPeerName(int num)
        {
            string PeerName = "";

            foreach (string key in UserToNum.Keys)
            {
                if (UserToNum[key].Equals(num))
                {
                    PeerName = key;
                    break;
                }
            }

            return PeerName;
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
            SendMsg("FileHost");
        }

        private void SendBotton_Click(object sender, EventArgs e)
        {
            SendMsg("Text");
        }

        private void button9_Click(object sender, EventArgs e)
        {

            Quit();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            ShowPeerLinkNum();
        }
    }
}
