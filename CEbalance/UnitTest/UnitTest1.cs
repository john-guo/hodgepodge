using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CEbalance.Symbol;
using CEbalance.Math;
using CEbalance;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UnitTest
{

    [TestClass]
    public class CompareTest
    {
        [TestMethod]
        public void Test()
        {
            int[,] m = new int[6, 4]
            {
                { 13, 33, 22, 4 },
                { 3 , 0, 0 , 2  },
                { 0, 9, 6, 7 },
                { 0, 0, 0, 3 },
                { 22, 33, 0, 0 },
                { 1, 0, 0, 0 }
            };

            Matrix<int> mm = new Matrix<int>(m);

            mm.Sort(false);

            mm.Print();
        }
    }

    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void EquationTest()
        {
            Equation equ;
            string estr;

            estr = "O2 + H2 -> H2O";
            equ = new Equation(estr);
            Assert.AreEqual(estr, equ.ToString());

            estr = "O + H -> HO";
            equ = new Equation(estr);
            Assert.AreEqual(estr, equ.ToString());

            estr = "SO4 + NaON2CO3H34O + Cl + H -> ClNaHO + Cl3";
            equ = new Equation(estr);
            Assert.AreEqual(estr, equ.ToString());

            estr = "H2 + Ca(CN)2 + NaAlF4 + FeSO4 + MgSiO3 + KI + H3PO4 + PbBrO4 + BrCl + CF2Cl2 + SO2 -> PbBr2 + CrCl3 + MgCO3 + KAl(OH)4 + Fe(SCN)3 + PI3 + Na2SiO3 + CaF + H2O";
            equ = new Equation(estr);
            Assert.AreEqual(estr, equ.ToString());
        }

        [TestMethod]
        public void MatrixTest()
        {
            int row = 3;
            int col = 4;

            Matrix<int> m = new Matrix<int>(row, col);

            Assert.AreEqual(row, m.Row);
            Assert.AreEqual(col, m.Col);

            m.SetData(new int[]
            {
                0, 1, 2, 3,
                4, 5, 6, 7,
                8, 9, 10, 11
            });

            Assert.AreEqual(0, m[0, 0]);
            Assert.AreEqual(1, m[0, 1]);
            Assert.AreEqual(2, m[0, 2]);
            Assert.AreEqual(3, m[0, 3]);
            Assert.AreEqual(8, m[2, 0]);
            Assert.AreEqual(7, m[1, 3]);

            m = new Matrix<int>(col, row);

            m.SetData(new int[]
            {
                0, 1, 2, 3,
                4, 5, 6, 7,
                8, 9, 10, 11
            });

            Assert.AreEqual(0, m[0, 0]);
            Assert.AreEqual(1, m[0, 1]);
            Assert.AreEqual(2, m[0, 2]);
            Assert.AreEqual(3, m[1, 0]);
            Assert.AreEqual(8, m[2, 2]);
            Assert.AreEqual(11, m[3, 2]);


        }

        [TestMethod]
        public void FractionTest()
        {
            Fraction f1 = new Fraction(2, 4);

            int i = (int)(f1 * 2);

            Fraction f2 = new Fraction(1, 1);

            Assert.AreEqual(1, i);

            Assert.AreEqual(f2, f1 * 2);

            f1 = new Fraction(1, 2);
            Assert.AreEqual(1, (int)(f1 + f1));
        }

        [TestMethod]
        public void FractionMatrixTest()
        {
            int row = 3;
            int col = 4;

            Matrix<Fraction> m = new Matrix<Fraction>(row, col);

            m.SetData(new Fraction[]
            {
                0, 1, 2, 3,
                4, 5, 6, 7,
                8, 9, 10, 11
            });

            m.GaussElimination();
            Assert.AreEqual(1, (int)m[0, 0]);
            Assert.AreEqual(0, (int)m[1, 0]);
            Assert.AreEqual(0, (int)m[2, 0]);

            m = new Matrix<Fraction>(col, row);

            m.SetData(new Fraction[]
            {
                0, 1, 2, 3,
                4, 5, 6, 7,
                8, 9, 10, 11
            });

            m.GaussElimination();
            Assert.AreEqual(1, (int)m[0, 0]);
            Assert.AreEqual(0, (int)m[1, 0]);
            Assert.AreEqual(0, (int)m[2, 0]);
        }

        [TestMethod]
        public void BalanceTest()
        {
            Equation equ;
            string estr;

            estr = "O2 + H2 -> H2O";
            equ = new Equation(estr);

            var matrix = equ.ToMatrix();

            Assert.AreEqual(2, matrix.Row);
            Assert.AreEqual(3, matrix.Col);

            var result = EquationBalance.Solve(matrix);

            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(1, (int)result[0]);
            Assert.AreEqual(2, (int)result[1]);
            Assert.AreEqual(2, (int)result[2]);


            estr = "C2H2 + O2 -> CO2 + H2O";
            equ = new Equation(estr);
            matrix = equ.ToMatrix();
            result = EquationBalance.Solve(matrix);
            Assert.AreEqual(2, (int)result[0]);
            Assert.AreEqual(5, (int)result[1]);
            Assert.AreEqual(4, (int)result[2]);
            Assert.AreEqual(2, (int)result[3]);
        }

        [TestMethod]
        public void ComplexTest()
        {
            var equ = new Equation("S+HNO3=H2SO4+NO2+H2O");
            equ.Balance();

            equ = new Equation("Cu+HNO3=Cu(NO3)2+NO2+H2O");
            equ.Balance();
        }
    }

    [TestClass]
    public class Calculate24Test
    {
        [TestMethod]
        public void Test2()
        {
            var cal = new Calculate24(3, 3, 3, 3);
            var r = cal.Run2(false);
            Assert.IsTrue(cal.Passed);

            cal = new Calculate24(6, 6, 6, 6);
            r = cal.Run2(false);
            Assert.IsTrue(cal.Passed);

            cal = new Calculate24(5, 5, 5, 1);
            r = cal.Run2(false);
            Assert.IsTrue(cal.Passed);
        }

        [TestMethod]
        public void Test()
        {
            var cal = new Calculate24(3, 3, 3, 3);
            var r = cal.Run(false);
            Assert.IsTrue(cal.Passed);

            cal = new Calculate24(6, 6, 6, 6);
            r = cal.Run(false);
            Assert.IsTrue(cal.Passed);

            cal = new Calculate24(5, 5, 5, 1);
            r = cal.Run(false);
            Assert.IsTrue(cal.Passed);
        }
    }
}
