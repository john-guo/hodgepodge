using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;
using TunnelMonitor;

namespace TM_WSH
{
    public partial class Service1 : ServiceBase
    {
        Monitor monitor;

        public Service1()
        {
            InitializeComponent();

            monitor = Monitor.FromConfig();
        }

        protected override void OnStart(string[] args)
        {
            monitor.Start();
        }

        protected override void OnStop()
        {
            monitor.Stop();
        }
    }
}
