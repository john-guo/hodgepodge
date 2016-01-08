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
        public static Dictionary<int, Fraction> Solve(Matrix<Fraction> m)
        {
            var result = new Dictionary<int, Fraction>();

            m.GaussElimination();

            int r = System.Math.Min(m.Row, m.Col);
            int lcm, gcd;

            for (int j = r; j < m.Col; ++j)
            {
                lcm = 0;
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
                        if (result.ContainsKey(j))
                            continue;

                        current = j;
                        result[current] = 0;
                        break;
                    }
                }
                if (current < 0)
                    continue;

                for (int j = 0; j < m.Col; ++j)
                {
                    if (j == current)
                        continue;

                    dynamic v = m[i, j];
                    if (v == 0)
                        continue;

                    if (!result.ContainsKey(j))
                    {
                        lcm = 0;
                        for (int k = 0; k < m.Row; ++k)
                        {
                            var u = m[k, j];
                            if (u == 0)
                                continue;

                            if (lcm == 0)
                            {
                                lcm = u.Denominator;
                                continue;
                            }

                            lcm = Utils.LCM(lcm, u.Denominator);
                        }

                        result[j] = lcm;
                    }

                    result[current] += v * result[j];
                }

                result[current] = -result[current];
            }

            for (int j = 0; j < m.Col; j++)
            {
                if (result.ContainsKey(j))
                    continue;
                result[j] = 0;
            }

            lcm = 0;
            foreach (var pair in result)
            {
                if (lcm == 0)
                {
                    lcm = pair.Value.Denominator;
                    continue;
                }
                lcm = Utils.LCM(lcm, pair.Value.Denominator);
            }
            if (lcm > 1)
            {
                foreach (var key in result.Keys.ToArray())
                {
                    result[key] *= lcm;
                }
            }

            gcd = 0;
            foreach (var pair in result)
            {
                if (gcd == 0)
                {
                    gcd = pair.Value;
                    continue;
                }
                gcd = Utils.GCD(gcd, pair.Value);
            }
            if (gcd > 1)
            {
                foreach (var key in result.Keys.ToArray())
                {
                    result[key] /= gcd;
                }
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
                    foreach (var b in a.Ions)
                    {
                        if (paramTable.ContainsKey(b.Name))
                            continue;

                        paramTable[b.Name] = index++;
                        equParams[paramTable[b.Name]] = new Dictionary<int, int>();
                    }
                }
            }

            int i = 0;
            while (i < equ.Left.Count)
            {
                foreach (var a in equ.Left[i].Formula)
                {
                    foreach (var b in a.Ions)
                    {
                        equParams[paramTable[b.Name]][i] = a.Key * b.Key;
                    }
                }
                ++i;
            }

            while (i < equ.Left.Count + equ.Right.Count)
            {
                foreach (var a in equ.Right[i - equ.Left.Count].Formula)
                {
                    foreach (var b in a.Ions)
                    {
                        equParams[paramTable[b.Name]][i] = -(a.Key * b.Key);
                    }
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

        public static void Trim(this Equation equ)
        {
            var matrix = equ.ToMatrix();
            var result = Solve(matrix);
            int index = 0;
            foreach (var m in equ.Left)
            {
                m.Factor = result[index++];
                if (m.Factor == 0)
                    throw new Exception();
            }
            foreach (var m in equ.Right)
            {
                m.Factor = result[index++];
                if (m.Factor == 0)
                    throw new Exception();
            }
        }
    }
}
