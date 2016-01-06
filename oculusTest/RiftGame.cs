using System;
using System.Linq;
using System.Text;
using SharpDX;
using SharpDX.Direct3D11;
using SharpOVR;

namespace RiftGame
{
    // Use these namespaces here to override SharpDX.Direct3D11
    using SharpDX.Toolkit;
    using SharpDX.Toolkit.Graphics;
    using SharpDX.DXGI;
    using System.Runtime.InteropServices;
    using System.IO;
    using SharpDX.Windows;
    using SharpDX.Toolkit.Input;

    /// <summary>
    /// Simple RiftGame game using SharpDX.Toolkit.
    /// </summary>
    public class RiftGame : Game
    {
        private GraphicsDeviceManager graphicsDeviceManager;

        private Matrix view;
        private Matrix projection;

        private Texture2D model;
        private SpriteBatch sprite;
        private Vector2 position;

        private HMD hmd;
        private Rect[] eyeRenderViewport;
        private D3D11TextureData[] eyeTexture;

        private RenderTarget2D renderTarget;
        private RenderTargetView renderTargetView;
        private ShaderResourceView renderTargetSRView;
        private DepthStencilBuffer depthStencilBuffer;
        private EyeRenderDesc[] eyeRenderDesc;
        private PoseF[] renderPose = new PoseF[2];
        private Vector3[] eyeViewOffset = new Vector3[2];

        private Vector3 headPos = new Vector3(0f, 0f, 0f);
        private float bodyYaw = 3.141592f;
        private OpenCV.Net.Capture capture;


        private RenderForm _form;
        private SharpDX.Direct3D11.Device _device;
        private SwapChain _swapChain;
        private DeviceContext _context;
        private GraphicsDevice _gdevice;
        private SpriteBatch _sprite;
        private RenderTarget2D _renderTarget;
        private DepthStencilBuffer _depthStencilBuffer;
        private Texture2D _model;
        private RenderTargetView _renderView;
        private DepthStencilView _depthView;

        private KeyboardManager keyMgr;
        /// <summary>
        /// Initializes a new instance of the <see cref="RiftGame" /> class.
        /// </summary>
        public RiftGame()
        {
            // Creates a graphics manager. This is mandatory.
            graphicsDeviceManager = new GraphicsDeviceManager(this);
            keyMgr = new KeyboardManager(this);
            //this.graphicsDeviceManager.IsFullScreen = true;

            // Setup the relative directory to the executable directory 
            // for loading contents with the ContentManager
            Content.RootDirectory = "Content";

            // Initialize OVR Library
            OVR.Initialize();

            // Create our HMD
            hmd = OVR.HmdCreate(0) ?? OVR.HmdCreateDebug(HMDType.DK2);

            // Match back buffer size with HMD resolution
            graphicsDeviceManager.PreferredBackBufferWidth = hmd.Resolution.Width;
            graphicsDeviceManager.PreferredBackBufferHeight = hmd.Resolution.Height;

        }

        protected override void Initialize()
        {
            keyMgr.Initialize();

            // Modify the title of the window
            Window.Title = "RiftGame";

            // Attach HMD to window
            var control = (RenderForm)Window.NativeWindow;
            control.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            control.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            control.DesktopLocation = new System.Drawing.Point(0, 0);
            hmd.AttachToWindow(control.Handle);

            // Create our render target
            //var renderTargetSize = hmd.GetDefaultRenderTargetSize(1.5f);
            var renderTargetSize = hmd.GetDefaultRenderTargetSize();
            renderTarget = RenderTarget2D.New(GraphicsDevice, renderTargetSize.Width, renderTargetSize.Height, new MipMapCount(1), PixelFormat.R8G8B8A8.UNorm, TextureFlags.RenderTarget | TextureFlags.ShaderResource);
            renderTargetView = (RenderTargetView)renderTarget;
            renderTargetSRView = (ShaderResourceView)renderTarget;

            // Create a depth stencil buffer for our render target
            depthStencilBuffer = DepthStencilBuffer.New(GraphicsDevice, renderTargetSize.Width, renderTargetSize.Height, DepthFormat.Depth32, true);

            // Adjust render target size if there were any hardware limitations
            renderTargetSize.Width = renderTarget.Width;
            renderTargetSize.Height = renderTarget.Height;

            // The viewport sizes are re-computed in case renderTargetSize changed
            eyeRenderViewport = new Rect[2];
            eyeRenderViewport[0] = new Rect(0, 0, renderTargetSize.Width / 2, renderTargetSize.Height);
            eyeRenderViewport[1] = new Rect((renderTargetSize.Width + 1) / 2, 0, eyeRenderViewport[0].Width, eyeRenderViewport[0].Height);

            // Create our eye texture data
            eyeTexture = new D3D11TextureData[2];
            eyeTexture[0].Header.API = RenderAPIType.D3D11;
            eyeTexture[0].Header.TextureSize = renderTargetSize;
            eyeTexture[0].Header.RenderViewport = eyeRenderViewport[0];
            eyeTexture[0].pTexture = ((SharpDX.Direct3D11.Texture2D)renderTarget).NativePointer;
            eyeTexture[0].pSRView = renderTargetSRView.NativePointer;

            // Right eye uses the same texture, but different rendering viewport
            eyeTexture[1] = eyeTexture[0];
            eyeTexture[1].Header.RenderViewport = eyeRenderViewport[1];

            // Configure d3d11
            var device = (SharpDX.Direct3D11.Device)GraphicsDevice;
            D3D11ConfigData d3d11cfg = new D3D11ConfigData();
            d3d11cfg.Header.API = RenderAPIType.D3D11;
            d3d11cfg.Header.BackBufferSize = hmd.Resolution;
            d3d11cfg.Header.Multisample = 1;
            d3d11cfg.pDevice = device.NativePointer;
            d3d11cfg.pDeviceContext = device.ImmediateContext.NativePointer;
            d3d11cfg.pBackBufferRT = ((RenderTargetView)GraphicsDevice.BackBuffer).NativePointer;
            d3d11cfg.pSwapChain = ((SharpDX.DXGI.SwapChain)GraphicsDevice.Presenter.NativePresenter).NativePointer;

            // Configure rendering
            eyeRenderDesc = new EyeRenderDesc[2];
            if (!hmd.ConfigureRendering(d3d11cfg, DistortionCapabilities.Chromatic | DistortionCapabilities.TimeWarp, hmd.DefaultEyeFov, eyeRenderDesc))
            {
                throw new Exception("Failed to configure rendering");
            }

            // Set enabled capabilities
            hmd.EnabledCaps = HMDCapabilities.LowPersistence | HMDCapabilities.DynamicPrediction;

            // Configure tracking
            hmd.ConfigureTracking(TrackingCapabilities.Orientation | TrackingCapabilities.Position | TrackingCapabilities.MagYawCorrection, TrackingCapabilities.None);


            // Dismiss the Heatlh and Safety Window
            hmd.EnableHSWDisplaySDKRender(false);
            hmd.DismissHSWDisplay();

            // Get HMD output
            var adapter = (Adapter)GraphicsDevice.Adapter;
            var hmdOutput = adapter.Outputs.FirstOrDefault(o => hmd.DeviceName.StartsWith(o.Description.DeviceName, StringComparison.OrdinalIgnoreCase));
            if (hmdOutput != null)
            {
                control.DesktopLocation = new System.Drawing.Point(hmdOutput.Description.DesktopBounds.Left, hmdOutput.Description.DesktopBounds.Top);

                //Window.BeginScreenDeviceChange(true);
                // Set game to fullscreen on rift
                //var swapChain = (SwapChain)GraphicsDevice.Presenter.NativePresenter;
                //var description = swapChain.Description.ModeDescription;
                //swapChain.ResizeTarget(ref description);
                //swapChain.SetFullscreenState(true, hmdOutput);
                //Window.EndScreenDeviceChange();
            }

            sprite = new SpriteBatch(this.GraphicsDevice);
            position = new Vector2(hmd.Resolution.Width / 2, hmd.Resolution.Height / 2);
            capture = OpenCV.Net.Capture.CreateCameraCapture(0);


            _form = new RenderForm("Normal");
            // SwapChain description
            var desc = new SwapChainDescription()
            {
                BufferCount = 1,
                ModeDescription =
                    new ModeDescription(_form.ClientSize.Width, _form.ClientSize.Height,
                                        new Rational(60, 1), Format.R8G8B8A8_UNorm),
                IsWindowed = true,
                OutputHandle = _form.Handle,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput | Usage.ShaderInput | Usage.BackBuffer
            };

            // Used for debugging dispose object references
            // Configuration.EnableObjectTracking = true;

            // Disable throws on shader compilation errors
            //Configuration.ThrowOnShaderCompileError = false;

            // Create Device and SwapChain

            SharpDX.Direct3D11.Device.CreateWithSwapChain(SharpDX.Direct3D.DriverType.Hardware, DeviceCreationFlags.None, desc, out _device, out _swapChain);
            _context = _device.ImmediateContext;
            _gdevice = GraphicsDevice.New(_device);
            _renderTarget = RenderTarget2D.New(_gdevice, renderTargetSize.Width, renderTargetSize.Height, new MipMapCount(1), PixelFormat.R8G8B8A8.UNorm, TextureFlags.RenderTarget | TextureFlags.ShaderResource);
            _depthStencilBuffer = DepthStencilBuffer.New(_gdevice, renderTargetSize.Width, renderTargetSize.Height, DepthFormat.Depth32, true);
            _sprite = new SpriteBatch(_gdevice);

            _swapChain.GetParent<Factory>().MakeWindowAssociation(_form.Handle, WindowAssociationFlags.IgnoreAll);

            var backBuffer = SharpDX.Direct3D11.Texture2D.FromSwapChain<SharpDX.Direct3D11.Texture2D>(_swapChain, 0);

            // Renderview on the backbuffer
            _renderView = new RenderTargetView(_device, backBuffer);

            // Create the depth buffer
            var depthBuffer = new SharpDX.Direct3D11.Texture2D(_device, new Texture2DDescription()
            {
                Format = Format.D32_Float_S8X24_UInt,
                ArraySize = 1,
                MipLevels = 1,
                Width = _form.ClientSize.Width,
                Height = _form.ClientSize.Height,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            });

            // Create the depth buffer view
            _depthView = new DepthStencilView(_device, depthBuffer);

            model = null;
            _model = null;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Load a 3D model
            // The [Ship.fbx] file is defined with the build action [ToolkitModel] in the project
            //model = Content.Load<Texture2D>("test");
            // Enable default lighting on model.
            //BasicEffect.EnableDefaultLighting(model, true);

            /*
            var cvimg = OpenCV.Net.CV.LoadImage(@"Content\test.png", OpenCV.Net.LoadImageFlags.Unchanged);
            System.Drawing.Bitmap bmp;
            
            if (cvimg.Depth == OpenCV.Net.IplDepth.U8 && cvimg.Channels == 3)
            {
                bmp = new System.Drawing.Bitmap(cvimg.Width, cvimg.Height, cvimg.WidthStep, System.Drawing.Imaging.PixelFormat.Format24bppRgb, cvimg.ImageData);
            }
            else
            {
                bmp = new System.Drawing.Bitmap(cvimg.Width, cvimg.Height, cvimg.WidthStep, System.Drawing.Imaging.PixelFormat.Format32bppArgb, cvimg.ImageData);
            
            }

            var stream = new MemoryStream();
            bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            bmp.Dispose();
            stream.Flush();
            stream.Seek(0, SeekOrigin.Begin);

            var img = Image.Load(stream);
            stream.Close();
            */

            //var d = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);
            //var buff = SharpDX.Utilities.AllocateMemory(d.Height * d.Stride);
            //var buff = new byte[d.Height * d.Stride];
            //Marshal.Copy(d.Scan0, buff, 0, buff.Length);
            //var img = Image.Load(buff);
            //var img = Image.Load(new DataPointer(d.Scan0, d.Height * d.Stride), true);

            //var img = Image.New2D(d.Width, d.Height, MipMapCount.Auto, PixelFormat.R8G8B8A8.UNorm, 1, d.Scan0);
            //bmp.UnlockBits(d);
            
            //var img = Image.Load(new DataPointer(cvimg.ImageData, cvimg.Height * cvimg.WidthStep));
            
            //Image.New(new ImageDescription()
            //{
            //    ArraySize = 1,
            //    Dimension = TextureDimension.Texture2D,
            //    Format = Format.R8G8B8A8_SNorm,
            //    Height = cvimg.Height,
            //    Width = cvimg.Width,
            //    MipLevels = 0,
            //}, cvimg.ImageData);

            //model = Texture2D.New(GraphicsDevice, img, usage: ResourceUsage.Dynamic);

            //img.Dispose();
            //cvimg.Dispose();

            _form.Show();

            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            keyMgr.Update(gameTime);

            var k = keyMgr.GetState();

            if (k.IsKeyDown(Keys.Escape))
                Exit();

            if (k.IsKeyDown(Keys.Enter) && k.IsKeyDown(Keys.Alt))
            {
                //this.graphicsDeviceManager.IsFullScreen = true;
                //Window.BeginScreenDeviceChange(true);
                //// Set game to fullscreen on rift
                //var swapChain = (SwapChain)GraphicsDevice.Presenter.NativePresenter;
                //var description = swapChain.Description.ModeDescription;
                //swapChain.ResizeTarget(ref description);
                //swapChain.SetFullscreenState(true, null);
                //Window.EndScreenDeviceChange();
                //_swapChain.SetFullscreenState(true, null);
            }

            base.Update(gameTime);

            // Calculates the world and the view based on the model size
            view = Matrix.LookAtRH(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0, 0.0f, 0), Vector3.UnitY);
            projection = Matrix.PerspectiveFovRH(0.9f, (float)GraphicsDevice.BackBuffer.Width / GraphicsDevice.BackBuffer.Height, 0.1f, 100.0f);
        }

        protected override bool BeginDraw()
        {
            if (!base.BeginDraw())
                return false;

            // Set Render Target and Viewport
            GraphicsDevice.SetRenderTargets(depthStencilBuffer, renderTarget);
            GraphicsDevice.SetViewport(0f, 0f, (float)renderTarget.Width, (float)renderTarget.Height);

            //_gdevice.SetRenderTargets(_depthStencilBuffer, _renderTarget);
            //_gdevice.SetViewport(0f, 0f, (float)_renderTarget.Width, (float)_renderTarget.Height);

            // Setup targets and viewport for rendering
            _context.Rasterizer.SetViewport(new Viewport(0, 0, _form.ClientSize.Width, _form.ClientSize.Height, 0.0f, 1.0f));
            _context.OutputMerger.SetTargets(_depthView, _renderView);


            // Begin frame
            hmd.BeginFrame(0);
            return true;
        }

        protected override void Draw(GameTime gameTime)
        {
            // Clear the screen
            //GraphicsDevice.Clear(Color.CornflowerBlue);

            CaptureModel(gameTime);

            for (int eyeIndex = 0; eyeIndex < 2; eyeIndex++)
            {
                var eye = hmd.EyeRenderOrder[eyeIndex];
                var renderDesc = eyeRenderDesc[(int)eye];
                var renderViewport = eyeRenderViewport[(int)eye];
                hmd.GetEyePoses(0, eyeViewOffset, renderPose);
                var pose = renderPose[(int)eye];

                // Calculate view matrix                
                var rollPitchYaw = Matrix.RotationY(bodyYaw);
                var finalRollPitchYaw = rollPitchYaw * pose.Orientation.GetMatrix();
                var finalUp = finalRollPitchYaw.Transform(Vector3.UnitY);
                var finalForward = finalRollPitchYaw.Transform(-Vector3.UnitZ);
                var shiftedEyePos = headPos + rollPitchYaw.Transform(pose.Position);
                view = Matrix.Translation(renderDesc.HmdToEyeViewOffset) * Matrix.LookAtRH(shiftedEyePos, shiftedEyePos + finalForward, finalUp);

                // Calculate projection matrix
                projection = OVR.MatrixProjection(renderDesc.Fov, 0.001f, 1.0f, true);
                projection.Transpose();

                // Set Viewport for our eye
                GraphicsDevice.SetViewport(renderViewport.ToViewportF());

                // Perform the actual drawing
                InternalDraw(gameTime);
            }


            _sprite.Begin();
            var pos = new Vector2(
                _form.ClientSize.Width / 2 - _model.Width / 2,
                _form.ClientSize.Height /2 - _model.Height / 2
            );
            _sprite.Draw(_model, pos, Color.White);
            _sprite.End();
        }

        protected override void EndDraw()
        {
            // Cancel original EndDraw() as the Present call is made through hmd.EndFrame()
            hmd.EndFrame(renderPose, eyeTexture);

            _swapChain.Present(0, PresentFlags.None);
        }

        private double lastCapture = 0;

        protected virtual void CaptureModel(GameTime gameTime)
        {
            var time = gameTime.TotalGameTime.TotalMilliseconds;

            var diff = time - 100;
            diff = diff < 0 ? 0 : diff;

            //if (lastCapture > diff)
            //    return;

            OpenCV.Net.IplImage img;
            if (!capture.GrabFrame())
            {
                img = OpenCV.Net.CV.LoadImage(@"Content\test.png", OpenCV.Net.LoadImageFlags.Unchanged);
            }
            else
            {
                img = capture.RetrieveFrame();
            }

            System.Drawing.Bitmap bmp;
            if (img.Depth == OpenCV.Net.IplDepth.U8)
            {
                if (img.Channels == 3)
                    bmp = new System.Drawing.Bitmap(img.Width, img.Height, img.WidthStep, System.Drawing.Imaging.PixelFormat.Format24bppRgb, img.ImageData);
                else //if (img.Channels == 4)
                    bmp = new System.Drawing.Bitmap(img.Width, img.Height, img.WidthStep, System.Drawing.Imaging.PixelFormat.Format32bppArgb, img.ImageData);
            }
            else
            {
                bmp = new System.Drawing.Bitmap(img.Width, img.Height, img.WidthStep, System.Drawing.Imaging.PixelFormat.Format8bppIndexed, img.ImageData);
            }

            //var bmp = (System.Drawing.Bitmap)System.Drawing.Bitmap.FromFile(@"Content\test.png");
            //System.Drawing.Bitmap newBmp = new System.Drawing.Bitmap(bmp, hmd.Resolution.Width, hmd.Resolution.Height * 2);
            /*
            using (System.Drawing.Graphics gr = System.Drawing.Graphics.FromImage(newBmp))
            {
                gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                gr.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                gr.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                gr.DrawImage((System.Drawing.Image)bmp, new System.Drawing.Rectangle(0, 0, hmd.Resolution.Width, hmd.Resolution.Height * 2));
            }
            */
            var stream = new MemoryStream();
            bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            stream.Flush();
            //newBmp.Dispose();

            stream.Seek(0, SeekOrigin.Begin);
            if (model != null)
            {
                var tmpimg = Image.Load(stream);
                model.SetData(tmpimg);
                _model.SetData(tmpimg);
                tmpimg.Dispose();
            }
            else
            {
                model = Texture2D.Load(GraphicsDevice, stream, usage: ResourceUsage.Dynamic);
                stream.Seek(0, SeekOrigin.Begin);
                _model = Texture2D.Load(_gdevice, stream, usage: ResourceUsage.Dynamic);
            }

            position.X = hmd.Resolution.Width / 2 - model.Width;
            position.Y = hmd.Resolution.Height / 2;

            lastCapture = time;

            bmp.Dispose();
            stream.Close();
            img.Dispose();
        }

        protected virtual void InternalDraw(GameTime gameTime)
        {
            // Use time in seconds directly
            var time = gameTime.TotalGameTime.TotalMilliseconds;

            // ------------------------------------------------------------------------
            // Draw the 3d model
            // ------------------------------------------------------------------------
            /*
            var world = Matrix.Scaling(0.003f) *
                        Matrix.RotationY(time) *
                        Matrix.Translation(0, -1.5f, 2.0f);
            */
            //model.Draw(GraphicsDevice, world, view, projection);


            /*
            var context = (DeviceContext)GraphicsDevice;
            DataStream ds;
            context.MapSubresource(model, 0, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None, out ds);
            var bmp = (System.Drawing.Bitmap)System.Drawing.Bitmap.FromFile(@"Content\test.png");
            var stream = new MemoryStream();
            bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            bmp.Dispose();
            stream.Flush();
            stream.Seek(0, SeekOrigin.Begin);

            var img = Image.Load(stream);
            stream.Close();

            ds.Write(img.DataPointer, 0, img.TotalSizeInBytes);
            bmp.Dispose();
            ds.Close();
            context.UnmapSubresource(model, 0);
            */

            //var d = b.LockBits(new System.Drawing.Rectangle(0, 0, b.Width, b.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            //stream.Write(d.Scan0, 0, d.Height * d.Stride);
            //b.UnlockBits(d);

            //model = Texture2D.Load(, @"Content\test.png");

            sprite.Begin();
            sprite.Draw(model, position, Color.White);
            sprite.End();

            //model.Dispose();

            base.Draw(gameTime);
        }

        protected override void Dispose(bool disposeManagedResources)
        {
            base.Dispose(disposeManagedResources);
            if (disposeManagedResources)
            {
                // Release the HMD
                hmd.Dispose();

                // Shutdown the OVR Library
                OVR.Shutdown();
            }            
        }
    }
}
