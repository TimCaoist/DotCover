using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotCover
{
    public class Dot3Device
    {
#if DEBUG
        private const string UserName = "IRA_AIRUI";
#endif
#if !DEBUG
        private const string UserName = "IRA11";
#endif
        private const string UserPwd = "1234";
        /// <summary>
        /// 一张A4大小的像素点大小
        /// </summary>
        private UInt32[] arBPointX = { 0, 4961, 4961, 0 };

        /// <summary>
        ///  一张A4大小的像素点大小
        /// </summary>
        private UInt32[] arBPointY = { 0, 0, 7016, 7016 };

        /// <summary>
        /// 一张A4的铺码工具宽
        /// </summary>
        private const int Width = 156;

        /// <summary>
        /// 一张A4的铺码工具高
        /// </summary>
        private const int Height = 217;

        /// <summary>
        /// 运行目录
        /// </summary>
        private readonly static string BasePath = AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>
        /// 多pdf合并
        /// </summary>
        /// <param name="pdfList"></param>
        /// <param name="outFile"></param>
        /// <param name="pageCount"></param>
        /// <returns></returns>
        public bool Merge(IList<string> pdfList, string outFile, out int pageCount)
        {
            pageCount = 0;
            if (pdfList.Count == 0) return false;
            Document doc = new Document();
            Stream stream = new FileStream(outFile, FileMode.Create);
            PdfWriter writer = PdfWriter.GetInstance(doc, stream);
            doc.Open();
            PdfContentByte cb = writer.DirectContent;
            PdfImportedPage newPage = null;
            PdfReader reader = null;

            for (int i = 0; i < pdfList.Count; i++)
            {
                reader = new PdfReader(pdfList[i]);
                int iPageNum = reader.NumberOfPages;
                for (int j = 1; j <= iPageNum; j++)
                {
                    pageCount++;
                    doc.NewPage();
                    newPage = writer.GetImportedPage(reader, j);
                    cb.AddTemplate(newPage, 0, 0);
                }
            }
            writer.Flush();
            doc.Close();
            stream.Close();
            writer.Close();
            if (reader != null)
            {
                reader.Close();
            }
            return true;
        }

        public bool CreateDotPdf(string pdfFile, string dotPdfFile, params string[] pages)
        {
            if (pages.Length == 0)
            {
                return false;
            }

            string strBGImage = pdfFile;
            string strPublishBGImage = dotPdfFile;
            OIDPublishImageGenerator oidPIGenerator = new OIDPublishImageGenerator();
            if (!oidPIGenerator.Initialize())
            {
                return false;
            }

            oidPIGenerator.SetUserInfo(UserName.ToCharArray(), UserPwd.ToCharArray());
            OIDBeginBuildState eBeginBuildState = OIDBeginBuildState.eBBState_OK;
            eBeginBuildState = (OIDBeginBuildState)oidPIGenerator.BeginBuildPublishImage(strBGImage.ToCharArray(), true);
            if (eBeginBuildState != OIDBeginBuildState.eBBState_OK)
            {
                oidPIGenerator.Uninitialize();
                return false;
            }

            /// Start publish tif
            int nPageIndex = 0;
            int nStartX = 0;
            int nStratY = 0;
            var point = GetStratPoistion(pages[nPageIndex]);
            nStartX = point.X;
            nStratY = point.Y;
            // Set start position for Position Code
            var pageCount = pages.Count();
            IList<string> files = new List<string>();
            try
            {
                var filePath = pdfFile;
                files.Add(filePath);
                while (oidPIGenerator.SetStartPosition(nPageIndex, nStartX, nStratY))
                {
                    var nObjectType = (int)(OIDPublishObjectType.eOID_OT_ElementCode);
                    var bAddResult = oidPIGenerator.AddObjectInfo(nPageIndex, 0x04800000, arBPointX, arBPointY, arBPointX.Length, 0, nObjectType);

                    nObjectType = (int)(OIDPublishObjectType.eOID_OT_PositionCode);
                    bAddResult = oidPIGenerator.AddObjectInfo(nPageIndex, uint.MaxValue, arBPointX, arBPointY, arBPointX.Length, 1, nObjectType);
                    ++nPageIndex;
                    if (nPageIndex < pageCount)
                    {
                        point = GetStratPoistion(pages[nPageIndex]);
                        nStartX = point.X;
                        nStratY = point.Y;
                        files.Add(filePath.Insert(filePath.LastIndexOf('.'), "_" + nPageIndex));
                    }
                    else
                    {
                        break;
                    }
                }

                int nPrintPointType = (int)(OIDPrintPointType.eOID_PrintPointType_3x3);
                int nPublishImageType = (int)(OIDPublishImageType.eOID_PIT_Publish_BG_Image);
                oidPIGenerator.BuildPublishImage(filePath.ToCharArray(), true, true, nPrintPointType, nPublishImageType);
                oidPIGenerator.EndBuildPublishImage();
            }
            finally
            {
                oidPIGenerator.Uninitialize();
            }

            if (files.Count > 1)
            {
                int count = 0;
                Merge(files, dotPdfFile, out count);
            }
            else
            {
                File.Copy(files[0], dotPdfFile, true);
            }

            return true;
        }

        private Point GetStratPoistion(string page)
        {
            var arry = page.Split('.');
            if (arry.Length < 4)
            {
                return Point.Empty;
            }

            var x = int.Parse(arry[2]) * Width;
            var y = int.Parse(arry[3]) * Height;
            return new Point(x, y);
        }
    }
}
