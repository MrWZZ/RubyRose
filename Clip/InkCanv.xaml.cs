using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Clip
{
    /// <summary>
    /// InkCanv.xaml 的交互逻辑
    /// </summary>
    public partial class InkCanv : Window
    {
        InkCanvas ink;
        ClipWindow win;
        System.Windows.Ink.DrawingAttributes inkAttr;
        List<float> lineSizes = new List<float>() { 3, 4, 6, 9, 15, 20, 25 };
        List<float> lineAlphas = new List<float>() { 0.2f, 0.4f, 0.6f, 0.8f, 1f };
        List<LineColor> lineColors = new List<LineColor>() { LineColor.Red, LineColor.Green, LineColor.Blue, LineColor.Yellow };

        public InkCanv()
        {
            InitializeComponent();
        }

        public void Init(ClipWindow clipWin)
        {
            win = clipWin;
            this.ink = clipWin.ink;

            ink.EditingMode = InkCanvasEditingMode.None;
            inkAttr = ink.DefaultDrawingAttributes;

            lineSizeCb.ItemsSource = lineSizes;
            lineAlphaCb.ItemsSource = lineAlphas;
            lineColorCb.ItemsSource = lineColors;

            lineSizeCb.SelectedIndex = 2;
            lineColorCb.SelectedIndex = 0;
            lineAlphaCb.SelectedIndex = 2;
        }

        public void SetPos(double x, double y)
        {
            Left = x - Width;
            Top = y;
        }

        private void lineBtn_Click(object sender, RoutedEventArgs e)
        {
            ink.IsHitTestVisible = true;
            ink.EditingMode = InkCanvasEditingMode.Ink;
        }

        private void lineSelectBtn_Click(object sender, RoutedEventArgs e)
        {
            ink.IsHitTestVisible = true;
            ink.EditingMode = InkCanvasEditingMode.Select;
        }

        private void lineClearBtn_Click(object sender, RoutedEventArgs e)
        {
            ink.Strokes.Clear();
        }

        private void xBtn_Click(object sender, RoutedEventArgs e)
        {
            ClipTool.Instance.CloseClipScreen(win);
        }

        public void lineColorCb_Selected(object sender, RoutedEventArgs e)
        {
            ChangeColor();
        }

        private void ChangeColor()
        {
            if (lineColorCb.SelectedIndex < 0 || lineAlphaCb.SelectedIndex < 0)
                return;

            switch (lineColors[lineColorCb.SelectedIndex])
            {
                case LineColor.Red:
                    inkAttr.Color = Colors.Red;
                    break;
                case LineColor.Green:
                    inkAttr.Color = Colors.Green;
                    break;
                case LineColor.Blue:
                    inkAttr.Color = Colors.Blue;
                    break;
                case LineColor.Yellow:
                    inkAttr.Color = Colors.Yellow;
                    break;
            }

            var alphaC = inkAttr.Color;
            alphaC.A = (byte)(255 * lineAlphas[lineAlphaCb.SelectedIndex]);
            inkAttr.Color = alphaC;
        }

        public void lineSizeCb_Selected(object sender, RoutedEventArgs e)
        {
            var index = lineSizeCb.SelectedIndex;
            inkAttr.Width = lineSizes[index];
            inkAttr.Height = lineSizes[index];
        }

        public enum LineColor
        {
            Red,
            Green,
            Blue,
            Yellow
        }

        private void UnholdBtn_Click(object sender, RoutedEventArgs e)
        {
            VisiblePanel(true);
        }

        private void HoldBtn_Click(object sender, RoutedEventArgs e)
        {
            VisiblePanel(false);
        }

        public void AutoCheckVisible(double targetWidth)
        {
            VisiblePanel(Width < targetWidth);
        }

        public void VisiblePanel(bool isVisible)
        {
            var hideVisible = isVisible ? Visibility.Collapsed : Visibility.Visible;
            var showVisible = isVisible ? Visibility.Visible : Visibility.Collapsed;

            hidePanel.Visibility = hideVisible;
            showPanel.Visibility = showVisible;
        }

        private void LineAlphaCb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ChangeColor();
        }
    }
}
