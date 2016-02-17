using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;
using System.Xml;
using OneNote = Microsoft.Office.Interop.OneNote;
using System.Drawing.Imaging;

namespace CropperMagnetPlugin
{
    public interface IOCR
    {
        string OCR(string filename);
        bool IsEnable { get; }
    }

    public abstract class OCRBase : IOCR
    {
        public abstract string OCR(string filename);
        public abstract bool Test();

        protected int _enableState = 0;
        public virtual bool IsEnable
        {
            get
            {
                if (_enableState > 0)
                {
                    return _enableState == 1 ? true : false;
                }

                if (Test())
                    _enableState = 1;
                else 
                   _enableState = 2;

                return _enableState == 1 ? true : false;
            }
        }
    }

    public class MODIOCR : OCRBase
    {
        public override string OCR(string filename)
        {
            MODI.Document doc = new MODI.Document();
            doc.Create(filename);
            MODI.Image img = (MODI.Image)doc.Images[0];
            img.OCR(MODI.MiLANGUAGES.miLANG_ENGLISH, true, true);
            var text = img.Layout.Text;
            doc.Close();
            return text;
        }

        public override bool Test()
        {
            try
            {
                new MODI.Document();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    public class TesseractOCR : OCRBase
    {
        Tesseract.TesseractEngine engine;
        public TesseractOCR()
        {
            engine = new Tesseract.TesseractEngine(@".\tessdata", "eng");
        }

        ~TesseractOCR()
        {
            if (!engine.IsDisposed)
                engine.Dispose();
        }

        public override string OCR(string filename)
        {
            using (var img = Tesseract.Pix.LoadFromFile(filename))
            using (var page = engine.Process(img))
            {
                return page.GetText();
            }
        }

        public override bool Test()
        {
            return true;
        }

    }

    public class OneNoteOCR : OCRBase
    {
        private const int PollInterval = 250;
        private const int PollAttempts = 2;

        private readonly OnenotePage _page;

        public OneNoteOCR()
        {
            try
            {
                _page = new OnenotePage(new OneNote.Application());
                _enableState = 1;
            }
            catch
            {
                _enableState = 2;
            }
        }

        ~OneNoteOCR()
        {
            if (_page != null)
                _page.Delete();
        }

        public override string OCR(string filename)
        {
            var image = Image.FromFile(filename);
            this._page.Reload();

            this._page.Clear();
            this._page.AddImage(image);
            image.Dispose();

            this._page.Save();

            int total = 0;
            do
            {
                System.Threading.Thread.Sleep(PollInterval);

                this._page.Reload();

                string result = this._page.ReadOcrText();
                if (result != null)
                {
                    _page.Delete();
                    return result;
                }
            } while (total++ < PollAttempts);

            _page.Delete();
            return string.Empty;
        }

        public override bool Test()
        {
            return false;
        }


        internal sealed class OnenotePage
        {
            private const String OneNoteNamespace = "http://schemas.microsoft.com/office/onenote/2013/onenote";
            private const String DefaultOutline = "<one:Outline xmlns:one=\"" + OneNoteNamespace + "\"><one:OEChildren><one:OE><one:T><![CDATA[A]]></one:T></one:OE></one:OEChildren></one:Outline>";

            private readonly OneNote.Application _app;
            private XmlDocument _document;
            private string _pageId;

            private XmlElement Oe
            {
                get
                {
                    return (XmlElement)this._document.GetElementsByTagName("OE", OneNoteNamespace)[0];
                    /*
                    var outline = this._document.GetElementsByTagName("Outline", OneNoteNamespace)[0];
                    if (outline == null)
                    {
                        this._document.InnerXml = DefaultOutline;
                        outline = _document.FirstChild;
                    }

                    var children = outline.OwnerDocument.GetElementsByTagName("OEChildren", OneNoteNamespace)[0];
                    return (XmlElement)children.OwnerDocument.GetElementsByTagName("OE", OneNoteNamespace)[0];
                    */
                }
            }

            public OnenotePage(OneNote.Application app)
            {
                this._app = app;

                this.LoadOrCreatePage();
            }

            public void Reload()
            {
                string strXml;
                this._app.GetPageContent(this._pageId, out strXml, OneNote.PageInfo.piBinaryData);

                this._document = new XmlDocument();
                this._document.InnerXml = strXml;
            }

            public void Clear()
            {
                this.Oe.RemoveAll();
            }

            public void Delete()
            {
                if (string.IsNullOrEmpty(this._pageId))
                    return;

                this._app.DeleteHierarchy(this._pageId);
                _pageId = null;
            }

            public void AddImage(Image image)
            {
                var img = this.CreateImageTag(image);
                this.Oe.AppendChild(img);
            }

            public string ReadOcrText()
            {
                var img = this.Oe.GetElementsByTagName("Image", OneNoteNamespace)[0];
                var ocrData = img.OwnerDocument.GetElementsByTagName("OCRData", OneNoteNamespace)[0];

                if (ocrData == null)
                    return null;

                var ocrText = ocrData.OwnerDocument.GetElementsByTagName("OCRText", OneNoteNamespace)[0].InnerText;
                return ocrText;
            }


            public void Save()
            {
                var xml = this._document.InnerXml;
                this._app.UpdatePageContent(xml);

                this._app.NavigateTo(this._pageId);
            }

            private XmlElement CreateImageTag(Image image)
            {
                var img = _document.CreateElement("Image", OneNoteNamespace);

                var data = _document.CreateElement("Data", OneNoteNamespace);
                data.InnerText = this.ToBase64(image);
                img.AppendChild(data);

                return img;
            }

            private string ToBase64(Image image)
            {
                using (var memoryStream = new MemoryStream())
                {
                    image.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);

                    var binary = memoryStream.ToArray();
                    return Convert.ToBase64String(binary);
                }
            }

            private void LoadOrCreatePage()
            {
                string hierarchy;
                this._app.GetHierarchy(String.Empty, OneNote.HierarchyScope.hsPages, out hierarchy);

                var doc = new XmlDocument();
                doc.InnerXml = hierarchy;
                var section = doc.GetElementsByTagName("Section", OneNoteNamespace)[0];
                if (section == null)
                    throw new Exception("No section found");

                var sectionId = section.Attributes["ID"].Value;
                this._app.CreateNewPage(sectionId, out this._pageId);

                this.Reload();
            }
        }
    }

    public static class OCRHelper
    {
        private static IOCR ocr;

        static OCRHelper()
        {
            ocr = new TesseractOCR();
        }

        public static void SetOCR(IOCR iocr)
        {
            ocr = iocr;
        }

        public static string OCR(string filename)
        {
            return ocr.OCR(filename);
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
