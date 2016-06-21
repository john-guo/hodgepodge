using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using TunnelMonitor;

namespace TM_WinForm
{
    public partial class Form1 : Form
    {
        Monitor monitor;
        public Form1()
        {
            InitializeComponent();
            monitor = Monitor.FromConfig();
            //monitor.Settings.Tunnel.StartInfo.RedirectStandardOutput = true;
            //monitor.Settings.Tunnel.OutputDataReceived += Tunnel_OutputDataReceived;
        }

        //private void Tunnel_OutputDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        //{
        //    var a = (Action)delegate
        //    {
        //        textBox1.AppendText(e.Data);
        //    };

        //    if (InvokeRequired)
        //    {
        //        Invoke(a);
        //    }
        //    else
        //    {
        //        a();
        //    }
        //}

        private void Form1_Load(object sender, EventArgs e)
        {
            monitor.Start();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            monitor.Stop();
        }
    }
}
