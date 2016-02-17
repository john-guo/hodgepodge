using System;
using System.Collections.Generic;
using System.Text;

namespace CropperMagnetPlugin
{
    public enum OCRType { Tesseract, MODI, OneNote };

    public class OCRSettings
    {
        static Dictionary<OCRType, IOCR> _cache = new Dictionary<OCRType, IOCR>()
        {
            { OCRType.Tesseract, new TesseractOCR() },
            { OCRType.MODI, new MODIOCR() },
            { OCRType.OneNote, new OneNoteOCR() }
        };

        public OCRType Type { get; set; }

        public bool IsEnable(OCRType type)
        {
            return GetOCR(type).IsEnable;
        }

        private IOCR GetOCR(OCRType type)
        {
            IOCR ocr = null;
            _cache.TryGetValue(type, out ocr);
            return ocr;
        }

        public IOCR GetOCR()
        {
            return GetOCR(Type);
        }
    }
}
