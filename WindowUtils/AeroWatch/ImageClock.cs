using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AeroWatch
{
    public class ImageClock : GraphicsClock
    {
        Image image;

        Graphics innerCanvas;

        public override void Initialize(string fileName)
        {
            image = new Bitmap(101, 101);
            innerCanvas = Graphics.FromImage(image);
            innerCanvas.CompositingMode = CompositingMode.SourceCopy;
            //innerCanvas.SmoothingMode = SmoothingMode.AntiAlias;
            //innerCanvas.InterpolationMode = InterpolationMode.HighQualityBicubic;
            //innerCanvas.PixelOffsetMode = PixelOffsetMode.HighQuality;
        }

        public override void Draw(Graphics canvas)
        {
            var time = DateTime.Now;

            innerCanvas.Clear(Color.Transparent);
            //innerCanvas.SmoothingMode = SmoothingMode.AntiAlias;
            //innerCanvas.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            int x1, y1, x2, y2;

            int center = image.Width / 2;

            int shortMark = (int)(38.0 / 50 * center);
            int longMark = (int)(36.0 / 50 * center);
            int totalRange = (int)(40.0 / 50 * center);
            int secondRange = (int)(30.0 / 50 * center);
            int minuteRange = (int)(20.0 / 50 * center);
            int hourRange = (int)(10.0 / 50 * center);

            for (int i = 0; i < 60; ++i)
            {
                Pen p = Pens.LightSkyBlue;

                int be = shortMark;

                if (i % 5 == 0)
                {
                    p = Pens.DarkGoldenrod;
                    be = longMark;
                }

                x1 = center + (int)(Math.Cos(i * Math.PI * 2 / 60) * be);
                y1 = center + (int)(Math.Sin(i * Math.PI * 2 / 60) * be);
                x2 = center + (int)(Math.Cos(i * Math.PI * 2 / 60) * totalRange);
                y2 = center + (int)(Math.Sin(i * Math.PI * 2 / 60) * totalRange);


                innerCanvas.DrawLine(p, x1, y1, x2, y2);
            }

            //x1 = 50 + (int)(Math.Cos(-Math.PI / 2 + time.Hour * Math.PI * 2 / 12 + (time.Minute * Math.PI * 2 / 60) / 60 * 5) * 0);
            //y1 = 50 + (int)(Math.Sin(-Math.PI / 2 + time.Hour * Math.PI * 2 / 12 + (time.Minute * Math.PI * 2 / 60) / 60 * 5) * 0);
            //x2 = 50 + (int)(Math.Cos(-Math.PI / 2 + time.Hour * Math.PI * 2 / 12 + (time.Minute * Math.PI * 2 / 60) / 60 * 5) * 10);
            //y2 = 50 + (int)(Math.Sin(-Math.PI / 2 + time.Hour * Math.PI * 2 / 12 + (time.Minute * Math.PI * 2 / 60) / 60 * 5) * 10);

            x1 = center + (int)(Math.Cos(-Math.PI / 2 + time.Second * Math.PI * 2 / 60) * 0);
            y1 = center + (int)(Math.Sin(-Math.PI / 2 + time.Second * Math.PI * 2 / 60) * 0);
            x2 = center + (int)(Math.Cos(-Math.PI / 2 + time.Second * Math.PI * 2 / 60) * secondRange);
            y2 = center + (int)(Math.Sin(-Math.PI / 2 + time.Second * Math.PI * 2 / 60) * secondRange);

            innerCanvas.DrawLine(Pens.HotPink, x1, y1, x2, y2);

            x1 = center + (int)(Math.Cos(-Math.PI / 2 + time.Minute * Math.PI * 2 / 60) * 0);
            y1 = center + (int)(Math.Sin(-Math.PI / 2 + time.Minute * Math.PI * 2 / 60) * 0);
            x2 = center + (int)(Math.Cos(-Math.PI / 2 + time.Minute * Math.PI * 2 / 60) * minuteRange);
            y2 = center + (int)(Math.Sin(-Math.PI / 2 + time.Minute * Math.PI * 2 / 60) * minuteRange);

            innerCanvas.DrawLine(Pens.Gold, x1, y1, x2, y2);

            x1 = center + (int)(Math.Cos(-Math.PI / 2 + time.Hour * Math.PI * 2 / 12) * 0);
            y1 = center + (int)(Math.Sin(-Math.PI / 2 + time.Hour * Math.PI * 2 / 12) * 0);
            x2 = center + (int)(Math.Cos(-Math.PI / 2 + time.Hour * Math.PI * 2 / 12) * hourRange);
            y2 = center + (int)(Math.Sin(-Math.PI / 2 + time.Hour * Math.PI * 2 / 12) * hourRange);

            innerCanvas.DrawLine(Pens.DarkGray, x1, y1, x2, y2);

            innerCanvas.Flush();

            //Canvas.SmoothingMode = SmoothingMode.AntiAlias;
            //Canvas.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            //Canvas.CompositingMode = CompositingMode.SourceOver;
            //Canvas.CompositingQuality = CompositingQuality.HighQuality;
            //Canvas.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            //canvas.SmoothingMode = SmoothingMode.AntiAlias;
            //canvas.InterpolationMode = InterpolationMode.High;
            //canvas.PixelOffsetMode = PixelOffsetMode.HighQuality;

            canvas.CompositingMode = CompositingMode.SourceCopy;
            canvas.DrawImage(image, new Rectangle(Point.Empty, image.Size), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);
        }

        protected override Size GetSize()
        {
            return image.Size;
        }

    }
}
