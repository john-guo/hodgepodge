using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PermutationCombination;

namespace ValidateIncomeTax
{
    class Program
    {
        class Config
        {
            public double income;
            public double rate;
            public double deduct;
        }

        static void Main(string[] args)
        {
            int[] c = { 1, 2, 3 };
            Dictionary<int, Config> maxConfig = new Dictionary<int, Config>
            {
                { 1, new Config() { income = 12000, rate = 0.1, deduct = 210} },
                { 2, new Config() { income = 25000, rate = 0.2, deduct = 1410} },
                { 3, new Config() { income = 35000, rate = 0.25, deduct = 2660 } },
                { 4, new Config() { income = 3000, rate = 0.03, deduct = 0 } },
            };
            Dictionary<int, Config> minConfig = new Dictionary<int, Config>
            {
                { 1, new Config() { income = 3001, rate = 0.1, deduct = 210} },
                { 2, new Config() { income = 12001, rate = 0.2, deduct = 1410} },
                { 3, new Config() { income = 25001, rate = 0.25, deduct = 2660 } },
                { 4, new Config() { income = 1, rate = 0.03, deduct = 0 } },
            };
            var v = c.RangeCombination(12).Max(item =>
            {
                var income = item.Sum(i => maxConfig[i].income) / 12.0;
                var ic = calculate(income);
                var newtax = 12.0 * (income * ic.rate - ic.deduct);

                var oldtax = item.Sum(i => maxConfig[i].income * maxConfig[i].rate - maxConfig[i].deduct);

                return oldtax - newtax;
            });
            Console.WriteLine($"{v}");

            Console.WriteLine("------------------------------------------");

            v = c.RangeCombination(12).Max(item =>
            {
                var income = item.Sum(i => minConfig[i].income) / 12.0;
                var ic = calculate(income);
                var newtax = 12.0 * (income * ic.rate - ic.deduct);

                var oldtax = item.Sum(i => minConfig[i].income * minConfig[i].rate - minConfig[i].deduct);

                return oldtax - newtax;
            });
            Console.WriteLine($"{v}");


            Console.ReadLine();
        }

        static Config calculate(double income)
        {
            if (income < 3000)
                return new Config() { rate = 0, deduct = 0 };
            if (income <= 12000)
                return new Config() { rate = 0.1, deduct = 210 };
            if (income <= 25000)
                return new Config() { rate = 0.2, deduct = 1410 };
            if (income <= 35000)
                return new Config() { rate = 0.25, deduct = 2660 };
            if (income <= 55000)
                return new Config() { rate = 0.3, deduct = 4410 };
            if (income <= 80000)
                return new Config() { rate = 0.35, deduct = 7160 };

            return new Config() { rate = 0.45, deduct = 15160 };
        }
    }
}
