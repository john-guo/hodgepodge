using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TaskTest;

namespace TaskTestConsole
{
    public class Class1
    {
        public static void Main()
        {
            var jobs = new JobScheduler();
            for (int i = 1; i <= 100; ++i)
            {
                int n = i;
                jobs.PenddingJob(job =>
                {
                    Console.WriteLine("{0} {1}", Thread.CurrentThread.ManagedThreadId, n);
                    Thread.Sleep(1000);
                }
                //, () =>
                //{
                //    Console.WriteLine("Start");
                //}, () =>
                //{
                //    Console.WriteLine("Finish");
                //}
                );

            }
            jobs.WaitAll().Wait();
            

            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
        }
    }
}
