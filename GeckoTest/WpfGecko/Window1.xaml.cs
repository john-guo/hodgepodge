using Gecko;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace WpfGecko
{
    /// <summary>
    /// Window1.xaml 的交互逻辑
    /// </summary>
    public partial class Window1 : Window
    {
        public string Id { get; private set; }
        public bool IsShow { get; private set; }
        public Window1(string id)
        {
            Id = id;
            IsShow = false;
            InitializeComponent();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Application.Current.MainWindow?.Close();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private double? _width;
        public double? ModelWidth
        {
            get
            {
                return _width;
            }
            set
            {
                if (value.HasValue)
                {
                    Width = value.Value;
                }

                _width = value;
            }
        }

        private double? _height;
        public double? ModelHeight
        {
            get
            {
                return _height;
            }
            set
            {
                if (value.HasValue)
                {
                    Height = value.Value;
                }

                _height = value;
            }
        }

        public void Clear()
        {
            ModelWidth = null;
            ModelHeight = null;
        }

        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var main = Application.Current.MainWindow as MainWindow;

            if (main == null || main.Mode == MainWindow.RenderMode.Auto)
                return;

            using (var context = new AutoJSContext(main.browser.Browser.Window))
            {
                context.EvaluateScript($"WinApp._onDoubleClick(\"{this.Id}\");");
            }
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            Window_MouseDoubleClick(null, null);
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            IsShow = true;
        }
    }
}
