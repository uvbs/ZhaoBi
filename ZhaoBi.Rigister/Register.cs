using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HttpCodeLib;
using Newtonsoft.Json;

namespace ZhaoBi.Rigister
{
    public class Register
    {
        /// <summary>
        /// 邀请注册码
        /// </summary>
        private readonly string _regisCode;
        /// <summary>
        /// 随机IP
        /// </summary>
        private readonly string _userIp;
        private string _phone;
        public string RegisPhone
        {
            get => _phone;
            set
            {
                _phone = value;
            }
        }
        private string _access_token;
        /// <summary>
        /// 获取参数
        /// </summary>
        public (string phone, string token) GetParamter
        {
            get
            {
                return (_phone, _access_token);
            }
        }
        private const string USERAGENT = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.106 Safari/537.36";
        /// <summary>
        /// 
        /// </summary>
        /// <param name="regisCode">邀请注册码</param>
        /// <param name="ip">随机IP</param>
        public Register(string regisCode, string ip)
        {
            this._regisCode = regisCode;
            this._userIp = ip;
        }
        /// <summary>
        /// 发送短信验证码
        /// </summary>
        /// <returns></returns>
        public bool SendMsg()
        {
            HttpHelpers http = new HttpHelpers();
            HttpItems item = new HttpItems
            {
                URL = "https://pcapi.licai.cn/api/send/newsms",
                Method = "POST",
                ContentType = "application/x-www-form-urlencoded;",
                Referer = "https://www.zhaobi.com/",
                UserAgent = USERAGENT,
                Postdata = $"platkey=zhaobi&codetype=quick&area=86&mobile={_phone}&param=FzmRandom4&ticket=&businessId="
            };
            item.Header.Add("Authorization", "Bearer null");
            item.Header.Add("FZM-REQUEST-OS", "web");
            item.Header.Add("FZM-USER-IP", _userIp);
            var result = http.GetHtml(item).Html;
#if DEBUG
            Console.WriteLine($"{nameof(SendMsg)} result:{result}");
#endif
            return result.Contains("message\":\"OK\"");
        }
        /// <summary>
        /// 注册
        /// </summary>
        /// <param name="code">短信验证码</param>
        /// <returns></returns>
        public bool Regis(string code)
        {
            HttpHelpers http = new HttpHelpers();
            HttpItems item = new HttpItems
            {
                URL = "https://pcapi.licai.cn/api/member/quickreg",
                Method = "POST",
                ContentType = "application/x-www-form-urlencoded;",
                Referer = "https://www.zhaobi.com/",
                UserAgent = USERAGENT,
                Postdata = $"type=sms&area=86&mobile={_phone}&code={code}&email=&redirect_uri=suibiantian&broker_code=&invite_code={_regisCode}"
            };
            item.Header.Add("Authorization", "Bearer null");
            item.Header.Add("FZM-REQUEST-OS", "web");
            item.Header.Add("FZM-USER-IP", _userIp);
            var result = http.GetHtml(item).Html;
#if DEBUG
            Console.WriteLine($"{nameof(Regis)} result:{result}");
#endif
            bool flag = this.Login(code);
            return result.Contains("message\":\"OK") && flag;
        }
        /// <summary>
        /// 获取登录accesstoken参数
        /// </summary>
        /// <param name="code">短信验证码</param>
        /// <returns></returns>
        private bool Login(string code)
        {
            HttpHelpers http = new HttpHelpers();
            HttpItems item = new HttpItems
            {
                URL = "https://pcapi.licai.cn/api/member/quicklogin",
                Method = "POST",
                ContentType = "application/x-www-form-urlencoded;",
                Referer = "https://www.zhaobi.com/",
                UserAgent = USERAGENT,
                Postdata = $"type=sms&area=86&mobile={_phone}&code={code}&logintype=code&redirect_uri=suibiantian&email=&password=&broker_code=&ticket=&businessId="
            };
            item.Header.Add("Authorization", "Bearer null");
            item.Header.Add("FZM-REQUEST-OS", "web");
            item.Header.Add("FZM-USER-IP", _userIp);
            var result = http.GetHtml(item).Html;
#if DEBUG
            Console.WriteLine($"{nameof(Login)} result:{result}");
#endif
            if (result.Contains("message\":\"OK"))
            {
                var json = JsonConvert.DeserializeAnonymousType(result, new { data = new { access_token = "" } });
                this._access_token = json.data.access_token;
                return true;
            }
            return false;
        }
    }
}
