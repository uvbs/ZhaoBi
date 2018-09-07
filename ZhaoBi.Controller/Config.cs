using System.Configuration;
using System.IO;

namespace ZhaoBi.Controller
{
    public class Config
    {

        public static string UserApi;
        public static string PassWord;


        /// <summary>
        /// 宽带连接名称
        /// </summary>
        public static string BroadbandName;
        /// <summary>
        /// 宽带连接账号
        /// </summary>
        public static string BroadbandUser;
        /// <summary>
        /// 宽带连接密码
        /// </summary>
        public static string BroadbandPswd;
        /// <summary>
        /// 邀请注册码
        /// </summary>
        public static string RegisCode;
        /// <summary>
        /// 等待超时时间
        /// </summary>
        public static int WaitTime;
        /// <summary>
        /// 注册成功文件夹
        /// </summary>
        public const string SUCC = "SUCC";
        /// <summary>
        /// 注册失败文件夹
        /// </summary>
        public const string FAIL = "FAIL";
        /// <summary>
        /// 加载配置文件,读取配置文件
        /// </summary>
        static Config()
        {
            Directory.CreateDirectory(SUCC);
            Directory.CreateDirectory(FAIL);
            BroadbandName = ConfigurationManager.AppSettings.Get("BroadbandName");
            BroadbandUser = ConfigurationManager.AppSettings.Get("BroadbandUser");
            BroadbandPswd = ConfigurationManager.AppSettings.Get("BroadbandPswd");
            BroadbandPswd = ConfigurationManager.AppSettings.Get("BroadbandPswd");
            UserApi = ConfigurationManager.AppSettings.Get("UserApi");
            PassWord = ConfigurationManager.AppSettings.Get("PassWord");
            WaitTime = int.Parse(ConfigurationManager.AppSettings.Get("WaitTime"));
            RegisCode = ConfigurationManager.AppSettings.Get("RegisCode");
        }
    }
}
