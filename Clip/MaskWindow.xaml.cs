using System.Windows;
using System.Windows.Media;

namespace Clip
{
    /// <summary>
    /// MaskWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MaskWindow : Window
    {
        public MaskWindow()
        {
            InitializeComponent();
        }

        public void SetPos(System.Windows.Forms.Screen screen)
        {
            Left = screen.Bounds.Left;
            Top = screen.Bounds.Top;
            Width = screen.Bounds.Width;
            Height = screen.Bounds.Height;

            Reset();
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == System.Windows.Input.Key.Escape)
            {
                ClipTool.Instance.CloseMaskScreen();
            }
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ClipTool.Instance.isClick = true;
            borderFrame.BorderThickness = new Thickness(ClipTool.SMALL_BORDER_TH);

            ClipTool.Instance.startPoint = System.Windows.Forms.Cursor.Position;

            var reY = System.Windows.Forms.Cursor.Position.Y - Top;
            var reX = System.Windows.Forms.Cursor.Position.X - Left;

            topMask.Height = reY;
            leftMask.Width = reX;
            bottomMask.Height = Height - reY;
            rightMask.Width = Width - reX;
        }

        private void Window_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ClipTool.Instance.UpdateMouseMove();
        }

        public void UpdateMouseMove()
        {
            if (ClipTool.Instance.isClick)
            {
                var reY = System.Windows.Forms.Cursor.Position.Y - Top;
                var reX = System.Windows.Forms.Cursor.Position.X - Left;

                if(IsUpdateWidth())
                {
                    if (System.Windows.Forms.Cursor.Position.X < ClipTool.Instance.startPoint.X)
                    {
                        leftMask.Width = reX;
                    }
                    else
                    {
                        rightMask.Width = Width - reX;
                    }
                }

                if (System.Windows.Forms.Cursor.Position.Y < ClipTool.Instance.startPoint.Y)
                {
                    topMask.Height = reY;
                }
                else
                {
                    bottomMask.Height = Height - reY;
                }
            }
        }

        private bool IsUpdateWidth()
        {
            if(Left < System.Windows.Forms.Cursor.Position.X && Left + Width > System.Windows.Forms.Cursor.Position.X)
            {
                return true;
            }
            return false;
        }

        private void Window_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ClipTool.Instance.isClick = false;
            ClipTool.Instance.ClipScreen();
        }

        private void Window_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if(ClipTool.Instance.isClick)
            {
                borderFrame.BorderThickness = new Thickness(ClipTool.SMALL_BORDER_TH);

                if (System.Windows.Forms.Cursor.Position.X < Left + Width / 2)
                {
                    leftMask.Width = 0;
                }
                else
                {
                    rightMask.Width = 0;
                }    

                var reY = ClipTool.Instance.startPoint.Y - Top;
                if (System.Windows.Forms.Cursor.Position.Y < ClipTool.Instance.startPoint.Y)
                {
                    bottomMask.Height = Height - reY;
                }
                else
                {
                    topMask.Height = reY;
                }
            }
        }

        private void Window_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (ClipTool.Instance.isClick)
            {
                if(ClipTool.Instance.IsInScreen(this))
                {
                    if(System.Windows.Forms.Cursor.Position.X > Left + Width)
                    {
                        rightMask.Width = 0;
                    }

                    if(System.Windows.Forms.Cursor.Position.X < Left)
                    {
                        leftMask.Width = 0;
                    }
                }
                else
                {
                    Reset();
                }
            }
        }

        public void Reset()
        {
            topMask.Height = Height / 2;
            bottomMask.Height = Height / 2;
            leftMask.Width = Width / 2;
            rightMask.Width = Width / 2;
            borderFrame.BorderThickness = new Thickness(0);
        }
    }
}
