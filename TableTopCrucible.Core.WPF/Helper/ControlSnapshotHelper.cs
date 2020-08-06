using Microsoft.Win32.SafeHandles;

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TableTopCrucible.Core.WPF.Helper
{
    public static class VisualUtility
    {
        public static BitmapSource CreateBitmap(
            this Visual visualToRender,
            double width,
            double height,
            bool undoTransformation)
        {
            if (visualToRender == null)
            {
                return null;
            }

            // The PixelsPerInch() helper method is used to read the screen DPI setting.
            // If you need to create a bitmap with a specified resolution, you could directly
            // pass the specified dpiX and dpiY values to RenderTargetBitmap constructor.
            RenderTargetBitmap bmp = new RenderTargetBitmap((Int32)Math.Ceiling(width),
                                                            (Int32)Math.Ceiling(height),
                                                            (Double)DeviceHelper.PixelsPerInch(Orientation.Horizontal),
                                                            (Double)DeviceHelper.PixelsPerInch(Orientation.Vertical),
                                                            PixelFormats.Pbgra32);

            // If we want to undo the transform, we could use VisualBrush trick.
            if (undoTransformation)
            {
                DrawingVisual dv = new DrawingVisual();
                using (DrawingContext dc = dv.RenderOpen())
                {
                    VisualBrush vb = new VisualBrush(visualToRender);
                    dc.DrawRectangle(vb, null, new Rect(new Point(), new Size(width, height)));
                }
                bmp.Render(dv);
            }
            else
            {
                bmp.Render(visualToRender);
            }

            return bmp;
        }
    }

    internal class DeviceHelper
    {
        public static Int32 PixelsPerInch(Orientation orientation)
        {
            Int32 capIndex = (orientation == Orientation.Horizontal) ? 0x58 : 90;
            using (DCSafeHandle handle = UnsafeNativeMethods.CreateDC("DISPLAY"))
            {
                return (handle.IsInvalid ? 0x60 : UnsafeNativeMethods.GetDeviceCaps(handle, capIndex));
            }
        }
    }

    internal sealed class DCSafeHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private DCSafeHandle() : base(true) { }

        protected override Boolean ReleaseHandle()
        {
            return UnsafeNativeMethods.DeleteDC(base.handle);
        }
    }

    [SuppressUnmanagedCodeSecurity]
    internal static class UnsafeNativeMethods
    {
        [DllImport("gdi32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern Boolean DeleteDC(IntPtr hDC);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern Int32 GetDeviceCaps(DCSafeHandle hDC, Int32 nIndex);

        [DllImport("gdi32.dll", EntryPoint = "CreateDC", CharSet = CharSet.Auto)]
        public static extern DCSafeHandle IntCreateDC(String lpszDriver,
            String lpszDeviceName, String lpszOutput, IntPtr devMode);

        public static DCSafeHandle CreateDC(String lpszDriver)
        {
            return UnsafeNativeMethods.IntCreateDC(lpszDriver, null, null, IntPtr.Zero);
        }
    }
}
