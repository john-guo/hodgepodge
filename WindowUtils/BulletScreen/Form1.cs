using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowUtils;

namespace BulletScreen
{
    public partial class Form1 : Form
    {
        BulletScreen screen;
        ToolWinScreen tws;

        public Form1()
        {
            InitializeComponent();
            screen = new BulletScreen();

            tws = new ToolWinScreen();
            tws.Width = 400;
            tws.Height = 400;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            screen.Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text))
                return;

            screen.AddMessage(textBox1.Text, button2.ForeColor);
        }

        private void Form1_Activated(object sender, EventArgs e)
        {
            TransparentForm.DonotRefresh = true;
        }

        private void Form1_Deactivate(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
                TransparentForm.DonotRefresh = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (fontDialog1.ShowDialog() != DialogResult.OK)
                return;

            button2.Font = fontDialog1.Font;
            screen.Font = fontDialog1.Font;

            button2.ForeColor = fontDialog1.Color;
        }


        private void button3_Click(object sender, EventArgs e)
        {
            if (tws.ShowDialog() != DialogResult.OK)
                return;

            screen.ReBoundScreen(tws.DesktopBounds);
        }
    }
}
