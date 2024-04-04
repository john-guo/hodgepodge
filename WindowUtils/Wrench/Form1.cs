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
using System.Drawing.Imaging;
using static WindowUtils.Utils.Dwm;
using static WindowUtils.Utils;
using System.Globalization;
using System.Runtime.InteropServices;

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
            return;

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
            var owner = Utils.GetAncestor(hwnd, Utils.GA_FLAGS.GA_ROOTOWNER);
            if (owner != IntPtr.Zero)
                hwnd = owner;
            if (hwnd == Handle)
                return;

            SetTargetWindow(hwnd);
        }

        private void SetTargetWindow(IntPtr hwnd)
        {
            if (hwnd == null)
                return;

            int pid;
            var tid = Utils.GetWindowThreadProcessId(hwnd, out pid);
            var process = Process.GetProcessById(pid);

            if (process == null || process.MainWindowHandle == Handle)
                return;

            var mainWnd = process.MainWindowHandle;

            target = mainWnd == IntPtr.Zero ? hwnd : mainWnd;

            var title = GetWindowText(target);

            if (string.IsNullOrWhiteSpace(title))
                title = process.ProcessName;

            Invoke((Action)delegate
            {
                toolStripStatusLabel1.Text = title;
            });

            button2.Enabled = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            checkBox1.Checked = false;
            checkBox2.Checked = false;
            checkBox3.Checked = false;
            checkBox4.Checked = false;
            checkBox5.Checked = false;
            checkBox6.Checked = false;
            checkBox7.Checked = false;

            trackBar1.Value = 100;

            var prochint = txtProc.Text.Trim();
            if (prochint.Length > 0)
            {
                if (int.TryParse(prochint, NumberStyles.HexNumber, null, out int pid))
                {
                    var hwd = new IntPtr(pid);
                    SetTargetWindow(hwd);
                }
                else
                {
                    var hwd = FindWindowsWithText(title => title.ToLower().Contains(prochint.ToLower())).FirstOrDefault();
                    SetTargetWindow(hwd);
                }

                return;
            }

            foreach (var v in Enum.GetValues(typeof(Utils.OCRId)))
            {
                Utils.OCRId id = (Utils.OCRId)v;

                if (id == Utils.OCRId.OCR_CROSS)
                    continue;

                Utils.SetSystemCursor(Cursors.Cross.Handle, id);
            }

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
                if (checkBox1.Checked)
                {
                    targetExStyle = Utils.RemoveWindowExStyle(target, Utils.ExtendedWindowStyles.WS_EX_TRANSPARENT);
                }
                else
                {
                    targetExStyle = Utils.RemoveWindowExStyle(target, Utils.ExtendedWindowStyles.WS_EX_TRANSPARENT | Utils.ExtendedWindowStyles.WS_EX_LAYERED);
                }
            }
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (target == IntPtr.Zero)
                return;

            if (checkBox3.Checked)
            {
                Utils.SetForegroundWindow(target);
                Utils.SetWindowPos(target, Utils.HWND.TOPMOST, 0, 0, 0, 0, Utils.SWP.NOMOVE | Utils.SWP.NOSIZE);
            }
            else
            {
                Utils.SetWindowPos(target, Utils.HWND.NOTOPMOST, 0, 0, 0, 0, Utils.SWP.NOMOVE | Utils.SWP.NOSIZE);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var x = int.Parse(textBox1.Text);
            var y = int.Parse(textBox2.Text);
            var w = int.Parse(textBox3.Text);
            var h = int.Parse(textBox4.Text);

            //Utils.SetWindowPos(target, Utils.HWND.NOTOPMOST, x, y, w, h, Utils.SWP.NOZORDER | Utils.SWP.SHOWWINDOW);
            Utils.MoveWindow(target, x, y, w, h, true);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var x = int.Parse(textBox1.Text);
            var y = int.Parse(textBox2.Text);
            var w = int.Parse(textBox3.Text);
            var h = int.Parse(textBox4.Text);

            var f = new Form();
            f.FormBorderStyle = FormBorderStyle.None;
            f.MaximumSize = new Size(w, h);
            f.Size = f.MaximumSize;
            f.Text = f.Handle.ToInt32().ToString();
            f.KeyPreview = true;
            f.BackColor = Color.Red;
            f.Show();
            f.Activate();

            Utils.MoveWindow(f.Handle, x, y, w, h, true);
        }

        private void Paste()
        {
            Utils.SendMessage(target, Utils.WM.PASTE, IntPtr.Zero, IntPtr.Zero);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (target == IntPtr.Zero)
                return;

            var img = PrintWindow2(target);
            Clipboard.SetImage(img);
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            if (target == IntPtr.Zero)
                return;

            if (checkBox4.Checked)
            {
                targetExStyle = Utils.RemoveWindowExStyle(target, Utils.ExtendedWindowStyles.WS_EX_APPWINDOW);
                targetExStyle = Utils.AddWindowExStyle(target, Utils.ExtendedWindowStyles.WS_EX_TOOLWINDOW);
            }
            else
            {
                targetExStyle = Utils.RemoveWindowExStyle(target, Utils.ExtendedWindowStyles.WS_EX_TOOLWINDOW);
                targetExStyle = Utils.AddWindowExStyle(target, Utils.ExtendedWindowStyles.WS_EX_APPWINDOW);
            }
        }

        public Bitmap PrintWindow(IntPtr hwnd)
        {
            Utils.RECT rc;
            Utils.GetWindowRect(hwnd, out rc);

            var width = rc.right - rc.left + 1;
            var height = rc.bottom - rc.top + 1;
            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            using (var gfxBmp = Graphics.FromImage(bmp))
            {
                gfxBmp.CopyFromScreen(rc.left, rc.top, 0, 0, new Size(width, height));
            }

            return bmp;
        }

        public Bitmap PrintWindow2(IntPtr hwnd)
        {
            Utils.RECT rc;
            Utils.GetWindowRect(hwnd, out rc);

            var width = rc.right - rc.left + 1;
            var height = rc.bottom - rc.top + 1;
            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            using (var gfxBmp = Graphics.FromImage(bmp))
            {
                IntPtr hdcBitmap = gfxBmp.GetHdc();
                Utils.PrintWindow(hwnd, hdcBitmap, 0);
                gfxBmp.ReleaseHdc(hdcBitmap);
            }

            return bmp;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (target == IntPtr.Zero)
                return;

            var img = PrintWindow(target);
            Clipboard.SetImage(img);
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            if (target == IntPtr.Zero)
                return;

            if (checkBox5.Checked)
            {

                var currExStyle = GetWindowLongPtr(target, WindowLongIndex.GWL_EXSTYLE);
                SetWindowLongPtr(target, WindowLongIndex.GWL_EXSTYLE, new IntPtr(currExStyle.ToInt64() & (long)~ExtendedWindowStyles.WS_EX_LAYERED));

                MARGINS margins = new MARGINS { leftWidth = -1 };
                DwmExtendFrameIntoClientArea(target, ref margins);

                SetWindowPos(target, IntPtr.Zero, 0, 0, 0, 0, SWP.NOMOVE | SWP.NOSIZE | SWP.NOZORDER | SWP.FRAMECHANGED | SWP.SHOWWINDOW);

                //WindowSheetOfGlass(target);
            }
            else
            {
                MARGINS margins = new MARGINS { leftWidth = 0 };
                DwmExtendFrameIntoClientArea(target, ref margins);
            }
        }

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {
            if (target == IntPtr.Zero)
                return;

            if (checkBox6.Checked)
            {
                WindowEnableBlurBehind(target);
            }
            else
            {
                DWM_BLURBEHIND dwm_BB = new DWM_BLURBEHIND(false);
                DwmEnableBlurBehindWindow(target, ref dwm_BB);
            }
        }

        private void checkBox7_CheckedChanged(object sender, EventArgs e)
        {
            if (target == IntPtr.Zero)
                return;

            if (checkBox7.Checked)
            {
                var oldStyles = GetWindowLongPtr(target, WindowLongIndex.GWL_STYLE);
                SetWindowLongPtr(target, WindowLongIndex.GWL_STYLE, new IntPtr(~(long)(WindowStyles.WS_CAPTION | WindowStyles.WS_THICKFRAME | WindowStyles.WS_MINIMIZEBOX | WindowStyles.WS_MAXIMIZEBOX | WindowStyles.WS_SYSMENU) & oldStyles.ToInt64()));
            }
            else
            {
                var oldStyles = GetWindowLongPtr(target, WindowLongIndex.GWL_STYLE);
                SetWindowLongPtr(target, WindowLongIndex.GWL_STYLE, new IntPtr((long)(WindowStyles.WS_CAPTION | WindowStyles.WS_THICKFRAME | WindowStyles.WS_MINIMIZEBOX | WindowStyles.WS_MAXIMIZEBOX | WindowStyles.WS_SYSMENU) | oldStyles.ToInt64()));
            }
            SetWindowPos(target, IntPtr.Zero, 0, 0, 0, 0, SWP.NOMOVE | SWP.NOSIZE | SWP.NOZORDER | SWP.FRAMECHANGED | SWP.SHOWWINDOW);
        }
    }
}
