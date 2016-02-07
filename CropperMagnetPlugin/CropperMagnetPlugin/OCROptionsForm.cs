using Fusion8.Cropper.Extensibility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace CropperMagnetPlugin
{
    public partial class OCROptionsForm : BaseConfigurationForm
    {
        public OCROptionsForm()
        {
            InitializeComponent();
        }

        
        public OCRSettings Settings { get; set; }


        public void InitializeSettings(OCRSettings settings)
        {
            Settings = settings;

            switch (Settings.Type)
            {
                case OCRType.Tesseract:
                    radioButton1.Checked = true;
                    break;
                case OCRType.MODI:
                    radioButton2.Checked = true;
                    break;
                case OCRType.OneNote:
                    radioButton3.Checked = true;
                    break;
            }
            radioButton1.Enabled = Settings.IsEnable(OCRType.Tesseract);
            radioButton2.Enabled = Settings.IsEnable(OCRType.MODI);
            radioButton3.Enabled = Settings.IsEnable(OCRType.OneNote);
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Type = OCRType.Tesseract;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Type = OCRType.MODI;
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Type = OCRType.OneNote;
        }
    }
}
