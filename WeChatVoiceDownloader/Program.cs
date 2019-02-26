using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace WeChatVoiceDownloader
{
    internal class Program
    {
        private const string _voiceKey = "voice_id";
        private const string _quitHint = "按任何键退出";
        private const string _resLink  = "https://res.wx.qq.com/voice/getvoice?mediaid=";
        private const string _testlink = @"https://mp.weixin.qq.com/mp/audio?_wxindex_=0&scene=104&__biz=MzI5MTEzNjEwNQ==&mid=2693359100&idx=1&voice_id=MzI5MTEzNjEwNV8yNjkzMzU5MDk5&sn=a984075eefe011486e8d20951dba3609&uin=&key=&devicetype=Windows+10&version=620603c8&lang=zh_CN&ascene=14&winzoom=1";

        private static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("至少需要一个参数,用法:WeChatVoiceDownloader.exe \"网页链接\"");
                ShowQuitMessage();
                return;
            }

            if (!Uri.TryCreate(args[0], UriKind.Absolute, out var uri))
            {
                Console.WriteLine("不是合法的uri");
                ShowQuitMessage();
                return;
            }

            var dict        = new Dictionary<string, string>();
            var collections = Regex.Matches(uri.Query, @"([\w\d_]+)=([\w\d_]+[^&]+)");
            foreach (Match collection in collections)
                dict.Add(collection.Groups[1].Value, collection.Groups[2].Value);

            if (!dict.ContainsKey(_voiceKey))
            {
                Console.WriteLine("没有找到资源,链接参数忘记用引号引起来?");
                ShowQuitMessage();
                return;
            }

            var fileName = dict[_voiceKey];
            using (var webClient = new WebClient())
            {
                webClient.Encoding = Encoding.UTF8;
                var content = webClient.DownloadString(args[0]);
                var match   = Regex.Match(content, @"  d.title = ""(.+)"";");
                if (match.Success)
                    fileName = match.Groups[1].Value;
            }

            var reslink = $"{_resLink}{dict[_voiceKey]}";
            using (var webClient = new WebClient())
            {
                var data        = webClient.DownloadData(reslink);
                var contentType = webClient.ResponseHeaders.Get("content-type");
                var ext         = contentType.Substring(contentType.LastIndexOf("/", StringComparison.Ordinal) + 1);
                Directory.CreateDirectory("data");

                using (var fs = new FileStream($"data/{fileName}.{ext}", FileMode.OpenOrCreate))
                {
                    fs.Write(data, 0, data.Length);
                    Console.WriteLine($"下载{fileName}.{ext}成功!");
                }
            }
        }

        private static void ShowQuitMessage()
        {
            Console.WriteLine(_quitHint);
            Console.ReadKey();
        }
    }
}