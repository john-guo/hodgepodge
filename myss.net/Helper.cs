using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ZXing;

namespace myss.net
{
    static class Helper
    {
        public static int Port = 1080;
        public static Process Proxy = null;
        static string ConfigurationStringBuilder(NameValueCollection configuration)
        {
            string str = "";
            return (((((((str + "Shadowsocks/bin/sslocal") + " -s " + configuration["Server"]) + " -p " + configuration["Port"]) + " -l " + Port) + " -k " + configuration["Password"]) + " -m " + configuration["EncryptionMethod"]) + " -b 0.0.0.0");
        }

        static string DecodeBarcode(Bitmap img)
        {
            string text;
            IBarcodeReader reader = new BarcodeReader();
            try
            {
                text = reader.Decode(img).Text;
            }
            catch (Exception)
            {
                throw new InvalidOperationException("BARCODE");
            }
            return text;
        }

        public static NameValueCollection GetConfigurationInformation(string QRCodeURI)
        {
            HttpWebRequest request = WebRequest.CreateHttp(QRCodeURI);
            request.Proxy = null;
            Stream responseStream = request.GetResponse().GetResponseStream();

            var r = DecodeBarcode(new Bitmap(responseStream));

            NameValueCollection values = new NameValueCollection();
            string s = r.Substring(5);
            bool flag = false;
            for (int i = 0; (i < 3) && !flag; i++)
            {
                try
                {
                    s = Encoding.UTF8.GetString(Convert.FromBase64String(s));
                    flag = true;
                }
                catch
                {
                    s = s + "=";
                }
            }
            if (!flag)
            {
                s = r.Substring(5);
            }
            if (!s.Contains<char>(':') || !s.Contains<char>('@'))
            {
                throw new UriFormatException("Shadowsocks URI");
            }
            if (!s.Split(new char[] { '@' })[1].Contains<char>(':') || (s.IndexOf('@') <= s.IndexOf(':')))
            {
                throw new UriFormatException("Shadowsocks URI");
            }
            values["EncryptionMethod"] = s.Substring(0, s.IndexOf(':'));
            values["Password"] = s.Substring(s.IndexOf(':') + 1, (s.IndexOf('@') - s.IndexOf(':')) - 1);
            values["Server"] = s.Substring(s.IndexOf('@') + 1, s.Split(new char[] { '@' })[1].IndexOf(':'));
            values["Port"] = s.Split(new char[] { '@' })[1].Substring(s.Split(new char[] { '@' })[1].IndexOf(':') + 1);
            return values;
        }

        public static void StartProxy(NameValueCollection config)
        {
            //foreach (Process process in Process.GetProcessesByName("node"))
            //{
            //    try
            //    {
            //        process.Kill();
            //    }
            //    catch
            //    {
            //    }
            //}
            StopProxy();

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                Arguments = ConfigurationStringBuilder(config),
                FileName = "node.exe",
                WindowStyle = ProcessWindowStyle.Hidden
            };
            Proxy = Process.Start(startInfo);
        }

        public static void StopProxy()
        {
            if (Proxy == null)
                return;

            Proxy.Kill();
            Proxy.Close();
        }
    }
}
