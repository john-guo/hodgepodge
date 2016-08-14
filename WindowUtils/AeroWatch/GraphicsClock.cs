using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowUtils;

namespace AeroWatch
{
    enum ClockType
    {
        FontClock,
        ImageClock
    }

    public abstract class GraphicsClock : IFloatingCanvas
    {
        public abstract void Draw(Graphics canvas);
        public abstract void Initialize(string fileName);
        public virtual Rectangle GetBounds()
        {
            var size = GetSize();
            var x = Screen.PrimaryScreen.Bounds.Width - size.Width;
            var y = Screen.PrimaryScreen.Bounds.Top;
            return new Rectangle(x, y, size.Width, size.Height);
        }

        protected abstract Size GetSize();
    }
}
