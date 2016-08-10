using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AeroWatch
{
    //(see: http://en.wikipedia.org/wiki/Hypotrochoid)
    public class BreathingGalaxies : ToolWinForm
    {
        double ox, oy, prevX, prevY;
        double t;
        bool allowResize;
        GraphicsPath path;
        Random rand;
        double R, r, d, hue;
        Pen p;
        Label label;
        bool rmode = false;

        public BreathingGalaxies()
        {
            t = prevX = prevY = 0;
            CanvasTimer.Interval = 16;
            Width = 500;
            Height = 500;
            allowResize = false;
            path = new GraphicsPath();
            rand = new Random();
            ox = oy = 250;

            Location = Point.Empty;

            label = new Label();
            label.Text = "Hello\r\nWorld\r\n!!!!";
            label.ForeColor = Color.Red;
            label.BackColor = Color.Black;
            label.AutoSize = true;
            Controls.Add(label);

            R = rand.Next(256);
            r = rand.Next(256);
            d = 9;
            p = new Pen(Color.FromArgb(rand.Next(256), rand.Next(256), rand.Next(256)));

            label.Text = string.Format("R={0}\r\nr={1}\r\nd={2}", R, r, d);


            transparencyKey = Color.Transparent;
            //R = 100;
            //r = 50;
            //d = 10;
            //p.Color = Color.Blue;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
        }

        protected override int ApplyExStyle(int exStyle)
        {
            return exStyle;
        }


        protected override void OnCanvasCreated(Graphics canvas)
        {
            base.OnCanvasCreated(canvas);
            allowResize = true;
        }

        protected override void OnDraw(Graphics canvas)
        {
            canvas.SmoothingMode = SmoothingMode.AntiAlias;
            canvas.CompositingQuality = CompositingQuality.HighQuality;
            canvas.InterpolationMode = InterpolationMode.HighQualityBicubic;

            var mouse = PointToClient(MousePosition);

            if (ox != mouse.X && oy != mouse.Y)
            {
                ox = prevX = mouse.X;
                oy = prevY = mouse.Y;
            }

            R = 1.0 * mouse.X / Width;
            r = 1.0 * mouse.Y / Height;
            hue = 1.0 * mouse.X / Height;

            //for (int i = 0; i < Width; i += 10)
            //{
            //    if (i == ox)
            //        canvas.DrawLine(Pens.LightCyan, i, 0, i, Height);
            //    else 
            //        canvas.DrawLine(Pens.LightGray, i, 0, i, Height);
            //}
            //for (int i = 0; i < Height; i += 10)
            //{
            //    if (i == oy)
            //        canvas.DrawLine(Pens.LightCyan, 0, i, Width, i);
            //    else
            //        canvas.DrawLine(Pens.LightGray, 0, i, Width, i);
            //}

            double x = (R - r) * Math.Cos(t) + d * Math.Cos((R / r - 1) * t) + ox;
            double y = (R - r) * Math.Sin(t) - d * Math.Sin((R / r - 1) * t) + oy;

            if (prevX == 0 && prevY == 0)
            {
                prevX = x;
                prevY = y;
                return;
            }

            path.AddLine((float)prevX, (float)prevY, (float)x, (float)y);

            prevX = x;
            prevY = y;

            p.Color = fromHSL(hue, 1, 0.5);

            hue -= 0.5;

            canvas.DrawPath(p, path);

            label.Text = string.Format("R={0}\r\nr={1}\r\nd={2}\r\n{3}", R, r, d, mouse);

            
            //t += 2 * Math.PI;

            if (rmode)
            {
                t -= 2 * Math.PI;
                d -= 2.5;
            }
            else
            {
                d += 2.5;
                t += 2 * Math.PI;
            }
            if (d >= ox)
                rmode = true;
            if (d <= 0)
                rmode = false;
        }

        protected override void OnResize(EventArgs e)
        {
            if (!allowResize)
                return;
            allowResize = false;

            path.Reset();

            prevX = prevY = 0;
            ox = Width / 2;
            oy = Height / 2;

            R = rand.Next((int)Math.Min(ox, oy));
            r = rand.Next((int)Math.Min(ox, oy));
            d = 9;//rand.Next((int)Math.Min(ox, oy));
            //label.Text = string.Format("R={0}\r\nr={1}\r\nd={2}\r\n{3}", R, r, d, PointToClient(MousePosition));

            p = new Pen(Color.FromArgb(rand.Next(32, 256), rand.Next(32, 256), rand.Next(32, 256)), 0.2f);

            ResizeCanvas();
        }

        Color fromHSL(double h, double s, double l)
        {
            double r, g, b;

            if (s == 0.0)
            {
                r = g = b = l; // achromatic
            }
            else
            {
                Func<double, double, double, double> hue2rgb = (_p, _q, _t) =>
                {
                    if (_t < 0.0) _t += 1.0;
                    if (_t > 1.0) _t -= 1.0;
                    if (_t < 1.0 / 6.0) return _p + (_q - _p) * 6.0 * _t;
                    if (_t < 1.0 / 2.0) return _q;
                    if (_t < 2.0 / 3.0) return _p + (_q - _p) * (2.0 / 3.0 - _t) * 6.0;
                    return _p;
                };

                //if (h < 0.0)
                //    h = 0.0;
                //if (h > 1.0)
                //    h = 1.0;

                var q = l < 0.5 ? l * (1.0 + s) : l + s - l * s;
                var p = 2.0 * l - q;
                r = hue2rgb(p, q, h + 1.0 / 3.0);
                g = hue2rgb(p, q, h);
                b = hue2rgb(p, q, h - 1.0 / 3.0);
            }

            return Color.FromArgb((int)Math.Round(r * 255), (int)Math.Round(g * 255), (int)Math.Round(b * 255));
        }
    }
}
