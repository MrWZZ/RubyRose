using ScreenLib;
using System.Collections.Generic;
using System.Windows;

namespace Clip
{
    public sealed class ClipTool
    {
        #region 单例

        private ClipTool() { }
        private static ClipTool instance;
        public static ClipTool Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ClipTool();
                }
                return instance;
            }
        }

        #endregion

        public bool isClick;
        public System.Drawing.Point startPoint;

        List<MaskWindow> maskWindowList = new List<MaskWindow>();
        List<ClipWindow> clipWindowList = new List<ClipWindow>();

        #region 页面遮罩

        public void ShowMaskScreen()
        {
            foreach (var screen in System.Windows.Forms.Screen.AllScreens)
            {
                var screenWindow = new MaskWindow();
                var bitmapSource = ScreenTool.Instance.GetBitmapSourceFromScreen(screen);
                screenWindow.background_image.Source = bitmapSource;
                screenWindow.SetPos(screen);
                screenWindow.Show();
                maskWindowList.Add(screenWindow);
            }
        }

        public void CloseMaskScreen()
        {
            foreach (var screen in maskWindowList)
            {
                screen.Close();
            }
            maskWindowList.Clear();
        }

        public void UpdateMouseMove()
        {
            var enterList = new List<MaskWindow>();
            foreach (var item in maskWindowList)
            {
                if (IsInScreen(item))
                {
                    enterList.Add(item);
                }
            }

            foreach (var item in enterList)
            {
                item.UpdateMouseMove();
            }
        }

        public bool IsInScreen(Window win)
        {
            var right = win.Left + win.Width;
            if (
                (right < startPoint.X && right < System.Windows.Forms.Cursor.Position.X) ||
                (win.Left > startPoint.X && win.Left > System.Windows.Forms.Cursor.Position.X)
               )
            {
                return false;
            }
            return true;
        }

        #endregion

        #region 剪切窗口

        /// <summary>
        /// 打开截屏蒙版，准备截屏
        /// </summary>
        public void ClipScreen()
        {
            var minX = System.Math.Min(startPoint.X, System.Windows.Forms.Cursor.Position.X);
            var minY = System.Math.Min(startPoint.Y, System.Windows.Forms.Cursor.Position.Y);

            var maxX = System.Math.Max(startPoint.X, System.Windows.Forms.Cursor.Position.X);
            var maxY = System.Math.Max(startPoint.Y, System.Windows.Forms.Cursor.Position.Y);

            var width = maxX - minX;
            var height = maxY - minY;

            if(width <= 0 || height <= 0)
            {
                return;
            }

            ResetMaskWindow();

            var source = ScreenTool.Instance.GetBitmapSourceFromScreen(minX, minY, width, height);

            var clipWin = new ClipWindow();
            clipWin.SetPos(minX, minY, width, height);
            clipWin.background.Source = source;
            clipWin.Show();

            clipWindowList.Add(clipWin);

            CloseMaskScreen();
        }

        public void CloseClipScreen(ClipWindow window)
        {
            clipWindowList.Remove(window);
            window.Close();
        }

        public void CloseAllClipScreen()
        {
            foreach (var item in clipWindowList)
            {
                item.Close();
            }
        }


        #endregion

        void ResetMaskWindow()
        {
            foreach (var item in maskWindowList)
            {
                item.Reset();
            }
        }

        void Clear()
        {
            foreach (var item in maskWindowList)
            {
                item.Close();
            }
            maskWindowList.Clear();
            foreach (var item in clipWindowList)
            {
                item.Close();
            }
            clipWindowList.Clear();
        }
    }
}
