using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    class Global
    {
        //current user info
        public static string User { get; set; }//当前账号
        public static string UserIP { get; set; }//当前ip
        public static string ServerIP { get; set; }//166.111.140.57
        public static int ServerPort { get; set; }//8000
        public static bool Login_cls { get; set; }
    
        // most current chat info
        public static string CurFriendName{ get; set; }
        public static string CurFriendIP { get; set; }
        //public static bool Chatting { get; set; }
        public static int CurFriendPort { get; set; }//11438
    }
}
