﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ARPQuery
{
    public class ARP
    {
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

        public static IEnumerable<IPAddress> GetAllSubAddress()
        {
            var link = NetworkInterface.GetAllNetworkInterfaces().Where(ni =>
                ni.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                ni.OperationalStatus == OperationalStatus.Up &&
                ni.GetIPProperties().GatewayAddresses.Any()).First();

            var addrInfo = link.GetIPProperties().UnicastAddresses.Where(addr => addr.Address.AddressFamily == AddressFamily.InterNetwork).First();

            var mask = addrInfo.IPv4Mask.GetAddressBytes();
            var count = mask.Aggregate(0u, (w, b) => w << 8 | (byte)(~b));

            var sub = mask.Zip(addrInfo.Address.GetAddressBytes(), (b1, b2) => (byte)(b1 & b2)).
                       Aggregate(0u, (w, b) => w << 8 | b);

            for (int i = 0; i <= count; ++i)
            {
                var ip = (uint)IPAddress.HostToNetworkOrder((int)sub + i);
                yield return new IPAddress(ip);
            }
        }

        [DllImport("Iphlpapi.dll")]
        public static extern uint SendARP(uint DestIP, uint SrcIP, ref ulong pMacAddr, ref uint PhyAddrLen);

        public static string GetMac(string ip)
        {
            IPAddress address;
            if (!IPAddress.TryParse(ip, out address)) return "";
            return GetMac(address);
        }

        public static ulong GetMacAddr(IPAddress address)
        {
            uint DestIP = BitConverter.ToUInt32(address.GetAddressBytes(), 0);
            ulong pMacAddr = 0;
            uint PhyAddrLen = 6;
            uint error_code = SendARP(DestIP, 0, ref pMacAddr, ref PhyAddrLen);
            return pMacAddr;
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

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Parallel.ForEach(GetAllSubAddress(), addr =>
                {
                    var mac = GetMacAddr(addr);
                    if (mac == 0)
                        return;

                    Console.WriteLine("{0} {1}", addr, ReadableMac(mac));
                });

                Console.WriteLine("OK");

                return;
            }

            Console.WriteLine("{0}", GetMac(args[0]));
        }
    }
}
