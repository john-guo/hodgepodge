using System;
using System.Collections.Generic;
using System.Drawing;
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

        public override void Initialize(Graphics obj, string fileName)
        {
            base.Initialize(obj, fileName);

            Canvas.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Canvas.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
            Canvas.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            Canvas.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

            clockImg = new Bitmap(101, 101);
            

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

        public override void Draw(DateTime time)
        {
            int x1, y1, x2, y2;

            for (int i = 0; i < 60; ++i)
            {
                Pen p = Pens.LightSkyBlue;

                int be = 38;

                if (i % 5 == 0)
                {
                    p = Pens.DarkGoldenrod;
                    be = 36;
                }

                x1 = 50 + (int)(Math.Cos(i * Math.PI * 2 / 60) * be);
                y1 = 50 + (int)(Math.Sin(i * Math.PI * 2 / 60) * be);
                x2 = 50 + (int)(Math.Cos(i * Math.PI * 2 / 60) * 40);
                y2 = 50 + (int)(Math.Sin(i * Math.PI * 2 / 60) * 40);


                Canvas.DrawLine(p, x1, y1, x2, y2);
            }

            //x1 = 50 + (int)(Math.Cos(-Math.PI / 2 + time.Hour * Math.PI * 2 / 12 + (time.Minute * Math.PI * 2 / 60) / 60 * 5) * 0);
            //y1 = 50 + (int)(Math.Sin(-Math.PI / 2 + time.Hour * Math.PI * 2 / 12 + (time.Minute * Math.PI * 2 / 60) / 60 * 5) * 0);
            //x2 = 50 + (int)(Math.Cos(-Math.PI / 2 + time.Hour * Math.PI * 2 / 12 + (time.Minute * Math.PI * 2 / 60) / 60 * 5) * 10);
            //y2 = 50 + (int)(Math.Sin(-Math.PI / 2 + time.Hour * Math.PI * 2 / 12 + (time.Minute * Math.PI * 2 / 60) / 60 * 5) * 10);

            x1 = 50 + (int)(Math.Cos(-Math.PI / 2 + time.Hour * Math.PI * 2 / 12) * 0);
            y1 = 50 + (int)(Math.Sin(-Math.PI / 2 + time.Hour * Math.PI * 2 / 12) * 0);
            x2 = 50 + (int)(Math.Cos(-Math.PI / 2 + time.Hour * Math.PI * 2 / 12) * 10);
            y2 = 50 + (int)(Math.Sin(-Math.PI / 2 + time.Hour * Math.PI * 2 / 12) * 10);

            Canvas.DrawLine(Pens.DarkGray, x1, y1, x2, y2);

            x1 = 50 + (int)(Math.Cos(-Math.PI / 2 + time.Minute * Math.PI * 2 / 60) * 0);
            y1 = 50 + (int)(Math.Sin(-Math.PI / 2 + time.Minute * Math.PI * 2 / 60) * 0);
            x2 = 50 + (int)(Math.Cos(-Math.PI / 2 + time.Minute * Math.PI * 2 / 60) * 20);
            y2 = 50 + (int)(Math.Sin(-Math.PI / 2 + time.Minute * Math.PI * 2 / 60) * 20);

            Canvas.DrawLine(Pens.Gold, x1, y1, x2, y2);

            x1 = 50 + (int)(Math.Cos(-Math.PI / 2 + time.Second * Math.PI * 2 / 60) * 0);
            y1 = 50 + (int)(Math.Sin(-Math.PI / 2 + time.Second * Math.PI * 2 / 60) * 0);
            x2 = 50 + (int)(Math.Cos(-Math.PI / 2 + time.Second * Math.PI * 2 / 60) * 30);
            y2 = 50 + (int)(Math.Sin(-Math.PI / 2 + time.Second * Math.PI * 2 / 60) * 30);

            Canvas.DrawLine(Pens.HotPink, x1, y1, x2, y2);

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

        public override Size GetSize()
        {
            return clockImg.Size;
        }

    }
}
