using ProcessViewerTestApp.WinApi;
using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ProcessViewerTestApp.Extensions
{
    public static class IconExtensions
    {
        public static ImageSource ToImageSource(this Icon icon)
        {
            if (icon == null)
                return null;

            //Bitmap bitmap = icon.ToBitmap();
            //IntPtr hBitmap = bitmap.GetHbitmap();

            //ImageSource wpfBitmap = Imaging.CreateBitmapSourceFromHBitmap(
            //    hBitmap,
            //    IntPtr.Zero,
            //    Int32Rect.Empty,
            //    BitmapSizeOptions.FromEmptyOptions());

            //NativeMethods.DeleteObjectByHandle(hBitmap);

            //return wpfBitmap;

            try
            {
                using (Bitmap bmp = icon.ToBitmap())
                {
                    var stream = new MemoryStream();
                    bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                    return BitmapFrame.Create(stream);
                }
            }
            finally { NativeMethods.DeleteObjectByHandle(icon.Handle); }

        }

    }
}
