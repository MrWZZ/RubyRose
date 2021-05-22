using ScreenLib;
using System.Windows;
using System.Windows.Input;

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

        public ClipWindow()
        {
            InitializeComponent();
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
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                Close();
            }
        }

        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SmallerWindow();
        }

        private void Window_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
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
            }
            else
            {
                Width = 100;
                Height = 100;
            }

            isMinShow = !isMinShow;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
