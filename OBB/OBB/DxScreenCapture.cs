using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using SlimDX;
using SlimDX.Direct3D9;

namespace OBB
{
    public static class DxScreenCapture
    {
        private static Device d;

        static DxScreenCapture()
        {
            var d3d = new Direct3D();
            var adapterInfo = d3d.Adapters.DefaultAdapter;
            PresentParameters parameters = new PresentParameters();
            parameters.BackBufferCount = 1;
            parameters.BackBufferFormat = Format.A8R8G8B8;
            parameters.BackBufferWidth = Screen.PrimaryScreen.Bounds.Width;
            parameters.BackBufferHeight = Screen.PrimaryScreen.Bounds.Height;
            parameters.Windowed = true;
            parameters.SwapEffect = SwapEffect.Copy;

            d = new Device(d3d, adapterInfo.Adapter, DeviceType.Hardware, IntPtr.Zero, CreateFlags.SoftwareVertexProcessing, parameters);
        }

        private static Surface _CaptureScreen()
        {
            //Surface s = d.GetBackBuffer(0, 0);

            Surface s = Surface.CreateOffscreenPlain(d, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, Format.A8R8G8B8, Pool.Scratch);
            //var result = d.GetSwapChain(0).GetFrontBufferData(s);
            var result = d.GetFrontBufferData(0, s);
            if (!result.IsSuccess)
                throw new Exception("Failed");

            return s;
        }

        public static Image CaptureScreen(ImageFileFormat fmt = ImageFileFormat.Bmp)
        {
            Image img;
            using (var s = _CaptureScreen())
            using (var ds = Surface.ToStream(s, fmt))
            {
                img = Image.FromStream(ds);
            }
            return img;
        }
    }
}
