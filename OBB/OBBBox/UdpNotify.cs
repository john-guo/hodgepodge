using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;

namespace OBB
{
    public class UdpNotify : IDisposable
    {
        private const int timeout = 1000;
        private UdpClient udp;
        private int port;
        private Task task;
        private CancellationTokenSource tokenSource;
        private byte[] data;
        private bool isBroadcast;

        private static IPAddress _broadcastAddress = Utility.GetBroadcastAddress();

        public delegate void OnDataHandler(IPEndPoint endPoint, byte[] data);

        public event OnDataHandler OnData;

        protected bool IsBroadcast
        {
            get { return isBroadcast; }
        }

        public UdpNotify(int port) : this(port, false)
        {
            
        }

        protected UdpNotify(int port, bool isBroadcast)
        {
            this.port = port;
            this.isBroadcast = isBroadcast;

            if (!isBroadcast)
            {
                udp = new UdpClient(port + 1);
            } 
            else
            {
                udp = new UdpClient(port);
            }
            udp.DontFragment = true;
            udp.EnableBroadcast = true;

            tokenSource = new CancellationTokenSource();
            task = new Task(Process, tokenSource.Token, TaskCreationOptions.LongRunning);
            task.Start();
        }

        public UdpNotify(int port, byte[] data)
            : this(port, true)
        {
            this.data = data;
        }

        private void Process()
        {
            try
            {
                while (!tokenSource.IsCancellationRequested)
                {
                    if (IsBroadcast)
                    {
                        Broadcast();
                    }
                    else
                    {
                        Receive();
                    }

                    Thread.Sleep(timeout);
                }
            }
            catch (OperationCanceledException ex)
            {
                Debug.WriteLine("Task canceled:{0}", ex);
            }
            finally
            {
                udp.Close();
            }
        }

        private void Broadcast()
        {
            udp.Send(data, data.Length, new IPEndPoint(_broadcastAddress, this.port + 1));
        }

        private void Receive()
        {
            IPEndPoint ip = new IPEndPoint(IPAddress.Any, 0);
            data = udp.Receive(ref ip);

            if (this.OnData != null)
            {
                OnData(ip, data);
            }
        }

        public void Dispose()
        {
            tokenSource.Cancel();
        }
    }
}
