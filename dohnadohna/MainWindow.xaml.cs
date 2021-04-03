using Microsoft.Win32;
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

namespace dohnadohna
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog()
            {
                InitialDirectory = StorageService.TargetDirectory,
                Filter = "文本文件|*.txt"
            };
            if (dialog.ShowDialog() == true)
            {
                var model = StorageService.Load(dialog.FileName);
                if (model is StaffModel staffModel)
                {
                    var page = staff.Content as Staff;
                    var viewModel = page.DataContext as StaffViewModel;
                    viewModel.Data = staffModel;
                    tab.SelectedIndex = 0;
                }
                else if (model is CustomerModel customerModel)
                {
                    var page = customer.Content as Customer;
                    var viewModel = page.DataContext as CustomerViewModel;
                    viewModel.Data = customerModel;
                    tab.SelectedIndex = 1;
                }
            }
        }
    }
}
