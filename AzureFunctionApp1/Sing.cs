using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Net.Http;

namespace AzureFunctionApp1
{
    public static class Sing
    {
        [FunctionName("Sing")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            req.HttpContext.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            string queryString = await new StreamReader(req.Body).ReadToEndAsync();
            var query = HttpUtility.ParseQueryString(queryString);

            var pinyin = query["pinyin"];
            var num = query["num"];
            var music = query["music"];

            if (string.IsNullOrWhiteSpace(pinyin))
            {
                return new BadRequestObjectResult("parameter pinyin");
            }
            if (string.IsNullOrWhiteSpace(num))
            {
                return new BadRequestObjectResult("parameter num");
            }
            if (string.IsNullOrWhiteSpace(music))
            {
                return new BadRequestObjectResult("parameter music");
            }

            using (var client = new HttpClient())
            {
                var content = new FormUrlEncodedContent(new Dictionary<string, string>()
                {
                    ["pinyin"] = pinyin,
                    ["num"] = num,
                    ["music"] = GetMusic(music),
                });
                var response = await client.PostAsync("http://52.175.149.25/sing.php", content);
                if (!response.IsSuccessStatusCode)
                    return new BadRequestObjectResult("system error");

                var buffer = await response.Content.ReadAsByteArrayAsync();
                var blob = Convert.ToBase64String(buffer);
                return new OkObjectResult(new { ret = $"data:audio/wav;base64,{blob}" });
            }
        }

        public static string GetMusic(string music)
        {
            Dictionary<string, char> dict = new Dictionary<string, char>()
            {
                ["c"] = Convert.ToChar(48),
                ["c#"] = Convert.ToChar(49),
                ["d"] = Convert.ToChar(50),
                ["d#"] = Convert.ToChar(51),
                ["e"] = Convert.ToChar(52),
                ["f"] = Convert.ToChar(53),
                ["f#"] = Convert.ToChar(54),
                ["g"] = Convert.ToChar(55),
                ["g#"] = Convert.ToChar(56),
                ["a"] = Convert.ToChar(57),
                ["a#"] = Convert.ToChar(58),
                ["b"] = Convert.ToChar(59),
                ["C"] = Convert.ToChar(60),
                ["C#"] = Convert.ToChar(61),
                ["D"] = Convert.ToChar(62),
                ["D#"] = Convert.ToChar(63),
                ["E"] = Convert.ToChar(64),
                ["F"] = Convert.ToChar(65),
                ["F#"] = Convert.ToChar(66),
                ["G"] = Convert.ToChar(67),
                ["G#"] = Convert.ToChar(68),
                ["A"] = Convert.ToChar(69),
                ["A#"] = Convert.ToChar(70),
                ["B"] = Convert.ToChar(71),
                ["1"] = Convert.ToChar(72),
                ["1#"] = Convert.ToChar(73),
                ["2"] = Convert.ToChar(74),
                ["2#"] = Convert.ToChar(75),
                ["3"] = Convert.ToChar(76),
                ["4"] = Convert.ToChar(77),
                ["4#"] = Convert.ToChar(78),
                ["5"] = Convert.ToChar(79),
                ["5#"] = Convert.ToChar(80),
                ["6"] = Convert.ToChar(81),
                ["6#"] = Convert.ToChar(82),
                ["7"] = Convert.ToChar(83),
            };
            var txt = music;
            var sb = new StringBuilder();
            for (int i = 0; i < txt.Length; ++i)
            {
                char ch = txt[i];
                if (char.IsWhiteSpace(ch))
                    continue;

                string key;
                char next = Convert.ToChar(0);
                if (i + 1 < txt.Length)
                {
                    next = txt[i + 1];
                }
                if (next == '#')
                {
                    i++;
                    key = $"{ch}{next}";
                }
                else
                {
                    key = $"{ch}";
                }

                if (dict.TryGetValue(key, out char tempo))
                {
                    sb.Append($"{tempo}");
                }
            }

            return sb.ToString();
        }
    }
}
