using System;
using System.Collections.Generic;
using System.IO;
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

namespace dohnadohna
{
    /// <summary>
    /// Interaction logic for Image.xaml
    /// </summary>
    public partial class Image : Window
    {
        bool _move;
        Point _old;
        string _filename;

        public Image(string filename)
        {
            InitializeComponent();
            _filename = filename;
            img.Source = new BitmapImage(new Uri(filename));
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)canvas.RenderSize.Width, (int)canvas.RenderSize.Height, 96d, 96d, System.Windows.Media.PixelFormats.Default);
            rtb.Render(canvas);

            BitmapEncoder pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(rtb));

            using (var fs = File.OpenWrite(_filename))
            {
                pngEncoder.Save(fs);
            }
        }

        private void img_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _move = true;
                _old = e.GetPosition(canvas);
            }
        }

        private void img_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!_move)
                return;

            var cur = e.GetPosition(canvas);
            var diff = cur - _old;

            Canvas.SetLeft(img, Canvas.GetLeft(img) + diff.X);
            Canvas.SetTop(img, Canvas.GetTop(img) + diff.Y);

            _old = cur;
        }

        private void img_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!_move)
                return;
            _move = false;
        }
    }
}
