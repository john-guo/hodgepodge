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
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();

            tbUsername.Text = AppInfo.Instance.Value.Username;
            pbPassword.Password = AppInfo.Instance.Value.Password;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(tbUsername.Text))
                AppInfo.Instance.Value.Username = tbUsername.Text;
            if (!string.IsNullOrWhiteSpace(pbPassword.Password))
                AppInfo.Instance.Value.Password = pbPassword.Password;

            Properties.Settings.Default.username = Crypto.Encrypt(AppInfo.Instance.Value.Username);
            Properties.Settings.Default.password = Crypto.Encrypt(AppInfo.Instance.Value.Password);

            Properties.Settings.Default.Save();
            Hide();
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = !AppInfo.Instance.Value.IsClosing;

            if (e.Cancel)
                Hide();
        }
    }
}
