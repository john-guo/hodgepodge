using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PermutationCombination;
using static System.Console;

namespace genComb
{
    class Program
    {
        static void Main(string[] args)
        {
            int count = int.Parse(args[0]);
            var prefix = string.Empty;
            var delimiter = " ";
            if (args.Length > 1)
            {
                prefix = args[1];
            }
            if (args.Length > 2)
            {
                delimiter = args[2];
            }

            var range = Enumerable.Range(1, count);

            for (int i = 1; i <= count; ++i)
            {
                WriteLine($"{i}:");
                foreach (var item in range.Combination(i))
                {
                    //WriteLine(item.Aggregate(string.Empty, (prev, j) => $"{prev} {j}"));

                    WriteLine(range.Aggregate(string.Empty, (prev, j) => $"{prev}{(string.IsNullOrWhiteSpace(prev)?string.Empty:(string.IsNullOrWhiteSpace(delimiter)?delimiter:" "+delimiter+" "))}{prefix}{j}={(item.Contains(j) ? 1 : 0)}"));
                }
            }
        }
    }
}
