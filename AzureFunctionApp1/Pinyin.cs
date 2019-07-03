using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;
using Microsoft.International.Converters.PinYinConverter;
using System.Linq;
using System.Web;

namespace AzureFunctionApp1
{
    public static class Pinyin
    {
        [FunctionName("Pinyin")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            req.HttpContext.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            string text = req.Query["text"];

            string queryString = await new StreamReader(req.Body).ReadToEndAsync();
            var query = HttpUtility.ParseQueryString(queryString);
            text = text ?? query["text"];

            if (string.IsNullOrWhiteSpace(text))
            {
                return new BadRequestObjectResult("Please pass a name on the query string or in the request body");
            }
            StringBuilder sb = new StringBuilder();
            int count = 0;
            foreach (var ch in text)
            {
                var py = "-";
                if (ChineseChar.IsValidChar(ch))
                {
                    var cc = new ChineseChar(ch);
                    py = cc.Pinyins.Where(pys => !string.IsNullOrWhiteSpace(pys))
                        .Select(pys => new { item = new string(pys.TakeWhile(c => !char.IsDigit(c)).ToArray()) })
                        .GroupBy(item => item.item)
                        .Select(g => new { count = g.Count(), item = g.Key })
                        .OrderByDescending(s => s.count)
                        .First().item;
                }
                sb.Append(py);
                sb.Append(" ");
                ++count;
            }

            return new OkObjectResult(new { num = count, pinyin = sb.ToString().Trim() });
        }
    }
}
