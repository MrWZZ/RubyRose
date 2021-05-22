using System;
using System.Windows;
using System.Windows.Input;
using System.Collections.Generic;
using HotkeyLib;
using HookLib;
using WpfLib;
using System.Windows.Controls;
using Forms = System.Windows.Forms;
using System.Windows.Media;
using System.Linq;
using System.IO;

namespace CmdLike
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Forms.Keys> funKeys = new List<Forms.Keys>();
        TextBox curOprateTB;
        const string CLEAR_HOTKEY = "清除";
        const string REPEAT = "(重复)";
        const string HAVE_CHANGE = "存在变更，记得点击应用是变更生效";
        // 临时设置的热键字典，点击应用后尝试将当前的设置生效
        public Dictionary<string, HotkeyData> tempHotkeyDic = new Dictionary<string, HotkeyData>();
        // 临时新添加的cmd命令字典，点击应用后尝试将当前的设置生效
        public List<CmdData> tempCmdList = new List<CmdData>();
        // 需要删除的自定义路径数据
        public List<string> tempDeleteCmdList = new List<string>();
        // 需要修改名字的cmd数据
        public Dictionary<string,string> tempChangeKeyDic = new Dictionary<string, string>();
        public Forms.NotifyIcon notifyIcon;
        private SolidColorBrush failColor = new SolidColorBrush(Colors.Red);
        private SolidColorBrush successColor = new SolidColorBrush(Colors.LightGreen);
        private SolidColorBrush normalColor = new SolidColorBrush(Colors.White);
        private SolidColorBrush changeColor = new SolidColorBrush(Colors.Yellow);

        private int addIndex = 0;

        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            InitNotify();

            Closing += MainWindow_Closing;
            openCheck.Click += OpenCheck_Click;

            // 先注册热键模块
            HotkeyTool.Instance.Init(this, OnHotkeyHandler);
            // 在加载热键配置进行注册
            CmdLikeTool.Instance.ReadConfig();

            InitHotkeyPanel();

            if(CmdLikeTool.Instance.IsSetCmdHotkey())
            {
                HideWindow();
            }
        }

        private void HideWindow()
        {
            WindowState = WindowState.Minimized;
            Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 添加托盘图标
        /// </summary>
        private void InitNotify()
        {
            //设置托盘的各个属性
            notifyIcon = new Forms.NotifyIcon
            {
                Text = CmdLikeTool.APPLICATION_NAME,
                Icon = Properties.Resources.favicon,
                Visible = true
            };

            //退出菜单项
            var exit = new Forms.MenuItem("退出");
            exit.Click += (o, e) => CmdLikeTool.Instance.Quit();
            //设置菜单
            var setting = new Forms.MenuItem("设置");
            setting.Click += (o, e) => CmdLikeTool.Instance.OpenMainPanel();

            //关联托盘控件
            var childen = new Forms.MenuItem[] { setting, exit };
            notifyIcon.ContextMenu = new Forms.ContextMenu(childen);
        }

        // 初始化命令页签
        private void InitCmdPanel()
        {
            ShowOriginCustomData();
            ShowMassage("提示：将文件或文件夹拖入窗口，生成快捷路径，使用对应的快捷指定可直接打开对应文件。");
        }

        // 显示正在生效的结果
        private void ShowOriginCustomData()
        {
            addIndex = 0;
            tempCmdList.Clear();
            tempDeleteCmdList.Clear();
            tempChangeKeyDic.Clear();

            var list = CmdLikeTool.Instance.GetAllCustomCmdData();
            list.Sort();
            cmdList.ItemsSource = list;
        }

        // 初始化预制命令页签
        private void InitSystemPanel()
        {
            systemList.ItemsSource = CmdLikeTool.Instance.GetSystemCmdData();
            ShowMassage("提示：预制一些常用的命令，不可变更。");
        }

        // 初始化热键页签
        private void InitHotkeyPanel()
        {
            funKeys.Clear();
            tempHotkeyDic.Clear();
            hotkeyList.ItemsSource = CmdLikeTool.Instance.GetAllHotkeyDataList();
            ShowMassage("提示：双击条目左侧的输入框，当颜色变为黄色时，可输入需要的注册的热键。");
        }

        // 初始化设置页签
        public void InitSettingPanel()
        {
            openCheck.IsChecked = CmdLikeTool.Instance.IsAutoOpen;
            ShowMassage("提示：这里设置一些杂项。");
        }

        private void OnHotkeyHandler(uint atomValue)
        {
            CmdLikeTool.Instance.TryCallHotkey(atomValue);
        }

        private void SetFocusStyle(TextBox box, bool flag)
        {
            box.Background = flag ? changeColor : normalColor;
        }

        private bool IsFunctionKey(Forms.Keys key)
        {
            switch(key)
            {
                case Forms.Keys.LMenu:
                case Forms.Keys.LShiftKey:
                case Forms.Keys.LControlKey:
                case Forms.Keys.LWin:
                case Forms.Keys.RMenu:
                case Forms.Keys.RShiftKey:
                case Forms.Keys.RControlKey:
                case Forms.Keys.RWin:
                    return true;
                default:
                    return false;
            }
        }

        private void OnKeyInput(HookTool.KeyStatus status, Forms.Keys key)
        {
            if(status == HookTool.KeyStatus.KeyDown)
            {
                // 如果时esc键则视为清除热键
                if(key == Forms.Keys.Escape)
                {
                    curOprateTB.Text = CLEAR_HOTKEY;
                    SaveTempHotkey(key, curOprateTB.Text);
                    return;
                }

                if(IsFunctionKey(key))
                {
                    var value = ConvertFormsKeyToKeyFlag(key);
                    if (funKeys.Contains(key)) { return; }

                    funKeys.Add(key);
                    return;
                }

                curOprateTB.Text = GetHotKeyName(key);
                SaveTempHotkey(key, curOprateTB.Text);
            }
            else
            {
                funKeys.Remove(key);
            }
        }

        /// <summary>
        /// 将forms类型功能键值转换为注册热键时的值
        /// </summary>
        private uint ConvertFormsKeyToKeyFlag(Forms.Keys key)
        {
            switch(key)
            {
                case Forms.Keys.LWin:
                case Forms.Keys.RWin:
                    return 8;
                case Forms.Keys.LShiftKey:
                case Forms.Keys.RShiftKey:
                    return 4;
                case Forms.Keys.LControlKey:
                case Forms.Keys.RControlKey:
                    return 2;
                case Forms.Keys.LMenu:
                case Forms.Keys.RMenu:
                    return 1;
            }
            return 0;
        }

        /// <summary>
        /// 转换名字
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private string GetkeyflagShowName(Forms.Keys key)
        {
            switch (key)
            {
                case Forms.Keys.LWin:
                case Forms.Keys.RWin:
                    return "Win";
                case Forms.Keys.LShiftKey:
                case Forms.Keys.RShiftKey:
                    return "Shift";
                case Forms.Keys.LControlKey:
                case Forms.Keys.RControlKey:
                    return "Ctrl";
                case Forms.Keys.LMenu:
                case Forms.Keys.RMenu:
                    return "Alt";
            }
            return string.Empty;
        }

        private void SaveTempHotkey(Forms.Keys normalKey, string showKey)
        {
            var data = curOprateTB.DataContext as HotkeyData;

            // 组个功能键
            uint combineFlag = 0;
            foreach (var fun in funKeys)
            {
                uint flag = ConvertFormsKeyToKeyFlag(fun);
                combineFlag = combineFlag | flag;
            }
            funKeys.Clear();

            tempHotkeyDic[data.funkey] = new HotkeyData()
            {
                KeyBinding = showKey,
                key = (uint)normalKey,
                keyFlag = combineFlag
            };

            ShowMassage(HAVE_CHANGE);
        }

        private string GetHotKeyName(Forms.Keys key)
        {
            funKeys.Sort();

            var sb = new System.Text.StringBuilder();
            foreach (var item in funKeys)
            {
                sb.Append(GetkeyflagShowName(item));
                sb.Append("+");
            }

            sb.Append(key.ToString());

            return sb.ToString();
        }

        private void ShowMassage(string msg)
        {
            tip.Text = msg;
        }

        private void ClearHotkeyLock()
        {
            if(curOprateTB == null) { return; }

            SetFocusStyle(curOprateTB, false);
            curOprateTB = null;
            HookTool.Instance.ClearHook();
        }

        public TextBox GetHoykeyBoxByID(string id)
        {
            var list = WpfTool.Instance.GetItemsInChildren<TextBox>(hotkeyList, "hotkeyTb");
            foreach (var item in list)
            {
                var data = item.DataContext as HotkeyData;
                if (data != null && data.funkey == id)
                {
                    return item;
                }
            }
            return null;
        }

        /// <summary>
        /// 尝试注册当前设置的热键
        /// </summary>
        private void ApplyHotkey()
        {
            if(tempHotkeyDic.Count == 0)
            {
                ShowMassage("当前没有修改的热键");
                return;
            }

            var haveFail = false;
            foreach (var item in tempHotkeyDic)
            {
                var data = item.Value;

                // 如果是esc键则注销热键
                if(data.KeyBinding == CLEAR_HOTKEY)
                {
                    CmdLikeTool.Instance.UnregHotkey(item.Key);
                    continue;
                }

                var hotkeyData = CmdLikeTool.Instance.GetHotkeyData(item.Key);
                // 判断是否已经注册，如果是，需要先注销
                if (hotkeyData != null && hotkeyData.atom > 0)
                {
                    // 判断新的热键和旧的热键是否一致
                    if(hotkeyData.KeyBinding == data.KeyBinding)
                    {
                        continue;
                    }

                    CmdLikeTool.Instance.UnregHotkey(item.Key);
                }

                // 注册当前热键
                var isSuccess = CmdLikeTool.Instance.RegHotkey(item.Key, data.KeyBinding, data.key, data.keyFlag);
                if (!isSuccess)
                {
                    // 注册失败，标记红色
                    var textBox = GetHoykeyBoxByID(item.Key);
                    if (textBox != null)
                    {
                        textBox.Background = failColor;
                        haveFail = true;
                    }
                }
            }

            if (haveFail)
            {
                ShowMassage("存在一些注册失败的热键，这些条目已被标红。");
            }
            else
            {
                ShowMassage("所有更改的热键注册成功！");
            }

            CmdLikeTool.Instance.SaveConfig();
        }

        // 刷新界面，当前数据是临时显示的，并未生效
        private void RefreshCustomPanel()
        {
            tempCmdList.Sort();
            var tempList = tempCmdList.ToList();

            // 被标记删除的就不添加了
            var existList = CmdLikeTool.Instance.GetAllCustomCmdData();
            existList.Sort();
            foreach (var item in existList)
            {
                if(!tempDeleteCmdList.Contains(item.KeyBinding))
                {
                    tempList.Add(item);
                }
            }

            cmdList.ItemsSource = tempList;
        }

        #region 页面事件

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            HideWindow();
        }

        private void Hotkey_MouseDown(object sender, MouseButtonEventArgs e)
        {
            curOprateTB = (TextBox)sender;
            HookTool.Instance.SetHook(OnKeyInput);
            SetFocusStyle((TextBox)sender, true);
        }

        private void Hotkey_MouseLeave(object sender, MouseEventArgs e)
        {
            ClearHotkeyLock();
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ClearHotkeyLock();
        }

        private void OpenCheck_Click(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            var result = CmdLikeTool.Instance.SetAutoStartup((bool)checkBox.IsChecked);
            if(result)
            {
                ShowMassage("开启启动更改成功");
                CmdLikeTool.Instance.SaveConfig();
            }
            else
            {
                checkBox.IsChecked = false;
                ShowMassage("开启启动更改失败");
            }
        }

        private void Grid_Drop(object sender, DragEventArgs e)
        {
            string fileName = "";
            try
            {
                fileName = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            }
            catch
            {
                ShowMassage("无法识别该文件。");
                return;
            }

            var key = fileName;
            if (File.Exists(fileName))
            {
                var file = new FileInfo(fileName);
                key = file.Name;
            }
            else if(Directory.Exists(fileName))
            {
                var dir = new DirectoryInfo(fileName);
                key = dir.Name;
            }
            else
            {
                ShowMassage("无法识别该文件。");
                return;
            }

            // 与原有的重复
            if (CmdLikeTool.Instance.GetAllCommandKeys().Contains(key))
            {
                // 额外加一个后缀避免重复
                key += REPEAT;
            }

            // 与新添加的重复
            foreach (var item in tempCmdList)
            {
                if(item.KeyBinding == key)
                {
                    key += REPEAT;
                    break;
                }
            }

            CmdData data = new CmdData();
            data.sortIndex = ++addIndex;
            data.KeyBinding = key;
            data.DescriptionBinding = fileName;
            tempCmdList.Add(data);

            // 刷新界面
            RefreshCustomPanel();

            ShowMassage(HAVE_CHANGE);
        }

        private void CmdApply_Click(object sender, RoutedEventArgs e)
        {
            // 先删除
            foreach (var item in tempDeleteCmdList)
            {
                CmdLikeTool.Instance.RemoveCustomCmdData(item);
            }
            tempDeleteCmdList.Clear();

            bool isAllSuccess = true;
            var keys = tempChangeKeyDic.Keys.ToArray();
            // 更改原有名字
            foreach (var item in keys)
            {
                var result = CmdLikeTool.Instance.ChangeCustomCmdDataKey(item, tempChangeKeyDic[item]);
                if (result)
                {
                    tempChangeKeyDic.Remove(item);
                }
                else
                {
                    isAllSuccess = false;
                    var tb = GetCmdBoxByKey(item);
                    tb.Background = failColor;
                }
            }

            // 再添加
            for (int i = tempCmdList.Count - 1; i >= 0 ; i--)
            {
                var item = tempCmdList[i];
                var result = CmdLikeTool.Instance.AddCustomCmdData(item.KeyBinding, item.DescriptionBinding);
                if (result)
                {
                    tempCmdList.RemoveAt(i);
                }
                else
                {
                    isAllSuccess = false;
                    var tb = GetCmdBoxByKey(item.KeyBinding);
                    tb.Background = failColor;
                }
            }

            if(isAllSuccess)
            {
                ShowMassage("所有更改应用成功");
            }
            else
            {
                ShowMassage("有部分更改应用失败");
            }
            
            CmdLikeTool.Instance.SaveConfig();
        }

        private TextBox GetCmdBoxByKey(string key)
        {
            var tbList = WpfTool.Instance.GetItemsInChildren<TextBox>(cmdList, "");
            foreach (var item in tbList)
            {
                var data = item.DataContext as CmdData;
                if(data.KeyBinding == key)
                {
                    return item;
                }
            }
            return null;
        }
        
        private void CmdDelete_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var data = button.DataContext as CmdData;
            if(tempCmdList.Contains(data))
            {
                tempCmdList.Remove(data);
            }
            
            if(CmdLikeTool.Instance.GetCmdData(data.KeyBinding) != null)
            {
                tempDeleteCmdList.Add(data.KeyBinding);
            }

            RefreshCustomPanel();
            ShowMassage(HAVE_CHANGE);
        }


        private void HotkeyApply_Click(object sender, RoutedEventArgs e)
        {
            ApplyHotkey();
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(e.AddedItems.Count == 0 || !IsLoaded) { return; }

            var tab = e.AddedItems[0];
            if (tab == cmdTab)
            {
                InitCmdPanel();
            }
            else if(tab == hotkeyTab)
            {
                InitHotkeyPanel();
            }
            else if(tab == systemTab)
            {
                InitSystemPanel();
            }
            else if(tab == settingTab)
            {
                InitSettingPanel();
            }
        }

        // 自定义路径命令变更
        private void CustomTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var box = (TextBox)sender;
            var data = box.DataContext as CmdData;
            if(tempCmdList.Contains(data))
            {
                data.KeyBinding = box.Text;
            }
            else if(CmdLikeTool.Instance.GetCmdData(data.KeyBinding) != null)
            {
                tempChangeKeyDic[data.KeyBinding] = box.Text;
            }
            box.Background = normalColor;
            ShowMassage(HAVE_CHANGE);
        }

        #endregion
    }
}
