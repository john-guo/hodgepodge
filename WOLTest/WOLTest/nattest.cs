using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NATUPNPLib;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Net;

namespace WOLTest
{
    public class NATTest
    {
        static string GetIP()
        {
            var link = NetworkInterface.GetAllNetworkInterfaces();
            foreach (var ni in link)
            {
                if (ni.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                    ni.OperationalStatus == OperationalStatus.Up &&
                    ni.GetIPProperties().GatewayAddresses.Count > 0)
                {
                    foreach (var ai in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ai.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            return ai.Address.ToString();
                        }
                    }
                }
            }

            return String.Empty;
        }


        static void Main()
        {
            //var upnp = new UPnPNAT();

            //var mappings = upnp.StaticPortMappingCollection;

            //if (mappings == null)
            //    return;

            //mappings.Add(9, "UDP", 9, GetIP(), true, "test");

            ConcurrentQueue<Tuple<IPAddress, ulong>> machines = new ConcurrentQueue<Tuple<IPAddress, ulong>>();

            Parallel.ForEach(WOL.GetAllSubAddress(true), addr =>
            {
                var mac = WOL.GetMacAddr(addr);
                if (mac == 0)
                    return;

                Console.WriteLine("{0} {1}", addr, WOL.ReadableMac(mac));
                machines.Enqueue(new Tuple<IPAddress, ulong>(addr, mac));
            });

            UdpClient udp = new UdpClient(new IPEndPoint(WOL.GetBroadcastAddress(), 9));
            foreach (var m in machines)
            {
                byte[] dgram = WOL.CreateMagicPacket(m.Item2);
                udp.Send(dgram, dgram.Length);
                Console.WriteLine("{0} send", m.Item1);
            }
            udp.Close();
        }
    }
}
