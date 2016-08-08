using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AeroWatch
{
    enum ClockType
    {
        FontClock,
        ImageClock
    }

    public abstract class GraphicsClock : IFloatingCanvas
    {
        public abstract Size GetSize();
        public abstract void Draw(Graphics canvas);
        public abstract void Initialize(string fileName);
    }
}
