using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using HtmlAgilityPack;
using Fizzler.Systems.HtmlAgilityPack;

namespace BulletScreen
{
    public class BulletSource_Tieba : BulletSource
    {
        WebClient client;
        private int lastTime;
        Queue<dynamic> queue;

        public BulletSource_Tieba()
        {
            client = new WebClient();
            client.Encoding = Encoding.UTF8;
            lastTime = 0;
            queue = new Queue<dynamic>();
        }

        public string ThreadId { get; set; }
        public string Title { get; set; }

        public override void Initialize()
        {
            var html = client.DownloadString(Properties.Settings.Default.tieba_url);
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml2(html);

            var link = doc.DocumentNode.QuerySelectorAll(".listThreadTitle>a").FirstOrDefault();
            var href = link?.Attributes["href"]?.Value;

            Title = link?.InnerText.Trim();
            ThreadId = Path.GetFileName(new Uri(href).LocalPath);
        }

        public override IEnumerable<BulletMessage> GetMessage()
        {
            var url = string.Format(@"http://tieba.baidu.com/live/getLiveDiscussInfo?threadId={0}&rn=30", ThreadId);
            var json = client.DownloadString(url);

            dynamic d = JsonSerializer.CreateDefault().Deserialize(new JsonTextReader(new StringReader(json)));

            long currTime = (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

            foreach (var item in d.data.post_list)
            {
                if (item.time <= lastTime)
                    continue;
                queue.Enqueue(new { item.content, item.time });
            }
            while (queue.Any())
            {
                var item = queue.Peek();
                if (item.time <= currTime)
                {
                    item = queue.Dequeue();
                    lastTime = item.time;
                    yield return new BulletMessage() { Content = item.content, Delay = 0 };
                }
                else
                    yield break;
            }
        }
    }
}
