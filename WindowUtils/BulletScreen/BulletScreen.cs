using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowUtils;

namespace BulletScreen
{
    public class BulletScreen : TransparentForm
    {
        private BulletManager bm;
        private const int maxMessageLength = 50;

        public BulletScreen(Font font)
        {
            CanvasTimer.Interval = 16;

            Left = Screen.PrimaryScreen.Bounds.Left;
            Top = Screen.PrimaryScreen.Bounds.Top;
            Width = Screen.PrimaryScreen.Bounds.Width;
            Height = Screen.PrimaryScreen.Bounds.Height;

            bm = new BulletManager(RectangleToScreen(ClientRectangle), font);
        }

        protected override void OnDraw(Graphics canvas)
        {
            bm.Update(canvas);   
        }

        protected override Rectangle OnResizeCanvas()
        {
            return bm.Bounds;
        }

        protected override void OnCanvasCreated(Graphics canvas)
        {
            canvas.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            canvas.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            canvas.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

            bm.UpdateLayout(canvas);
        }


        public override Font Font
        {
            get
            {
                return bm.Font;
            }

            set
            {
                bm.Font = value;
                bm.UpdateLayout();
            }
        }

        public void AddMessage(string message, Color color, int delayMS = 0)
        {
            if (color == transparencyKey)
                color = Color.FromArgb(transparencyKey.ToArgb() + 1); 

            bm.Shot(message.Substring(0, Math.Min(message.Length, maxMessageLength)), color, delayMS);
        }

        public void ReBoundScreen(Rectangle bounds)
        {
            bm.Bounds = bounds;
            ResizeCanvas();
        }
    }
}
