using System;
using System.Collections.Generic;
using System.Text;
using Fusion8.Cropper.Extensibility;
using CropperMagnetPlugin;
using System.Windows.Forms;

namespace CropperPlugins
{
    public class CropperMagnet : DesignablePlugin
    {
        public override string Description
        {
            get
            {
                return "CropperMagnet";
            }
        }

        public override string Extension
        {
            get
            {
                return "png";
            }
        }

        protected override void ImageCaptured(object sender, ImageCapturedEventArgs e)
        {
            var text = OCRHelper.OCRMagnet(e.FullSizeImage);
            Clipboard.SetText(text);
        }
    }
}
