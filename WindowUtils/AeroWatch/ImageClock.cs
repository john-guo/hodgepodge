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

        public override void Initialize(Graphics obj, string fileName)
        {
            base.Initialize(obj, fileName);

            image = Image.FromFile(fileName);
        }

        public override void Draw(DateTime time)
        {
            Canvas.DrawImageUnscaledAndClipped(image, new Rectangle(Point.Empty, image.Size));
        }

        public override Size GetSize()
        {
            return image.Size;
        }

    }
}
