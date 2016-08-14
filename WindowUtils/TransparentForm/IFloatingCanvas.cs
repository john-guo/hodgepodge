using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowUtils
{
    public interface IFloatingCanvas
    {
        Rectangle GetBounds();
        void Draw(Graphics canvas);
        void Initialize(string fileName);
    }
}
