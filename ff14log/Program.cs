using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

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


    public enum LogBattleType
    {
        None,
        Cast,
        Heal,
        Damage,
        Dead,
        Rescue,
        Item
    }

    public class LogBattleItem
    {
        public const string flag = @"";

        public int time;
        public string main;
        public string target;
        public string skill;
        public string hp;
        public LogBattleType type;

        public LogBattleItem()
        {
            time = 0;
            main = String.Empty;
            target = String.Empty;
            skill = String.Empty;
            hp = String.Empty;
            type = LogBattleType.None;
        }

        public string GetName(string name)
        {
            var target = name.Trim();
            if (target.StartsWith(flag))
                target = target.Substring(flag.Length - 1).Trim();

            return target;
        }

        public bool IsEmpty(string name)
        {
            if (String.IsNullOrWhiteSpace(name))
                return true;

            var target = name.Trim();
            if (target.StartsWith(flag))
                return true;

            return false;
        }

        public bool IsNone
        {
            get { return type == LogBattleType.None; }
        }

        public int Hp
        {
            get
            {
                int d = 0;

                if (String.IsNullOrWhiteSpace(hp))
                    return d;

                if (!Int32.TryParse(hp, out d))
                {
                    var ex = new Regex(@"(\d+)\(.*\)");
                    d = Int32.Parse(ex.Match(hp).Groups[1].Value);
                }

                return d;
            }
        }
    }

    public class LogAnalyzer
    {
        const string damagePointPattern = @"\d+\([\+\-]\d+%\)|\d+";
        const string attackPattern = @"(?:(?<target>.+)发动攻击|)\s*(?:|)\s*";
        const string damagePattern = attackPattern + @"(?:暴击！|)\s*(?<victim>.+?)(?:格挡住了！|招架住了！|)\s*受到(?:了|)(?<damage>" + damagePointPattern + ")点伤(?:害|)(?:。|)";
        const string missPattern = attackPattern + @"失误了！";

        static readonly Regex exCast = new Regex(@"(?<target>.+)(?:咏唱|发动)了“(?<skill>.+)”");
        static readonly Regex exCasting = new Regex(@"(?<target>.+)正在(?:咏唱|发动)“(?<skill>.+)”。");
        static readonly Regex exCastStop = new Regex(@"(?<target>.+)中断了(?:咏唱|发动)“(?<skill>.+)”。");
        static readonly Regex exCastInterrupt = new Regex(@"(?<target>.+)的“(?<skill>.+)”(?:被打断|中断)了。");
        static readonly Regex exMount = new Regex(@"(?<target>.+)乘上了(?<skill>.+)(?:。|)");
        static readonly Regex exBuff = new Regex(@"(?:对|)(?<target>.+)附加了“(?<skill>.+)”(?:的|)效果。");
        static readonly Regex exEbuff = new Regex(@"(?<target>.+)陷入了“(?<skill>.+)”效果。");
        static readonly Regex exHealing = new Regex(@"(?:暴击！\s*|)(?<target>.+)(?:恢复|吸收)了(?<hp>\d+)点(?:体力|魔力|技力)。");
        static readonly Regex exUnbuff = new Regex(@"(?<target>.+)的“(?<skill>.+)”状态效果消失了。");
        static readonly Regex exRescue = new Regex(@"(?<target>.+)从无法战斗状态复苏了。");
        static readonly Regex exDamage = new Regex(damagePattern);
        static readonly Regex exAura = new Regex(@"“(?<skill>.+)”效果发动(?:。|)");
        static readonly Regex exMiss = new Regex(missPattern);
        static readonly Regex exAllMiss = new Regex("失误！|失误了！");
        static readonly Regex exThreat = new Regex(@"(?<target>.+)仇恨提高。");
        static readonly Regex exNothing = new Regex(@"对(?<target>.+)没有效果。");
        static readonly Regex exBeat = new Regex(@"(?<target>.+?)(?:将|被)(?<monster>.+)打倒了。");
        static readonly Regex exBeatOne = new Regex(@"(?<target>.+)被打倒了。");
        static readonly Regex exResist = new Regex(@"(?<target>.+)完全抵抗住了“(?<skill>.+)”。");
        static readonly Regex exBreak = new Regex(@"(?<target>.+)破坏了(?<monster>.+)。");
        static readonly Regex exItem = new Regex(@"(?<target>.+)使用了“(?<skill>.+)”");
        static readonly Regex exImmune = new Regex(@"无效！(?<target>.+)没有受到伤害。");
        static readonly Regex exImmune2 = new Regex(@"(?<target>.+)发动攻击\s*\s*对(?<monster>.+)无效。");
        static readonly Regex exImmune3 = new Regex(@"(?<target>.+)令“(?<skill>.+)”无效化了。");

        static Dictionary<int, List<AnalyzeItem>> regexDict = new Dictionary<int, List<AnalyzeItem>>();

        static string lastTarget = String.Empty;

        abstract class AnalyzeItem
        {
            protected Regex rExpression { get; private set; }
            private LogBattleItem item;

            protected AnalyzeItem(Regex ex)
            {
                item = new LogBattleItem();
                rExpression = ex;
            }

            public bool Match(string content)
            {
                var m = rExpression.Match(content);
                if (!m.Success)
                    return false;

                item = Process(m);

                return true;
            }

            public LogBattleItem GetItem() { return item; }

            protected abstract LogBattleItem Process(Match m);
        }

        class TextAnalyzeItem : AnalyzeItem
        {
            public TextAnalyzeItem(Regex ex)
                : base(ex)
            { }

            protected override LogBattleItem Process(Match m)
            {
                LogBattleItem item = new LogBattleItem();
                item.type = LogBattleType.None;

                return item;
            }
        }

        class DamageAnalyzeItem : AnalyzeItem
        {
            public DamageAnalyzeItem()
                : base(exDamage)
            { }

            protected override LogBattleItem Process(Match m)
            {
                LogBattleItem item = new LogBattleItem();
                item.type = LogBattleType.Damage;
                item.main = m.Groups["target"].Value;
                item.target = m.Groups["victim"].Value;
                item.hp = m.Groups["damage"].Value;
                item.skill = String.Empty;

                if (item.IsEmpty(item.main) && !String.IsNullOrWhiteSpace(lastTarget))
                {
                    item.main = lastTarget;
                    lastTarget = String.Empty;
                }

                return item;
            }
        }

        class CastAnalyzeItem: AnalyzeItem
        {
            private bool targetLink;

            public CastAnalyzeItem(Regex ex, bool tlink = false)
                : base(ex)
            {
                targetLink = tlink;
            }

            protected override LogBattleItem Process(Match m)
            {
                LogBattleItem item = new LogBattleItem();
                item.type = LogBattleType.Cast;
                item.main = m.Groups["target"].Value;
                item.target = String.Empty;
                item.hp = String.Empty;
                item.skill = m.Groups["skill"].Value;

                if (targetLink)
                {
                    lastTarget = item.main;
                }

                return item;
            }
        }

        class SkillAnalyzeItem: AnalyzeItem
        {
            public SkillAnalyzeItem(Regex ex)
                : base(ex)
            { }

            protected override LogBattleItem Process(Match m)
            {
                LogBattleItem item = new LogBattleItem();
                item.type = LogBattleType.Cast;
                item.main = String.Empty;
                item.target = String.Empty;
                item.hp = String.Empty;
                item.skill = m.Groups["skill"].Value;

                return item;
            }
        }

        class TargetAnalyzeItem: AnalyzeItem
        {
            LogBattleType type;

            public TargetAnalyzeItem(Regex ex, LogBattleType type = LogBattleType.Cast)
                : base(ex)
            {
                this.type = type;
            }

            protected override LogBattleItem Process(Match m)
            {
                LogBattleItem item = new LogBattleItem();
                item.type = this.type;
                item.main = m.Groups["target"].Value;
                item.target = String.Empty;
                item.hp = String.Empty;
                item.skill = String.Empty;

                return item;
            }
        }

        class DeadAnalyzeItem : AnalyzeItem
        {
            public DeadAnalyzeItem()
                : this(exBeat)
            { }

            protected DeadAnalyzeItem(Regex ex)
                : base(ex)
            { }

            protected override LogBattleItem Process(Match m)
            {
                LogBattleItem item = new LogBattleItem();
                item.type = LogBattleType.Dead;
                item.main = m.Groups["target"].Value;
                item.target = m.Groups["monster"].Value;
                item.hp = String.Empty;
                item.skill = String.Empty;

                return item;
            }
        }

        class BreakAnalyzeItem : DeadAnalyzeItem
        {
            public BreakAnalyzeItem()
                : base(exBreak)
            { }

            protected override LogBattleItem Process(Match m)
            {
                LogBattleItem item = new LogBattleItem();
                item.type = LogBattleType.Dead;
                item.main = m.Groups["target"].Value;
                item.target = m.Groups["monster"].Value;
                item.hp = String.Empty;
                item.skill = String.Empty;

                return item;
            }
        }

        class HealAnalyzeItem : AnalyzeItem
        {
            public HealAnalyzeItem()
                : base(exHealing)
            { }

            protected override LogBattleItem Process(Match m)
            {
                LogBattleItem item = new LogBattleItem();
                item.type = LogBattleType.Dead;
                item.main = m.Groups["target"].Value;
                item.target = String.Empty;
                item.hp = m.Groups["hp"].Value;
                item.skill = String.Empty;

                return item;
            }
        }


        readonly static AnalyzeItem aiDamage = new DamageAnalyzeItem();
        readonly static AnalyzeItem aiCasting = new CastAnalyzeItem(exCasting);
        readonly static AnalyzeItem aiCast = new CastAnalyzeItem(exCast, true);
        readonly static AnalyzeItem aiCastStop = new CastAnalyzeItem(exCastStop);
        readonly static AnalyzeItem aiCastInterrupt = new CastAnalyzeItem(exCastInterrupt);
        readonly static AnalyzeItem aiMount = new CastAnalyzeItem(exMount);
        readonly static AnalyzeItem aiBuff = new CastAnalyzeItem(exBuff);
        readonly static AnalyzeItem aiUnbuff = new CastAnalyzeItem(exUnbuff);
        readonly static AnalyzeItem aiEBuff = new CastAnalyzeItem(exEbuff);
        readonly static AnalyzeItem aiResist = new CastAnalyzeItem(exResist);
        readonly static AnalyzeItem aiAura = new SkillAnalyzeItem(exAura);
        readonly static AnalyzeItem aiThreat = new TargetAnalyzeItem(exThreat);
        readonly static AnalyzeItem aiMiss = new TargetAnalyzeItem(exMiss);
        readonly static AnalyzeItem aiNothing = new TargetAnalyzeItem(exNothing);
        readonly static AnalyzeItem aiImmune = new TargetAnalyzeItem(exImmune);
        readonly static AnalyzeItem aiImmune2 = new TargetAnalyzeItem(exImmune2);
        readonly static AnalyzeItem aiImmune3 = new TargetAnalyzeItem(exImmune3);
        readonly static AnalyzeItem aiRescue = new TargetAnalyzeItem(exRescue, LogBattleType.Rescue);
        readonly static AnalyzeItem aiDead = new DeadAnalyzeItem();
        readonly static AnalyzeItem aiDeadOne = new TargetAnalyzeItem(exBeatOne, LogBattleType.Dead);
        readonly static AnalyzeItem aiBreak = new BreakAnalyzeItem();
        readonly static AnalyzeItem aiHealing = new HealAnalyzeItem();
        readonly static AnalyzeItem aiAllMiss = new TextAnalyzeItem(exAllMiss);
        readonly static AnalyzeItem aiItem = new CastAnalyzeItem(exItem);

        static LogAnalyzer() 
        {
            regexDict.Add(41, new List<AnalyzeItem>() { aiDamage, aiThreat });
            regexDict.Add(42, new List<AnalyzeItem>() { aiMiss, aiNothing, aiResist, aiAllMiss, aiImmune, aiImmune2, aiImmune3 });
            regexDict.Add(43, new List<AnalyzeItem>() { aiCasting, aiCast, aiCastStop, aiCastInterrupt, aiMount });
            regexDict.Add(44, new List<AnalyzeItem>() { aiItem });
            regexDict.Add(45, new List<AnalyzeItem>() { aiHealing });
            regexDict.Add(46, new List<AnalyzeItem>() { aiBuff, aiAura });
            regexDict.Add(47, new List<AnalyzeItem>() { aiBuff, aiEBuff });
            regexDict.Add(48, new List<AnalyzeItem>() { aiUnbuff });
            regexDict.Add(49, new List<AnalyzeItem>() { aiUnbuff });
            regexDict.Add(58, new List<AnalyzeItem>() { aiRescue, aiDead, aiDeadOne, aiBreak });
            regexDict.Add(169, new List<AnalyzeItem>() { aiDamage, aiThreat });
            regexDict.Add(170, new List<AnalyzeItem>() { aiNothing, aiResist, aiAllMiss, aiImmune, aiImmune2, aiImmune3 });
            regexDict.Add(171, new List<AnalyzeItem>() { aiCasting });
            regexDict.Add(173, new List<AnalyzeItem>() { aiHealing });
            regexDict.Add(174, new List<AnalyzeItem>() { aiBuff, aiAura });
            regexDict.Add(175, new List<AnalyzeItem>() { aiBuff, aiEBuff });
            regexDict.Add(176, new List<AnalyzeItem>() { aiUnbuff });
            regexDict.Add(177, new List<AnalyzeItem>() { aiUnbuff });
            regexDict.Add(186, new List<AnalyzeItem>() { aiDead, aiDeadOne });
        }

        static readonly int[] ignoreMainType = { 0, 2, 32, 34 };
        static readonly int[] ignoreSubType = { 57, 60, 62, 64, 65, 66, 70, 185, 201 };

        public LogBattleItem Analyse(LogItem item)
        {
            if (ignoreMainType.Contains(item.type1))
                return null;

            if (ignoreSubType.Contains(item.type2))
                return null;

            if (!regexDict.ContainsKey(item.type2))
            {
                throw new Exception(String.Format("{0},{1},{2}", item.type1, item.type2, item.AllContent));
            }

            var content = item.AllContent.Trim();
            bool matched = false;

            LogBattleItem lbi = null;
            foreach (var bi in regexDict[item.type2])
            {
                if (!bi.Match(content))
                    continue;

                matched = true;
                lbi = bi.GetItem();
                lbi.time = item.timestamp;
                break;    
            }

            if (!matched)
            {
                throw new Exception(String.Format("{0},{1},{2}", item.type1, item.type2, item.AllContent));
            }

            return lbi;
        }
    }

    class Program
    {

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        /*
        static void Main(string[] args)
        {
            //Export(args.Length > 0 ? args[0] : "log.txt", args.Length == 0);

            List<LogItem> items = new List<LogItem>();
            List<LogBattleItem> table = new List<LogBattleItem>();
            var a = new LogAnalyzer();
            var alllogs = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.log", SearchOption.TopDirectoryOnly);
            foreach (var file in alllogs)
            {
                var logs = LogParser.Parse(file);

                foreach (var l in logs)
                {
                    var i = a.Analyse(l);
                    if (i == null)
                        continue;
                    items.Add(l);
                    table.Add(i);
                }

                var secs = items.Last().timestamp - items.First().timestamp;
                var damage = table.Where(i => i.type == LogBattleType.Damage && i.main == "Link").Sum(i => i.Hp);

                Console.WriteLine("{0} {1}", damage, damage / secs);

                items.Clear();
                table.Clear();
            }

            Console.ReadLine();
        }

        static void Export(string logName, bool exportAll)
        {
            var logfile = logName;
            if (exportAll)
            {
                File.Create(logfile).Close();
                var alllogs = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.log", SearchOption.TopDirectoryOnly);
                foreach (var file in alllogs)
                {
                    var logs = LogParser.Parse(file);
                    File.AppendAllLines(logfile, logs.Select(i => String.Format("[{0}] {1} {2} {3}", i.LocalTime, i.type1, i.type2, i.AllContent)));
                }

                return;
            }

            var filename = logName;
            var result = LogParser.Parse(filename);

            logfile = Path.ChangeExtension(filename, "txt");
            File.Create(logfile).Close();
            File.AppendAllLines(logfile, result.Select(i => String.Format("[{0}] {1} {2} {3}", i.LocalTime, i.type1, i.type2, i.AllContent)));
        }
        */
    }
}
