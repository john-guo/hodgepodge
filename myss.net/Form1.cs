using Newtonsoft.Json;
using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.Specialized;
using System.Net.Sockets;
using System.Diagnostics;

namespace myss.net
{
    public partial class Form1 : Form
    {
        private List<listItem> itemList = new List<listItem>();

        public Form1()
        {
            InitializeComponent();
        }

        private void Import(Stream stream)
        {
            var reader = new StreamReader(stream);
            var json = new JsonTextReader(reader);
            var ser = new JsonSerializer();
            var obj = ser.Deserialize(json, typeof(List<ss>));
            List<ss> ssList = obj as List<ss>;

            itemList = ssList.ConvertAll(s =>
            {
                var item = new listItem();
                item.item = s;
                item.speed = 0;
                item.config = null;
                //item.config = Helper.GetConfigurationInformation("https://shadowsocks.net/media/" + s.qr);
                return item;
            });

            listBox1.Items.Clear();
            itemList.ForEach(i =>
            {
                if (i.item.online != 1)
                    return;
                listBox1.Items.Add(i);
            });
        }

        private void ImportUrl(string url)
        {
            var req = WebRequest.CreateHttp(url);
            var stream = req.GetResponse().GetResponseStream();

            Import(stream);
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = Application.StartupPath;

            ImportUrl("https://shadowsocks.net/api");
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var item = listBox1.SelectedItem as listItem;
            if (item != null && item.config == null)
            {
                var cursor = Cursor;
                Cursor = Cursors.WaitCursor;
                //label2.Text = String.Format("Proxy probing...");
                try
                {
                    item.config = Helper.GetConfigurationInformation("https://shadowsocks.net/media/" + item.item.qr);
                }
                catch
                {
                    item.item.online = 0;
                }
                //label2.Text = String.Format("");
                Cursor = cursor;
            }
        }

        async private void button2_Click(object sender, EventArgs e)
        {
            var cursor = Cursor;
            Cursor = Cursors.WaitCursor;

            listBox1.Items.Clear();
            listBox1.Items.Add("Testing...");

            for (int j = 0; j < itemList.Count; ++j)
            {
                await Task.Run(() =>
                {
                    var i = itemList[j];

                    if (i.item.online != 1)
                        return;

                    if (i.config == null)
                    {
                        try 
                        {
                            i.config = Helper.GetConfigurationInformation("https://shadowsocks.net/media/" + i.item.qr);
                        }
                        catch
                        {
                            i.item.online = 0;
                            return;
                        }
                    }

                    var client = new TcpClient();
                    try
                    {
                        var watcher = Stopwatch.StartNew();
                        client.Connect(i.config["Server"], int.Parse(i.config["Port"]));
                        watcher.Stop();
                        i.speed = watcher.ElapsedMilliseconds;
                        client.Close();
                    }
                    catch
                    {
                        i.item.online = 0;
                    }
                });

                listBox1.Items.Clear();
                listBox1.Items.Add(String.Format("Testing... {0}/{1}", j + 1, itemList.Count));
            }

            listBox1.Items.Clear();
            itemList.Where(i => i.item.online == 1)
                .OrderBy(i => i.speed)
                .ToList()
                .ForEach(i => listBox1.Items.Add(i));

            Cursor = cursor;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Helper.Port = int.Parse(textBox1.Text);

            var item = listBox1.SelectedItem as listItem;
            if (item != null)
            {
                if (item.config == null)
                {
                    try
                    {
                        item.config = Helper.GetConfigurationInformation("https://shadowsocks.net/media/" + item.item.qr);
                    }
                    catch
                    {
                        item.item.online = 0;
                        return;
                    }
                }
                Helper.StartProxy(item.config);
                label2.Text = String.Format("Proxy is running. {0} {1}", item.item.name, item.item.country); 
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Helper.StopProxy();
        }

        private void importFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var result = openFileDialog1.ShowDialog();
            if (result != DialogResult.OK)
                return;

            Import(openFileDialog1.OpenFile());
        }

        private void importUrlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var form = new Form();
            var txtbox = new TextBox();
            txtbox.Width = 100;
            form.Controls.Add(txtbox);
            form.ShowDialog();
        }
    }

    class listItem
    {
        public ss item;
        public long speed;
        public NameValueCollection config;
        public override string ToString()
        {
            return String.Format("{0} {1} {2} {3}", item.id, item.name, item.country, speed);
        }
    }

    struct ss
    {
        public string qr;
        public string name;
        public string country;
        public string api;
        public int online;
        public int id;
        public override string ToString()
        {
            return base.ToString();
        }
    }
}
