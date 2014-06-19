using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace deamon
{
    class Program
    {
        static void Main(string[] args)
        {
            do
            {
                var r = Monitor();

                if (r)
                    Thread.Sleep(5000);
                else
                    Thread.Sleep(100);
            } while (true);
        }
        static bool Monitor()
        {
            var processes = Process.GetProcessesByName("node");
            if (processes.Any())
                return false;

            var info = new ProcessStartInfo("node.exe", "app");
            info.WorkingDirectory = @"E:\liteWebGameServer\framework";
            Process.Start(info);
            return true;
        }
    }
}
