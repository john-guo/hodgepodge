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

namespace GithubSync
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Login login;

        public MainWindow()
        {
            AppInfo.Instance.Value.IsClosing = false;

            Crypto.LoadKey();

            InitializeComponent();

            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.username))
                AppInfo.Instance.Value.Username = Crypto.Decrypt(Properties.Settings.Default.username);
            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.password))
                AppInfo.Instance.Value.Password = Crypto.Decrypt(Properties.Settings.Default.password);

            if (string.IsNullOrWhiteSpace(AppInfo.Instance.Value.Username))
            {
                login = new Login();
                login.ShowDialog();
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (login == null)
            {
                login = new Login();
            }

            login.ShowDialog();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            AppInfo.Instance.Value.IsClosing = true;

            if (login != null)
                login.Close();
        }
    }
}
