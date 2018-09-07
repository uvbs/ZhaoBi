using HttpCodeLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ZhaoBi.Controller.Ma
{
    public class GetMa
    {
        public static string GlobalToken { get; private set; }
        public const string proJetId = "13088";
        /// <summary>
        /// 当前获取到的验证码
        /// </summary>
        public string CurMsg { get; private set; }
        public string CurPhone { get; private set; }
        /// <summary>
        /// 平台登录
        /// </summary>
        /// <param name="apiId">账号API</param>
        /// <param name="pswd">账号密码</param>
        /// <returns></returns>
        public static bool Login(string apiId, string pswd)
        {
            var result = string.Empty;
            HttpHelpers http = new HttpHelpers();
            HttpItems item = new HttpItems();
            item.URL = $"http://api.ndd001.com/do.php?action=loginIn&name={apiId}&password={pswd}";
            try
            {
                result = http.GetHtml(item).Html;
#if DEBUG
                Console.WriteLine($"{nameof(Login)} result:{result}");
#endif
            }
            catch (Exception ex)
            {
                ;
#if DEBUG
                Console.WriteLine($"{nameof(Login)} ex :{ ex.Message}");
#endif
            }

            if (result.Contains("1|"))
            {
                GlobalToken = result.Replace("1|", "");
                return true;
            }
            return false;
        }
        /// <summary>
        /// 获取手机号
        /// </summary>
        /// <returns>成功返回true,可用CurPhone获取</returns>
        public bool GetPhone()
        {
            var result = string.Empty;
            HttpHelpers http = new HttpHelpers();
            HttpItems item = new HttpItems();
            item.URL = "http://api.ndd001.com/do.php?action=getPhone&sid=" + proJetId + "&token=" + GlobalToken;
            try
            {
                result = http.GetHtml(item).Html;
#if DEBUG
                Console.WriteLine($"{nameof(GetPhone)} result:{result}");
#endif
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine($"{nameof(GetPhone)} result:{ex.Message}");
#endif
            }
            if (result.Contains("1|"))
            {
                this.CurPhone = result.Replace("1|", "");
                return true;
            }
            return false;
        }
        /// <summary>
        /// 获取收到的短信
        /// </summary>
        /// <returns>成功返回true,可用CurMsg获取</returns>
        public bool GetMessAge()
        {
            var result = string.Empty;
            HttpHelpers http = new HttpHelpers();
            HttpItems item = new HttpItems();
            item.URL = "http://api.ndd001.com/do.php?action=getMessage&sid=" + proJetId + "&phone=" + CurPhone + "&token=" + GlobalToken;
            try
            {
                result = http.GetHtml(item).Html;
#if DEBUG
                Console.WriteLine($"{nameof(GetMessAge)} result:{result}");
#endif
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine($"{nameof(GetMessAge)} result:{ex.Message}");
#endif
            }
            var mList = Regex.Matches(result, "本次验证码为: (\\d+),请");
            if (mList.Count > 0)
            {
                CurMsg = mList[0].Groups[1].Value;
#if DEBUG
                Console.WriteLine($"CurMsg :{CurMsg}");
#endif
                return true;
            }
            return false;
        }
        /// <summary>
        /// 拉黑手机号
        /// </summary>
        public void AddBlack()
        {
            HttpHelpers http = new HttpHelpers();
            HttpItems item = new HttpItems();
            try
            {
                item.URL = "http://api.ndd001.com/do.php?action=addBlacklist&sid=" + proJetId + "&phone=" + CurPhone + "&token=" + GlobalToken;
                var result = http.GetHtml(item).Html;
            }
            catch { }
        }
    }
}
