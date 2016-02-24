using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NATUPNPLib;
using System.Net.NetworkInformation;
using System.Net.Sockets;

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
            var upnp = new UPnPNAT();

            var mappings = upnp.StaticPortMappingCollection;

            if (mappings == null)
                return;

            mappings.Add(9, "UDP", 9, GetIP(), true, "test");
        }
    }
}
