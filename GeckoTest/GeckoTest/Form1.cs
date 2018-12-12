using Gecko;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace GeckoTest
{
    public partial class Form1 : Form
    {
        GeckoWebBrowser geckoWebBrowser;

        public Form1()
        {
            InitializeComponent();
            FormBorderStyle = FormBorderStyle.None;
            //WindowState = FormWindowState.Maximized;

#if !DEBUG
            TopMost = true;
#endif
            //var cat = CategoryManager.Categories;
            //var js = cat.First();
            //var entries =  CategoryManager.GetCategoryEntries(js);
            //CategoryManager.AddCategoryEntry("JavaScript global constructor", "my", "test", false, true);



            geckoWebBrowser = new GeckoWebBrowser
            {
                Dock = DockStyle.Fill,
                NoDefaultContextMenu = true,
            };
            //geckoWebBrowser.CreateWindow += GeckoWebBrowser_CreateWindow;
            //geckoWebBrowser.DOMContentLoaded += GeckoWebBrowser_DOMContentLoaded;
            geckoWebBrowser.Load += GeckoWebBrowser_Load;

            Controls.Add(geckoWebBrowser);
            geckoWebBrowser.EnableDefaultFullscreen();
            var url = Properties.Settings.Default.Url;
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
            {
                var location = System.Reflection.Assembly.GetExecutingAssembly().Location;
                url = Path.Combine(Path.GetDirectoryName(location), url);
            }

            geckoWebBrowser.Navigate(url);
        }

        private void GeckoWebBrowser_Load(object sender, DomEventArgs e)
        {
            WE.Make(Handle);
            WindowState = FormWindowState.Maximized;
        }

        /*
        private void test(string s)
        {
            using (var context = new AutoJSContext(geckoWebBrowser.Window))
            {
                context.EvaluateScript($"test('{s}');");
            }
        }

        private void GeckoWebBrowser_DOMContentLoaded(object sender, DomEventArgs e)
        {

            //geckoWebBrowser.AddMessageEventListener("callback", s => test(s));

            //using (var context = new AutoJSContext(geckoWebBrowser.Window))
            //{
            //    context.EvaluateScript("fireEvent('callback', 'test');");
            //}
        }
        */

        private void GeckoWebBrowser_CreateWindow(object sender, GeckoCreateWindowEventArgs e)
        {
            geckoWebBrowser.Navigate(e.Uri);
            e.Cancel = true;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var behindWindow = WinNative.SetParent(Handle, IntPtr.Zero);
            if (behindWindow != IntPtr.Zero)
            {
                WinNative.SendMessage(behindWindow, WinNative.WM_CLOSE, 0, 0);
                //WinNative.RedrawWindow(behindWindow, IntPtr.Zero, IntPtr.Zero, WinNative.RedrawWindowFlags.UpdateNow | WinNative.RedrawWindowFlags.Invalidate | WinNative.RedrawWindowFlags.Erase);
            }
            Close();
        }
    }
}
