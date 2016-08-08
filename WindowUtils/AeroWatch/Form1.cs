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
                SetupClock();
            }
        }

        Dictionary<ClockType, GraphicsClock> clocks = new Dictionary<ClockType, GraphicsClock>();

        public Form1()
        {
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

        private void SetupClock()
        {
            SetupCanvas(clocks[current].GetSize());
        }

        private void LoadClock()
        {
            GraphicsClock iclock;

            iclock = new FontClock(color, size);
            iclock.Initialize("Pixel LCD-7.ttf");
            clocks.Add(ClockType.FontClock, iclock);

            iclock = new ImageClock();
            iclock.Initialize("clock.png");
            clocks.Add(ClockType.ImageClock, iclock);

            SetupClock();
        }
    }
}
