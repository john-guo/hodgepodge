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
            Test();
            ReadLine();
            return;
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

        static void Test()
        {
            var range = Enumerable.Range(1, 12);
            foreach (var item in range.Combination(4))
            {
                int ret = cal(item);
                if (ret >= 10)
                {
                    WriteLine("{0} -> {1}", item.Aggregate("", (a, b) => $"{a} {b}"), ret);
                }
            }
        }

        static int cal(IEnumerable<int> input)
        {
            if (input.All(i => i == 0))
                return 1;
            int prev = 0;
            List<int> next = new List<int>();
            foreach (var item in input)
            {
                if (prev == 0)
                {
                    prev = item;
                    continue;
                }
                next.Add(Math.Abs(item - prev));
                prev = item;
            }
            next.Add(Math.Abs(input.First() - input.Last()));
            return 1 + cal(next);
        }
    }
}
