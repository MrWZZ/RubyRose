using System;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Runtime.InteropServices;
using System.IO;
using System.Windows;

namespace ScreenLib
{
    public sealed class ScreenTool
    {
        #region 单例

        private ScreenTool() { }
        private static ScreenTool instance;
        public static ScreenTool Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ScreenTool();
                }
                return instance;
            }
        }

        #endregion

        [DllImport("gdi32")]
        static extern int DeleteObject(IntPtr o);

        public Bitmap GetBitmapFromScreen(int x, int y, int width, int height)
        {
            var bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            using (Graphics memoryGrahics = Graphics.FromImage(bitmap))
            {
                memoryGrahics.CopyFromScreen(x, y, 0, 0,  new System.Drawing.Size(width, height), CopyPixelOperation.SourceCopy);
            }

            return bitmap;
        }

        #region BitmapSource

        public BitmapSource GetBitmapSourceFromScreen(int x, int y, int width, int height)
        {
            var bitmap = GetBitmapFromScreen(x, y, width, height);
            return BitmapToBitmapSource(bitmap);
        }

        public BitmapSource GetBitmapSourceFromScreen(System.Windows.Forms.Screen screen)
        {
            var bitmap = GetBitmapFromScreen(screen.Bounds.X, screen.Bounds.Y, screen.Bounds.Size.Width, screen.Bounds.Size.Height);
            return BitmapToBitmapSource(bitmap);
        }

        public BitmapSource BitmapToBitmapSource(Bitmap bitmap)
        {
            IntPtr intPtrl = bitmap.GetHbitmap();
            BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(intPtrl, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            DeleteObject(intPtrl);
            return bitmapSource;
        }

        #endregion

        #region BitmapImage

        public BitmapImage GetBitmapImageFromScreen(int x, int y, int width, int height)
        {
            var bitmap = GetBitmapFromScreen(x, y, width, height);
            return BitmapToBitmapImage(bitmap);
        }

        public BitmapImage GetBitmapImageFromScreen(System.Windows.Forms.Screen screen)
        {
            var bitmap = GetBitmapFromScreen(screen.Bounds.X, screen.Bounds.Y, screen.Bounds.Size.Width, screen.Bounds.Size.Height);
            return BitmapToBitmapImage(bitmap);
        }

        public BitmapImage BitmapToBitmapImage(Bitmap bitmap)
        {
            BitmapImage bitmapImage = new BitmapImage();
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = ms;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
            }

            return bitmapImage;
        }

        #endregion  
    }
}
