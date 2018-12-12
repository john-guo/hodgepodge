using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace TunnelMonitor
{
    public sealed class MonitorSettings
    {
        public int CheckIntervalInSeconds { get; set; }
        public string CheckUrl { get; set; }
        public string CheckString { get; set; }

        private string tunnelPath;
        public string TunnelPath
        {
            get
            {
                if (Path.IsPathRooted(tunnelPath))
                {
                    return tunnelPath;
                }

                return Path.Combine(TunnelWorkingDirectory, tunnelPath);
            }

            set { tunnelPath = value; }
        }


        public string TunnelArguments { get; set; }

        private string tunnelWorkingDirectory;
        public string TunnelWorkingDirectory
        {
            get { return tunnelWorkingDirectory; }

            set
            {
                if (Path.IsPathRooted(value))
                {
                    tunnelWorkingDirectory = value;
                    return;
                }

                tunnelWorkingDirectory = Path.Combine(Environment.CurrentDirectory, value);
            }
        }

        private Process process = null;
        public Process Tunnel
        {
            get
            {
                if (process != null)
                    return process;

                if (string.IsNullOrEmpty(TunnelPath))
                    return null;

                process = new Process();
                process.StartInfo = new ProcessStartInfo(TunnelPath, TunnelArguments);
                process.StartInfo.WorkingDirectory = TunnelWorkingDirectory;
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                return process;
            }
        }
    }
}
