using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AeroWatch
{
    public class PictureClock : GraphicsClock
    {
        class PictureShard
        {
            public int L { get; set; }
            public int T { get; set; }
            public int R { get; set; }
            public int B { get; set; }
            public int X { get; set; }
            public int Y { get; set; }

            public PictureShard() { }

            public PictureShard(int l, int t, int r, int b, int x, int y)
            {
                L = l;
                T = t;
                R = r;
                B = b;
                X = x;
                Y = y;
            }

            public int Width
            {
                get
                {
                    return R - L + 1;
                }
            }

            public int Height
            {
                get
                {
                    return B - T + 1;
                }
            }

            public int XtoL
            {
                get
                {
                    return X - L;
                }
            }

            public int YtoT
            {
                get
                {
                    return Y - T;
                }
            }

            public int RtoX
            {
                get
                {
                    return R - X;
                }
            }

            public int BtoY
            {
                get
                {
                    return B - Y;
                }
            }
        }

        Image img;
        Bitmap clock;
        Bitmap sp;
        Bitmap mp;
        Bitmap hp;
        Bitmap result;
        PictureShard ps_clock;
        PictureShard ps_sp;
        PictureShard ps_mp;
        PictureShard ps_hp;
        Graphics innerCanvas;
        Size targetSize;
        int centerX;
        int centerY;

        public override void Initialize(string fileName)
        {
            targetSize = new Size(50, 50);
            img = Image.FromFile(fileName);
            
            ps_clock = new PictureShard(194, 32, 955, 771, 579, 410);
            ps_sp = new PictureShard(1, 1, 31, 321, 16, 239);
            ps_mp = new PictureShard(51, 2, 77, 220, 64, 207);
            ps_hp = new PictureShard(101, 2, 132, 163, 117, 147);

            int width = ps_clock.Width;
            int height = ps_clock.Height;
            centerX = ps_clock.XtoL;
            centerY = ps_clock.YtoT;

            clock = new Bitmap(width, height);
            using (var g = Graphics.FromImage(clock))
            {
                g.InterpolationMode = InterpolationMode.High;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.DrawImage(img, Rectangle.FromLTRB(0, 0, width - 1, height - 1), ps_clock.L, ps_clock.T, width, height, GraphicsUnit.Pixel);
            }

            sp = new Bitmap(width, height);
            using (var g = Graphics.FromImage(sp))
            {
                g.InterpolationMode = InterpolationMode.High;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.DrawImage(img, Rectangle.FromLTRB(centerX - ps_sp.XtoL, centerY - ps_sp.YtoT, centerX + ps_sp.RtoX, centerY + ps_sp.BtoY), ps_sp.L, ps_sp.T, ps_sp.Width, ps_sp.Height, GraphicsUnit.Pixel);
            }

            mp = new Bitmap(width, height);
            using (var g = Graphics.FromImage(mp))
            {
                g.InterpolationMode = InterpolationMode.High;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.DrawImage(img, Rectangle.FromLTRB(centerX - ps_mp.XtoL, centerY - ps_mp.YtoT, centerX + ps_mp.RtoX, centerY + ps_mp.BtoY), ps_mp.L, ps_mp.T, ps_mp.Width, ps_mp.Height, GraphicsUnit.Pixel);
            }

            hp = new Bitmap(width, height);
            using (var g = Graphics.FromImage(hp))
            {
                g.InterpolationMode = InterpolationMode.High;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.DrawImage(img, Rectangle.FromLTRB(centerX - ps_hp.XtoL, centerY - ps_hp.YtoT, centerX + ps_hp.RtoX, centerY + ps_hp.BtoY), ps_hp.L, ps_hp.T, ps_hp.Width, ps_hp.Height, GraphicsUnit.Pixel);
            }

            result = new Bitmap(width, height);
            innerCanvas = Graphics.FromImage(result);
        }

        public override void Draw(Graphics canvas)
        {
            var time = DateTime.Now;

            innerCanvas.ResetTransform();
            innerCanvas.DrawImage(clock, 0, 0);

            innerCanvas.TranslateTransform(centerX, centerY);
            innerCanvas.RotateTransform(360.0f / 60 * time.Second);
            innerCanvas.TranslateTransform(-centerX, -centerY);
            innerCanvas.DrawImage(sp, 0, 0);

            innerCanvas.ResetTransform();
            innerCanvas.TranslateTransform(centerX, centerY);
            innerCanvas.RotateTransform(360.0f / 60 * time.Minute);
            innerCanvas.TranslateTransform(-centerX, -centerY);
            innerCanvas.DrawImage(mp, 0, 0);

            innerCanvas.ResetTransform();
            innerCanvas.TranslateTransform(centerX, centerY);
            innerCanvas.RotateTransform(360.0f / 12 * time.Hour);
            innerCanvas.TranslateTransform(-centerX, -centerY);
            innerCanvas.DrawImage(hp, 0, 0);

            canvas.DrawImage(result, 0, 0, targetSize.Width, targetSize.Height);
        }

        protected override Size GetSize()
        {
            return targetSize;
        }

    }
}
