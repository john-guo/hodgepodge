using System;
using FFmpeg.AutoGen;
using BumpKit;
using FFmpeg;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;

namespace video2gif
{
    public static class Video2Gif
    {
        public static void Process(string videoUrl, string gifName,
            TimeSpan begin, TimeSpan end, int? width, int? height, int? delay,
            Action<int> progress = null, Action done = null,
            bool isVerbose = false)
        {
            FFmpegMediaInfo.InitDllDirectory();
            if (isVerbose)
                ffmpeg.av_log_set_level(ffmpeg.AV_LOG_QUIET);
            using (var media = new FFmpegMediaInfo(videoUrl))
            {
                var gifstream = File.Open(gifName, FileMode.Create, FileAccess.Write, FileShare.None);
                using (var gif = new GifEncoder(gifstream))
                {
                    if (delay.HasValue)
                        gif.FrameDelay = TimeSpan.FromMilliseconds(delay.Value);
                    media.ProcessFrames(begin, end, width, height, delay, (img, ts, i) =>
                    {
                        if (isVerbose) 
                            progress(i);

                        gif.AddFrame(img);
                    });
                }

                gifstream.Flush();
                gifstream.Close();
            }

            if (isVerbose)
            {
                done();
            }
        }
    }
}
