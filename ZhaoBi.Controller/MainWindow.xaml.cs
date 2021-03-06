﻿using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ZhaoBi.Controller.Entity;
using ZhaoBi.Controller.Ma;
using ZhaoBi.Controller.Vpn;
using ZhaoBi.Rigister;

namespace ZhaoBi.Controller
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

        }
        /// <summary>
        /// 程序运行控制器线程
        /// </summary>
        private Task _AppTask;
        /// <summary>
        /// 是否运行
        /// </summary>
        private bool _ISRUN;
        private void startMenu_Click(object sender, RoutedEventArgs e)
        {
            if (GetMa.GlobalToken == null)
            {
                MessageBox.Show("请先登录平台!");
                return;
            }
            _ISRUN = true;
            _AppTask = Task.Factory.StartNew(Controller);
        }

        private void endMenu_Click(object sender, RoutedEventArgs e)
        {
            _ISRUN = false;
        }
        /// <summary>
        /// 控制器
        /// </summary>
        private void Controller()
        {
            for (int i = 0; i < Grid1.Items.Count; i++)
            {
                if (!_ISRUN) break;
                var _RandIp = RandIp();
#if DEBUG
                Console.WriteLine($"随机IP为 :{_RandIp}");
#endif
                var item = Grid1.Items[i] as RegisInfo;
                Dispatcher.Invoke(() => { Grid1.ScrollIntoView(item); });
                GetMa get = new GetMa();
                item.Status = "获取手机号";
                while (true)
                {
                    if (get.GetPhone())
                    {
                        break;
                    }
                    Thread.Sleep(2000);
                }
                Register register = new Register(Config.RegisCode, _RandIp);
                register.RegisPhone = get.CurPhone;
                item.Status = "发送验证码";
                if (register.SendMsg())
                {
                    item.Status = "等待验证码";
                    if (WaitCode(get, Config.WaitTime))
                    {
                        var code = get.CurMsg;
                        item.Status = "注册中";
                        // var flag = rigister.Regis(code);
                        if (register.Regis(code))
                        {
                            item.Status = "上传中";
                            var (phone, token) = register.GetParamter;
                            UpLoadIMG upload = new UpLoadIMG(Path.Combine(item.CertifiCatePath, "正面.jpg"), Path.Combine(item.CertifiCatePath, "手持.jpg"), token, _RandIp);
                            var flag = upload.Submit();
                            if (flag)
                            {
                                item.Status = "上传成功";
                                File.AppendAllText("SUCC.txt", phone + "----" + token + "\r\n");
                            }
                            else
                            {
                                item.Status = "上传失败";
                            }
                            DirMove(item.CertifiCatePath, flag ? Config.SUCC : Config.FAIL);
                        }
                        else
                        {
                            DirMove(item.CertifiCatePath, Config.FAIL);
                            item.Status = "注册失败";
                        }
                    }
                    else
                    {
                        DirMove(item.CertifiCatePath, Config.FAIL);
                        item.Status = "等待验证码超时";
                    }
                }
                else
                {
                    DirMove(item.CertifiCatePath, Config.FAIL);
                    item.Status = "验证码发送失败";
                }
                get.AddBlack();
#if DEBUG
                Console.WriteLine("切换IP...");
                Thread.Sleep(5000);
#else
                 ChangeIp();
#endif
            }
            MessageBox.Show("完成!");
        }
        /// <summary>
        /// 等待验证码
        /// </summary>
        /// <param name="get">等待对象</param>
        /// <param name="time">最长等待时间(秒)</param>
        /// <returns></returns>
        private bool WaitCode(GetMa get, int time)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            while (true)
            {
#if DEBUG
                Console.WriteLine("等待验证码中...");
#endif
                if (get.GetMessAge())
                {
                    return true;
                }
                if (sw.ElapsedMilliseconds / 1000 > time)
                {
                    break;
                }
                Thread.Sleep(2000);
            }
            return false;
        }
        /// <summary>
        /// 移动文件夹
        /// </summary>
        /// <param name="path1"></param>
        /// <param name="path2"></param>
        private void DirMove(string path1, string path2)
        {
            Directory.Move(path1, Path.Combine(path2, Path.GetFileName(path1)));
        }

        private void loginMa_Click(object sender, RoutedEventArgs e)
        {
            var flag = GetMa.Login(Config.UserApi, Config.PassWord);
            if (flag)
            {
                MessageBox.Show("登陆成功!");
                return;
            }
            MessageBox.Show("登陆失败!");
        }
        #region MyRegion
        private void Grid1_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = System.Windows.DragDropEffects.Copy;
            e.Handled = true;
        }
        private void Grid1_PreviewDrop(object sender, DragEventArgs e)
        {
            if (!(e.Data.GetData(System.Windows.DataFormats.FileDrop) is string[] paths)) return;
            if (!Directory.Exists(paths[0]))
            {
                MessageBox.Show("请导入文件目录!");
            }
            var dirArr = Directory.GetDirectories(paths[0]);
            if (dirArr.Length <= 0)
            {
                MessageBox.Show("没有找到文件!");
                return;
            }
            for (int i = 0; i < dirArr.Length; i++)
            {
                RegisInfo regis = new RegisInfo();
                regis.CertifiCatePath = dirArr[i];
                Grid1.Items.Add(regis);
            }
        }

        private void Grid1_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            if (e.Row.Item is RegisInfo info)
            {
                info.Index = e.Row.GetIndex() + 1;
            }
        }
        #endregion
        private Random rdint = new Random();
        /// <summary>
        /// 取随机IP
        /// </summary>
        /// <returns></returns>
        private string RandIp()
        {
            int[][] range = {new int[]{607649792,608174079},
                             new int[]{1038614528,1039007743},
                             new int[]{1783627776,1784676351},
                             new int[]{2035023872,2035154943},
                             new int[]{2078801920,2079064063},
                             new int[]{-1950089216,-1948778497},
                             new int[]{-1425539072,-1425014785},
                             new int[]{-1236271104,-1235419137},
                             new int[]{-770113536,-768606209},
                             new int[]{-569376768,-564133889},
             };
            int index = rdint.Next(10);
            string ip = Num2ip(range[index][0] + new Random().Next(range[index][1] - range[index][0]));
            return ip;
        }

        private string Num2ip(int ip)
        {
            int[] b = new int[4];
            string x = string.Empty;
            b[0] = (ip >> 24) & 0xff;
            b[1] = (ip >> 16) & 0xff;
            b[2] = (ip >> 8) & 0xff;
            b[3] = ip & 0xff;
            x = string.Join(".", b);
            return x;
        }
        /// <summary>
        /// 切换IP
        /// </summary>
        public static void ChangeIp()
        {
            VPNHelper vpn = new VPNHelper();
            SinASDL asdl = new SinASDL();
            while (true)
            {
                asdl.ASDLName = Config.BroadbandName;
                asdl.Username = Config.BroadbandUser;
                asdl.Password = Config.BroadbandPswd;
                vpn.VPNName = asdl.ASDLName;
                vpn.TryDisConnectVPN(asdl.ASDLName);
                Thread.Sleep(5000);
                if (asdl.Connect() == 0)
                {
                    break;
                }
            }
        }
    }
}
