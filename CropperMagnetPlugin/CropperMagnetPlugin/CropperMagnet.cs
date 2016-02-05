using System;
using System.Collections.Generic;
using System.Text;
using Fusion8.Cropper.Extensibility;
using CropperMagnetPlugin;
using System.Windows.Forms;

namespace CropperMagnetPlugin
{
    public class CropperMagnet : DesignablePlugin, IConfigurablePlugin
    {
        private MagnetSettings pluginSettings = new MagnetSettings();
        private OCROptionsForm configurationForm;

        public BaseConfigurationForm ConfigurationForm
        {
            get
            {
                if (configurationForm == null)
                {
                    configurationForm = new OCROptionsForm();
                    configurationForm.OptionsSaved += ConfigurationForm_OptionsSaved;
                    configurationForm.Type = pluginSettings.Type;
                }

                return configurationForm;
            }
        }

        private void ConfigurationForm_OptionsSaved(object sender, EventArgs e)
        {
            pluginSettings.Type = configurationForm.Type;
            OCRHelper.SetOCR(pluginSettings.GetOCR());
        }

        public override string Description
        {
            get
            {
                return "Magnet";
            }
        }

        public override string Extension
        {
            get
            {
                return "png";
            }
        }

        public bool HostInOptions
        {
            get
            {
                return true;
            }
        }

        public object Settings
        {
            get
            {
                return pluginSettings;
            }

            set
            {
                pluginSettings = (MagnetSettings)value;
            }
        }

        protected override void ImageCaptured(object sender, ImageCapturedEventArgs e)
        {
            var text = OCRHelper.OCRMagnet(e.FullSizeImage);
            Clipboard.SetText(text);
        }

    }

    public enum OCRType { Tesseract, MODI };

    public class MagnetSettings
    {
        static Dictionary<OCRType, IOCR> _cache = new Dictionary<OCRType, IOCR>()
        {
            {OCRType.Tesseract, new TesseractOCR() },
            {OCRType.MODI, new MODIOCR() }
        };

        public OCRType Type { get; set; }

        public IOCR GetOCR()
        {
            IOCR ocr = null;
            _cache.TryGetValue(Type, out ocr);
            return ocr;
        }
    }
}
