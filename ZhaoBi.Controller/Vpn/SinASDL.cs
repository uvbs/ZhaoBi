using Microsoft.Win32;
using System;
using System.Diagnostics;
namespace ZhaoBi.Controller.Vpn
{
    public class SinASDL
    {
        private static string _adlskeys = @"RemoteAccess\Profile";
        public static string adlskeys
        {
            get
            {
                return _adlskeys;
            }
        }
        /// <summary>
        /// 获取本机的拨号名称，也就是本机上所有的拨号
        /// </summary>
        /// <returns></returns>
        public static string[] GetASDLNames()
        {
            RegistryKey RegKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(adlskeys);
            if (RegKey != null)
                return RegKey.GetSubKeyNames();
            else
                return null;
        }
        private string _asdlname = null;
        private ProcessWindowStyle _windowstyle = ProcessWindowStyle.Hidden;
        /// <summary>
        /// 实例化一个ASDL连接
        /// </summary>
        /// <param name="asdlname">ASDL名称，如“宽带连接”</param>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="windowstyle">窗口显示方式，默认为因此拨号过程</param>
        public SinASDL(string asdlname, string username = null, string password = null, ProcessWindowStyle windowstyle = ProcessWindowStyle.Hidden)
        {
            this.ASDLName = asdlname;
            this.Username = username;
            this.Password = password;
            this.WindowStyle = windowstyle;
        }
        public SinASDL()
        {
        }
        /// <summary>
        /// 拨号名称
        /// </summary>
        public string ASDLName
        {
            get
            {
                return this._asdlname;
            }
            set
            {
                this._asdlname = value;
            }
        }
        /// <summary>
        /// 拨号进程的窗口方式
        /// </summary>
        public ProcessWindowStyle WindowStyle
        {
            get
            {
                return this._windowstyle;
            }
            set
            {
                this._windowstyle = value;
            }
        }
        private string _username = null;  //用户名
        private string _password = null;  //密码
        /// <summary>
        /// 用户名
        /// </summary>
        public string Username
        {
            get
            {
                return this._username;
            }
            set
            {
                this._username = value;
            }
        }
        /// <summary>
        /// 密码
        /// </summary>
        public string Password
        {
            get
            {
                return this._password;
            }
            set
            {
                this._password = value;
            }
        }
        /// <summary>
        /// 开始拨号
        /// </summary>
        /// <returns>返回拨号进程的返回值</returns>
        public int Connect()
        {
            Process pro = new Process();
            pro.StartInfo.FileName = "rasdial.exe";
            pro.StartInfo.Arguments = this.ASDLName + " " + this.Username + " " + this.Password;
            pro.StartInfo.WindowStyle = this.WindowStyle;
            pro.Start();
            pro.WaitForExit();
            return pro.ExitCode;
        }
        /// <summary>
        /// 端口连接
        /// </summary>
        /// <returns></returns>
        public int Disconnect()
        {
            Process pro = new Process();
            pro.StartInfo.FileName = "rasdial.exe";
            pro.StartInfo.Arguments = this.ASDLName + " /DISCONNECT";
            pro.StartInfo.WindowStyle = this.WindowStyle;
            pro.Start();
            pro.WaitForExit();
            return pro.ExitCode;
        }
    }
}
