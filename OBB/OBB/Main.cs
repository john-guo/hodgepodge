using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace OBB
{
    public static class SerializationExtensions
    {
        public static string Serialize<T>(this T obj)
        {
            var serializer = new DataContractSerializer(obj.GetType());
            using (var writer = new StringWriter())
            using (var stm = new XmlTextWriter(writer))
            {
                serializer.WriteObject(stm, obj);
                return writer.ToString();
            }
        }
        public static T Deserialize<T>(this string serialized)
        {
            var serializer = new DataContractSerializer(typeof(T));
            using (var reader = new StringReader(serialized))
            using (var stm = new XmlTextReader(reader))
            {
                return (T)serializer.ReadObject(stm);
            }
        }
    }

    public class Entry
    {
        [Serializable]
        public class GameSetting
        {
            public string name;
            public string gameExe;
            public string gameWorkPath;
            public string gameArguments;
            public string description;
            public string thumbnail;
            public string sendKey;
            public int sendKeyDelay;
            public bool needPlay;
            public string playExe;
            public string playWorkPath;
            public string playArguments;
        }



        [STAThread]
        public static void Main(string[] Args)
        {
            //Console.ReadLine();

            //Dictionary<string, GameSetting> xml = new Dictionary<string, GameSetting>();
            //xml["test"] = new GameSetting() { name = "test" };
            //File.WriteAllText("test.json", JsonConvert.SerializeObject(xml, Newtonsoft.Json.Formatting.Indented));
            //var result = System.Diagnostics.Process.GetProcesses().Select(p => p.StartInfo.FileName);

            //var link = NetworkInterface.GetAllNetworkInterfaces().Where(ni =>
            //    ni.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
            //    ni.OperationalStatus == OperationalStatus.Up &&
            //    ni.GetIPProperties().GatewayAddresses.Any()).First();

            //var addrInfo = link.GetIPProperties().UnicastAddresses.Where(addr => addr.Address.AddressFamily == AddressFamily.InterNetwork).First();


            //var bytes = addrInfo.IPv4Mask.GetAddressBytes().Select(b => (byte)~b)
            //    .Zip(addrInfo.Address.GetAddressBytes(), (b1, b2) => (byte)(b1 | b2))
            //    .ToArray();


            //var address = new IPAddress(bytes);


            Application.Run(new Form1());
        }
    }
}
