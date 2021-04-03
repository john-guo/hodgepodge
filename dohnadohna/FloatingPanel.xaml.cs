using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace dohnadohna
{
    /// <summary>
    /// Interaction logic for FloatingPanel.xaml
    /// </summary>
    [ContentProperty("Items")]
    public partial class FloatingPanel : UserControl
    {
        private Point old;
        private bool move;

        public static readonly RoutedEvent ClosedEvent = EventManager.RegisterRoutedEvent("Closed", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(FloatingPanel));

        public event RoutedEventHandler Closed
        {
            add { AddHandler(ClosedEvent, value); }
            remove { RemoveHandler(ClosedEvent, value); }
        }

        public ItemCollection Items
        {
            get { return _itemsControl.Items; }
        }

        public FloatingPanel()
        {
            InitializeComponent();
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                move = true;
                old = e.GetPosition(canvas);
            }
        }

        private void popup_MouseMove(object sender, MouseEventArgs e)
        {
            if (!move)
                return;

            var cur = e.GetPosition(canvas);
            var diff = cur - old;
            Canvas.SetLeft(popup, Canvas.GetLeft(popup) + diff.X);
            Canvas.SetTop(popup, Canvas.GetTop(popup) + diff.Y);
            old = cur;
        }

        private void popup_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!move)
                return;
            move = false;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Hidden;
        }

        private void floatingPanel_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Visibility == Visibility.Visible)
            {
                var x = Canvas.GetLeft(popup);
                var y = Canvas.GetTop(popup);
                if (x >= double.Epsilon || y >= double.Epsilon)
                    return;
                var left = (ActualWidth - popup.ActualWidth) / 2;
                var top = (ActualHeight - popup.ActualHeight) / 2;

                Canvas.SetLeft(popup, left);
                Canvas.SetTop(popup, top);
            }
            else
            {
                RaiseEvent(new RoutedEventArgs(ClosedEvent));
            }
        }
    }
}
