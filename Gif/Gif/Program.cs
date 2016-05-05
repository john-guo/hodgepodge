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
using video2gif;

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

            TimeSpan begin = options.Begin.HasValue ? TimeSpan.FromMilliseconds(options.Begin.Value) : TimeSpan.Zero;
            TimeSpan end = options.End.HasValue ? TimeSpan.FromMilliseconds(options.End.Value) : TimeSpan.MaxValue;
            if (end <= begin)
            {
                end = TimeSpan.MaxValue;
            }

            FFmpegMediaInfo.InitDllDirectory();
            if (!options.Verbose)
                ffmpeg.av_log_set_level(ffmpeg.AV_LOG_QUIET);

            Video2Gif.Process(url, gifname, begin, end, 
                options.Width, options.Height, options.Delay, 
                i => Console.Write("="), 
                () => {
                    Console.WriteLine();
                    Console.WriteLine("Done.");
                }, true);
        }
    }
}
