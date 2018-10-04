using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using HtmlAgilityPack;

namespace DownloadImage
{
    class Program
    {
        static void Main(string[] args)
        {
            string dir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
            var inifle = new IniFile(dir + "\\" + "bin"+"\\"+"Debug"+"\\"+"config.ini");
            string minpagestr = inifle.Read("MinPage").ToString();
            string maxpagestr = inifle.Read("MaxPage").ToString();
            string url = inifle.Read("Url").ToString();
            string saveLocation = inifle.Read("SaveLocation").ToString();
            saveLocation = dir + "\\" + saveLocation;

            Program p = new Program();

            for (int k = Int32.Parse(minpagestr); k <= Int32.Parse(maxpagestr); k++)
            {
                List<string> list = p.parseHtml(url + k.ToString());
                for (int i = 0; i < list.Count; i++)
                {

                    if (list[i].Contains("https") && list[i] != null)
                    {
                        if (list[i].Contains("amp;"))
                        {
                            list[i] = list[i].Replace("amp;", "");
                        }
                        p.processDownload(list[i], saveLocation);
                    }
                }
            }
            Console.ReadKey();
        }


        private List<string> parseHtml(string url)
        {
            var html = url;
            HtmlWeb web = new HtmlWeb()
            {
                AutoDetectEncoding = false,
                OverrideEncoding = Encoding.UTF8
            };
            HtmlDocument document = web.Load(html);

            var theardImg = document.DocumentNode.SelectNodes("//img/@src");
            return theardImg == null ? new List<string>() : theardImg.ToList().ConvertAll(
              r => r.Attributes.ToList().ConvertAll(
              i => i.Value)).SelectMany(j => j).ToList();
        }

        private void processDownload(string imageUrl, string saveLocation)
        {
            byte[] imageBytes;
            try
            {
                HttpWebRequest imageRequest = (HttpWebRequest)WebRequest.Create(imageUrl);
                var uri = new Uri(imageUrl);
                var fileName = System.IO.Path.GetFileName(uri.LocalPath);
                saveLocation = saveLocation + fileName;
                try
                {
                    WebResponse imageResponse = imageRequest.GetResponse();
                    Stream responseStream = imageResponse.GetResponseStream();
                    using (BinaryReader br = new BinaryReader(responseStream))
                    {
                        imageBytes = br.ReadBytes(500000);
                        br.Close();
                    }
                    responseStream.Close();
                    imageResponse.Close();

                    FileStream fs = new FileStream(saveLocation, FileMode.Create);
                    BinaryWriter bw = new BinaryWriter(fs);

                    try
                    {
                        bw.Write(imageBytes);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    finally
                    {
                        fs.Close();
                        bw.Close();
                    }
                    Console.WriteLine("Done: " + imageUrl);
                    WriteLog("Done: " + imageUrl);
                }
                catch (Exception ex)
                {
                    WriteLog(ex.Message + "--" + imageUrl);
                    Console.WriteLine(ex.Message + "--" + imageUrl);
                }
            }catch(Exception ex)
            {
                WriteLog(ex.Message);
            }
        }

        private void WriteLog(string strLog)
        {
            StreamWriter log;
            FileStream fileStream = null;
            DirectoryInfo logDirInfo = null;
            FileInfo logFileInfo;

            string logFilePath = "C:\\Logs\\";
            logFilePath = logFilePath + System.DateTime.Today.ToString("MM-dd-yyyy") + "." + "log";
            logFileInfo = new FileInfo(logFilePath);
            logDirInfo = new DirectoryInfo(logFileInfo.DirectoryName);
            if (!logDirInfo.Exists) logDirInfo.Create();
            if (!logFileInfo.Exists)
            {
                fileStream = logFileInfo.Create();
            }
            else
            {
                fileStream = new FileStream(logFilePath, FileMode.Append);
            }
            log = new StreamWriter(fileStream);
            DateTime now = DateTime.Now;
            strLog = now.ToString("yyyy-MM-dd HH:mm:ss.fff: ") + strLog;
            log.WriteLine(strLog);
            log.Close();
        }
    }
}
