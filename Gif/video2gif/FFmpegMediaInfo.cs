using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FFmpeg.AutoGen;
using System.Runtime.InteropServices;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace FFmpeg
{
    public unsafe class FFmpegMediaInfo : IDisposable
    {
        internal AVFormatContext* AVFormatContext;

        /// <summary>The filename as passed from AVUTIL</summary>
        public string Filename { get; internal set; }
        /// <summary>The duration</summary>
        public TimeSpan Duration { get; internal set; }
        /// <summary>The average bit rate</summary>
        public long BitRate { get; internal set; }
        /// <summary>The video resolution of the first video stream</summary>
        public Size VideoResolution { get; internal set; }
        /// <summary>Found stream information</summary>
        public List<FFmpegStreamInfo> Streams { get; internal set; }
        /// <summary>Meta information catalog</summary>
        public Dictionary<string, string> Metadata { get; internal set; }


        [DllImport("avcodec-57", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private unsafe extern static byte* avcodec_get_name(AVCodecID id);

        private static string getAvCodecName(AVCodecID id)
        {
            var ptr = avcodec_get_name(id);
            return Marshal.PtrToStringAnsi((IntPtr)ptr);
        }

        public const long TIME_FACTOR = TimeSpan.TicksPerSecond / ffmpeg.AV_TIME_BASE;

        #region Constructors

        public FFmpegMediaInfo()
        {
            InitDllDirectory();

            this.Metadata = new Dictionary<string, string>();
            this.Streams = new List<FFmpegStreamInfo>();
        }

        public FFmpegMediaInfo(string filename) : this()
        {
            this.OpenFileOrUrl(filename);
        }

        #endregion

        /// <summary>
        /// Disposes all active objects and opens a file or url
        /// </summary>
        /// <param name="url">path of the media file</param>
        public void OpenFileOrUrl(string url)
        {
            this.Dispose();

            // Initialize instance of AVUTIL, AVCODEC, AVFORMAT
            ffmpeg.av_register_all();
            ffmpeg.avcodec_register_all();
            ffmpeg.avformat_network_init();

            // Open source file
            AVFormatContext* pFormatContext = ffmpeg.avformat_alloc_context();
            if (ffmpeg.avformat_open_input(&pFormatContext, url, null, null) != 0)
                throw new Exception("File or URL not found!");

            if (ffmpeg.avformat_find_stream_info(pFormatContext, null) != 0)
                throw new Exception("No stream information found!");

            // Collect general media information
            this.Filename = new String(pFormatContext->filename);
            this.Metadata = ToDictionary(pFormatContext->metadata);
            this.Duration = ToTimeSpan(pFormatContext->duration);
            this.BitRate = pFormatContext->bit_rate;

            // Collect information about streams
            bool isFirstVideoStream = true;
            for (int i = 0; i < pFormatContext->nb_streams; i++)
            {
                #region Read stream information
                FFmpegStreamInfo info = new FFmpegStreamInfo();
                info.AVStream = pFormatContext->streams[i];
                info.Index = pFormatContext->streams[i]->index;
                switch (pFormatContext->streams[i]->codec->codec_type)
                {
                    case AVMediaType.AVMEDIA_TYPE_VIDEO:
                        info.StreamType = FFmpegStreamType.Video;
                        info.Video_Width = pFormatContext->streams[i]->codec->width;
                        info.Video_Height = pFormatContext->streams[i]->codec->height;
                        info.Video_FPS = ToDouble(pFormatContext->streams[i]->codec->framerate);
                        if (isFirstVideoStream)
                        {
                            this.VideoResolution = new Size(info.Video_Width, info.Video_Height);
                            isFirstVideoStream = false;
                        }
                        break;
                    case AVMediaType.AVMEDIA_TYPE_AUDIO:
                        info.StreamType = FFmpegStreamType.Audio;
                        info.Audio_SampleRate = pFormatContext->streams[i]->codec->sample_rate;
                        info.Audio_Channels = pFormatContext->streams[i]->codec->channels;
                        break;
                    case AVMediaType.AVMEDIA_TYPE_SUBTITLE:
                        info.StreamType = FFmpegStreamType.Subtitle;
                        break;
                    default:
                        info.StreamType = FFmpegStreamType.Unknown;
                        break;
                }
                info.MetaData = ToDictionary(pFormatContext->streams[i]->metadata);
                if (info.MetaData.ContainsKey("language")) info.Language = info.MetaData["language"];
                info.BitRate = pFormatContext->streams[i]->codec->bit_rate;
                info.FourCC = ToFourCCString(pFormatContext->streams[i]->codec->codec_tag);
                info.CodecName = getAvCodecName(pFormatContext->streams[i]->codec->codec_id);
                this.Streams.Add(info);
                #endregion
            }

            this.AVFormatContext = pFormatContext;
            _isDisposed = false;
        }

        public void ProcessFrames(TimeSpan startTime, TimeSpan endTime, int? width, int? height, int? delay, Action<Bitmap, TimeSpan, int> progress)
        {
            FFmpegStreamInfo stream = this.Streams.FirstOrDefault(s => s.StreamType == FFmpegStreamType.Video);

            if (stream == null || stream.StreamType != FFmpegStreamType.Video)
                throw new Exception("No video stream selected!");

            ExtractFrames(stream.AVStream, startTime, endTime, width, height, delay, (i, ts, img) => progress(img, ts, i));
        }

        private unsafe void ExtractFrames(AVStream* vidStream, TimeSpan startTime, TimeSpan endTime, int? width, int? height, int? delay, Action<int, TimeSpan, Bitmap> progress)
        {
            #region Preparations
            AVCodecContext codecContext = *(vidStream->codec);
            int src_width = codecContext.width;
            int src_height = codecContext.height;
            int dest_width = width.HasValue ? width.Value : src_width;
            int dest_height = height.HasValue ? height.Value : src_height;

            long duration = this.AVFormatContext->duration;
            AVPixelFormat sourcePixFmt = codecContext.pix_fmt;
            AVCodecID codecId = codecContext.codec_id;
            var convertToPixFmt = AVPixelFormat.AV_PIX_FMT_BGR24;

            SwsContext* pConvertContext = ffmpeg.sws_getContext(src_width, src_height, sourcePixFmt, dest_width, dest_height, convertToPixFmt, ffmpeg.SWS_FAST_BILINEAR, null, null, null);
            if (pConvertContext == null)
                throw new Exception("Could not initialize the conversion context");
            AVCodecContext* pCodecContext = &codecContext;

            var pConvertedFrame = (AVPicture*)ffmpeg.av_frame_alloc();
            int convertedFrameBufferSize = ffmpeg.avpicture_get_size(convertToPixFmt, dest_width, dest_height);
            var pConvertedFrameBuffer = (sbyte*)ffmpeg.av_malloc((uint)convertedFrameBufferSize);
            ffmpeg.avpicture_fill(pConvertedFrame, pConvertedFrameBuffer, convertToPixFmt, dest_width, dest_height);

            AVCodec* pCodec = ffmpeg.avcodec_find_decoder(codecId);
            if (pCodec == null)
                throw new Exception("Unsupported codec");

            if ((pCodec->capabilities & ffmpeg.CODEC_CAP_TRUNCATED) == ffmpeg.CODEC_CAP_TRUNCATED)
                pCodecContext->flags |= ffmpeg.CODEC_FLAG_TRUNCATED;

            if (ffmpeg.avcodec_open2(pCodecContext, pCodec, null) < 0)
                throw new Exception("Could not open codec");

            AVFrame* pDecodedFrame = ffmpeg.av_frame_alloc();

            var packet = new AVPacket();
            AVPacket* pPacket = &packet;
            ffmpeg.av_init_packet(pPacket);

            AVCodecContext cont = *vidStream->codec;
            #endregion

            // Seek for key frames only - otherwise first frames are currupted until a key frame is decoded
            this.AVFormatContext->seek2any = 0;

            var currTS = startTime.Ticks / TIME_FACTOR;

            if (currTS > 0)
                ffmpeg.av_seek_frame(this.AVFormatContext, -1, currTS, ffmpeg.AVSEEK_FLAG_BACKWARD);

            TimeSpan pos;
            double timeBase = ToDouble(vidStream->time_base); // DTS or PTS timestamp to seconds multiplicator
            Bitmap img;
            int f = 0;
            TimeSpan prev = TimeSpan.Zero;
            bool end;
            do
            {
                // Decode next image - ATTENTION: Get a image copy or it will be unallocated!
                img = ExtractNextImage2(pCodecContext, pPacket, vidStream, pConvertContext, pDecodedFrame, pConvertedFrame, dest_width, dest_height, false, timeBase, delay, prev, out pos, out end);

                if (img == null && end) break;
                if (img == null) continue;

                prev = pos;

                if (progress != null)
                    progress(++f, pos, img);

                img.Dispose();
                if (pos >= endTime) break;

            } while (true); // end while

            #region Free allocated memory
            ffmpeg.av_free(pConvertedFrame);
            ffmpeg.av_free(pConvertedFrameBuffer);
            ffmpeg.sws_freeContext(pConvertContext);
            ffmpeg.av_free(pDecodedFrame);
            ffmpeg.avcodec_close(pCodecContext);
            #endregion
        }

        private unsafe Bitmap ExtractNextImage2(AVCodecContext* pCodecContext, AVPacket* pPacket, AVStream* vidStream, SwsContext* pConvertContext, AVFrame* pDecodedFrame, AVPicture* pConvertedFrame, int width, int height, bool createCopy, double timeBase, int? delay, TimeSpan prev, out TimeSpan pos, out bool end)
        {
            pos = new TimeSpan();
            end = false;
            Bitmap result = null;

            int gotPicture = 0;

            while (gotPicture != 1)
            {
                if (ffmpeg.av_read_frame(this.AVFormatContext, pPacket) < 0)
                {
                    end = true;
                    result = null;
                    break;
                }

                if (pPacket->stream_index != vidStream->index)
                    continue;

                gotPicture = 0;
                int size = ffmpeg.avcodec_decode_video2(pCodecContext, pDecodedFrame, &gotPicture, pPacket);
                if (size < 0)
                    throw new Exception("Error while decoding frame!");

                if (gotPicture == 1)
                {
                    // Get current position from frame
                    pos = ToTimeSpan(ffmpeg.av_frame_get_best_effort_timestamp(pDecodedFrame), timeBase);

                    if (delay.HasValue && prev != TimeSpan.Zero && (pos - prev).TotalMilliseconds < delay)
                    {
                        return null;
                    }

                    // Extract image
                    sbyte** src = &pDecodedFrame->data0;
                    sbyte** dst = &pConvertedFrame->data0;
                    int src_height = pCodecContext->height;
                    ffmpeg.sws_scale(pConvertContext, src, pDecodedFrame->linesize, 0, src_height, dst, pConvertedFrame->linesize);
                    var imageBufferPtr = new IntPtr(pConvertedFrame->data0);
                    int linesize = pConvertedFrame->linesize[0];
                    result = new Bitmap(width, height, linesize, PixelFormat.Format24bppRgb, imageBufferPtr);
                }

            }

            return result;
        }

        private bool _isDisposed = true;
        public void Dispose()
        {
            if (_isDisposed) return;

            AVFormatContext* pFormatContext = this.AVFormatContext;
            ffmpeg.avformat_close_input(&pFormatContext);

            _isDisposed = true;
        }


        #region static methods

        #region InitDllDirectory

        internal static void InitDllDirectory()
        {
            if (_initDllDirectoryDone) return;

            var dllpath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), Environment.Is64BitOperatingSystem ? "x64" : "x86");

            SetDllDirectory(dllpath); // In .NET 4 you can use Environment.Is64BitProcess

            _initDllDirectoryDone = true;
        }

        private static bool _initDllDirectoryDone = false;

        [DllImport("kernel32", SetLastError = true)]
        private static extern bool SetDllDirectory(string lpPathName);

        #endregion

        #region Conversions

        private unsafe static Dictionary<string, string> ToDictionary(AVDictionary* dict)
        {
            Dictionary<string, string> result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            int size = ffmpeg.av_dict_count(dict);
            int AV_DICT_IGNORE_SUFFIX = 2;
            AVDictionaryEntry* curr = null;
            for (int n = 0; n < size; n++)
            {
                curr = ffmpeg.av_dict_get(dict, "", curr, AV_DICT_IGNORE_SUFFIX);
                string key = new String(curr->key);
                string val = new String(curr->value);
                result[key] = val;
            }

            return result;
        }

        private static string ToFourCCString(uint value)
        {
            return value == 0 ? "" : Encoding.ASCII.GetString(new byte[]
            {
                Convert.ToByte(value & 0xFF),
                Convert.ToByte((value >> 8) & 0xFF),
                Convert.ToByte((value >> 16) & 0xFF),
                Convert.ToByte((value >> 24) & 0xFF)
            });
        }

        private static double ToDouble(AVRational value)
        {
            return Convert.ToDouble(value.num) / Convert.ToDouble(value.den);
        }
        private static string ToDescString(AVRational value)
        {
            double dVal = Math.Floor(ToDouble(value) * 10.0) / 10;
            return dVal.ToString();
        }

        private static TimeSpan ToTimeSpan(long value)
        {
            return new TimeSpan(value * TIME_FACTOR);
        }
        private static TimeSpan ToTimeSpan(long value, double timeBase)
        {
            return TimeSpan.FromSeconds(Convert.ToDouble(value) * timeBase);
        }

        private static string ToFormattedString(TimeSpan s)
        {
            return String.Format("{0}:{1:00}:{2:00}" /*.{3:000}" */, s.Hours, s.Minutes, s.Seconds, s.Milliseconds);
        }

        #endregion

        #endregion
    }
    
    public enum FFmpegStreamType { Video, Audio, Subtitle, Unknown }

    public unsafe class FFmpegStreamInfo
    {
        internal AVStream* AVStream;

        /// <summary>The index of the stream</summary>
        public int Index { get; set; }
        /// <summary>The type of the stream</summary>
        public FFmpegStreamType StreamType { get; set; }
        /// <summary>The name of the codec</summary>
        public string CodecName { get; set; }
        /// <summary>The FourCC code of the codec</summary>
        public string FourCC { get; set; }
        /// <summary>The average bit rate of this stream</summary>
        public long BitRate { get; set; }
        /// <summary>Video width (only filled if this is a video stream)</summary>
        public int Video_Width { get; set; }
        /// <summary>Video height (only filled if this is a video stream)</summary>
        public int Video_Height { get; set; }
        /// <summary>Video frames per second (only filled if this is a video stream)</summary>
        public double Video_FPS { get; set; }
        /// <summary>Audio sample rate (only filled if this is a audio stream)</summary>
        public int Audio_SampleRate { get; set; }
        /// <summary>Number of audio channels in this stream (only filled if this is a audio stream)</summary>
        public int Audio_Channels { get; set; }
        /// <summary>Copy of MetaData["language"] if exists</summary>
        public string Language { get; set; }
        /// <summary>A collection of meta information of this stream</summary>
        public Dictionary<string, string> MetaData { get; set; }

        internal FFmpegStreamInfo()
        {
            this.MetaData = new Dictionary<string, string>();
            this.StreamType = FFmpegStreamType.Unknown;
        }

        public override string ToString()
        {
            switch (this.StreamType)
            {
                case FFmpegStreamType.Video:
                    return String.Format("Video[{0}/{1}]", this.FourCC, this.CodecName, this.BitRate);
                case FFmpegStreamType.Audio:
                    return String.Format(String.IsNullOrEmpty(this.Language) || this.Language.Equals("und", StringComparison.OrdinalIgnoreCase) ? "Audio[{0}/{1}]" : "Audio[{0}/{1}/{3}]", this.FourCC, this.CodecName, this.BitRate, this.Language);
                case FFmpegStreamType.Subtitle:
                    return String.Format("Subs[{0}]", this.Language);
                default:
                    return "Unknown";
            }
        }
    }
}
