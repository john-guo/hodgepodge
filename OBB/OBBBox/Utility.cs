using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Newtonsoft.Json;
using System.IO;

namespace OBB
{
    public static class Utility
    {
        public static IPAddress GetRealIp()
        {
            var link = NetworkInterface.GetAllNetworkInterfaces().Where(ni =>
                ni.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                ni.OperationalStatus == OperationalStatus.Up &&
                ni.GetIPProperties().GatewayAddresses.Any()).First();

            var address = link.GetIPProperties().UnicastAddresses.Select(ip => ip.Address).Where(addr => addr.AddressFamily == AddressFamily.InterNetwork).First();

            return address;
        }

        public static string GetJsonResult(JsonStatus status, string reason = "")
        {
            var result = new JsonResult()
            {
                status = status,
                reason = reason
            };

            return JsonConvert.SerializeObject(result);
        }

        public static Uri GetServiceUri(string address, int port, string command)
        {
            return new Uri(String.Format("http://{0}:{1}/op/{2}", address, port, command));
        }

        public static string GetProcName(string path)
        {
            return Path.GetFileNameWithoutExtension(path);
        }

        public static IPAddress GetBroadcastAddress()
        {
            var link = NetworkInterface.GetAllNetworkInterfaces().Where(ni =>
                ni.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                ni.OperationalStatus == OperationalStatus.Up &&
                ni.GetIPProperties().GatewayAddresses.Any()).First();

            var addrInfo = link.GetIPProperties().UnicastAddresses.Where(addr => addr.Address.AddressFamily == AddressFamily.InterNetwork).First();

            var bytes = addrInfo.IPv4Mask.GetAddressBytes().Select(b => (byte)~b)
                .Zip(addrInfo.Address.GetAddressBytes(), (b1, b2) => (byte)(b1 | b2))
                .ToArray();


            return new IPAddress(bytes);
        }
    }
}
