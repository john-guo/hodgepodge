using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wrench
{
    /// <summary>
    /// Used to draw a moving icon on screen. Each change in position requires a call
    /// to DrawIcon. To remove the icon from the screen call ClearIcon.
    /// </summary>
    public class CursorFollower
    {
        /// <summary>
        /// The max_size specifies the max width and height for the icons you will be using
        /// Keep it at a minimum for optimal performance and memory use
        /// </summary>
        private const int max_size = 32;
        private System.Drawing.Point lastCursorDrawLocation;
        private System.Drawing.Bitmap screenCaptureBuffer;
        private System.Drawing.Bitmap overlappedScreenCaptureBuffer;
        private System.Drawing.Bitmap overlappedRenderBuffer;

        [System.Runtime.InteropServices.DllImport("User32.dll")]
        private static extern IntPtr GetDC(IntPtr hwnd);
        [System.Runtime.InteropServices.DllImport("User32.dll")]
        private static extern int ReleaseDC(IntPtr hwnd, IntPtr dc);

        public CursorFollower()
        {
            screenCaptureBuffer = new System.Drawing.Bitmap(max_size, max_size);
            overlappedScreenCaptureBuffer = new System.Drawing.Bitmap(max_size, max_size);
            overlappedRenderBuffer = new System.Drawing.Bitmap(max_size * 3, max_size * 3);
            lastCursorDrawLocation = System.Drawing.Point.Empty;
        }

        ~CursorFollower()
        {
            ClearIcon();
            screenCaptureBuffer.Dispose();
            screenCaptureBuffer = null;
            overlappedScreenCaptureBuffer.Dispose();
            overlappedScreenCaptureBuffer = null;
            overlappedRenderBuffer.Dispose();
            overlappedRenderBuffer = null;
        }

        /// <summary>
        /// Place an icon on the screen at a specified location, erasing any previously
        /// drawn icon. If a control has drawn over the old position, an artifact will
        /// be left behind.
        /// </summary>
        /// <param name="source">The icon to draw. It must have dimensions equal to or less than CursorFollower.max_size.</param>
        /// <param name="drawPoint">The screen location for the upper left corner of the icon</param>
        public void DrawIcon(System.Drawing.Icon source, System.Drawing.Point drawPoint)
        {
            if (source.Width > max_size || source.Height > max_size) throw new System.ArgumentException("The icon's width or height is larger than max_size", "System.Drawing.Icon source");
            System.Drawing.Point pointMax = new System.Drawing.Point(max_size, max_size);
            System.Drawing.Size sizeMax = new System.Drawing.Size(max_size, max_size);
            IntPtr desktopDC = GetDC(IntPtr.Zero);
            System.Drawing.Graphics desktopGraphics = System.Drawing.Graphics.FromHdc(desktopDC);

            System.Drawing.Graphics graphicsScreenCaptureBuffer = System.Drawing.Graphics.FromImage(screenCaptureBuffer);

            if (lastCursorDrawLocation != System.Drawing.Point.Empty)
            {
                //there is an old copy on the screen that must be removed
                if (System.Math.Abs(lastCursorDrawLocation.X - drawPoint.X) < max_size && System.Math.Abs(lastCursorDrawLocation.Y - drawPoint.Y) < max_size)
                {
                    //if the new location and old location overlap, make one bitmap that represents the erase and redraw to remove flicker
                    System.Drawing.Point capOffset = lastCursorDrawLocation - new System.Drawing.Size(drawPoint);

                    System.Drawing.Graphics graphicsOverlappedScreenCaptureBuffer = System.Drawing.Graphics.FromImage(overlappedScreenCaptureBuffer);
                    graphicsOverlappedScreenCaptureBuffer.CopyFromScreen(drawPoint.X, drawPoint.Y, 0, 0, sizeMax);
                    graphicsOverlappedScreenCaptureBuffer.DrawImageUnscaled(screenCaptureBuffer, capOffset);
                    graphicsOverlappedScreenCaptureBuffer.Dispose();

                    System.Drawing.Graphics graphicsOverlappedRenderBuffer = System.Drawing.Graphics.FromImage(overlappedRenderBuffer);
                    graphicsOverlappedRenderBuffer.Clear(System.Drawing.Color.Transparent);
                    graphicsOverlappedRenderBuffer.DrawImageUnscaled(screenCaptureBuffer, capOffset + sizeMax);
                    graphicsOverlappedRenderBuffer.DrawIconUnstretched(source, new System.Drawing.Rectangle(pointMax, sizeMax));
                    graphicsOverlappedRenderBuffer.Dispose();

                    desktopGraphics.DrawImageUnscaled(overlappedRenderBuffer, drawPoint - sizeMax);

                    graphicsScreenCaptureBuffer.DrawImageUnscaled(overlappedScreenCaptureBuffer, 0, 0);
                }
                else
                {
                    //the new and old location don't overlap
                    desktopGraphics.DrawImageUnscaled(screenCaptureBuffer, lastCursorDrawLocation.X, lastCursorDrawLocation.Y);
                    graphicsScreenCaptureBuffer.CopyFromScreen(drawPoint.X, drawPoint.Y, 0, 0, sizeMax);
                    desktopGraphics.DrawIconUnstretched(source, new System.Drawing.Rectangle(drawPoint, sizeMax));
                }
            }
            else
            {
                //no icon to erase from screen
                graphicsScreenCaptureBuffer.CopyFromScreen(drawPoint.X, drawPoint.Y, 0, 0, sizeMax);
                desktopGraphics.DrawIconUnstretched(source, new System.Drawing.Rectangle(drawPoint, sizeMax));
            }
            lastCursorDrawLocation = drawPoint;
            graphicsScreenCaptureBuffer.Dispose();
            desktopGraphics.Dispose();
            int r = ReleaseDC(IntPtr.Zero, desktopDC);
        }

        /// <summary>
        /// Remove existing icon from the screen
        /// </summary>
        public void ClearIcon()
        {
            if (lastCursorDrawLocation != System.Drawing.Point.Empty)
            {
                IntPtr desktopDC = GetDC(IntPtr.Zero);
                System.Drawing.Graphics gfx = System.Drawing.Graphics.FromHdc(desktopDC);
                gfx.DrawImageUnscaled(screenCaptureBuffer, lastCursorDrawLocation.X, lastCursorDrawLocation.Y);
                lastCursorDrawLocation = System.Drawing.Point.Empty;
            }
        }
    }
}
