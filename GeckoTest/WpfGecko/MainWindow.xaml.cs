using Gecko;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Gecko.Utils;
using System.Windows.Interop;
using Gecko.DOM;
using Newtonsoft.Json;

namespace WpfGecko
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer timer;
        Window1 model;
        GeckoImageElement canvas;

        public MainWindow()
        {
            InitializeComponent();
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(1000 / 24)
            };
            timer.Tick += Timer_Tick;
            model = new Window1();
            Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            timer.Stop();
            Application.Current.Shutdown();
        }

        private BitmapSource extractBitmapSource()
        {
            var data = canvas.Src;
            if (string.IsNullOrWhiteSpace(data))
                return null;
            var bytes = Convert.FromBase64String(data.Substring("data:image/png;base64,".Length));
            using (var ms = new System.IO.MemoryStream(bytes))
            {
                var decoder = new PngBitmapDecoder(ms, BitmapCreateOptions.IgnoreImageCache | BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                return decoder.Frames[0];
            }
        }

        private void WinAppCallback(string json)
        {
            var point = JsonConvert.DeserializeAnonymousType(json, new { x = 0, y = 0 });
            model.Left += point.x;
            model.Top += point.y;

            if (model.Left >= SystemParameters.PrimaryScreenWidth)
                model.Left = -model.Width;
            else if (model.Left <= -model.Width)
                model.Left = SystemParameters.PrimaryScreenWidth;
            if (model.Top >= SystemParameters.PrimaryScreenHeight)
                model.Top = -model.Height;
            else if (model.Top <= -model.Height)
                model.Top = SystemParameters.PrimaryScreenHeight;

        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (canvas == null)
            {
                canvas = browser.Browser.Document.GetElementById("__appview") as GeckoImageElement;

                if (canvas == null)
                {
                    return;
                }

                model.ModelWidth = int.TryParse(canvas.GetAttribute("width"), out int width) ? width : (int?)null;
                model.ModelHeight = int.TryParse(canvas.GetAttribute("height"), out int height) ? height : (int?)null;
                model.Show();
            }

            var bs = extractBitmapSource();
            if (bs == null)
                return;
            if (!model.ModelWidth.HasValue)
            {
                model.ModelWidth = bs.Width;
            }
            if (!model.ModelHeight.HasValue)
            {
                model.ModelHeight = bs.Height;
            }

            var mouse = System.Windows.Forms.Control.MousePosition;
            var point = PresentationSource.FromVisual(this).CompositionTarget.TransformFromDevice.Transform(new Point(mouse.X, mouse.Y));
            var left = (int)model.Left;
            var top = (int)model.Top;
            var w = (int)model.Width;
            var h = (int)model.Height;

            var x = point.X - left;
            var y = point.Y - top;

            if (x < 0)
                x = 0;
            if (y < 0)
                y = 0;
            if (x >= w)
                x = w - 1;
            if (y >= h)
                y = h - 1;

            using (var context = new AutoJSContext(browser.Browser.Window))
            {
                context.EvaluateScript($"WinApp.mousemove({{x:{x}, y:{y}}});");
            }

            model.bgImg.Source = bs;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            browser.Browser.EnableDefaultFullscreen();
            browser.Browser.DOMContentLoaded += Browser_DOMContentLoaded;
            var url = Properties.Settings.Default.Url;
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
            {
                var location = System.Reflection.Assembly.GetExecutingAssembly().Location;
                url = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(location), url);
            }

            browser.Browser.Navigate(url);
        }


        private void Browser_DOMContentLoaded(object sender, DomEventArgs e)
        {
            browser.Browser.AddMessageEventListener("WinAppCallback", args => WinAppCallback(args));
            Hide();
            timer.Start();
        }
    }
}
