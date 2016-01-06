using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CEbalance.Math
{
    public class Utils
    {
        public static int GCD(int a, int b)
        {
            int n1 = System.Math.Max(a, b);
            int n2 = System.Math.Min(a, b);
            while (n2 != 0)
            {
                n1 = n1 > n2 ? n1 : n2;
                int m = n1 % n2;
                n1 = n2;
                n2 = m;
            }

            return n1;
        }

        public static int LCM(int a, int b)
        {
            return a * b / GCD(a, b);
        }
    }
}
