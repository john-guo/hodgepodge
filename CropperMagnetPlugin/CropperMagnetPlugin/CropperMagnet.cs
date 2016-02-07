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
        private OCRSettings pluginSettings = new OCRSettings();
        private OCROptionsForm configurationForm;

        public BaseConfigurationForm ConfigurationForm
        {
            get
            {
                if (configurationForm == null)
                {
                    configurationForm = new OCROptionsForm();
                    configurationForm.InitializeSettings(pluginSettings);
                    configurationForm.OptionsSaved += ConfigurationForm_OptionsSaved;
                }

                return configurationForm;
            }
        }

        private void ConfigurationForm_OptionsSaved(object sender, EventArgs e)
        {
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
                pluginSettings = (OCRSettings)value;
            }
        }

        protected override void ImageCaptured(object sender, ImageCapturedEventArgs e)
        {
            var text = OCRHelper.OCRMagnet(e.FullSizeImage);
            Clipboard.SetText(text);
        }

    }
}
