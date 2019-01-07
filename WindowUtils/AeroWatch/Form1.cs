using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowUtils;

namespace AeroWatch
{
    public class Form1 : TransparentForm
    {
        Color color = Color.WhiteSmoke;
        int size = 12;
        ClockType current = ClockType.FontClock;
        internal ClockType Current
        {
            set
            {
                current = value;
                ResizeCanvas();
            }
        }

        Dictionary<ClockType, GraphicsClock> clocks = new Dictionary<ClockType, GraphicsClock>();

        public Form1()
        {
            //transparencyKey = Color.Transparent;
        }

        protected override void OnDraw(Graphics canvas)
        {
            clocks[current].Draw(canvas);
        }

        protected override void OnLoad(EventArgs e)
        {
            LoadClock();
            base.OnLoad(e);
        }

        protected override Rectangle OnResizeCanvas()
        {
            GraphicsClock v;
            if (!clocks.TryGetValue(current, out v))
                return Rectangle.Empty;

            return v.GetBounds();
        }

        private void LoadClock()
        {
            GraphicsClock iclock;

            iclock = new FontClock(color, size);
            iclock.Initialize("Pixel LCD-7.ttf");
            clocks.Add(ClockType.FontClock, iclock);

            iclock = new ImageClock();
            iclock.Initialize(string.Empty);
            clocks.Add(ClockType.ImageClock, iclock);

            iclock = new PictureClock();
            iclock.Initialize("clock.png");
            clocks.Add(ClockType.PictureClock, iclock);
        }
    }
}
