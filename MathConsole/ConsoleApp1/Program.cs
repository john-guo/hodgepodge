using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static double c(double n)
        {
            return Math.Sqrt(1 + n * c(n + 1));
        }


        static void Main(string[] args)
        {
            double m = 0;
            for (var n = 70; n>=1; --n)
            {
                m = n * Math.Sqrt(1 + m);
                Console.WriteLine($"{n} {m}");
            }

            Console.WriteLine(m);
            Console.ReadLine();
        }
    }
}
