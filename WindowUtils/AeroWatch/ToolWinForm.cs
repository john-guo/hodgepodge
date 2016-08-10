using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static WindowUtils.Utils;

namespace AeroWatch
{
    public abstract class ToolWinForm : TransparentForm
    {
        public ToolWinForm()
        {
            FormBorderStyle = FormBorderStyle.SizableToolWindow;
        }

        protected override int ApplyExStyle(int exStyle)
        {
            return exStyle
                    | (int)ExtendedWindowStyles.WS_EX_TOOLWINDOW
                    | (int)ExtendedWindowStyles.WS_EX_LAYERED;
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case 0x84:
                    base.WndProc(ref m);
                    if ((int)m.Result == 1)
                        m.Result = (IntPtr)2;
                    return;
            }

            base.WndProc(ref m);
        }

        protected override void OnActivated(EventArgs e)
        {
            DonotRefresh = true;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyData == Keys.Escape)
            {
                Hide();
                DialogResult = DialogResult.Cancel;
                DonotRefresh = false;
            }
            else if (e.KeyData == Keys.Enter)
            {
                Hide();
                DialogResult = DialogResult.OK;
                DonotRefresh = false;
            }
        }
    }
}
