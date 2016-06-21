using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gma.System.MouseKeyHook;
using WindowUtils;
using System.Threading;
using System.Diagnostics;

namespace Wrench
{
    public partial class Form1 : Form
    {
        IKeyboardMouseEvents g_events;
        IntPtr target = IntPtr.Zero;
        IntPtr targetExStyle = IntPtr.Zero;

        public Form1()
        {
            InitializeComponent();

            g_events = Hook.GlobalEvents();
        }

        private void G_events_MouseMove(object sender, MouseEventArgs e)
        {
            var hwnd = Utils.WindowFromPoint(e.Location);
            if (hwnd == Handle)
                return;

            int pid;
            var tid = Utils.GetWindowThreadProcessId(hwnd, out pid);
            var process = Process.GetProcessById(pid);

            if (process.MainWindowHandle == Handle)
                return;

            Utils.RECT r;
            Utils.GetWindowRect(process.MainWindowHandle, out r);

            Trace.WriteLine(string.Format("{0}, {1}, {2}, {3}", r.left, r.right, r.top, r.bottom));

            var g = Graphics.FromHwnd(process.MainWindowHandle);
            g.DrawRectangle(Pens.Blue, 0, 0, r.right - r.left + 1, r.bottom - r.top + 1);
            g.Flush();
            g.Dispose();
        }

        private void G_events_MouseClick(object sender, MouseEventArgs e)
        {
            uint param = 0;
            Utils.SystemParametersInfo(Utils.SystemParametersInfoAction.SPI_SETCURSORS, 0, ref param, 0);

            g_events.MouseClick -= G_events_MouseClick;
            g_events.MouseMove -= G_events_MouseMove;

            var hwnd = Utils.WindowFromPoint(e.Location);
            if (hwnd == Handle)
                return;

            int pid;
            var tid = Utils.GetWindowThreadProcessId(hwnd, out pid);
            var process = Process.GetProcessById(pid);
            if (process.MainWindowHandle == Handle)
                return;

            target = hwnd;

            Invoke((Action)delegate
            {
                label1.Text = process.MainWindowTitle;
            });


        }

        private void button1_Click(object sender, EventArgs e)
        {
            foreach (var v in Enum.GetValues(typeof(Utils.OCRId)))
            {
                Utils.OCRId id = (Utils.OCRId)v;

                if (id == Utils.OCRId.OCR_CROSS)
                    continue;

                Utils.SetSystemCursor(Cursors.Cross.Handle, id);
            }

            checkBox1.Checked = false;
            checkBox2.Checked = false;
            checkBox3.Checked = false;
            trackBar1.Value = 100;

            g_events.MouseClick += G_events_MouseClick;
            g_events.MouseMove += G_events_MouseMove;
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            if (target == IntPtr.Zero)
                return;

            label2.Text = trackBar1.Value.ToString();
            var alpha = trackBar1.Value / 100.0f * 255;
            Utils.SetLayeredWindowAttributes(target, 0, (byte)alpha, 2);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (target == IntPtr.Zero)
                return;

            trackBar1.Enabled = checkBox1.Checked;

            if (checkBox1.Checked)
            {
                targetExStyle = Utils.AddWindowExStyle(target, Utils.ExtendedWindowStyles.WS_EX_LAYERED);
            }
            else
            {
                targetExStyle = Utils.RemoveWindowExStyle(target, Utils.ExtendedWindowStyles.WS_EX_LAYERED);
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (target == IntPtr.Zero)
                return;

            if (checkBox2.Checked)
            {
                targetExStyle = Utils.AddWindowExStyle(target, Utils.ExtendedWindowStyles.WS_EX_TRANSPARENT | Utils.ExtendedWindowStyles.WS_EX_LAYERED);
            }
            else
            {
                targetExStyle = Utils.RemoveWindowExStyle(target, Utils.ExtendedWindowStyles.WS_EX_TRANSPARENT | Utils.ExtendedWindowStyles.WS_EX_LAYERED);
            }
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (target == IntPtr.Zero)
                return;

            if (checkBox3.Checked)
            {
                Utils.SetWindowPos(target, Utils.HWND.TOPMOST, 0, 0, 0, 0, Utils.SWP.NOMOVE | Utils.SWP.NOSIZE);
            }
            else
            {
                Utils.SetWindowPos(target, Utils.HWND.NOTOPMOST, 0, 0, 0, 0, Utils.SWP.NOMOVE | Utils.SWP.NOSIZE);
            }
        }
    }
}
