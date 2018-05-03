using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics;

namespace WorkingTime
{
    class Program
    {
        struct WorkingTime
        {
            private long totalSeconds;

            public const long SecondsPerMinute = TimeSpan.TicksPerMinute / TimeSpan.TicksPerSecond;
            public const long SecondsPerHour = TimeSpan.TicksPerHour / TimeSpan.TicksPerSecond;
            public const long SecondsPerDay = SecondsPerHour * 9;

            public static WorkingTime FromSeconds(long seconds)
            {
                var wt = new WorkingTime()
                {
                    totalSeconds = seconds
                };

                return wt;
            }

            public double TotalSeconds => totalSeconds;

            public int Seconds => (int)(((TotalSeconds % SecondsPerDay) % SecondsPerHour) % SecondsPerMinute);

            public int Minutes => (int)(((TotalSeconds % SecondsPerDay) % SecondsPerHour) / SecondsPerMinute);

            public int Hours => (int)((TotalSeconds % SecondsPerDay) / SecondsPerHour);

            public int Days => (int)(TotalSeconds / SecondsPerDay);
        }

        const int MarginSecondsPerDay = 0 * (int)WorkingTime.SecondsPerMinute;

        static void Main(string[] args)
        {
            var today = DateTime.Today;

            var diff = 1 - (today.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)today.DayOfWeek);

            var begin = today.AddDays(diff);
            var end = begin.AddDays(5);

            Dictionary<DayOfWeek, DateTime> dayTime = new Dictionary<DayOfWeek, DateTime>();

            var log = EventLog.GetEventLogs().First(l => l.Log == "System");

            var wts = log.Entries.OfType<EventLogEntry>().Where(e => e.TimeGenerated >= begin && e.TimeGenerated <= end)
                .GroupBy(e => e.TimeGenerated.DayOfWeek).Select(g => {
                    var max = g.Max(e => e.TimeGenerated);
                    if (today.DayOfWeek == g.Key)
                        max = DateTime.Now;
                    var wt = (max - g.Min(e => e.TimeGenerated)).Add(TimeSpan.FromSeconds(MarginSecondsPerDay));
                    return new { g.Key, WT = wt };
                 });

            foreach (var wt in wts)
            {
                Console.WriteLine($"{wt.Key} : {wt.WT.Hours}:{wt.WT.Minutes}:{wt.WT.Seconds} ({wt.WT.TotalSeconds})");
            }

            var total = WorkingTime.FromSeconds((long)wts.Sum(w => w.WT.TotalSeconds));
            var remain = WorkingTime.FromSeconds((long)(total.TotalSeconds - TimeSpan.FromHours(5 * 9).TotalSeconds));

            Console.WriteLine($"{total.Days} {total.Hours}:{total.Minutes}:{total.Seconds} ({total.TotalSeconds})");
            Console.WriteLine($"{remain.Days} {remain.Hours}:{remain.Minutes}:{remain.Seconds} ({remain.TotalSeconds})");

            Console.ReadLine();
        }
    }
}
