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
using System.IO;

namespace WpfGecko
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public enum RenderMode
        {
            Auto,
            Manual,
        } 

        public class CanvasWindow
        {
            public Window1 Window { get; set; }
            public GeckoCanvasElement Canvas { get; set; }
        }

        public class Settings
        {
            public bool Debug { get; set; }
            public int FPS { get; set; }
        }

        public RenderMode Mode { get; private set; }
        DispatcherTimer timer;
        Dictionary<string, CanvasWindow> windows;
        Dictionary<string, Action<string, string>> commands;
        Settings config;

        public MainWindow()
        {
            InitializeComponent();

            config = new Settings()
            {
                Debug = Properties.Settings.Default.Debug,
                FPS = Properties.Settings.Default.FPS,
            };

            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(1000 / config.FPS)
            };
            timer.Tick += Timer_Tick;
            windows = new Dictionary<string, CanvasWindow>();
            commands = new Dictionary<string, Action<string, string>>();
            Mode = RenderMode.Manual;
            RegisterCommand();
            Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            timer.Stop();
            Application.Current.Shutdown();
        }

        private BitmapSource ExtractBitmapSource(string data)
        {
            if (string.IsNullOrWhiteSpace(data))
                return null;
            var bytes = Convert.FromBase64String(data.Substring("data:image/png;base64,".Length));
            using (var ms = new System.IO.MemoryStream(bytes))
            {
                var decoder = new PngBitmapDecoder(ms, BitmapCreateOptions.IgnoreImageCache | BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                return decoder.Frames[0];
            }
        }

        private void RegisterCommand()
        {
            commands.Add("config", ActionConfig);
            commands.Add("create", ActionCreate);
            commands.Add("render", ActionRender);
            commands.Add("move", ActionMove);
            commands.Add("setWidth", ActionSetWidth);
            commands.Add("setHeight", ActionSetHeight);
            commands.Add("resize", ActionResize);
            commands.Add("setOpacity", ActionSetOpcity);
        }

        private Window1 getModel(string id)
        {
            return windows[id]?.Window;
        }

        private void ActionConfig(string id, string parameters)
        {
            dynamic settings = JsonConvert.DeserializeObject(parameters);
            config.Debug = settings.debug ?? Properties.Settings.Default.Debug;

            var prevFPS = config.FPS;
            config.FPS = settings.fps ?? Properties.Settings.Default.FPS;
            if (prevFPS != config.FPS)
            {
                timer.Interval = TimeSpan.FromMilliseconds(1000 / config.FPS);
            }
        }

        private void ActionCreate(string id, string parameters)
        {
            if (windows.ContainsKey(id))
            {
                windows[id].Canvas = null;
                windows[id].Window.Clear();
                return;
            }
            windows[id] = new CanvasWindow()
            {
                Window = new Window1(id),
                Canvas = null,
            };
        }

        private void ActionRender(string id, string parameters)
        {
            if (!windows.TryGetValue(id, out CanvasWindow window))
                return;

            var img = JsonConvert.DeserializeAnonymousType(parameters, new { src = "" });
            RenderModel(window, img.src);
        }

        private void ActionMove(string id, string parameters)
        {
            var model = getModel(id);
            if (model == null)
                return;

            var point = JsonConvert.DeserializeAnonymousType(parameters, new {x=0, y=0});
            model.Left += point.x;
            model.Top += point.y;
        }

        private void ActionSetWidth(string id, string parameters)
        {
            var model = getModel(id);
            if (model == null)
                return;

            var size = JsonConvert.DeserializeAnonymousType(parameters, new { w = 0, h = 0 });
            model.ModelWidth = size.w;
        }

        private void ActionSetHeight(string id, string parameters)
        {
            var model = getModel(id);
            if (model == null)
                return;

            var size = JsonConvert.DeserializeAnonymousType(parameters, new { w = 0, h = 0 });
            model.ModelHeight = size.h;
        }

        private void ActionResize(string id, string parameters)
        {
            var model = getModel(id);
            if (model == null)
                return;

            var size = JsonConvert.DeserializeAnonymousType(parameters, new { w = 0, h = 0 });
            model.ModelWidth = size.w;
            model.ModelHeight = size.h;
        }

        private void ActionSetOpcity(string id, string parameters)
        {
            var model = getModel(id);
            if (model == null)
                return;

            var args = JsonConvert.DeserializeAnonymousType(parameters, new { opacity = .0d });
            model.Opacity = args.opacity;
        }

        private void WinAppCallback(string json)
        {
            var command = JsonConvert.DeserializeAnonymousType(json, new { id="", command="", parameters="" });
            if (!commands.TryGetValue(command.command, out Action<string, string> action))
                return;

            action(command.id, command.parameters);
        }

        private void InformationNotify(Window1 win)
        {
            using (var context = new AutoJSContext(browser.Browser.Window))
            {
                var args = $"{{id:\"{win.Id}\", left:{win.Left}, top:{win.Top}, width:{win.Width}, height:{win.Height},opacity:{win.Opacity}}}";
                context.EvaluateScript($"WinApp._windowNotify({args});");
            }
        }

        private void InformationNotify()
        {
            var mouse = System.Windows.Forms.Control.MousePosition;
            var point = PresentationSource.FromVisual(this).CompositionTarget.TransformFromDevice.Transform(new Point(mouse.X, mouse.Y));
            var x = point.X;
            var y = point.Y;

            using (var context = new AutoJSContext(browser.Browser.Window))
            {
                foreach (var pair in windows)
                {
                    var win = pair.Value.Window;
                    if (!win.IsLoaded)
                        continue;
                    var args = $"{{id:\"{win.Id}\", left:{win.Left}, top:{win.Top}, width:{win.Width}, height:{win.Height},opacity:{win.Opacity}}}";
                    context.EvaluateScript($"WinApp._windowNotify({args});");
                }

                context.EvaluateScript($"WinApp._onMouseMove({{x:{x}, y:{y}}});");
            }
        }

        private void RenderModel(CanvasWindow window)
        {
            if (window.Canvas == null)
            {
                window.Canvas = browser.Browser.Document.SelectFirst(Properties.Settings.Default.CanvasXPath) as GeckoCanvasElement;
                if (window.Canvas == null)
                {
                    if (!config.Debug)
                    {
                        MessageBox.Show("Doesn't contain any canvas!");
                        Close();
                    }
                    return;
                }
            }

            RenderModel(window, window.Canvas.ToDataURL("image/png"));
        }

        private void RenderModel(CanvasWindow window, string src)
        {
            if (!window.Window.IsShow)
            {
                window.Window.Show();
                if (Mode == RenderMode.Manual)
                {
                    InformationNotify(window.Window);
                }
            }

            var bs = ExtractBitmapSource(src);
            if (bs == null)
                return;
            if (!window.Window.ModelWidth.HasValue)
            {
                window.Window.ModelWidth = bs.Width;
            }
            if (!window.Window.ModelHeight.HasValue)
            {
                window.Window.ModelHeight = bs.Height;
            }

            window.Window.bgImg.Source = bs;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (Mode == RenderMode.Manual)
            {
                InformationNotify();
                return;
            }

            foreach (var pair in windows)
            {
                RenderModel(pair.Value);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            var url = Properties.Settings.Default.Url;
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
            {
                var location = System.Reflection.Assembly.GetExecutingAssembly().Location;
                url = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(location), url);
            }

            browser.Browser.EnableDefaultFullscreen();
            browser.Browser.NavigationError += Browser_NavigationError;
            browser.Browser.DocumentCompleted += Browser_DocumentCompleted;
            browser.Browser.ConsoleMessage += Browser_ConsoleMessage;
            browser.Browser.AddMessageEventListener("WinAppCallback", args => WinAppCallback(args));
            browser.Browser.Navigate(url);
            browser.Visibility = Visibility.Collapsed;
        }

        private void Browser_NavigationError(object sender, Gecko.Events.GeckoNavigationErrorEventArgs e)
        {
            if (!config.Debug)
            {
                MessageBox.Show($"Navigation Error {e.ErrorCode:X}");
                Application.Current.Shutdown();
            }
        }

        private void ProbeRenderMode(GeckoWindow window)
        {
            using (var context = new AutoJSContext(window))
            {
                context.EvaluateScript("WinApp !== undefined", out string result);
                if (bool.TryParse(result, out bool bresult) && bresult)
                {
                    Mode = RenderMode.Manual;
                }
                else
                {
                    Mode = RenderMode.Auto;
                }
            }

            if (Mode == RenderMode.Auto)
            {
                ActionCreate("__auto__", null);
            }
        }

        private void Browser_DocumentCompleted(object sender, Gecko.Events.GeckoDocumentCompletedEventArgs e)
        {
            browser.Visibility = Visibility.Visible;
            ProbeRenderMode(e.Window);

            if (!config.Debug)
            {
                if (Mode == RenderMode.Auto)
                {
                    Height = MinHeight;
                }
                Hide();
            }
            else
            {
                ShowInTaskbar = true;
                WindowStyle = WindowStyle.SingleBorderWindow;
            }

            timer.Start();
        }

        private void Browser_ConsoleMessage(object sender, ConsoleMessageEventArgs e)
        {
            using (var sw = File.AppendText("console.log"))
            {
                sw.WriteLine(e.Message);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUrl.Text))
                return;
            timer.Stop();
            browser.Browser.Navigate(txtUrl.Text);
        }

        private void TxtUrl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Button_Click(null, null);
                e.Handled = true;
            }
        }
    }
}
