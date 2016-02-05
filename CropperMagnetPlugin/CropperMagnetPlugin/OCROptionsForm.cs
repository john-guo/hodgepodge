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

            switch (Type)
            {
                case OCRType.Tesseract:
                    radioButton1.Checked = true;
                    break;
                case OCRType.MODI:
                    radioButton2.Checked = true;
                    break;
            }
        }

        public OCRType Type { get; set; }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            Type = OCRType.Tesseract;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            Type = OCRType.MODI;
        }
    }
}
