using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OtpSharp;
using Base32;
using ZXing;
using ZXing.Rendering;

namespace winForm
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        Totp otp;
        byte[] key;
        BarcodeReader reader = new BarcodeReader();
        BarcodeWriter writer = new BarcodeWriter();
        BitmapRenderer render = new BitmapRenderer();

        private void button1_Click(object sender, EventArgs e)
        {
            key = KeyGeneration.GenerateRandomKey(40);

            textBox1.Text = Base32Encoder.Encode(key);

            otp = new Totp(key);
            label1.Text = otp.ComputeTotp();

            var url = KeyUrl.GetTotpUrl(key, "VERYVERYVERYVERYLONG");
            label2.Text = url;

            writer.Format = BarcodeFormat.QR_CODE;
            writer.Options.PureBarcode = false;
            writer.Options.Margin = 0;
            writer.Options.Width = 175;
            writer.Options.Height = 175;
            var matrix = writer.Encode(url);
            pictureBox1.Image = render.Render(matrix, BarcodeFormat.QR_CODE, string.Empty);
            pictureBox1.Invalidate();

            timer1.Enabled = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            key = Base32Encoder.Decode(textBox2.Text);
            otp = new Totp(key);
            label3.Text = otp.ComputeTotp();

            timer1.Enabled = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            label3.Text = otp.ComputeTotp();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var d = openFileDialog1.ShowDialog();
            if (d != DialogResult.OK)
                return;

            pictureBox1.ImageLocation = openFileDialog1.FileName;
        }

        private void pictureBox1_LoadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            var result = reader.Decode(pictureBox1.Image as Bitmap);
            label4.Text = result.Text;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var result = reader.Decode(pictureBox1.Image as Bitmap);
            label4.Text = result.Text;
        }
    }
}
