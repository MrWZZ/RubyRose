using System;
using System.Collections.Generic;
using System.Windows;

namespace CmdLike
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class CmdWindow : Window
    {
        // 自动提示文本列表
        List<string> tipList;
        bool needTip = true;

        public CmdWindow()
        {
            InitializeComponent();
            SetLeftBottom();
            cmd.Focus();
            Deactivated += CmdWindow_Deactivated;
            tipList = CmdLikeTool.Instance.GetAllCommandKeys();
        }

        private void CmdWindow_Deactivated(object sender, System.EventArgs e)
        {
            CmdLikeTool.Instance.CloseCmdWindow();
        }

        private void SetLeftBottom()
        {
            var area = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea;
            Left = area.Left;
            Top = area.Bottom - Height;
        }

        private void Cmd_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            HideTip();
            // 关闭
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                CmdLikeTool.Instance.CloseCmdWindow();
            }
            else if(e.Key == System.Windows.Input.Key.Enter)
            {
                DealCmdCommand();
            }

            if (e.Key == System.Windows.Input.Key.Back)
            {
                needTip = false;
            }
            else
            {
                needTip = true;
            }
        }

        private void Cmd_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            AutoTip(cmd.Text);
        }
        
        // 自动提示
        private void AutoTip(string str)
        {
            if (str.Length == 0 || !needTip || tipList == null)
            {
                return;
            }
            str = str.Trim();
            needTip = false;
            foreach (var tip in tipList)
            {
                if (tip.Length < str.Length)
                {
                    continue;
                }

                if (tip.StartsWith(str))
                {
                    cmd.Text = tip;
                    cmd.Select(str.Length, tip.Length);
                    return;
                }
            }
        }

        private void DealCmdCommand()
        {
            var command = cmd.Text.Trim();
            if(string.IsNullOrEmpty(command)) { return; }

            CmdData data = CmdLikeTool.Instance.GetCmdData(command);    
            if(data != null)
            {
                try
                {
                    data.DoCommand();
                    CmdLikeTool.Instance.CloseCmdWindow();
                }
                catch(Exception e)
                {
                    ShowTip(e.Message);
                }
            }
            else
            {
                ShowTip("指令不存在");
            }
        }

        private void ShowTip(string msg)
        {
            cmd.Text = string.Empty;
            tip.Content = msg;
        }

        private void HideTip()
        {
            tip.Content = string.Empty;
        }
    }
}
