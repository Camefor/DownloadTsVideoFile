using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace GetTsVideoFile {
    class Program {

        static string MTsFilePath = System.AppDomain.CurrentDomain.BaseDirectory + "\\TS\\";  // +N.ts
        static readonly HttpClient client = new HttpClient();
        static string MURL;
        static string MFileName;
        static string MFinallyFileName;

        static string DownloadFileName;
        static int ErrorCount;
        static int CurrentIndex;
        static async Task Main(string[] args) {
            // https://cn4.5311444.com/hls/20181013/2555f74c25289c0d9eadb961c1ac692d/1539419461/film_00000.ts

            var test01 = "https://cn4.5311444.com/hls/20181013/2555f74c25289c0d9eadb961c1ac692d/1539419461/film_00000.ts";
            Console.WriteLine("输入视频的开始地址...");
            Console.WriteLine("例如: {0}", test01);
            test01 = Console.ReadLine().Trim();
            var ara = test01.Trim().Split('/');
            var oneClips = ara[ara.Length - 1];//一个视频片段名称(film_00000.ts)
            var dir = ara[ara.Length - 2];//可以当作文件夹名。或者合成的视频名称(1539419461)
            var baseUrl = ara[ara.Length - 3];//2555f74c25289c0d9eadb961c1ac692d
            var tar02 = oneClips.Split('.');
            var sdsds = tar02[0];//film_00000
            var preStr = sdsds.Split('_')[0];//film
            var videoHeaderTs = preStr + '_';//视频文件头 + 生成的  …….ts结尾
            var _baseUrl = test01.Remove(test01.Length - oneClips.Length);//链接地址 base  +videoHeader
            //https://cn4.5311444.com/hls/20181013/2555f74c25289c0d9eadb961c1ac692d/1539419461/


            var finallyDir = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, dir);
            if (!Directory.Exists(finallyDir)) {
                Directory.CreateDirectory(finallyDir);
            }

            for (int i = 0; i < 1500; i++) {
                CurrentIndex = i;
                GotoNext:
                {
                    if (ErrorCount >= 10) {
                        i = CurrentIndex + 1;
                    }
                    //i += 1; //继续下一个
                }
                MFinallyFileName = "2d2cb98ec7b000";
                /**
                 * 拼接url
                 * N
                 * N :1---9 格式为 00N
                 * N : 10 --- 99  0N
                 * N : 100 ----H-00 N
                 * **/
                var generalTs = "";
                if (i <= 9) {
                    generalTs = i.ToString("d5") + ".ts";  //加三个0
                }
                if (10 <= i && i <= 99) {
                    generalTs = "0" + i.ToString("d4") + @".ts";
                }
                if (100 <= i) {
                    generalTs = i.ToString("d3") + @".ts";
                }

                var oneFile = videoHeaderTs + generalTs;//"film_00000.ts"
                MURL = _baseUrl + oneFile;//"https://cn4.5311444.com/hls/20181013/2555f74c25289c0d9eadb961c1ac692d/1539419461/film_00000.ts"
                DownloadFileName = i.ToString();
                //MFinallyFileName = Path.Combine(dir, oneFile);

                MTsFilePath = Path.Combine(finallyDir, oneFile);//"D:\\Documents\\LearnCSharp\\DownloadTsVideoFile\\bin\\Debug\\1539419461\\film_00000.ts"
                if (!File.Exists(MTsFilePath)) {
                    using (File.Create(MTsFilePath)) {
                        //
                    }
                    //File.Create(MTsFilePath); //https://www.cnblogs.com/hqbhonker/p/3494042.html
                }
                ReTry:
                try {
                    //Thread.Sleep(500);
                    HttpResponseMessage response = await client.GetAsync(MURL);
                    Console.WriteLine("正在下载第 : {0}个视频片段...", i + 1);
                    response.EnsureSuccessStatusCode();
                    var respnseBody = await response.Content.ReadAsByteArrayAsync();
                    //Console.WriteLine(respnseBody);
                    //Console.WriteLine(respnseBody.Length);
                    SaveTsFile(respnseBody, MTsFilePath); //保存
                } catch (HttpRequestException ex) {
                    Console.WriteLine("Message: {0}", ex.Message);
                    Thread.Sleep(500);
                    //计下载失败数 ，超过就跳过下载
                    ErrorCount += 1;
                    if (ErrorCount <= 10) {
                        goto ReTry; //重试
                    }
                    if (ErrorCount > 10 && ErrorCount < 50) {
                        goto GotoNext;//放弃该片段，继续下一个视频
                    }
                    if (ErrorCount >= 50) {
                        //终止程序运行
                        goto End;
                        //System.Environment.Exit(0);
                    }
                }
            }
            End: Console.WriteLine("下载任务结束.");
            Console.ReadKey();
            //System.Environment.Exit(0);
        }
        static void SaveTsFile(byte[] aResult, string aFilePath) {
            try {
                //https://stackoverflow.com/questions/18021662/system-io-ioexception-the-process-cannot-access-the-file-because-it-is-being-us?rq=1

                if (!System.IO.File.Exists(aFilePath)) {
                    System.IO.File.Create(aFilePath);
                }
                //using (StreamWriter sw = new StreamWriter(aFilePath, true, System.Text.Encoding.Default))
                //{
                //    sw.Write(aResult.ToString());
                //}

                //将stream转换成 byte[]

                //https://www.cnblogs.com/niuniu0108/p/7306350.html
                //把byte[] 写入文件
                var _buff = aResult;
                Thread.Sleep(100);
                using (FileStream SourceStream = new FileStream(aFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)) {
                    using (BinaryWriter binaryWriter = new BinaryWriter(SourceStream)) {
                        binaryWriter.Write(_buff);
                    }
                }
                Console.WriteLine("下载成功.");
            } catch (Exception ex) {
                Console.WriteLine("Message: ", ex.Message);
            }
        }




        #region 

        public static string Post(string url, string content) {
            string result = "";
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";

            #region 添加Post 参数
            byte[] data = Encoding.UTF8.GetBytes(content);
            req.ContentLength = data.Length;
            using (Stream reqStream = req.GetRequestStream()) {
                reqStream.Write(data, 0, data.Length);
                reqStream.Close();
            }
            #endregion

            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            Stream stream = resp.GetResponseStream();
            //获取响应内容
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8)) {
                result = reader.ReadToEnd();
            }
            return result;
        }

        //static async Task<Byte[]> GetTsFileAsync(string url, string ahttpMethond = "get")
        //{
        //    try
        //    {
        //        var _result;
        //        HttpClient httpClient = new HttpClient();
        //        await httpClient.GetAsync(url).ContinueWith(
        //            async (requestTask) =>
        //            {
        //                HttpResponseMessage response = requestTask.Result;
        //                response.EnsureSuccessStatusCode();
        //                return await response.Content.ReadAsByteArrayAsync().ContinueWith(
        //                       (readTask) => readTask.Result);
        //            });

        //    }
        //    catch (Exception)
        //    {
        //        return null;
        //    }
        //    return _result;
        //}


        /***
         * http://api.worldbank.org/countries?format=json&page=1&per_page=300
         * 
         * **/
        #endregion



    }
}

