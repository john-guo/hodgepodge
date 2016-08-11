using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

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
            //ThreadId = "4720937026";
            lastTime = 0;
            queue = new Queue<dynamic>();
        }

        public string ThreadId { get; set; }

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
