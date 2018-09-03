using HuSe.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SD.Common;
using SD.PI.Web.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace DotCover
{
    class Program
    {
#if DEBUG
        private readonly static string Domain = "http://192.168.1.240:8086/smartcampus/v2/dotmatrix/materialexamconfirm";
#endif
#if !DEBUG
        private readonly static string Domain = "http://api.shendupeiban.com/smartcampus/v2/dotmatrix/materialexamconfirm";
#endif

        private static readonly Socket socket;

        private static readonly Thread ClientThread = new Thread(WaitConnection);

        private const string TempName = "temp";

        private const string SerializeFolder = "datas";

        private readonly static string RuntimePath = AppDomain.CurrentDomain.BaseDirectory;

        /// <summary> 
        /// 将一个序列化后的byte[]数组还原         
        /// </summary>
        /// <param name="Bytes"></param>         
        /// <returns></returns> 
        public static DotModel BytesToObject(string data)
        {
            return JsonConvert.DeserializeObject<DotModel>(data);
        }

        public static byte[] ObjectToBytes(object obj)
        {
            return UTF8Encoding.Default.GetBytes(JsonConvert.SerializeObject(obj));
        }

        static Program()
        {
            LogUtil.Write("API地址：" + Domain);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ip = IPAddress.Any;
            IPEndPoint point = new IPEndPoint(ip, Convert.ToInt32(8500));
            socket.Bind(point);
            socket.Listen(10);
            LogUtil.Write("初始化套接字成功！");
            if (Directory.Exists(TempName))
            {
                var files = Directory.EnumerateFiles(Path.Combine(RuntimePath, TempName));
                ThreadPool.QueueUserWorkItem(wr =>
                {
                    try
                    {
                        foreach (var file in files)
                        {
                            File.Delete(file);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogUtil.Error(ex);
                    }
                });
            }

            if (!Directory.Exists(SerializeFolder))
            {
                Directory.CreateDirectory(SerializeFolder);
            }
            else
            {
               var d = DownloadHandler.Instance;
            }
        }

        private static void WaitConnection()
        {
            while (true)
            {
                var client = socket.Accept();
                byte[] buffer = new byte[1024 * 1024 * 3];
                var len = client.Receive(buffer);
                var str = UTF8Encoding.Default.GetString(buffer, 0, len);
                LogUtil.Write("接收到客户端请求！" + str);
                if (str == "hello")
                {
                    client.Send(UTF8Encoding.Default.GetBytes("hi"));
                    client.Shutdown(SocketShutdown.Both);
                    client.Close();
                    continue;
                }

                var model = BytesToObject(str);
                client.Shutdown(SocketShutdown.Both);
                client.Close();
                SaveModelData(model);
            }
        }

        private static void SaveModelData(DotModel model)
        {
            HuSe.WebClientUtil.DownloadFile(new MetaData
            {
                Url = model.Url,
                Data = model
            }, DownloadHandler.Instance);
        }
        
        static void Main(string[] args)
        {
            //Dot3Device dot3Device = new Dot3Device();
            //dot3Device.CreateDotPdf(@"C:\Users\Administrator\Desktop\1.pdf", @"C:\Users\Administrator\Desktop\2.pdf", "a3.0.0.0");
            ClientThread.Start();
        }
    }
}
