using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Login LoginWin = new Login();
            LoginWin.ShowDialog();
            while (!Global.Login_cls) { LoginWin.ShowDialog(); }
            if (LoginWin.DialogResult == DialogResult.OK && Global.Login_cls)
            {
                LoginWin.Close();
                Application.Run(new Main());
                System.Environment.Exit(0); //关闭时结束所有进程
            }

        }
    }
}
