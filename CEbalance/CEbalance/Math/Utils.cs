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
            int x = System.Math.Abs(a);
            int y = System.Math.Abs(b);
            if (x == y)
                return x;

            int n1 = x, n2 = y;
            if (y > x)
            {
                n1 = y;
                n2 = x;
            }

            while (n2 != 0)
            {
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
