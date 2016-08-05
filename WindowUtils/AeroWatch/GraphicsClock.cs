using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AeroWatch
{
    public abstract class GraphicsClock : IClock<Graphics>
    {
        public abstract void Draw(DateTime time);
        public abstract Size GetSize();

        protected Graphics Canvas { get; private set; }

        public void SetCanvas(Graphics g)
        {
            Canvas = g;
        }

        public virtual void Initialize(Graphics obj, string fileName)
        {
            SetCanvas(obj);
        }
    }
}
