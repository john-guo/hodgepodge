using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AeroWatch
{
    public class FontClock : GraphicsClock
    {
        private Font paintFont;
        //private Brush paintBrush;
        private Color paintColor;
        int fontSize;

        public FontClock(Color color, int size)
        {
            fontSize = size;
            paintColor = color;
            //paintBrush = new SolidBrush(color);
        }

        public override void Initialize(string fileName)
        {
            var privateFonts = new PrivateFontCollection();
            privateFonts.AddFontFile(fileName);
            paintFont = new Font(privateFonts.Families.First(), fontSize);
        }

        public override void Draw(Graphics canvas)
        {
            var time = DateTime.Now;

            canvas.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            canvas.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            TextRenderer.DrawText(canvas, time.ToLongTimeString(), paintFont, Point.Empty, paintColor);
        }

        protected override Size GetSize()
        {
            return TextRenderer.MeasureText(DateTime.MaxValue.ToLongTimeString(), paintFont);
        }
    }
}
