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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Reflection;

namespace ImageUtils
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        double outWidth, outHeight;
        double outDpiX = 0, outDpiY = 0;
        private void MeasureImage(BitmapImage image, int row, int col)
        {
            outWidth = image.PixelWidth * col;
            outHeight = image.PixelHeight * row;
            outDpiX = image.DpiX;
            outDpiY = image.DpiY;
        }

        private void OpenFiles_Click(object sender, RoutedEventArgs e)
        {
            var tfd = PresentationSource.FromVisual(Application.Current.MainWindow).CompositionTarget.TransformFromDevice;
            var ttd = PresentationSource.FromVisual(Application.Current.MainWindow).CompositionTarget.TransformToDevice;

            var dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
            var dpiYProperty = typeof(SystemParameters).GetProperty("Dpi", BindingFlags.NonPublic | BindingFlags.Static);

            var Device_dpiX = (int)dpiXProperty.GetValue(null, null);
            var Device_dpiY = (int)dpiYProperty.GetValue(null, null);

            var Logical_DpiX = Device_dpiX * tfd.M11;
            var Logical_DpiY = Device_dpiY * tfd.M22;

            DrawingGroup imageDrawings = new DrawingGroup();
            int row = 7, col = 14;
            
            MeasureImage(new BitmapImage(new Uri("合成 1_00001.jpg", UriKind.Relative)), row, col);
            double left = 0;
            for (int j = 0; j < row; ++j)
            {
                left = 0; 
                for (int i = 0; i < col; ++i)
                {
                    var filename = "合成 1_" + $"{j * col + i}".PadLeft(5, '0') + ".jpg";
                    var image = new BitmapImage(new Uri(filename, UriKind.Relative));
                    var drawing = new ImageDrawing
                    {
                        Rect = new Rect(left, j * image.Height / 2 , image.Width / 2, image.Height / 2),
                        ImageSource = image
                    };
                    left += image.Width / 2;
                    imageDrawings.Children.Add(drawing);
                }
            }

            var drawingVisual = new DrawingVisual(); 

            using (var drawingContext = drawingVisual.RenderOpen())
            {
                drawingContext.DrawDrawing(imageDrawings);
            }

            var bitmap = new RenderTargetBitmap((int)outWidth  / 2, (int)outHeight / 2, outDpiX, outDpiY, PixelFormats.Pbgra32);
            bitmap.Render(drawingVisual);

            var encoder = new JpegBitmapEncoder() { QualityLevel = 25 };
            encoder.Frames.Add(BitmapFrame.Create(bitmap));

            using (var stream = new FileStream("out.jpg", FileMode.Create))
            {
                encoder.Save(stream);
            }
        }
    }
}
