using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using SharpOVR;
using SharpDX.Toolkit;
using SharpDX.Toolkit.Graphics;
using System.Drawing;
using System.Runtime.InteropServices;
//using System.Drawing;

namespace oculusTest
{
    class Program
    {
        [STAThread]
        static void Main()
        {
            using (var program = new RiftGame.RiftGame())
                program.Run();

            /*
            var form = new SharpDX.Windows.RenderForm();
            OVR.Initialize();
            var hmd = OVR.HmdCreate(0) ?? OVR.HmdCreateDebug(HMDType.DK1);
            hmd.AttachToWindow(form.Handle);
            var renderTargetSize = hmd.GetDefaultRenderTargetSize();

            // SwapChain description
            var desc = new SwapChainDescription()
            {
                BufferCount = 1,
                ModeDescription =
                    new ModeDescription(renderTargetSize.Width, renderTargetSize.Height,
                                        new Rational(60, 1), Format.R8G8B8A8_UNorm),
                IsWindowed = true,
                OutputHandle = form.Handle,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput | Usage.ShaderInput | Usage.BackBuffer,
                Flags = SwapChainFlags.None,
            };

            // Used for debugging dispose object references
            // Configuration.EnableObjectTracking = true;

            // Disable throws on shader compilation errors
            //Configuration.ThrowOnShaderCompileError = false;

            // Create Device and SwapChain
            Device device;
            SwapChain swapChain;
            Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.None, desc, out device, out swapChain);
            var context = device.ImmediateContext;

            // Ignore all windows events
            var factory = swapChain.GetParent<Factory>();
            factory.MakeWindowAssociation(form.Handle, WindowAssociationFlags.IgnoreAll);


            var renderTarget = SharpDX.Direct3D11.Texture2D.FromSwapChain<SharpDX.Direct3D11.Texture2D>(swapChain, 0);
            var renderTargetView = new RenderTargetView(device, renderTarget);
            var renderTargetSRView = new ShaderResourceView(device, renderTarget);
            renderTarget.Dispose();

            // Depth buffer
            var zbufferTexture = new SharpDX.Direct3D11.Texture2D(device, new Texture2DDescription()
            {
                Format = Format.D16_UNorm,
                ArraySize = 1,
                MipLevels = 1,
                Width = renderTargetSize.Width,
                Height = renderTargetSize.Height,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            });

            var zbufferView = new DepthStencilView(device, zbufferTexture);
            zbufferTexture.Dispose();

            // The viewport sizes are re-computed in case renderTargetSize changed
            var eyeRenderViewport = new Rect[2];
            eyeRenderViewport[0] = new Rect(0, 0, renderTargetSize.Width / 2, renderTargetSize.Height);
            eyeRenderViewport[1] = new Rect((renderTargetSize.Width + 1) / 2, 0, eyeRenderViewport[0].Width, eyeRenderViewport[0].Height);

            // Create our eye texture data
            D3D11TextureData[] eyeTexture = new D3D11TextureData[2];
            eyeTexture[0].Header.API = RenderAPIType.D3D11;
            eyeTexture[0].Header.TextureSize = renderTargetSize;
            eyeTexture[0].Header.RenderViewport = eyeRenderViewport[0];
            //eyeTexture[0].pTexture = ((SharpDX.Direct3D11.Texture2D)renderTarget).NativePointer;

            eyeTexture[0].pTexture = SharpDX.Direct3D11.Texture2D.FromSwapChain<SharpDX.Direct3D11.Texture2D>(swapChain, 0).NativePointer;
            eyeTexture[0].pSRView = renderTargetSRView.NativePointer;

            // Right eye uses the same texture, but different rendering viewport
            eyeTexture[1] = eyeTexture[0];
            eyeTexture[1].Header.RenderViewport = eyeRenderViewport[1];

            D3D11ConfigData d3d11cfg = new D3D11ConfigData();
            d3d11cfg.Header.API = RenderAPIType.D3D11;
            d3d11cfg.Header.BackBufferSize = hmd.Resolution;
            d3d11cfg.Header.Multisample = 1;
            d3d11cfg.pDevice = device.NativePointer;
            d3d11cfg.pDeviceContext = device.ImmediateContext.NativePointer;
            //d3d11cfg.pBackBufferRT = ((RenderTargetView)graphicsDevice.BackBuffer).NativePointer;
            d3d11cfg.pBackBufferRT = renderTargetView.NativePointer;
            d3d11cfg.pSwapChain = swapChain.NativePointer;
            //d3d11cfg.pSwapChain = ((SharpDX.DXGI.SwapChain)graphicsDevice.Presenter.NativePresenter).NativePointer;
            

            // Configure rendering
            var eyeRenderDesc = new EyeRenderDesc[2];
            if (!hmd.ConfigureRendering(d3d11cfg, DistortionCapabilities.Chromatic | DistortionCapabilities.TimeWarp, hmd.DefaultEyeFov, eyeRenderDesc))
            {
                throw new Exception("Failed to configure rendering");
            }


            // Set enabled capabilities
            hmd.EnabledCaps = HMDCapabilities.LowPersistence | HMDCapabilities.DynamicPrediction;

            // Configure tracking
            hmd.ConfigureTracking(TrackingCapabilities.Orientation | TrackingCapabilities.Position | TrackingCapabilities.MagYawCorrection, TrackingCapabilities.None);

            // Dismiss the Heatlh and Safety Window
            //hmd.DismissHSWDisplay();

            // Get HMD output
            var gdevice = GraphicsDevice.New(device);
            var adapter = (Adapter)(gdevice).Adapter;
            var hmdOutput = adapter.Outputs.FirstOrDefault(o => hmd.DeviceName.StartsWith(o.Description.DeviceName, StringComparison.OrdinalIgnoreCase));
            if (hmdOutput != null)
            {
                // Set game to fullscreen on rift
                //var swapChain = (SwapChain)graphicsDevice.Presenter.NativePresenter;
                var description = swapChain.Description.ModeDescription;
                swapChain.ResizeTarget(ref description);
                swapChain.SetFullscreenState(true, hmdOutput);
            }


            PoseF[] renderPose = new PoseF[2];
            Vector3[] eyeViewOffset = new Vector3[2];

            device.ImmediateContext.Rasterizer.SetViewport(0, 0, renderTargetSize.Width, renderTargetSize.Height);
            device.ImmediateContext.OutputMerger.SetTargets(zbufferView, renderTargetView);

            Bitmap b = (Bitmap)Bitmap.FromFile("1.jpg");

            var t = SharpDX.Toolkit.Graphics.Texture2D.Load(gdevice, "1.jpg");
            var s = new SpriteBatch(gdevice);


            RenderLoop.Run(form, () =>
            {
                hmd.BeginFrame(0);

                gdevice.Clear(SharpDX.Color.CornflowerBlue);

                for (int eyeIndex = 0; eyeIndex < 2; eyeIndex++)
                {



                    var eye = hmd.EyeRenderOrder[eyeIndex];
                    var renderDesc = eyeRenderDesc[(int)eye];
                    var renderViewport = eyeRenderViewport[(int)eye];
                    
                    hmd.GetEyePoses(0, eyeViewOffset, renderPose);
                    var pose = renderPose[(int)eye];

                    Vector3 headPos = new Vector3(0f, 0f, -5f);
                    float bodyYaw = 3.141592f;

                    // Calculate view matrix                
                    var rollPitchYaw = Matrix.RotationY(bodyYaw);
                    var finalRollPitchYaw = rollPitchYaw * pose.Orientation.GetMatrix();
                    var finalUp = finalRollPitchYaw.Transform(Vector3.UnitY);
                    var finalForward = finalRollPitchYaw.Transform(-Vector3.UnitZ);
                    var shiftedEyePos = headPos + rollPitchYaw.Transform(pose.Position);
                    Matrix view = Matrix.Translation(renderDesc.HmdToEyeViewOffset) * Matrix.LookAtRH(shiftedEyePos, shiftedEyePos + finalForward, finalUp);



                    //var d = b.LockBits(new System.Drawing.Rectangle(0, 0, b.Width, b.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    //var bytes = new byte[d.Stride * d.Height];
                    //Marshal.Copy(d.Scan0, bytes, 0, bytes.Length);
                    //b.UnlockBits(d);
                    
                    //var t = SharpDX.Direct3D11.Texture2D.FromMemory(device, new DataPointer(d.Scan0, d.Stride * d.Height));
                    //var t = SharpDX.Direct3D11.Texture2D.FromFile(device, "1.jpg");
                    //eyeTexture[eyeIndex].pTexture = t.NativePointer;

                    var proj = OVR.MatrixProjection(renderDesc.Fov, 0.001F, 1000F, true);
                    proj.Transpose();
                    
                    device.ImmediateContext.Rasterizer.SetViewport(renderViewport.ToViewportF());


                    //var m = SharpDX.Toolkit.Graphics.Image.Load("1.jpg");
                    s.Begin();
                    Vector2 pos = new Vector2(0, 0);
                    s.Draw(t, pos, SharpDX.Color.White);
                    s.End();
                }

                hmd.EndFrame(renderPose, eyeTexture);

                //swapChain.Present(0, PresentFlags.None);
            });
             * */
        }
    }
}
