using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using OBB;
using OBB.Properties;
using System.Net;
using HttpServer;
using HttpServer.HttpModules;
using System.Xml.Serialization;
using System.IO;

using GameSettings = System.Collections.Generic.Dictionary<string, OBB.GameSetting>;
using GameInfoTable = System.Collections.Generic.Dictionary<string, OBB.GameInfo>;
using Newtonsoft.Json;
using System.Diagnostics;

namespace OBBBox
{
    public partial class Form1 : Form
    {
        UdpNotify notify;
        HttpServer.HttpServer server;
        WebClient client;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            client = new WebClient();
            client.Encoding = Encoding.UTF8;

            var info = new OBBInfo();
            info.ip = IPAddress.Loopback.ToString();
            info.notifyPort = Settings.Default.notifyPort;
            info.servicePort = Settings.Default.servicePort;
            info.serviceRoot = Settings.Default.serviceRoot;
             
            if (String.IsNullOrWhiteSpace(info.serviceRoot))
                info.serviceRoot = Environment.CurrentDirectory;

            OBBContext.Current.Info = info;
            OBBContext.Current.Mode = Settings.Default.mode;

            server = new HttpServer.HttpServer();
            var fileModule = new FileModule("/", OBBContext.Current.Info.serviceRoot);
            var myModule = new MyModule();
            fileModule.AddDefaultMimeTypes();
            server.Add(myModule);
            server.Add(fileModule);
            server.Start(IPAddress.Any, OBBContext.Current.Info.servicePort);

            if (OBBContext.Current.IsMaster)
            {
                OBBContext.Current.MasterInfo = OBBContext.Current.Info;

                notifyIcon1.Icon = Resources.master;
                var port = IPAddress.HostToNetworkOrder(OBBContext.Current.Info.servicePort);
                byte[] portData = BitConverter.GetBytes(port);
                notify = new UdpNotify(OBBContext.Current.Info.notifyPort, portData);
            }
            else
            {
                OBBContext.Current.LoadGameConfig();

                notifyIcon1.Icon = Resources.slave;
                notify = new UdpNotify(OBBContext.Current.Info.notifyPort);
                notify.OnData += Notify_OnData;
            }

            this.Text = OBBContext.Current.Mode.ToString();
            notifyIcon1.Text = OBBContext.Current.Mode.ToString();
            refreshTimer.Start();
            button1.Enabled = OBBContext.Current.IsMaster;
        }

        private void Notify_OnData(System.Net.IPEndPoint endPoint, byte[] data)
        {
            try
            {
                if (OBBContext.Current.IsMaster)
                    return;

                if (OBBContext.Current.MasterInfo != null)
                    return;

                var info = new OBBInfo();
                info.ip = endPoint.Address.ToString();
                info.notifyPort = endPoint.Port;
                info.servicePort = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(data, 0));

                OBBContext.Current.MasterInfo = info;

                Register();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private void Register()
        {
            try
            {
                var registerUri = Utility.GetServiceUri(OBBContext.Current.MasterInfo.ip,
                                    OBBContext.Current.MasterInfo.servicePort, "register");
                var r = new RegisterInfo();
                r.servicePort = OBBContext.Current.Info.servicePort;
                r.game = OBBContext.Current.GameInfo;

                var postData = JsonConvert.SerializeObject(r);
                var result = JsonConvert.DeserializeObject<JsonResult>(client.UploadString(registerUri, postData));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private void refreshTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (OBBContext.Current.IsMaster)
                {
                    List<SlaveOBBInfo> bad = new List<SlaveOBBInfo>();
                    foreach (var slave in OBBContext.Current.SlaveInfo)
                    {
                        var queryUri = Utility.GetServiceUri(slave.ip, slave.servicePort, "query");
                        try
                        {
                            var json = client.DownloadString(queryUri);
                            slave.GameInfo = JsonConvert.DeserializeObject<GameInfoTable>(json);
                        }
                        catch (Exception ex)
                        {
                            bad.Add(slave);
                            Debug.WriteLine(ex);
                            continue;
                        }
                    }

                    bad.ForEach(g => OBBContext.Current.SlaveInfo.Remove(g));
                }
                else
                {
                    Register();

                    Bitmap screen = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);

                    using (Graphics g = Graphics.FromImage(screen))
                    {
                        g.CopyFromScreen(0, 0, 0, 0, screen.Size, CopyPixelOperation.SourceCopy);
                    }
                    screen.Save("screen.jpg");

                    OBBContext.Current.SaveGameData();

                    //var img = DxScreenCapture.CaptureScreen(ImageFileFormat.Jpg);
                    //img.Save("screen.jpg");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(String.Format("http://{0}:{1}/index.html", 
                OBBContext.Current.MasterInfo.ip, OBBContext.Current.MasterInfo.servicePort));
        }
    }
}
