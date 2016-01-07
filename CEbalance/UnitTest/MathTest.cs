using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MathNet.Numerics.LinearAlgebra;

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
    }
}
