using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WpfGecko
{
    public class MascotForm : Form
    {
        private struct Vector2
        {
            public int x;

            public int y;

            public Vector2(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct BGRA
        {
            public byte B;

            public byte G;

            public byte R;

            public byte A;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct BLENDFUNC
        {
            public byte BlendOp;

            public byte BlendFlags;

            public byte SourceConstantAlpha;

            public byte AlphaFormat;
        }

        private const int ULW_COLORKEY = 1;

        private const int ULW_ALPHA = 2;

        private const int ULW_OPAQUE = 4;

        private const byte AC_SRC_OVER = 0;

        private const byte AC_SRC_ALPHA = 1;

        private MascotForm.BLENDFUNC blend;

        public int HitTestCount;

        public int HitTestCountTmp;

        public event MouseEventHandler _LeftMouseDown;

        public event MouseEventHandler _LeftMouseUp;

        public event MouseEventHandler _RightMouseDown;

        public event MouseEventHandler _RightMouseUp;

        public event MouseEventHandler _MiddleMouseDown;

        public event MouseEventHandler _MiddleMouseUp;

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams createParams = base.CreateParams;
                createParams.ExStyle |= 524288;
                return createParams;
            }
        }

        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern int UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst, ref MascotForm.Vector2 pptDst, ref MascotForm.Vector2 psize, IntPtr hdcSrc, ref MascotForm.Vector2 pprSrc, int crKey, ref MascotForm.BLENDFUNC pblend, int dwFlags);

        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll", ExactSpelling = true)]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern IntPtr CreateCompatibleDC(IntPtr hDC);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern int DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll", ExactSpelling = true)]
        private static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern int DeleteObject(IntPtr hObject);

        public MascotForm()
        {
            this.Text = "";
            this.AllowDrop = false;
            base.FormBorderStyle = FormBorderStyle.None;
            base.TopMost = true;
            base.ShowInTaskbar = false;
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            this.DoubleBuffered = true;
            this.blend = default(MascotForm.BLENDFUNC);
            this.blend.BlendOp = 0;
            this.blend.BlendFlags = 0;
            this.blend.AlphaFormat = 1;
            base.Capture = true;
            base.FormClosed += new FormClosedEventHandler(this.OnFormClosed);
            base.MouseDown += new MouseEventHandler(this.Form_MouseDown);
            base.MouseUp += new MouseEventHandler(this.Form_MouseUp);
            this.HitTestCount = 0;
        }

        private void OnFormClosed(object sender, EventArgs e)
        {
            base.MouseDown -= new MouseEventHandler(this.Form_MouseDown);
            base.MouseUp -= new MouseEventHandler(this.Form_MouseUp);
        }

        private void Form_MouseDown(object sender, MouseEventArgs e)
        {
            MouseEventHandler mouseEventHandler = null;
            MouseButtons button = e.Button;
            if (button != MouseButtons.Left)
            {
                if (button != MouseButtons.Right)
                {
                    if (button == MouseButtons.Middle)
                    {
                        mouseEventHandler = this._MiddleMouseDown;
                    }
                }
                else
                {
                    mouseEventHandler = this._RightMouseDown;
                }
            }
            else
            {
                mouseEventHandler = this._LeftMouseDown;
            }
            if (mouseEventHandler != null)
            {
                mouseEventHandler(this, e);
            }
        }

        private void Form_MouseUp(object sender, MouseEventArgs e)
        {
            MouseEventHandler mouseEventHandler = null;
            MouseButtons button = e.Button;
            if (button != MouseButtons.Left)
            {
                if (button != MouseButtons.Right)
                {
                    if (button == MouseButtons.Middle)
                    {
                        mouseEventHandler = this._MiddleMouseUp;
                    }
                }
                else
                {
                    mouseEventHandler = this._RightMouseUp;
                }
            }
            else
            {
                mouseEventHandler = this._LeftMouseUp;
            }
            if (mouseEventHandler != null)
            {
                mouseEventHandler(this, e);
            }
        }

        public void Repaint(Bitmap bitmap, byte opacity)
        {
            if (bitmap.PixelFormat != PixelFormat.Format32bppArgb)
            {
                throw new ApplicationException("The bitmap must be 32ppp with alpha-channel.");
            }
            IntPtr dC = MascotForm.GetDC(IntPtr.Zero);
            IntPtr intPtr = MascotForm.CreateCompatibleDC(dC);
            IntPtr intPtr2 = IntPtr.Zero;
            IntPtr hObject = IntPtr.Zero;
            intPtr2 = bitmap.GetHbitmap(System.Drawing.Color.FromArgb(0));
            hObject = MascotForm.SelectObject(intPtr, intPtr2);
            MascotForm.Vector2 vector = new MascotForm.Vector2(bitmap.Width, bitmap.Height);
            MascotForm.Vector2 vector2 = new MascotForm.Vector2(0, 0);
            MascotForm.Vector2 vector3 = new MascotForm.Vector2(base.Left, base.Top);
            this.blend.SourceConstantAlpha = opacity;
            MascotForm.UpdateLayeredWindow(base.Handle, dC, ref vector3, ref vector, intPtr, ref vector2, 0, ref this.blend, 2);
            if (intPtr2 != IntPtr.Zero)
            {
                MascotForm.SelectObject(intPtr, hObject);
                MascotForm.DeleteObject(intPtr2);
            }
            MascotForm.DeleteDC(intPtr);
            MascotForm.ReleaseDC(IntPtr.Zero, dC);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 132)
            {
                this.HitTestCount++;
            }
            m.Result = (IntPtr)1;
            base.WndProc(ref m);
        }
    }
}
