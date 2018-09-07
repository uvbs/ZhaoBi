using HttpCodeLib;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace ZhaoBi.Rigister
{
    public class UpLoadIMG
    {
        /// <summary>
        /// 身份证正面
        /// </summary>
        private readonly string _certifiCateAPath;
        /// <summary>
        /// 手持身份证
        /// </summary>
        private readonly string _certifiCateBPath;
        /// <summary>
        /// accessToken 登录返回
        /// </summary>
        private readonly string _authorization;
        /// <summary>
        /// 伪造IP
        /// </summary>
        private readonly string _userIp;
        /// <summary>
        /// 浏览器UA
        /// </summary>
        private const string USERAGENT = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.106 Safari/537.36";
        public UpLoadIMG(string certificateAPath, string certificateBPath, string accessToken, string ip)
        {
            this._certifiCateAPath = certificateAPath;
            this._certifiCateBPath = certificateBPath;
            this._authorization = accessToken;
            this._userIp = ip;
        }
        /// <summary>
        /// 获取动态Token
        /// </summary>
        /// <returns></returns>
        private string UploadToken()
        {
            HttpItems item = new HttpItems
            {
                URL = "https://pcapi.licai.cn/api/certification/UploadToken",
                Method = "POST",
                ContentType = "application/x-www-form-urlencoded;",
                Referer = "https://www.zhaobi.com/",
                UserAgent = UpLoadIMG.USERAGENT
            };
            item.Header.Add("FZM-USER-IP", _userIp);
            item.Header.Add("Authorization", $"Bearer {_authorization}");
            var result = new HttpHelpers().GetHtml(item).Html;
#if DEBUG
            Console.WriteLine($"{nameof(UploadToken)} result: {result}");
#endif
            var json = JsonConvert.DeserializeAnonymousType(result, new { data = new { token = "" } });
            var token = json.data.token;
            return token;
        }
        /// <summary>
        /// 上传证件
        /// </summary>
        /// <param name="token"></param>
        /// <param name="_certifiCatePath"></param>
        /// <returns></returns>
        private string UploadFile(string token, string _certifiCatePath)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://ossupload.licai.cn/upload/certificate");
            request.Method = "POST";
            request.ContentType = "multipart/form-data; boundary=----WebKitFormBoundaryptq5khV1RAo639Gs";
            request.Referer = "https://www.zhaobi.com/";
            request.UserAgent = UpLoadIMG.USERAGENT;
            request.Headers.Add("FZM-Ca-AppKey", "zhaobi");
            var restream = request.GetRequestStream();
            var startStr = Resource.start.Replace("{token}", token) + "\r\n\r\n";
            byte[] startByte = Encoding.UTF8.GetBytes(startStr);
            byte[] endByte = Encoding.UTF8.GetBytes(Resource.end);
            FileStream fs = new FileStream(_certifiCatePath, FileMode.Open, FileAccess.Read);
            byte[] bArr = new byte[fs.Length];
            fs.Read(bArr, 0, bArr.Length);
            fs.Close();
            restream.Write(startByte, 0, startByte.Length);
            restream.Write(bArr, 0, bArr.Length);
            restream.Write(Encoding.UTF8.GetBytes("\r\n"), 0, Encoding.UTF8.GetBytes("\r\n").Length);
            restream.Write(endByte, 0, endByte.Length);
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            Stream instream = response.GetResponseStream();
            StreamReader sr = new StreamReader(instream, Encoding.UTF8);
            string result = sr.ReadToEnd();
#if DEBUG
            Console.WriteLine($"{nameof(UploadFile)} result: {result}");
#endif
            sr.Close();
            if (result.Contains("succ\",\"code\":200"))
            {
                var json = JsonConvert.DeserializeAnonymousType(result, new { data = new { mid = "" } });
                return json.data.mid;
            }
            return string.Empty;
        }
        /// <summary>
        /// 识别证件正面
        /// </summary>
        /// <param name="mid"></param>
        /// <returns></returns>
        private (string name, string cardid) Ocr(string mid)
        {
            HttpItems item = new HttpItems
            {
                URL = "https://pcapi.licai.cn/api/Certification/Ocr",
                Method = "POST",
                ContentType = "application/x-www-form-urlencoded;",
                Referer = "https://www.zhaobi.com/",
                UserAgent = UpLoadIMG.USERAGENT,
                Postdata = $"mid={mid}"
            };
            item.Header.Add("Authorization", $"Bearer {_authorization}");
            item.Header.Add("FZM-USER-IP", _userIp);
            item.Header.Add("FZM-REQUEST-OS", "web");
            var result = new HttpHelpers().GetHtml(item).Html;
#if DEBUG
            Console.WriteLine($"{nameof(Ocr)} result: {result}");
#endif
            var json = JsonConvert.DeserializeAnonymousType(result, new
            {
                data = new
                {
                    address = "",
                    birthday = "",
                    cardid = "",
                    name = "",
                    nation = "",
                    provider = "",
                    sex = "",
                }
            });
            return (json.data.name, json.data.cardid);
        }
        private bool Identity(string cartid, string name, string midA, string midB)
        {
            HttpItems item = new HttpItems
            {
                URL = "https://pcapi.licai.cn/api/certification/identity",
                Method = "POST",
                ContentType = "application/x-www-form-urlencoded;",
                Referer = "https://www.zhaobi.com/",
                UserAgent = USERAGENT,
                Postdata = $"country=CN&cardtype=1&cardid={cartid}&name={UrlEncode(name)}&mid={midA}&matchmid={midB}"
            };
            item.Header.Add("Authorization", "Bearer " + _authorization);
            item.Header.Add("FZM-REQUEST-OS", "web");
            item.Header.Add("FZM-USER-IP", this._userIp);
            var result = new HttpHelpers().GetHtml(item).Html;
#if DEBUG
            Console.WriteLine($"{nameof(Identity)} result: {result}");
#endif
            return result.Contains("message\":\"OK");
        }
        public bool Submit()
        {
            try
            {
                var token = this.UploadToken();
                var midA = UploadFile(token, this._certifiCateAPath);
                var midB = UploadFile(token, this._certifiCateBPath);
                var (name, cardid) = this.Ocr(midA);
                bool flag = this.Identity(cardid, name, midA, midB);
                return flag;
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine($"{nameof(Submit)} ex: {ex.Message}");
#endif
                return false;
            }

        }
        private  string UrlEncode(string str)
        {
            if (str == null) return "";
            var builder = new StringBuilder();
            foreach (var c in str)
            {
                if (HttpUtility.UrlEncode(c.ToString()).Length > 1)
                {
                    builder.Append(HttpUtility.UrlEncode(c.ToString()).ToUpper());
                }
                else { builder.Append(c); }
            }
            return builder.ToString();
        }

    }
}
