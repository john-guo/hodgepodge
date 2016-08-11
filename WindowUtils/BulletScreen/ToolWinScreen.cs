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
    public class ToolWinScreen : ToolWinForm
    {
        public ToolWinScreen()
        {
            Opacity = 0.5;
            UseTimerCanvas = false;
            StartPosition = FormStartPosition.Manual;
        }

        protected override void OnDraw(Graphics canvas)
        {
            canvas.Clear(Color.LightCyan);
        }
    }
}
