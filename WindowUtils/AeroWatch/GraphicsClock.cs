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
        ImageClock,
        PictureClock
    }

    public abstract class GraphicsClock : IFloatingCanvas
    {
        public abstract void Draw(Graphics canvas);
        public abstract void Initialize(string fileName);
        public virtual Rectangle GetBounds()
        {
            var size = GetSize();
            return new Rectangle(0, 0, size.Width, size.Height);
        }

        protected abstract Size GetSize();
    }
}
