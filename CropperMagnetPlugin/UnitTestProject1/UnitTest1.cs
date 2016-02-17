using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CropperMagnetPlugin;
using System.Drawing;
using Microsoft.Office.Interop.OneNote;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var o = new OneNoteOCR();
            OCRHelper.SetOCR(o);
            var image = Image.FromFile("test.jpg");
            var text = OCRHelper.OCRMagnet("test.jpg");
            //OCRHelper.Exit();
        }

        [TestMethod]
        public void OneNoteTest()
        {
        }
    }
}
