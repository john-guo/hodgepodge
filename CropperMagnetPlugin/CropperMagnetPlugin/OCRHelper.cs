using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;

namespace CropperMagnetPlugin
{
    public static class OCRHelper
    {
        public static string OCR(string filename)
        {
            MODI.Document doc = new MODI.Document();
            doc.Create(filename);
            MODI.Image img = (MODI.Image)doc.Images[0];
            img.OCR(MODI.MiLANGUAGES.miLANG_ENGLISH, true, true);
            var text = img.Layout.Text;
            doc.Close();
            return text;
        }

        public static string OCR(Image img)
        {
            var file = Path.GetTempFileName();
            file = Path.ChangeExtension(file, "png");
            img.Save(file);
            var text = OCR(file);
            File.Delete(file);

            return text;
        }

        private static string MakeMagnet(string text)
        {
            var sb = new StringBuilder();

            foreach (var c in text)
            {
                if (char.IsWhiteSpace(c) || char.IsControl(c))
                    continue;

                sb.Append(c);
            }

            var hash = sb.ToString();

            if (hash.ToLower().StartsWith("magnet"))
                return hash;

            return string.Format("magnet:?xt=urn:btih:{0}", hash);
        }

        public static string OCRMagnet(string filename)
        {
            var text = OCR(filename);
            return MakeMagnet(text);
        }

        public static string OCRMagnet(Image img)
        {
            var text = OCR(img);
            return MakeMagnet(text);
        }
    }
}
