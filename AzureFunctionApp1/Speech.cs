using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using NAudio.Wave;

namespace AzureFunctionApp1
{
    public static class Speech
    {
        [FunctionName("Speech")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            req.HttpContext.Response.Headers.Add("Access-Control-Allow-Origin", "*");

            string text = req.Query["text"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            text = text ?? data?.text;

            if (string.IsNullOrWhiteSpace(text))
            {
                return new BadRequestObjectResult("Please pass a name on the query string or in the request body");
            }

            try
            {
                var speechConfig = SpeechConfig.FromSubscription("8dff62a8959a49bc8500f58fdd08967c", "westus2");
                speechConfig.SpeechSynthesisLanguage = "zh-CN";
                speechConfig.SpeechSynthesisVoiceName = "zh-CN-XiaoxiaoNeural";

                using (var stream = new MemoryStream())
                using (var wave = new WaveFileWriter(stream, new WaveFormat(16000, 16, 1)))
                using (var synthesizer = new SpeechSynthesizer(speechConfig, AudioConfig.FromStreamOutput(new WaveFilePush() { Stream = wave })))
                {
                    var result = await synthesizer.SpeakTextAsync(text);
                    if (result.Reason == ResultReason.Canceled)
                    {
                        return new BadRequestObjectResult(SpeechSynthesisCancellationDetails.FromResult(result));
                    }
                    else if (result.Reason == ResultReason.SynthesizingAudioCompleted)
                    {
                        var blob = Convert.ToBase64String(stream.ToArray());
                        return new OkObjectResult(new { ret = $"data:audio/wav;base64,{blob}" });
                    }
                }
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex.ToString());
            }

            return new OkResult();
        }

        class WaveFilePush : PushAudioOutputStreamCallback
        {
            public WaveFileWriter Stream { get; set; }
            public override uint Write(byte[] dataBuffer)
            {
                Stream.Write(dataBuffer);
                Stream.Flush();
                return (uint)dataBuffer.Length;
            }
        }
    }
}
