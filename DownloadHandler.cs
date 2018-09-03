using HuSe.Interface;
using HuSe.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SD.PI.Web.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace DotCover
{
    public class DownloadHandler : DefaultHuSeConfig, IProcessNotify
    {
#if DEBUG
        private readonly static string Domain = "http://192.168.1.240:8086/smartcampus/v2/dotmatrix/materialexamconfirm";
#endif
#if !DEBUG
        private readonly static string Domain = "http://api.shendupeiban.com/smartcampus/v2/dotmatrix/materialexamconfirm";
#endif

        private readonly static DownloadHandler self = new DownloadHandler();

        private const string SerializeFolder = "datas";

        private static string runtimePath;

        private readonly Thread CoverThread;

        private AutoResetEvent autoReset = new AutoResetEvent(false);

        private HashSet<string> excutingFiles = new HashSet<string>();

        public override string LocalFolder {
            get {
                return string.Concat(runtimePath, "temp");
            }
        }

        public DownloadHandler()
        {
            runtimePath = AppDomain.CurrentDomain.BaseDirectory;
            HuSe.WebClientUtil.ResetConfig(this);
            CoverThread = new Thread(HanlderCover);
            CoverThread.Start();
        }

        public static DownloadHandler Instance {
            get {
                return self;
            }
        }

        public void BatchSucceed(long batchId, IEnumerable<MetaData> datas)
        {
        }

        public void Error(Exception exception, MetaData wrapperData)
        {
            LogUtil.Error(exception);
        }

        public void Progress(MetaData userState, long bytesReceived, long totalBytesToReceive, int progressPercentage)
        {
        }

        public void Succeed(MetaData wrapperData)
        {
            DotModel model = (DotModel)wrapperData.Data;
            try
            {
                model.LocalUrl = wrapperData.FullPath;
                LogUtil.Write(model.Id + "下载文件成功！");
            }
            catch (Exception ex)
            {
                LogUtil.Error(ex);
            }

            SerializeUtil.Serialize(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SerializeFolder, string.Concat(model.Id, string.Intern(".bin"))), model);
            autoReset.Set();
        }

        private DotModel GetFirstModel()
        {
            var path = Path.Combine(runtimePath, SerializeFolder);
            var file = Directory.EnumerateFiles(path, "*.bin").FirstOrDefault(f => !excutingFiles.Contains(f));
            if (file == null)
            {
                return null;
            }

            var dotModel = SerializeUtil.Deserialize<DotModel>(file);
            if (!File.Exists(dotModel.LocalUrl))
            {
                HuSe.WebClientUtil.DownloadFile(new MetaData
                {
                    Url = dotModel.Url,
                    Data = dotModel,
                    LocalFileName = dotModel.Url.GetHashCode() + ".pdf"
                }, this);

                return null;
            }

            excutingFiles.Add(file);
            return dotModel;
        }

        private void HanlderCover()
        {
            while (true)
            {
                var dotModel = GetFirstModel();
                if (dotModel != null)
                {
                    Cover(dotModel);
                }

                autoReset.WaitOne();
            }
        }

        private void Cover(DotModel model)
        {
            try
            {
                LogUtil.Write(model.Id + "开始铺码!");
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                Dot3Device dot3Device = new Dot3Device();
                var coverPdf = model.LocalUrl.Replace(".pdf", "_cover.pdf");
                dot3Device.CreateDotPdf(model.LocalUrl, coverPdf, model.Pages);
                stopwatch.Stop();
                LogUtil.Write(string.Concat(model.Id, "铺码完成!铺码用时:" , stopwatch.ElapsedMilliseconds / 1000 , "秒"));
                autoReset.Set();

                ThreadPool.QueueUserWorkItem((wr) =>
                {
                    Upload(model, coverPdf);
                });
            }
            catch (Exception ex)
            {
                LogUtil.Error(ex);
                model.IsSucced = false;
                CallBack(model);
            }
        }

        private void Upload(DotModel model, string coverPdf)
        {
            var nameValues = new NameValueCollection
                    {
                        { "key", Guid.NewGuid() + Path.GetExtension(coverPdf) },
                        { "token", model.Token }
                    };

            try
            {
                var str = SD.Common.Http.Upload(SD.Common.Http.QINIU_UPLOAD, System.IO.File.ReadAllBytes(coverPdf), nameValues);
                if (str.StartsWith("error:"))
                {
                    model.IsSucced = false;
                    LogUtil.Write(str, "Waring");
                    return;
                }

                var result = JsonConvert.DeserializeObject<JObject>(str);
                model.Url = string.Concat(model.BaseUrl, "/", result["key"]);
                model.IsSucced = true;
                LogUtil.Write(model.Url + "上传铺码文档成功!");
            }
            catch (Exception ex)
            {
                LogUtil.Error(ex);
                model.IsSucced = false;
            }
            finally
            {
                CallBack(model);
            }
        }

        private void CallBack(DotModel model)
        {
            try
            {
                var postData = JsonConvert.SerializeObject(new
                {
                    temp_id = model.Id.ToString(),
                    dot_pdf_url = model.Url,
                    state = model.IsSucced.ToString().ToLower(),
                });

                var str = SD.Common.Http.PostWebRequest(Domain, postData);
                LogUtil.Write(model.Id + "提交信息" + str);
            }
            catch (Exception ex)
            {
                LogUtil.Write(model.Id + "上传出错");
                LogUtil.Error(ex);
            }
            finally
            {
                var fileName = Path.Combine(runtimePath, SerializeFolder, string.Concat(model.Id, ".bin"));
                excutingFiles.Remove(fileName);
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                    LogUtil.Write(model.Id + "删除成功!");
                }
            }
        }
    }
}
