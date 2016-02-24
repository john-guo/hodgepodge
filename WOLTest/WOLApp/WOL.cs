using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace WOLApp
{
    public class WOL
    {
        public static byte[] CreateMagicPacket(ulong mac)
        {
            return CreateMagicPacket(BitConverter.GetBytes(mac).Take(6).ToArray());
        }

        public static byte[] CreateMagicPacket(byte[] macAddress)
        {
            byte[] array = new byte[0x66];
            for (int i = 0; i < 6; i++)
            {
                array[i] = 0xff;
            }
            for (int j = 1; j <= 0x10; j++)
            {
                macAddress.CopyTo(array, (int)(j * 6));
            }
            return array;
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


        public static IEnumerable<IPAddress> GetAllSubAddress(bool exclusiveSelf = false)
        {
            var link = NetworkInterface.GetAllNetworkInterfaces().Where(ni =>
                ni.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                ni.OperationalStatus == OperationalStatus.Up &&
                ni.GetIPProperties().GatewayAddresses.Any()).First();

            var addrInfo = link.GetIPProperties().UnicastAddresses.Where(addr => addr.Address.AddressFamily == AddressFamily.InterNetwork).First();

            var self = addrInfo.Address.GetAddressBytes();
            var mask = addrInfo.IPv4Mask.GetAddressBytes();
            var count = mask.Aggregate(0u, (w, b) => w << 8 | (byte)(~b));

            var sub = mask.Zip(self, (b1, b2) => (byte)(b1 & b2)).
                       Aggregate(0u, (w, b) => w << 8 | b);

            var selfIp = BitConverter.ToInt32(self, 0);

            for (int i = 0; i <= count; ++i)
            {
                var ip = (uint)IPAddress.HostToNetworkOrder((int)sub + i);
                if (exclusiveSelf && ip == selfIp)
                    continue;
                yield return new IPAddress(ip);
            }
        }

        public static string GetMac(string ip)
        {
            IPAddress address;
            if (!IPAddress.TryParse(ip, out address)) return "";
            return GetMac(address);
        }

        public static ulong GetMacAddr(IPAddress address)
        {
            uint destIP = BitConverter.ToUInt32(address.GetAddressBytes(), 0);
            ulong macAddr = 0;
            uint macAddrLen = 6;
            uint errorCode = SendARP(destIP, 0, ref macAddr, ref macAddrLen);
            return macAddr;
        }

        public static string GetMac(IPAddress address)
        {
            var pMacAddr = GetMacAddr(address);
            return ReadableMac(pMacAddr);
        }

        public static string ReadableMac(ulong mac)
        {
            byte[] bytes = BitConverter.GetBytes(mac);
            return BitConverter.ToString(bytes, 0, 6);
        }

        [DllImport("Iphlpapi.dll")]
        public static extern uint SendARP(uint destIP, uint srcIP, ref ulong macAddr, ref uint macAddrLen);
    }
}
