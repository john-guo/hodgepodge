using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace ARPQuery
{
    public class ARP
    {
        [DllImport("Iphlpapi.dll")]
        public static extern uint SendARP(uint DestIP, uint SrcIP, ref ulong pMacAddr, ref uint PhyAddrLen);

        public static string GetMac(string ip)
        {
            IPAddress address;
            if (!IPAddress.TryParse(ip, out address)) return "";
            uint DestIP = BitConverter.ToUInt32(address.GetAddressBytes(), 0);
            ulong pMacAddr = 0;
            uint PhyAddrLen = 6;
            uint error_code = SendARP(DestIP, 0, ref pMacAddr, ref PhyAddrLen);
            byte[] bytes = BitConverter.GetBytes(pMacAddr);
            return BitConverter.ToString(bytes, 0, 6);
        }

        static void Main(string[] args)
        {
            if (args.Length == 0)
                return;

            Console.WriteLine("{0}", GetMac(args[0]));
        }
    }
}
