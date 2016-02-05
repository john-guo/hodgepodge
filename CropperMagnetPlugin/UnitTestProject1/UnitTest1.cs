using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CropperMagnetPlugin;
using System.Drawing;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var text = OCRHelper.OCRMagnet("test.bmp");
            //OCRHelper.Exit();
        }
    }
}
