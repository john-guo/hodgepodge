using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ff14log
{

    public static class LogConstant
    {
        public const byte begin = 0x02;
        public const byte end = 0x03;
        public const long magicNum = 0xfa8;
        public const byte splitByte = 0x3A;
    }

    public struct LogDesc
    {
        public byte type;
        public byte size;
        public byte[] content;
    }

    public static class BinaryReaderExt
    {
        public static byte[] ReadBytesUntil(this BinaryReader reader, byte b, bool omit = true)
        {
            List<byte> bytes = new List<byte>();
            if (omit) 
            {
                byte current;
                while ((current = reader.ReadByte()) != b)
                {
                    bytes.Add(current);
                }
            }
            else
            {
                while (reader.PeekChar() != b)
                {
                    bytes.Add(reader.ReadByte());
                } 
            }

            return bytes.ToArray();
        }
    }

    public class LogItem
    {
        public int timestamp;
        public int type1;
        public int type2;
        public LogDesc desc1;
        public LogDesc desc2;
        public string content;

        public List<LogDesc> descs;
        public List<string> contents;

        public LogItem()
        {
            this.descs = new List<LogDesc>();
            this.contents = new List<string>();
        }

        public string AllContent
        {
            get
            {
                return String.Format("{0}{1}", content,
                    contents.Aggregate((a, b) => String.Format("{0}{1}", a, b)));
            }
        }

        public DateTime LocalTime
        {
            get
            {
                var dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                return dt.AddSeconds(timestamp).ToLocalTime();
            }
        }

        public static LogItem Read(BinaryReader reader)
        {
            LogItem item = new LogItem();

            var header = reader.ReadBytesUntil(LogConstant.splitByte);

            var bs = header.Take(8).ToArray();
            item.timestamp = Convert.ToInt32(Encoding.UTF8.GetString(bs), 16);

            bs = header.Skip(8).Take(2).ToArray();
            item.type1 = Convert.ToInt32(Encoding.UTF8.GetString(bs), 16);

            bs = header.Skip(10).Take(2).ToArray();
            item.type2 = Convert.ToInt32(Encoding.UTF8.GetString(bs), 16);


            var desc = reader.ReadBytesUntil(LogConstant.splitByte);

            var descReader = new BinaryReader(new MemoryStream(desc));
            item.desc1 = ReadLogDesc(descReader);
            var content = descReader.ReadBytesUntil(LogConstant.begin, false);

            item.content = Encoding.UTF8.GetString(content);
            
            item.desc2 = ReadLogDesc(descReader);

            descReader.Close();
            descReader.Dispose();

            var count = CountContent(reader);

            content = reader.ReadBytes(count - 12);

            descReader = new BinaryReader(new MemoryStream(content));
            List<byte> tbs = new List<byte>();
            do 
            {
                int ch = descReader.PeekChar();
                if (ch == -1) 
                {
                    if (tbs.Count > 0) 
                    {
                        item.contents.Add(Encoding.UTF8.GetString(tbs.ToArray()));
                        tbs.Clear();
                    }
                    break;
                }
                if (ch == LogConstant.begin) 
                {
                    item.descs.Add(ReadLogDesc(descReader));

                    if (tbs.Count > 0) 
                    {
                        item.contents.Add(Encoding.UTF8.GetString(tbs.ToArray()));
                        tbs.Clear();
                    }
                } 
                else
                {
                    tbs.Add(descReader.ReadByte());
                }
            } while (true);

            descReader.Close();
            descReader.Dispose();

            return item;
        }

        public static void Reset(BinaryReader reader)
        {
            do
            {
                reader.ReadBytesUntil(LogConstant.splitByte);
                if (reader.PeekChar() == LogConstant.begin)
                    break;
            } while (true);

            reader.BaseStream.Position = reader.BaseStream.Position - 13;
        }

        private static LogDesc ReadLogDesc(BinaryReader reader)
        {
            LogDesc d;
            var begin = reader.ReadByte();
            d.type = reader.ReadByte();
            d.size = reader.ReadByte();
            d.content = reader.ReadBytes(d.size - 1);
            var end = reader.ReadByte();

            return d;
        }

        private static int CountContent(BinaryReader reader)
        {
            var current = reader.BaseStream.Position;

            do
            {
                reader.ReadBytesUntil(LogConstant.splitByte);
                if (reader.PeekChar() == LogConstant.begin)
                    break;
            } while (true);

            var count = reader.BaseStream.Position - current - 1;
            reader.BaseStream.Position = current;

            return (int)count;
        }
    }

    public static class LogParser
    {
        public static List<LogItem> Parse(string fileName)
        {
            var fs = File.OpenRead(fileName);
            var reader = new BinaryReader(fs);
            reader.BaseStream.Seek(LogConstant.magicNum, SeekOrigin.Begin);

            List<LogItem> itemlist = new List<LogItem>();
            while (true)
            {
                try
                {
                    var item = LogItem.Read(reader);
                    itemlist.Add(item);
                }
                catch
                {
                    try
                    {
                        LogItem.Reset(reader);
                    }
                    catch
                    {
                        break;
                    }
                }
            }

            reader.Close();
            reader.Dispose();

            return itemlist;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
                return;

            var filename = args[0];
            var result = LogParser.Parse(filename);

            var logfile = Path.ChangeExtension(filename, "txt");
            File.Create(logfile).Close();
            File.AppendAllLines(logfile, result.Select(i => String.Format("[{0}] {1} {2} {3}", i.LocalTime, i.type1, i.type2, i.AllContent)));
        }
    }
}
