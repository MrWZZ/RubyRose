using ScreenLib;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Clip
{
    /// <summary>
    /// ClipWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ClipWindow : Window
    {
        bool isMinShow = false;
        int winWidth = 0;
        int winHeight = 0;

        InkCanv inkWin;

        public ClipWindow()
        {
            InitializeComponent();

            ink.IsHitTestVisible = false;
            inkWin = new InkCanv();
            inkWin.Init(this);

            CompositionTarget.Rendering += new EventHandler(WindowPositionChange);

            Closed += ClipWindow_Closed;
        }

        private void ClipWindow_Closed(object sender, EventArgs e)
        {
            inkWin.Close();
        }

        //位置发生变化
        void WindowPositionChange(object sender, EventArgs e)
        {
            SetInkPos();
        }

        public void SetPos(int x, int y, int width, int height)
        {
            Left = x - ClipTool.SMALL_BORDER_TH;
            Top = y - ClipTool.SMALL_BORDER_TH;

            winWidth = width + ClipTool.SMALL_BORDER_TH * 2;
            winHeight = height + ClipTool.SMALL_BORDER_TH * 2;

            Width = winWidth;
            Height = winHeight;
            canvas.Width = width;
            canvas.Height = height;
            canvas.Margin = new Thickness(ClipTool.SMALL_BORDER_TH);

            border.BorderThickness = new Thickness(ClipTool.SMALL_BORDER_TH);
            SetInkPos();

            inkWin.Show();
            inkWin.AutoCheckVisible(Width);
        }

        private void SetInkPos()
        {
            inkWin.SetPos(Left + Width, Top + Height);
        }

        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            SmallerWindow();
        }

        private void Window_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ink.IsHitTestVisible)
            {
                ink.IsHitTestVisible = false;
                return;
            }
                
            ClipImageToClipboard();
        }

        private void ClipImageToClipboard()
        {
            if (!isMinShow)
            {
                var source = ScreenTool.Instance.GetBitmapSourceFromScreen(
                    (int)Left + ClipTool.SMALL_BORDER_TH,
                    (int)Top + ClipTool.SMALL_BORDER_TH,
                    (int)Width - ClipTool.SMALL_BORDER_TH * 2,
                    (int)Height - ClipTool.SMALL_BORDER_TH * 2);

                Clipboard.SetImage(source);
                ClipTool.Instance.CloseClipScreen(this);
            }
        }

        private void SmallerWindow()
        {
            if (isMinShow)
            {
                Width = winWidth;
                Height = winHeight;

                border.BorderThickness = new Thickness(ClipTool.SMALL_BORDER_TH);

                inkWin.Show();
            }
            else
            {
                if (Width < ClipTool.MIN_WIDTH && Height < ClipTool.MIN_HEIGHT)
                    return;

                border.BorderThickness = new Thickness(ClipTool.BIG_BORDER_TH);

                var minW = Math.Min(Width, ClipTool.MIN_WIDTH);
                var minH = Math.Min(Height, ClipTool.MIN_HEIGHT);

                Width = minW;
                Height = minH;

                inkWin.VisiblePanel(false);
                ink.IsHitTestVisible = false;
                inkWin.Hide();
            }

            isMinShow = !isMinShow;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
