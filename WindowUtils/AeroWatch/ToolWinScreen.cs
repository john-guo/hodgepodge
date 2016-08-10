using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static WindowUtils.Utils;

namespace AeroWatch
{
    public class ToolWinScreen : ToolWinForm
    {
        public ToolWinScreen()
        {
            Opacity = 0.5;
            UseTimerCanvas = false;
        }

        protected override void OnDraw(Graphics canvas)
        {
            canvas.Clear(Color.Pink);
        }
    }
}
