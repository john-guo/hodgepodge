using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;

namespace OBB
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        extern static IntPtr GetDesktopWindow();

        private void Form1_Load(object sender, EventArgs e)
        {
            //var handle = GetDesktopWindow();
            //var desc = new SwapChainDescription()
            //{
            //    BufferCount = 1,
            //    Usage = Usage.RenderTargetOutput,
            //    OutputHandle = handle,
            //    IsWindowed = true,
            //    ModeDescription = new ModeDescription(0, 0, new Rational(60, 1), Format.R8G8B8A8_UNorm),
            //    SampleDescription = new SampleDescription(1, 0),
            //    Flags = SwapChainFlags.AllowModeSwitch,
            //    SwapEffect = SwapEffect.Discard
            //};

            //D3Device.CreateWithSwapChain(DriverType.Hardware,
            //    DeviceCreationFlags.None,
            //    desc,
            //    out d,
            //    out swapChain);

            //context = d.ImmediateContext;

            

        }

        private void button1_Click(object sender, EventArgs e)
        {


            var fileName = "D:\\software\\DK2 myself\\CyberSpaceDK2_MM - 一个翻转游乐设施\\CyberSpace_DK2_MM_v1.exe";
            var argument = "-popupwindow";
            var directory = "D:\\software\\DK2 myself\\CyberSpaceDK2_MM - 一个翻转游乐设施\\";


            var pInfo = new ProcessStartInfo(fileName, argument);
            pInfo.UseShellExecute = true;
            pInfo.CreateNoWindow = true;
            pInfo.WorkingDirectory = directory;
            pInfo.WindowStyle = ProcessWindowStyle.Maximized;
            var process = Process.Start(pInfo);

            //NativeWin32.SetForegroundWindow(process.MainWindowHandle.ToInt32());
            new Task(() =>
            {
                process.WaitForInputIdle();
                SendKeys.SendWait("{Enter}");
                process.WaitForInputIdle();
                SendKeys.SendWait(" ");
            }).Start();

            DIK key = DIK.DIK_SPACE;
            //new Task(() =>
            //{
            //    Debug.WriteLine("SendKey Begin");
            //    for (int i = 0; i <= 100; ++i)
            //    {
            //        SendKeys.SendKeyPress(key);
            //        Thread.Sleep(1000);
            //    }
            //    Debug.WriteLine("SendKey End");
            //}).Start();

        }
    }
}
