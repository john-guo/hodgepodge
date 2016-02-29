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
        Progress progress;
        Github git;

        public MainWindow()
        {
            AppInfo.Instance.Value.IsClosing = false;

            Crypto.LoadKey();

            progress = new Progress(this);

            InitializeComponent();

            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.username))
                AppInfo.Instance.Value.Username = Crypto.Decrypt(Properties.Settings.Default.username);
            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.password))
                AppInfo.Instance.Value.Password = Crypto.Decrypt(Properties.Settings.Default.password);

            login = new Login();

            if (string.IsNullOrWhiteSpace(AppInfo.Instance.Value.Username))
            {
                login.ShowDialog();
            }

            git = new Github(AppInfo.Instance.Value.Username, AppInfo.Instance.Value.Password);

            Initialize();
        }

        private async void Initialize()
        {
            try
            {
                progress.Show();
                lvRespositoris.ItemsSource = await git.GetRepositoris();
                progress.Hide();
            }
            catch (Octokit.AuthorizationException)
            {
                MessageBox.Show("Invalid Username or Password.");

                login.ShowDialog();

                Initialize();
            }
        }


        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            login.ShowDialog();
            git.SetCredentials(AppInfo.Instance.Value.Username, AppInfo.Instance.Value.Password);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            AppInfo.Instance.Value.IsClosing = true;

            login.Close();
            progress.Close();
        }

        private async void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            var rep = lvRespositoris.SelectedValue as Octokit.Repository;
            if (rep == null)
                return;

            bool result = await git.RemoveRepository(rep.Name);
            if (result)
            {
                lvRespositoris.ItemsSource = await git.GetRepositoris();
            }
        }

        private async void btNew_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbName.Text))
                return;

            bool result = await git.NewRepository(tbName.Text);
            if (result)
            {
                lvRespositoris.ItemsSource = await git.GetRepositoris();
            }
        }

        private async void lvRespositoris_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = e.AddedItems.OfType<Octokit.Repository>().FirstOrDefault();
            if (selected == null)
                return;

            lvFiles.ItemsSource = null;

            git.Mapping(selected.Name, Environment.CurrentDirectory);

            progress.Show();
            //var files = await git.GetFiles(selected.Name);
            //var ret = await git.InitializeContants(selected.Name);
            var files = await git.GetContants(selected.Name);
            progress.Hide();
            //if (files == null || files.Count == 0)
            //return;
            //if (!ret)
            //    return;
            if (files == null || files.Count == 0)
                return;

            //lvFiles.ItemsSource = files.First().Value;
            //lvFiles.ItemsSource = git[selected.Name].Contents;
            lvFiles.ItemsSource = files;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            progress.Owner = this;
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {

        }
    }
}
