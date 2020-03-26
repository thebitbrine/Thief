using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

namespace Thief
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Enter URL: ");
            baseUrl = Console.ReadLine();
            File.AppendAllText("Last.Run.txt", $"== STARTING RUN FOR {baseUrl} @ {DateTime.Now.ToString()} == \r\n");
            
            Crawl(baseUrl);
        }

        public static string baseUrl = "";
        public static List<string> CollectedLinks = new List<string>();
        public static List<string> DownloadedLinks = new List<string>();
        public static void Crawl(string URL)
        {
            var HTML = Get(URL);
            DownloadedLinks.Add(URL);

            if (HTML != null)
            {
                var Links = ExtractLinks(HTML);
                for (int i = 0; i < Links.Length - 1; i++)
                {
                    if (Links[i].EndsWith("/"))
                    {
                        new Thread(() => Crawl(Links[i])).Start();
                    }
                }
            }
            else
            {
                Console.WriteLine($"Cannot fetch {URL}");
            }

        }

        public static string[] ExtractLinks(string HTML)
        {
            List<string> iCollectedLinks = new List<string>();
            while (HTML.Contains("<a href=\""))
            {
                string url = GetBetween(HTML, "<a href=\"", "\"");
                HTML = HTML.Replace($"<a href=\"{url}\"", "");
                if (!url.ToLower().StartsWith("http"))
                    url = $"{baseUrl}/{url}".Replace("//", "/").Replace(":/","://");
                else
                    if (!url.ToLower().Contains(baseUrl.Split('/')[3].Replace("/", "")))
                    continue;

                lock (CollectedLinks)
                {
                    if (!CollectedLinks.Contains(url))
                    {
                        CollectedLinks.Add(url);
                        iCollectedLinks.Add(url);
                        File.AppendAllText("Last.Run.txt", $"{url}\r\n");
                        Console.WriteLine("\n" + url);
                    }
                }
                Console.Write(".");
            }
            return iCollectedLinks.ToArray();
        }

        public static string Get(string Link)
        {
            WebClient webClient = new WebClient();
            webClient.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.149 Safari/537.36");

            for (int i = 0; i < 30; i++)
            {
                try
                {
                    return webClient.DownloadString(Link);
                }
                catch
                {
                    Thread.Sleep(i * 666);
                }
            }
            return null;
        }

        public static string GetBetween(string strSource, string strStart, string strEnd)
        {
            int Start, End;
            if (!string.IsNullOrWhiteSpace(strSource) && strSource.Contains(strStart) && strSource.Contains(strEnd))
            {
                Start = strSource.IndexOf(strStart, 0) + strStart.Length;
                End = strSource.IndexOf(strEnd, Start);
                return strSource.Substring(Start, End - Start);
            }
            else
            {
                return null;
            }
        }
    }
}
