using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace simple_screensaver
{
    public partial class Form1 : Form
    {
        KeyBordHook kbh;
        MouseHook mh;

        public Form1()
        {
            InitializeComponent();
            BackColor = Color.Black;

            kbh = new KeyBordHook();
            mh = new MouseHook();
            kbh.OnKeyDownEvent += Kh_OnKeyDownEvent;
            mh.OnMouseActivity += Mh_OnMouseActivity;

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ShowInTaskbar = false;
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;

#if DEBUG
            TopMost = false;
#else
            TopMost = true;
#endif
            Cursor.Hide();

            var url = Properties.Settings.Default.url;

            if (string.IsNullOrEmpty(Settings.WorkDir))
            {
                if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
                {
                    if (!Path.IsPathRooted(url))
                        url = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), url);
                }
            }
            else
            {
                url = Path.Combine(Settings.WorkDir, url);
            }

            webBrowser1.Navigate(url);

#if DEBUG
#else
            kbh.Start();
            mh.Start();
#endif
        }

        private void Exit()
        {
            kbh.Stop();
            mh.Stop();

            Application.Exit();
        }

        private void Kh_OnKeyDownEvent(object sender, KeyEventArgs e)
        {
             Exit();
        }

        private void Mh_OnMouseActivity(object sender, MouseEventArgs e)
        {
             Exit();
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            webBrowser1.Visible = true;
        }
    }
}
