using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Net.Sockets;
using System.Diagnostics;
using System.Net;
using System.IO;
using System.Net.Cache;

namespace fetchIpList
{
    class Program
    {
        const int port = 443;
        const int count = 20;
        const int processCount = 10;

        struct Result 
        {
            public string ip;
            public long timeout;
        }

        static void Main(string[] args)
        {
            //if (args.Length < 1)
            //{
            //    Console.WriteLine("usage: fetchIpList url");
            //    return;
            //}

            //var url = args[0];
            var url = "https://raw.githubusercontent.com/justjavac/Google-IPs/master/README.md";

            run(url).Wait();

#if DEBUG
            Console.ReadLine();
#endif
        }

        async static Task run(string url, bool isUrl = true)
        {
            var app = new Program();
            var ipList = (isUrl ? app.getIpListFromUrl(url) : app.getIpListFromFile(url)).ToList();

            var total = ipList.Count();
            var offset = 0;
            var takeCount = processCount;

            var tasks = new List<Task<IEnumerable<Result>>>();

            while (offset < total)
            {
                if (total - offset < takeCount)
                {
                    takeCount = total - offset;
                }

                var slice = ipList.Skip(offset).Take(takeCount);

                tasks.Add(app.go(slice));

                offset += takeCount;
            }

            var resultList = new List<Result>();
            foreach (var task in tasks)
            {
                resultList.AddRange(await task);
            }

#if DEBUG
            Console.WriteLine("Total: {0} {1}", total, resultList.Count);
#endif

            var o = resultList
                    .Where(r => r.timeout != long.MaxValue)
                    .OrderBy(r => r.timeout)
                    .ThenBy(r => r.ip);

            File.WriteAllText("list.txt", o.Aggregate(String.Empty, (prev, r) => String.Format("{0}{1}{2}", prev, Environment.NewLine, r.ip)));

            var ips = o.Take(count)
                    .Aggregate(String.Empty, (prev, r) => String.Format("{0}|{1}", prev, r.ip));

            File.WriteAllText("ip.txt", ips);
        }

        async Task<IEnumerable<Result>> go(IEnumerable<string> list)
        {
            var rlist = new List<Result>();
            foreach (var ip in list)
            {
                var r = await testIp(ip);

                if (r.timeout == long.MaxValue)
                    continue;

                rlist.Add(r);
            }

            return rlist.AsEnumerable();
        }

        IEnumerable<string> getIpListFromUrl(string url)
        {
            var web = new WebClient();
            var html = web.DownloadString(url);
            var document = new HtmlDocument();
            document.LoadHtml(html);
            var links = document.DocumentNode.SelectNodes("//a");
            foreach (var link in links)
            {
                IPAddress address;
                if (!IPAddress.TryParse(link.InnerText, out address))
                    continue;

                yield return address.ToString();
            }
        }

        IEnumerable<string> getIpListFromFile(string path)
        {
            var lines = File.ReadAllLines(path);
            foreach (var line in lines)
            {
                var str = line.Trim();
                if (String.IsNullOrWhiteSpace(str))
                    continue;

                IPAddress address;
                if (!IPAddress.TryParse(str, out address))
                    continue;

                yield return address.ToString();
            }
        }

        async Task<Result> testIp(string ip)
        {
            var client = WebRequest.CreateHttp(String.Format("https://{0}", ip));
            //client.Timeout = 10000;
            client.Method = "HEAD";
            client.AllowAutoRedirect = false;
            client.KeepAlive = false;
            client.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);

            var result = new Result();
            result.ip = ip;
            try
            {
                var watch = Stopwatch.StartNew();
                var response = (HttpWebResponse)await client.GetResponseAsync();
                watch.Stop();

                if (response.Server != "gws")
                {
                    throw new NotSupportedException();
                }

                result.timeout = watch.ElapsedMilliseconds;
            }
            catch
            {
                result.timeout = long.MaxValue;
            }

#if DEBUG
            if (result.timeout != long.MaxValue)
                Console.WriteLine("{0} {1}", result.ip, result.timeout);
#endif
            return result;
        }
    }
}
