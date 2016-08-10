using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AeroWatch
{
    public partial class LauncherForm : Form
    {
        Form1 clockForm;

        public LauncherForm()
        {
            InitializeComponent();
            Visible = false;
            SetVisibleCore(false);
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            clockForm.Opacity = 0.25;
            toolStripMenuItem3.Checked = true;
            toolStripMenuItem4.Checked = false;
            toolStripMenuItem5.Checked = false;
            toolStripMenuItem6.Checked = false;
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            clockForm.Opacity = 0.5;
            toolStripMenuItem3.Checked = false;
            toolStripMenuItem4.Checked = true;
            toolStripMenuItem5.Checked = false;
            toolStripMenuItem6.Checked = false;
        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            clockForm.Opacity = 0.75;
            toolStripMenuItem3.Checked = false;
            toolStripMenuItem4.Checked = false;
            toolStripMenuItem5.Checked = true;
            toolStripMenuItem6.Checked = false;
        }

        private void toolStripMenuItem6_Click(object sender, EventArgs e)
        {
            clockForm.Opacity = 1.0;
            toolStripMenuItem3.Checked = false;
            toolStripMenuItem4.Checked = false;
            toolStripMenuItem5.Checked = false;
            toolStripMenuItem6.Checked = true;
        }

        private void watchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            clockForm.Current = ClockType.FontClock;
            watchToolStripMenuItem.Checked = true;
            clockToolStripMenuItem.Checked = false;
        }

        private void clockToolStripMenuItem_Click(object sender, EventArgs e)
        {
            clockForm.Current = ClockType.ImageClock;
            watchToolStripMenuItem.Checked = false;
            clockToolStripMenuItem.Checked = true;
        }

        private void tsmExit_Click(object sender, EventArgs e)
        {
            clockForm.Close();
            Close();
        }

        private void tsmDisplay_Click(object sender, EventArgs e)
        {
            clockForm.ShowMe = !clockForm.ShowMe;
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            clockForm.ShowMe = true;
        }

        private void LauncherForm_Load(object sender, EventArgs e)
        {
            clockForm = new Form1();
            clockForm.FormShow += ClockForm_ClockShow;
            clockForm.FormHide += ClockForm_ClockHide;
            clockForm.Load += ClockForm_Load;
            clockForm.Show();

            var test = new BreathingGalaxies();
            test.Show();

            //var toolWin = new ToolWinScreen();
            //if (toolWin.ShowDialog() == DialogResult.OK)
            //{
            //    clockForm.Width = toolWin.Width;
            //    clockForm.Height = toolWin.Height;
            //    clockForm.Left = toolWin.Left;
            //    clockForm.Top = toolWin.Top;
            //}
            //toolWin.Show();

            //var bs = new BulletScreen();
            //bs.Show();

            //bs.bm.Shot("测试", Color.Red);
            //bs.bm.Shot("测试测试测试测试", Color.GreenYellow);
        }

        private void ClockForm_Load(object sender, EventArgs e)
        {
            toolStripMenuItem5_Click(sender, e);
        }

        private void ClockForm_ClockHide(object sender, EventArgs e)
        {
            tsmDisplay.Text = "Show";
        }

        private void ClockForm_ClockShow(object sender, EventArgs e)
        {
            tsmDisplay.Text = "Hide";
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            TransparentForm.DonotRefresh = true;
        }

        private void contextMenuStrip1_Closing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            TransparentForm.DonotRefresh = false;
        }
    }
}
