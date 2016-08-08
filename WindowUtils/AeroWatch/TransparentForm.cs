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

namespace AeroWatch
{

    public partial class TransparentForm : Form
    {
        bool _isShow = true;
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
                    timer1.Start();
                    Show();
                    FormShow(this, EventArgs.Empty);
                }
                else
                {
                    timer1.Stop();
                    Hide();
                    FormHide(this, EventArgs.Empty);
                }
            }
        }

        public event EventHandler FormShow = delegate { };
        public event EventHandler FormHide = delegate { };

        BufferedGraphicsContext bgContext = new BufferedGraphicsContext();
        BufferedGraphics bg;
        readonly Color transparencyKey = Color.White;
        internal bool DonotRefresh = false;

        public TransparentForm()
        {
            InitializeComponent();
        }

        protected virtual void OnDraw(Graphics canvas)
        { }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!DonotRefresh)
                TopMost = true;

            bg.Graphics.Clear(transparencyKey);
            OnDraw(bg.Graphics);
            bg.Graphics.Flush();
            bg.Render();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            UpdateStyles();
            base.OnHandleCreated(e);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x00000020 | 0x00080000 | 0x00000080;
                return cp;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            TransparencyKey = transparencyKey;
            timer1.Start();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            timer1.Stop();
            bg.Dispose();
        }

        private void SetupCanvas()
        {
            var g = CreateGraphics();
            bgContext.Invalidate();
            bg = bgContext.Allocate(g, ClientRectangle);
        }

        protected void SetupCanvas(Size size)
        {
            Width = size.Width;
            Height = size.Height;
            Left = Screen.PrimaryScreen.Bounds.Width - Width;
            Top = Screen.PrimaryScreen.Bounds.Top;

            SetupCanvas();
        }
    }
}
