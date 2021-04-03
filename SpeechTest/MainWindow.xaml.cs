using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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

namespace SpeechTest
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            List<AudioDevice> device = new List<AudioDevice>() {
                new AudioDevice { Name = "1" },
                new AudioDevice { Name = "xxxxxxxxxxxxxxxxxxx" },
                new AudioDevice { Name = "aaaaaaaaaaaaaaa" },
            };

            choice.DataContext = device;
            HttpRequestMessage m = new HttpRequestMessage();
            m.Method = HttpMethod.Get;
            m.RequestUri = new Uri("https://www.test.com");
            m.Content = new StringContent("test");
            m.Headers.Add("User", "OK");
            //choice.ItemsSource = device;
        }

        public class AudioDevice
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
