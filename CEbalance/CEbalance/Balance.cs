using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CEbalance.Math;
using CEbalance.Symbol;

namespace CEbalance
{
    public static class Balance
    {
        public static Dictionary<int, Fraction> Slove(Matrix<Fraction> m)
        {
            var result = new Dictionary<int, Fraction>();

            m.GaussElimination();

            int r = System.Math.Min(m.Row, m.Col);

            for (int j = r; j < m.Col; ++j)
            {
                int lcm = 0;
                for (int i = 0; i < r; ++i)
                {
                    var v = m[i, j];
                    if (v == 0)
                        continue;

                    if (lcm == 0)
                    {
                        lcm = v.Denominator;
                        continue;
                    }

                    lcm = Utils.LCM(lcm, v.Denominator);
                }

                result[j] = lcm;
            }

            for (int i = r - 1; i >= 0; i--)
            {
                if (m.allZero(i))
                    continue;

                int current = -1;
                for (int j = 0; j < m.Col; ++j)
                {
                    dynamic v = m[i, j];
                    if (v == 0)
                        continue;

                    if (v == 1 && current == -1)
                    {
                        current = j;
                        result[current] = 0;
                        continue;
                    }

                    result[current] += v * result[j];
                }

                if (current >= 0)
                    result[current] = -result[current];
            }

            return result;
        }

        public static Matrix<Fraction> ToMatrix(this Equation equ)
        {
            Dictionary<int, Dictionary<int, int>> equParams = new Dictionary<int, Dictionary<int, int>>();
            Dictionary<string, int> paramTable = new Dictionary<string, int>();
            int index = 0;
            foreach (var m in equ.Left)
            {
                foreach (var a in m.Formula)
                {
                    if (paramTable.ContainsKey(a.Name))
                        continue;

                    paramTable[a.Name] = index++;
                    equParams[paramTable[a.Name]] = new Dictionary<int, int>();
                }
            }

            int i = 0;
            while (i < equ.Left.Count)
            {
                foreach (var a in equ.Left[i].Formula)
                {
                    equParams[paramTable[a.Name]][i] = a.Key;
                }
                ++i;
            }

            while (i < equ.Left.Count + equ.Right.Count)
            {
                foreach (var a in equ.Right[i - equ.Left.Count].Formula)
                {
                    equParams[paramTable[a.Name]][i] = -a.Key;
                }
                ++i;
            }

            Matrix<Fraction> matrix = new Matrix<Fraction>(index, i);

            for (int row = 0; row < index; row++)
            {
                for (int col = 0; col < i; col++)
                {
                    if (equParams[row].ContainsKey(col))
                    {
                        matrix[row, col] = equParams[row][col];
                    }
                    else
                    {
                        matrix[row, col] = 0;
                    }
                }
            }

            return matrix;
        }
    }
}
