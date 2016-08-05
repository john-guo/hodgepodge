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
    public partial class Form1 : Form
    {
        bool _isShow = true;
        bool ShowMe
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
                    tsmDisplay.Text = "Hide";
                }
                else
                {
                    timer1.Stop();
                    Hide();
                    tsmDisplay.Text = "Show";
                }
            }
        }

        BufferedGraphicsContext bgContext = new BufferedGraphicsContext();
        BufferedGraphics bg;
        readonly Color transparencyKey = Color.White;
        Color color = Color.WhiteSmoke;
        int size = 12;
        ClockType current = ClockType.FontClock;
        ClockType Current
        {
            set
            {
                current = value;

                var g = CreateGraphics();
                SetupClock(g);
            }
        }


        enum ClockType
        {
            FontClock,
            ImageClock
        }
        Dictionary<ClockType, GraphicsClock> clocks = new Dictionary<ClockType, GraphicsClock>();


        public Form1()
        {
            InitializeComponent();
        }

        private void tsmExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void tsmDisplay_Click(object sender, EventArgs e)
        {
            ShowMe = !ShowMe;
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            ShowMe = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
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
            //Utils.AddWindowExStyle(Handle, Utils.ExtendedWindowStyles.WS_EX_TRANSPARENT | Utils.ExtendedWindowStyles.WS_EX_LAYERED | Utils.ExtendedWindowStyles.WS_EX_TOOLWINDOW);
            var g = CreateGraphics();
            g.PageUnit = GraphicsUnit.Pixel;
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
            bg.Graphics.PageUnit = GraphicsUnit.Pixel;
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

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            Opacity = 0.25;
            toolStripMenuItem3.Checked = true;
            toolStripMenuItem4.Checked = false;
            toolStripMenuItem5.Checked = false;
            toolStripMenuItem6.Checked = false;
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            Opacity = 0.5;
            toolStripMenuItem3.Checked = false;
            toolStripMenuItem4.Checked = true;
            toolStripMenuItem5.Checked = false;
            toolStripMenuItem6.Checked = false;
        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            Opacity = 0.75;
            toolStripMenuItem3.Checked = false;
            toolStripMenuItem4.Checked = false;
            toolStripMenuItem5.Checked = true;
            toolStripMenuItem6.Checked = false;
        }

        private void toolStripMenuItem6_Click(object sender, EventArgs e)
        {
            Opacity = 1.0;
            toolStripMenuItem3.Checked = false;
            toolStripMenuItem4.Checked = false;
            toolStripMenuItem5.Checked = false;
            toolStripMenuItem6.Checked = true;
        }

        private void watchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Current = ClockType.FontClock;
            watchToolStripMenuItem.Checked = true;
            clockToolStripMenuItem.Checked = false;
        }

        private void clockToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Current = ClockType.ImageClock;
            watchToolStripMenuItem.Checked = false;
            clockToolStripMenuItem.Checked = true;
        }
    }
}
