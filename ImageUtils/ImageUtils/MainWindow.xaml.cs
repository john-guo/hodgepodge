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
            DrawingGroup imageDrawings = new DrawingGroup();

            int row = 10, col = 16;

            MeasureImage(new BitmapImage(new Uri("(1).jpg", UriKind.Relative)), row, col);

            double left = 0;
            for (int j = 0; j < row; ++j)
            {
                left = 0; 
                for (int i = 0; i < col; ++i)
                {
                    var filename = $"({j * col + i + 1}).jpg";
                    var image = new BitmapImage(new Uri(filename, UriKind.Relative));
                    var drawing = new ImageDrawing
                    {
                        Rect = new Rect(left, j * image.Height , image.Width, image.Height),
                        ImageSource = image
                    };
                    left += image.Width;
                    imageDrawings.Children.Add(drawing);
                }
            }

            var drawingVisual = new DrawingVisual(); 

            using (var drawingContext = drawingVisual.RenderOpen())
            {
                drawingContext.DrawDrawing(imageDrawings);
            }

            var bitmap = new RenderTargetBitmap((int)outWidth, (int)outHeight, outDpiX, outDpiY, PixelFormats.Pbgra32);
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
