using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Collections.Generic;
using System.Windows.Forms;
using OpenHardwareMonitor.Hardware;
using System.IO;
using System.Management;
using System.Net.Sockets;
using System.Net.NetworkInformation;

namespace hwInfo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            AddContent(String.Format("机器名: {0}", Environment.MachineName));
            AddContent(String.Format("用户名: {0}", Environment.UserName));
            AddContent(String.Format("域名: {0}", Environment.UserDomainName));

            NW();

            var computer = new Computer();
            computer.CPUEnabled = true;
            computer.GPUEnabled = true;
            computer.HDDEnabled = true;
            computer.RAMEnabled = true;
            computer.MainboardEnabled = true;
            computer.HardwareAdded += Computer_HardwareAdded;

            computer.Open();
            
            
            //textBox1.Text = computer.GetReport();
            computer.Close();

            HDD();
        }

        private void AddContent(string content)
        {
            textBox1.AppendText(String.Format("{0}{1}", content, Environment.NewLine));
        }

        private void NW()
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
                            AddContent(String.Format("Mac: {0}", ni.GetPhysicalAddress()));
                            AddContent(String.Format("IP: {0}", ai.Address));
                            return;
                        }
                    }
                }
            }
        }

        private void HDD()
        {
            var query = new SelectQuery("select * from Win32_DiskDrive");
            var searcher = new ManagementObjectSearcher(query);
            var result = searcher.Get();
            foreach (var o in result)
            {
                var total = 0.0;
                searcher.Query.QueryString = String.Format("select * from Win32_DiskPartition where DiskIndex='{0}'", o["Index"]);
                var result2 = searcher.Get();

                foreach (var p in result2)
                {
                    var searcher2 = new ManagementObjectSearcher(String.Format("ASSOCIATORS OF {{{0}}} WHERE AssocClass = Win32_LogicalDiskToPartition", p));
                    var result3 = searcher2.Get();

                    foreach (var q in result3)
                    {
                        var size = q["Size"];
                        total += (double)((ulong)size) / (1024UL * 1024UL * 1024UL);
                    }
                }

                AddContent(String.Format("驱动器: {0} 容量: {1:F2} G", o["Caption"], total));
            }
        }

        private void Computer_HardwareAdded(IHardware hardware)
        {
            hardware.Update();
            switch (hardware.HardwareType)
            {
                case HardwareType.Mainboard:
                    AddContent(String.Format("主板: {0}", hardware.Name));
                    break;
                case HardwareType.CPU:
                    AddContent(String.Format("CPU: {0} 核心数: {1}", hardware.Name, Environment.ProcessorCount));
                    break;
                case HardwareType.RAM:
                    var total = 0.0f;
                    foreach (var sensor in hardware.Sensors)
                    {
                        if (sensor.SensorType != SensorType.Data)
                            continue;

                        total += sensor.Value.Value;
                    }
                    AddContent(String.Format("内存: {0:F1}G", total));
                    break;
                //case HardwareType.HDD:
                //    AddContent(String.Format("硬盘: {0}", hardware.Name));
                //    break;
                case HardwareType.GpuAti:
                    AddContent(String.Format("显卡: {0}", hardware.Name));
                    break;
                case HardwareType.GpuNvidia:
                    AddContent(String.Format("显卡: {0}", hardware.Name));
                    break;
            }
        }
    }
}
