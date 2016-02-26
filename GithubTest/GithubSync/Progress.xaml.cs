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

namespace GithubSync
{
    /// <summary>
    /// Interaction logic for Progress.xaml
    /// </summary>
    public partial class Progress : Window
    {
        Window _parent;

        public Progress(Window parent)
        {
            _parent = parent;
            DataContext = parent;
            InitializeComponent();

            Left = _parent.Left;
            Top = _parent.Top;
        }

        private void ProgressBar_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Left = _parent.Left;
            Top = _parent.Top;
        }
    }
}
