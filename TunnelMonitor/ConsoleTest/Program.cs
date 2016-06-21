using System;
using System.Collections.Generic;
using System.Text;
using TunnelMonitor;

namespace ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var monitor = Monitor.FromConfig();

            monitor.Start();

            Console.ReadLine();
        }
    }
}
