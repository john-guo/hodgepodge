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
    enum ClockType
    {
        FontClock,
        ImageClock
    }


    public partial class Form1 : Form
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
                    ClockShow(this, EventArgs.Empty);
                }
                else
                {
                    timer1.Stop();
                    Hide();
                    ClockHide(this, EventArgs.Empty);
                }
            }
        }

        public event EventHandler ClockShow = delegate { };
        public event EventHandler ClockHide = delegate { };

        BufferedGraphicsContext bgContext = new BufferedGraphicsContext();
        BufferedGraphics bg;
        readonly Color transparencyKey = Color.White;
        Color color = Color.WhiteSmoke;
        int size = 12;
        ClockType current = ClockType.FontClock;
        internal ClockType Current
        {
            set
            {
                current = value;

                var g = CreateGraphics();
                SetupClock(g);
            }
        }

        internal bool DonotRefresh = false;

        Dictionary<ClockType, GraphicsClock> clocks = new Dictionary<ClockType, GraphicsClock>();


        public Form1()
        {
            InitializeComponent();
        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!DonotRefresh)
                TopMost = true;

            var now = DateTime.Now;
            bg.Graphics.Clear(transparencyKey);
            clocks[current].Draw(now);
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

        private void Form1_Load(object sender, EventArgs e)
        {
            var g = CreateGraphics();
            LoadClock(g);

            SetupClock(g);

            TransparencyKey = transparencyKey;

            timer1.Start();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            timer1.Stop();
            bg.Dispose();
        }

        private void SetupClock(Graphics g)
        {
            var size = clocks[current].GetSize();
            Width = size.Width;
            Height = size.Height;
            Left = Screen.PrimaryScreen.Bounds.Width - Width;
            Top = Screen.PrimaryScreen.Bounds.Top;

            bgContext.Invalidate();
            bg = bgContext.Allocate(g, ClientRectangle);
            UpdateClockCanvas(bg.Graphics);
        }

        private void LoadClock(Graphics g)
        {
            GraphicsClock iclock;

            iclock = new FontClock(color, size);
            iclock.Initialize(g, "Pixel LCD-7.ttf");
            clocks.Add(ClockType.FontClock, iclock);

            iclock = new ImageClock();
            iclock.Initialize(g, "clock.png");
            clocks.Add(ClockType.ImageClock, iclock);
        }

        private void UpdateClockCanvas(Graphics g)
        {
            foreach (var clock in clocks)
            {
                clock.Value.SetCanvas(g);
            }
        }
    }
}
