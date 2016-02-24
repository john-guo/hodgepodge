using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WOLApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            textBox.Text = WOL.GetBroadcastAddress().ToString();
            try
            {
                Machine.Load("machine.csv");
                dataGrid.ItemsSource = Machine.machines;
            } catch { }
        }


        private async void button_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("It will take long time to probe, ARE YOU SURE?!", "Confirm", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.No)
                return;

            progressbar.IsIndeterminate = true;
            await Task.Run(() =>
            {
                Parallel.ForEach(WOL.GetAllSubAddress(true), addr =>
                {
                    var mac = WOL.GetMacAddr(addr);
                    if (mac == 0)
                        return;

                    Machine.machines.Enqueue(new Machine() { IP = addr.ToString(), mac = mac });
                });
            });
            Machine.Save("machine.csv");
            progressbar.IsIndeterminate = false;

            dataGrid.ItemsSource = Machine.machines;
        }

        private async void button1_Click(object sender, RoutedEventArgs e)
        {
            string ip;
            int port;

            UdpClient udp = new UdpClient();
            try
            {
                ip = textBox.Text;
                port = int.Parse(textBox_Copy.Text);
                IPAddress.Parse(ip);
            }
            catch
            {
                ip = WOL.GetBroadcastAddress().ToString();
                port = 9;
                MessageBox.Show("There are some errors, use default settings.");
            }

            foreach (var m in Machine.machines)
            {
                byte[] dgram = WOL.CreateMagicPacket(m.mac);
                udp.Send(dgram, dgram.Length, ip, m.Port == 0 ? port : m.Port);
            }

            udp.Close();

            MessageBox.Show("Done.");
        }
    }
}
