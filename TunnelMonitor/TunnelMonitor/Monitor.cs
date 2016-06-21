using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading;
using System.Diagnostics;
using System.Linq;
using ConfigSettings = TunnelMonitor.Properties.Settings;

namespace TunnelMonitor
{
    public class Monitor
    {
        bool running;
        System.Timers.Timer checkTimer;
        WebClient client;
        public MonitorSettings Settings { get; private set; }

        public static Monitor FromConfig()
        {
            return new Monitor(ConfigSettings.Default.CheckInterval,
                ConfigSettings.Default.CheckUrl,
                ConfigSettings.Default.CheckString,
                ConfigSettings.Default.TunnelPath,
                ConfigSettings.Default.TunnelArguments,
                ConfigSettings.Default.TunnelWorkingDirectory);
        }

        public Monitor()
            : this(10, string.Empty, string.Empty, "plink", "-load tunnel", Environment.CurrentDirectory)
        {
        }

        public Monitor(int checkIntevalInSeconds, string checkUrl, string checkString,
            string tunnelPath, string tunnelArguments, string tunnelWorkingDirectory)
        {
            running = false;
            Settings = new MonitorSettings()
            {
                CheckIntervalInSeconds = checkIntevalInSeconds,
                CheckUrl = checkUrl,
                CheckString = checkString,
                TunnelPath = tunnelPath,
                TunnelArguments = tunnelArguments,
                TunnelWorkingDirectory = tunnelWorkingDirectory
            };

            InitTimer();
            client = new WebClient();
        }

        private void InitTimer()
        {
            checkTimer = new System.Timers.Timer(Settings.CheckIntervalInSeconds * 1000);
            checkTimer.Elapsed += CheckTimer_Elapsed;
        }

        private void CheckTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (running && Check())
            {
                return;
            }

            Run();
        }

        private bool Check()
        {
            string checkstr = string.Empty;
            try
            {
                checkstr = client.DownloadString(Settings.CheckUrl);
            }
            catch
            {
                return false;
            }

            if (string.IsNullOrEmpty(Settings.CheckString))
                return true;

            return checkstr == Settings.CheckString;
        }

        public void Start()
        {
            Run();

            if (running)
            {
                checkTimer.Start();
            }
        }

        public void Stop()
        {
            checkTimer.Stop();

            Kill();
        }

        private void Kill()
        {
            Settings.Tunnel?.Kill();
            Settings.Tunnel?.WaitForExit();
            //var pName = Settings.Tunnel?.ProcessName;
            //if (!string.IsNullOrEmpty(pName))
            //{
            //    SpinWait.SpinUntil(() => !Process.GetProcessesByName(pName).Any());
            //}
            running = false;
        }

        private void Run()
        {
            if (running)
            {
                Kill();
            }

            running = Settings.Tunnel?.Start()??false;
        }
    }
}
