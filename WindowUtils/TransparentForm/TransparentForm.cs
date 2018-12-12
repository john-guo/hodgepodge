using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static WindowUtils.Utils;

namespace WindowUtils
{
    public abstract partial class TransparentForm : Form
    {
        private bool _isShow = true;
        public bool ShowMe
        {
            get
            {
                return _isShow;
            }
            set
            {
                if (_isShow == value)
                    return;

                _isShow = value;
                if (_isShow)
                {
                    StartTimerCanvas();
                    Show();
                    FormShow(this, EventArgs.Empty);
                }
                else
                {
                    StopTimerCanvas();
                    Hide();
                    FormHide(this, EventArgs.Empty);
                }
            }
        }

        public event EventHandler FormShow = delegate { };
        public event EventHandler FormHide = delegate { };

        public static bool DonotRefresh = false;

        private Graphics g;
        protected bool useDoubleBuffer = true;
        private BufferedGraphicsContext bgContext = new BufferedGraphicsContext();
        private bool useTimerCanvas = true;
        private BufferedGraphics bg;
        protected Color transparencyKey = Color.Black;
        protected bool UseTimerCanvas
        {
            get
            {
                return useTimerCanvas;
            }
            set
            {
                if (useTimerCanvas == value)
                    return;

                useTimerCanvas = value;
                if (useTimerCanvas)
                    StartTimerCanvas();
                else
                    StopTimerCanvas();
            }
        }

        protected Timer CanvasTimer { get { return timer1; } }

        public TransparentForm()
        {
            InitializeComponent();

            AllowTransparency = true;
        }


        protected abstract void OnDraw(Graphics canvas);

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!useTimerCanvas)
                return;

            if (!DonotRefresh)
                TopMost = true;

            if (useDoubleBuffer)
            {
                bg.Graphics.Clear(transparencyKey);
                bg.Graphics.Flush();
                OnDraw(bg.Graphics);
                bg.Graphics.Flush();
                bg.Render();
            }
            else
            {
                g.Clear(transparencyKey);
                OnDraw(g);
                g.Flush();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (useTimerCanvas)
            {
                base.OnPaint(e);
                return;
            }

            e.Graphics.Clear(transparencyKey);
            OnDraw(e.Graphics);
            e.Graphics.Flush();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            UpdateStyles();
            base.OnHandleCreated(e);
        }

        protected virtual int ApplyExStyle(int exStyle)
        {
            return exStyle 
                    | (int)ExtendedWindowStyles.WS_EX_TRANSPARENT
                    | (int)ExtendedWindowStyles.WS_EX_TOOLWINDOW
                    | (int)ExtendedWindowStyles.WS_EX_LAYERED;
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle = ApplyExStyle(cp.ExStyle);
                return cp;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ResizeCanvas();
            TransparencyKey = transparencyKey;

            StartTimerCanvas();
        }

        protected virtual Rectangle OnResizeCanvas()
        {
            return RectangleToScreen(ClientRectangle);
        }

        protected virtual void OnCanvasCreated(Graphics canvas)
        {

        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            StopTimerCanvas();
            bg.Dispose();
        }

        protected virtual void ResizeCanvas()
        {
            SetupCanvas(OnResizeCanvas());
        }

        private void StartTimerCanvas()
        {
            if (!useTimerCanvas)
                return;

            timer1.Start();
        }

        private void StopTimerCanvas()
        {
            if (timer1.Enabled)
                timer1.Stop();
        }

        private void SetupCanvas()
        {
            g = CreateGraphics();
            if (useDoubleBuffer)
            {
                bgContext.Invalidate();
                bg = bgContext.Allocate(g, ClientRectangle);
                OnCanvasCreated(bg.Graphics);
            }
            else
            {
                OnCanvasCreated(g);
            }
        }

        private void SetupCanvas(Rectangle bounds)
        {
            Width = bounds.Width;
            Height = bounds.Height;
            Left = bounds.Left;
            Top = bounds.Top;

            SetupCanvas();
        }
    }
}
