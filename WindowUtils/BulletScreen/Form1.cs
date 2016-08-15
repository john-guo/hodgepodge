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
using System.IO;

namespace BulletScreen
{
    public partial class Form1 : Form
    {
        BulletScreen screen;
        ToolWinScreen tws;
        IBulletSource source;

        public Form1()
        {
            InitializeComponent();

            tws = new ToolWinScreen();
            tws.Width = 400;
            tws.Height = 400;

            button2.Font = fontDialog1.Font;
            button2.ForeColor = fontDialog1.Color;

            screen = new BulletScreen(button2.Font);
            source = new BulletSource_Tieba();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            screen.Show();
            backgroundWorker1.RunWorkerAsync();
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
            button2.ForeColor = fontDialog1.Color;

            screen.Font = fontDialog1.Font;
        }


        private void button3_Click(object sender, EventArgs e)
        {
            if (tws.ShowDialog() != DialogResult.OK)
                return;

            screen.ReBoundScreen(tws.DesktopBounds);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                try
                {
                    foreach (var message in source.GetMessage())
                    {
                        Invoke((Action)delegate
                        {
                            try
                            {
                                screen.AddMessage(message.Content, button2.ForeColor, message.Delay);
                            }
                            catch
                            {
                            }
                        });
                    }
                }
                catch
                {
                }
            });
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox2.Text.Trim()))
                return;

            (source as BulletSource_Tieba).ThreadId = textBox2.Text;
            timer1.Start();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            source.Initialize();

            Invoke((Action)delegate {
                textBox2.Text = (source as BulletSource_Tieba).ThreadId;
                label1.Text = (source as BulletSource_Tieba).Title;
                timer1.Start();
            });
        }
    }
}
