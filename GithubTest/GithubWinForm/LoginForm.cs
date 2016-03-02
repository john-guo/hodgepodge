using GithubSync;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GithubWinForm
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();

            tbUsername.Text = AppInfo.Instance.Value.Username;
            pbPassword.Text = AppInfo.Instance.Value.Password;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Hide();
        }

        private void LoginForm_Shown(object sender, EventArgs e)
        {

        }

        private void LoginForm_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(tbUsername.Text))
                AppInfo.Instance.Value.Username = tbUsername.Text;
            if (!string.IsNullOrWhiteSpace(pbPassword.Text))
                AppInfo.Instance.Value.Password = pbPassword.Text;

            Properties.Settings.Default.username = Crypto.Encrypt(AppInfo.Instance.Value.Username);
            Properties.Settings.Default.password = Crypto.Encrypt(AppInfo.Instance.Value.Password);

            Properties.Settings.Default.Save();

            Hide();
        }
    }
}
