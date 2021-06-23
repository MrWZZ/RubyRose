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
        int borderThick = 3;
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
            Left = x - borderThick;
            Top = y - borderThick;

            winWidth = width + borderThick * 2;
            winHeight = height + borderThick * 2;

            Width = winWidth;
            Height = winHeight;
            canvas.Width = width;
            canvas.Height = height;
            canvas.Margin = new Thickness(borderThick);

            border.BorderThickness = new Thickness(borderThick);
            SetInkPos();
            inkWin.Show();
        }

        private void SetInkPos()
        {
            inkWin.SetPos(Left, Top + Height);
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
                    (int)Left + borderThick,
                    (int)Top + borderThick,
                    (int)Width - borderThick * 2,
                    (int)Height - borderThick * 2);

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
                inkWin.Show();
            }
            else
            {
                Width = 100;
                Height = 100;

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
