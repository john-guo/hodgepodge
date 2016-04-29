using GithubSync;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GithubWinForm
{
    public partial class Form1 : Form
    {
        LoginForm login;
        Github git;

        public Form1()
        {
            AppInfo.Instance.Value.IsClosing = false;
            Crypto.LoadKey();

            login = new LoginForm();

            InitializeComponent();

            

            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.username))
                AppInfo.Instance.Value.Username = Crypto.Decrypt(Properties.Settings.Default.username);
            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.password))
                AppInfo.Instance.Value.Password = Crypto.Decrypt(Properties.Settings.Default.password);

            if (string.IsNullOrWhiteSpace(AppInfo.Instance.Value.Username))
            {
                login.ShowDialog();
            }

            git = new Github(AppInfo.Instance.Value.Username, AppInfo.Instance.Value.Password);

            Initialize();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            login.Close();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            login.ShowDialog();
        }

        private async void Initialize()
        {
            try
            {
                ShowProgress();

                git.WorkSpace = Properties.Settings.Default.root;

                var list = await git.GetRepositoris();

                UIInvoke(() => {
                    lvRespositoris.Items.AddRange(list.Select(i => new ListViewItem(i.Name)).ToArray());
                    toolStripTextBox1.Text = git.WorkSpace;
                });

                
                HideProgress();
            }
            catch (Octokit.AuthorizationException)
            {
                MessageBox.Show("Invalid Username or Password.");

                login.ShowDialog();

                Initialize();
            }
        }


        private void UIInvoke(Action func)
        {
            if (InvokeRequired)
            {
                Invoke(func);
            }
            else
            {
                func();
            }
        }

        private void ShowProgress()
        {
            UIInvoke(() =>
            {
                progressBar1.Style = ProgressBarStyle.Marquee;
            });
        }

        private void HideProgress()
        {
            UIInvoke(() =>
            {
                progressBar1.Style = ProgressBarStyle.Blocks;
            });
        }

        private void AddNodes(TreeNode root, IList<GithubItem> items)
        {
            if (items == null)
                return;

            foreach (var item in items)
            {
                var node = new TreeNode(item.IsFile ? item.Name : item.Name + "/");
                node.Tag = item;
                root.Nodes.Add(node);
                AddNodes(node, item.Items);
            }
        }

        private async void lvRespositoris_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lvRespositoris.SelectedItems.Count <= 0)
                return;
            var name = lvRespositoris.SelectedItems[0].Text;

            treeView1.TopNode.Nodes.Clear();
            treeView1.TopNode.Tag = name;

            ShowProgress();
            do
            {
                try
                {
                    await git.SyncAll(name);

                    UIInvoke(() =>
                    {
                        AddNodes(treeView1.TopNode, git[name].Items);
                        treeView1.ExpandAll();
                    });
                }
                catch
                {

                }
            } while (false);
            HideProgress();
        }

        private async void openInExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lvRespositoris.SelectedItems.Count <= 0)
                return;
            var name = lvRespositoris.SelectedItems[0].Text;

            await Task.Yield();
            Process.Start(new ProcessStartInfo(git.GetPath(name))
            {
                UseShellExecute = true
            });
        }

        private static Task IdleYield()
        {
            var idleTcs = new TaskCompletionSource<bool>();
            // subscribe to Application.Idle
            EventHandler handler = null;
            handler = (s, e) =>
            {
                Application.Idle -= handler;
                idleTcs.SetResult(true);
            };
            Application.Idle += handler;
            return idleTcs.Task;
        }

        private async Task WaitUntil<T>(Func<T> func, Func<T, bool> predicate)
        {
            T o = func();
            while (!predicate(o))
                await IdleYield();
        }

        private async void syncFromGithubToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lvRespositoris.SelectedItems.Count <= 0)
                return;
            var name = lvRespositoris.SelectedItems[0].Text;

            ShowProgress();
            try
            {
                await git.SyncAll(name);
                var scheduler = git.SaveContents(name);
                do
                {
                    await WaitUntil(() => scheduler, o => o.IsEmpty);
                    if (!scheduler.HasFailedJobs)
                        break;
                    scheduler.Retry();
                } while (true);
            }
            catch
            {

            }
            HideProgress();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            var result = folderBrowserDialog1.ShowDialog();
            if (result != DialogResult.OK)
                return;

            if (string.IsNullOrWhiteSpace(folderBrowserDialog1.SelectedPath))
                return;

            git.WorkSpace = folderBrowserDialog1.SelectedPath;
            toolStripTextBox1.Text = git.WorkSpace;

            Properties.Settings.Default.root = git.WorkSpace;
            Properties.Settings.Default.Save();
        }

        private void toolStripTextBox1_TextChanged(object sender, EventArgs e)
        {
            if (git.WorkSpace == toolStripTextBox1.Text)
                return;

            git.WorkSpace = toolStripTextBox1.Text;
            Properties.Settings.Default.root = git.WorkSpace;
            Properties.Settings.Default.Save();
        }
    }
}
