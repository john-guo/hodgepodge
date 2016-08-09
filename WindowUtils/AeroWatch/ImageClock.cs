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
        Rectangle secondPointer = new Rectangle(0, 0, 45, 402);
        Rectangle minutePointer = new Rectangle(50, 0, 35, 277);
        Rectangle hourPointer = new Rectangle(100, 0, 60, 205);
        Rectangle clock = new Rectangle(200, 40, 944, 912);
        Image clockImg, secondImg, minuteImg, hourImg;

        Graphics innerCanvas;

        public override void Initialize(string fileName)
        {
            clockImg = new Bitmap(101, 101);
            //image = new Bitmap(100 * 32 + 1, 100 * 32 + 1);
            image = new Bitmap(101, 101);
            innerCanvas = Graphics.FromImage(image);

            //image = Image.FromFile(fileName);

            //clockImg = new Bitmap(clock.Width, clock.Height);
            //secondImg = new Bitmap(secondPointer.Width, secondPointer.Height);
            //minuteImg = new Bitmap(minutePointer.Width, minutePointer.Height);
            //hourImg = new Bitmap(hourPointer.Width, hourPointer.Height);

            //using (var g = Graphics.FromImage(clockImg))
            //{
            //    g.PageUnit = GraphicsUnit.Pixel;
            //    g.DrawImage(image, 0, 0, clock, GraphicsUnit.Pixel);
            //    g.Flush();
            //}
            //using (var g = Graphics.FromImage(secondImg))
            //{
            //    g.PageUnit = GraphicsUnit.Pixel;
            //    g.DrawImage(image, 0, 0, secondPointer, GraphicsUnit.Pixel);
            //    g.Flush();
            //}

            //secondImg.RotateFlip(RotateFlipType.Rotate180FlipNone);
            ////secondImg.Save("test.png");

            //using (var g = Graphics.FromImage(minuteImg))
            //{
            //    g.PageUnit = GraphicsUnit.Pixel;
            //    g.DrawImage(image, 0, 0, minutePointer, GraphicsUnit.Pixel);
            //    g.Flush();
            //}
            //using (var g = Graphics.FromImage(hourImg))
            //{
            //    g.PageUnit = GraphicsUnit.Pixel;
            //    g.DrawImage(image, 0, 0, hourPointer, GraphicsUnit.Pixel);
            //    g.Flush();
            //}
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
            canvas.DrawImage(image, new Rectangle(Point.Empty, clockImg.Size), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);

            ////Canvas.DrawImage(clockImg, Point.Empty);
            //Canvas.TranslateTransform(clock.Width / 2, clock.Height / 2);
            //Canvas.RotateTransform(time.Second * 6);
            ////Canvas.DrawImage(hourImg, 0, 0);
            ////Canvas.DrawImage(minuteImg, 0, 0);
            //Canvas.DrawImage(secondImg, -28, -103);
            //Canvas.RotateTransform(-time.Second * 6);
            //Canvas.TranslateTransform(-clock.Width / 2, -clock.Height / 2);
            ////Canvas.DrawImageUnscaledAndClipped(image, new Rectangle(Point.Empty, image.Size));
        }

        protected override Size GetSize()
        {
            return clockImg.Size;
        }

    }
}
