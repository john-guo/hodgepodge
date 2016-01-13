using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MathNet.Numerics.LinearAlgebra;
using System.Linq.Expressions;

namespace UnitTest
{
    [TestClass]
    public class MathTest
    {
        [TestMethod]
        public void MatrixTest()
        {
            var m = Matrix<double>.Build.Random(500, 500);
            var v = Vector<double>.Build.Random(500);
            var y = m.Solve(v);

            /*
            var m = 
            Matrix<double>.Build.DenseOfArray(new double[,]
            {
                { 2, 0, -1, 0 },
                { 2, 1, 0, -2 },
                { 0, 2, -2, -1 }
            });

            var v = Matrix<double>.Build.DenseOfArray(new double[,]
                {
                    { 0, 0, 0, 0 },
                    { 0, 0, 0, 0 },
                    { 0, 0, 0, 0 }
                });

            var r = 
            m.Solve(v);
            */
            
        }

        [TestMethod]
        public void ExpressionTest()
        {
            var s = "3+(1*3)+2";
            var e1 = Expression.Multiply(Expression.Constant(1), Expression.Constant(3));
            var e2 = Expression.Add(Expression.Constant(3), e1);
            var e3 = Expression.Add(e2, Expression.Constant(2));
            var r = Expression.Lambda<Func<int>>(e3).Compile()();
            Assert.AreEqual(8, r);
        }
    }
}
