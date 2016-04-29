using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using BjSTools.Media.FFmpeg;
using BumpKit;
using System.Drawing;
using System.Net;
using FFmpeg.AutoGen;
using System.Runtime.InteropServices;
using CommandLine;
using CommandLine.Text;

namespace Gif
{
    class Options
    {
        [ValueOption(0)]
        [Option('u', HelpText = "Url")]
        public string Url { get; set; }

        [Option('b', HelpText = "Begin in milliseconds")]
        public int? Begin { get; set; }

        [Option('e', HelpText = "End in milliseconds")]
        public int? End { get; set; }

        [Option('o', DefaultValue = "a.gif", HelpText = "Output filename")]
        public string Gif { get; set; }

        [Option('w', HelpText = "With in pixel")]
        public int? Width { get; set; }

        [Option('h', HelpText = "Height in pixel")]
        public int? Height { get; set; }

        [Option("delay", DefaultValue = 200, HelpText = "Delay each frame in milliseconds")]
        public int Delay { get; set; }

        [Option('v', DefaultValue = false, HelpText = "More logs")]
        public bool Verbose { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this,
              (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }

    }


    class Program
    {
        static void Main(string[] args)
        {
            var options = new Options();
            Parser.Default.ParseArguments(args, options);

            if (string.IsNullOrWhiteSpace(options.Url))
            {
                Console.WriteLine("{0}", options.GetUsage());
                return;
            }

            var url = options.Url;
            var gifname = options.Gif;
            int delay = options.Delay;

            //var url = @"http://flv5.bn.netease.com/videolib3/1603/28/mwXEB9539/SD/mwXEB9539.flv?start=689&end=1082716";
            //var filename = "test.flv";
            //var gifname = "test.gif";
            //var web = new WebClient();
            //web.DownloadFile(url, filename);

            FFmpegMediaInfo.InitDllDirectory();
            if (!options.Verbose)
                ffmpeg.av_log_set_level(ffmpeg.AV_LOG_QUIET);

            using (var media = new FFmpegMediaInfo(url))
            {
                var stream = media.Streams.FirstOrDefault(s => s.StreamType == FFmpegStreamType.Video);
                if (stream == null)
                    return;

                TimeSpan begin = options.Begin.HasValue ? TimeSpan.FromMilliseconds(options.Begin.Value) : TimeSpan.Zero;
                TimeSpan end = options.End.HasValue ? TimeSpan.FromMilliseconds(options.End.Value) : TimeSpan.MaxValue;

                var frames = media.GetFrames(stream.Index, begin, end, 
                    i => {
                        if (options.Verbose)
                            Console.Write("=");
                        return false;
                    });
                var gifstream = File.OpenWrite(gifname);
                using (var gif = new GifEncoder(gifstream))
                {
                    TimeSpan prev = TimeSpan.Zero;

                    foreach (var frame in frames)
                    {
                        int width = options.Width.HasValue ? options.Width.Value : frame.Picture.Width;
                        int height = options.Height.HasValue ? options.Height.Value : frame.Picture.Height;

                        if (prev == TimeSpan.Zero || (frame.Position - prev).TotalMilliseconds >= delay)
                        {
                            if (width == frame.Picture.Width && height == frame.Picture.Height)
                                gif.AddFrame(frame.Picture);
                            else 
                                gif.AddFrame(frame.Picture.ScaleToFit(new Size(width, height)));
                            if (options.Verbose)
                                Console.Write("=");
                            prev = frame.Position;
                        }
                    }
                }
                if (options.Verbose)
                {
                    Console.WriteLine();
                    Console.WriteLine("Done.");
                }
                gifstream.Flush();
                gifstream.Close();
            }
        }
    }
}
