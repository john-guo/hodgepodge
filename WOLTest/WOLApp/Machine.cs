using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WOLApp
{
    public class Machine
    {
        public static ConcurrentQueue<Machine> machines = new ConcurrentQueue<Machine>();
        public static void Save(string filename)
        {
            using (var f = File.OpenWrite(filename))
            using (var writer = new StreamWriter(f))
            {
                foreach (var m in machines)
                {
                    writer.WriteLine("{0},{1},{2}", m.IP, m.mac, m.Port);
                }
                writer.Flush();
                writer.Close();
            }
        }

        public static void Load(string filename)
        {
            using (var f = File.OpenRead(filename))
            using (var reader = new StreamReader(f))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine().Trim();
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    var slice = line.Split(',');

                    string ip = String.Empty;
                    ulong mac = 0;
                    int port = 0;
                    if (slice.Length == 2)
                    {
                        ip = slice[0];
                        ulong.TryParse(slice[1], out mac);
                    }

                    if (slice.Length == 3)
                        int.TryParse(slice[2], out port);

                    machines.Enqueue(new Machine() { IP = ip, mac = mac, Port = port });
                }

                reader.Close();
            }
        }


        public string IP
        {
            get; set;
        }

        public int Port
        {
            get; set;
        }

        public ulong mac;

        private string macAddr;
        public string Mac
        {
            get
            {
                if (!string.IsNullOrEmpty(macAddr))
                    return macAddr;

                return macAddr = WOL.ReadableMac(mac);
            }
        }
    }
}
