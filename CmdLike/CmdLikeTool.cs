using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Forms = System.Windows.Forms;
using Clip;
using HotkeyLib;
using System.IO;
using CmdLib;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Diagnostics;

namespace CmdLike
{
    public class CmdLikeTool
    {
        #region 单例

        private CmdLikeTool() { }
        private static CmdLikeTool instance;
        public static CmdLikeTool Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new CmdLikeTool();
                    instance.Init();
                }
                return instance;
            }
        }

        #endregion    

        const string CONFIG_PATH = "./config.txt";

        public const string APPLICATION_NAME = "RUBY_ROSE";
        public const string OpenCmdWindowKey = "OpenCmdWindowKey";
        public const string OpenClipScreenWindowKey = "OpenClipScreenWindowKey";

        private MainWindow mainWindow;
        private CmdWindow cmdWin;
        // 给添加自定义数据进行计数
        private int addCustomTime;

        /// <summary>
        /// 注册热键的全局原子映射热键字典
        /// </summary>
        private Dictionary<uint, string> atomToHotkeyidDic = new Dictionary<uint, string>();

        /// <summary>
        /// 配置数据
        /// </summary>
        private Config config = new Config();
        #region 初始化

        private void Init()
        {
            mainWindow = System.Windows.Application.Current.Windows[0] as MainWindow;
            InitHotkeyData();
            InitCmdData();
        }

        private void InitHotkeyData()
        {
            AddHotkeyData(OpenCmdWindowKey, OpenCmdWindow, "打开CmdLike窗口");
            AddHotkeyData(OpenClipScreenWindowKey, OpenClipScreenWindow, "打开截屏窗口");
        }

        private void AddHotkeyData(string key, Action fun, string description)
        {
            var data = new HotkeyData(key, fun);
            data.DescriptionBinding = description;
            config.hotkeyDic[key] = data;
        }

        private void InitCmdData()
        {
            AddSystemCmdData(".calc", CmdCommandType.CmdCommand, "calc", "打开计算器");
            AddSystemCmdData(".mspaint", CmdCommandType.CmdCommand, "mspaint", "打开图画");
            AddSystemCmdData(".appwiz.cpl", CmdCommandType.CmdCommand, "appwiz.cpl", "打开程序管理");
            AddSystemCmdData(".control", CmdCommandType.CmdCommand, "control", "打开控制中心");
            AddSystemCmdData(".notepad", CmdCommandType.CmdCommand, "notepad", "打开记事本");

            AddSystemCmdData(".setting", CmdCommandType.ToolFunction, OpenMainPanel, "打开设置面板");
            AddSystemCmdData(".quit", CmdCommandType.ToolFunction, Quit, "退出应用");
            AddSystemCmdData(".close all clip", CmdCommandType.ToolFunction, ClipTool.Instance.CloseAllClipScreen, "关闭所有截屏界面");
        }

        private void AddSystemCmdData(string key, CmdCommandType type, string cmdCommand, string description)
        {
            var data = new CmdData(key);
            data.KeyBinding = key;
            data.DescriptionBinding = description;
            data.type = type;
            data.cmdCommand = cmdCommand;
            config.systemDic[key] = data;
        }

        private void AddSystemCmdData(string key, CmdCommandType type, Action action, string description)
        {
            var data = new CmdData(key);
            data.KeyBinding = key;
            data.DescriptionBinding = description;
            data.type = type;
            data.function = action;
            config.systemDic[key] = data;
        }

        /// <summary>
        /// 添加自定义路径
        /// </summary>
        /// <param name="key"></param>
        /// <param name="path"></param>
        public bool AddCustomCmdData(string key, string path)
        {
            if(config.cmdDic.ContainsKey(key) || config.systemDic.ContainsKey(key) || string.IsNullOrEmpty(key))
            {
                return false;
            }

            // 检测路径是否存在
            if(!File.Exists(path) && !Directory.Exists(path))
            {
                return false;
            }

            var data = new CmdData(key);
            data.KeyBinding = key;
            data.DescriptionBinding = path;
            data.type = CmdCommandType.OpenFile;
            data.sortIndex = ++addCustomTime;
            config.cmdDic[key] = data;

            return true;
        }

        /// <summary>
        /// 更改名字
        /// </summary>
        /// <param name="oldKey"></param>
        /// <param name="newkey"></param>
        /// <returns></returns>
        public bool ChangeCustomCmdDataKey(string oldKey, string newkey)
        {
            if(string.IsNullOrEmpty(newkey) || config.systemDic.ContainsKey(oldKey))
            {
                return false;
            }

            if(config.cmdDic.ContainsKey(oldKey) && !config.cmdDic.ContainsKey(newkey))
            {
                var data = config.cmdDic[oldKey];
                config.cmdDic.Remove(oldKey);

                data.KeyBinding = newkey;
                config.cmdDic[newkey] = data;

                return true;
            }

            return false;
        }

        /// <summary>
        /// 是否已经设置了cmd快捷方式
        /// </summary>
        /// <returns></returns>
        public bool IsSetCmdHotkey()
        {
            return config.hotkeyDic[OpenCmdWindowKey].atom > 0;
        }

        /// <summary>
        /// 删除自定义路径
        /// </summary>
        /// <param name="key"></param>
        public void RemoveCustomCmdData(string key)
        {
            config.cmdDic.Remove(key);
        }

        /// <summary>
        /// 获取所有热键数据
        /// </summary>
        /// <returns></returns>
        public List<HotkeyData> GetAllHotkeyDataList()
        {
            return config.hotkeyDic.Values.ToList();
        }

        /// <summary>
        /// 根据 key 获取热键数据
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public HotkeyData GetHotkeyData(string key)
        {
            HotkeyData data = null;
            config.hotkeyDic.TryGetValue(key, out data);
            return data;
        }

        /// <summary>
        /// 尝试根据atom的值调用热键
        /// </summary>
        /// <param name="atom"></param>
        public void TryCallHotkey(uint atom)
        {
            string key = string.Empty;
            if (atomToHotkeyidDic.TryGetValue(atom, out key))
            {
                config.hotkeyDic[key].operation.Invoke();
            }
        }

        /// <summary>
        /// 获取自定义快捷命令数据
        /// </summary>
        /// <returns></returns>
        public List<CmdData> GetAllCustomCmdData()
        {
            List<CmdData> list = new List<CmdData>();
            foreach (var item in config.cmdDic)
            {
                list.Add(item.Value.Clone());
            }
            return list;
        }

        /// <summary>
        /// 获取预制快捷命令数据
        /// </summary>
        /// <returns></returns>
        public List<CmdData> GetSystemCmdData()
        {
            List<CmdData> list = new List<CmdData>();
            foreach (var item in config.systemDic)
            {
                list.Add(item.Value.Clone());
            }
            return list;
        }

        /// <summary>
        /// 获取所有已注册的命令列表
        /// </summary>
        /// <returns></returns>
        public List<string> GetAllCommandKeys()
        {
            var result = new List<string>();

            result.AddRange(config.cmdDic.Keys);
            result.AddRange(config.systemDic.Keys);

            return result;
        }

        /// <summary>
        /// 是否是开机启动
        /// </summary>
        /// <returns></returns>
        public bool IsAutoOpen
        {
            get
            {
                return config.IsAutoOpen;
            }
            set
            {
                config.IsAutoOpen = value;
            }
        }

        /// <summary>
        /// 根据key获取cmdData
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public CmdData GetCmdData(string key)
        {
            CmdData data = null;
            if(config.cmdDic.TryGetValue(key, out data))
            {
                return data.Clone();
            }

            if(config.systemDic.TryGetValue(key, out data))
            {
                return data.Clone();
            }

            return null;
        }

        /// <summary>
        /// 保存配置文件
        /// </summary>
        public void SaveConfig()
        {
            //序列化json
            DataContractJsonSerializer formatter = new DataContractJsonSerializer(typeof(Config));
            using (MemoryStream stream = new MemoryStream())
            {
                formatter.WriteObject(stream, config);
                string result = Encoding.UTF8.GetString(stream.ToArray());
                File.WriteAllText(CONFIG_PATH, result);
            }
        }

        /// <summary>
        /// 读取保存在本地的配置文件
        /// </summary>
        public void ReadConfig()
        {
            try
            {
                if (File.Exists(CONFIG_PATH))
                {
                    var isHotkeySuccess = true;
                    var content = File.ReadAllText(CONFIG_PATH);

                    DataContractJsonSerializer formatter = new DataContractJsonSerializer(typeof(Config));
                    using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(content)))
                    {
                        var readConfig = formatter.ReadObject(stream) as Config;

                        // 注册系统热键
                        foreach (var item in readConfig.hotkeyDic)
                        {
                            var data = item.Value;
                            if (item.Value.IsVaild())
                            {
                                var curResult = RegHotkey(item.Key, data.KeyBinding, data.key, data.keyFlag);
                                if(!curResult)
                                {
                                    isHotkeySuccess = false;
                                }
                            }
                        }

                        if(!isHotkeySuccess)
                        {
                            ShowNotify("热键注册失败，请打开设置面板查看详细信息。");
                        }

                        var isCmdSuccess = true;
                        // 填充自定义路径
                        foreach (var item in readConfig.cmdDic)
                        {
                            var curResult = AddCustomCmdData(item.Key, item.Value.DescriptionBinding);
                            if(!curResult)
                            {
                                isCmdSuccess = false;
                            }
                        }

                        if (!isCmdSuccess)
                        {
                            ShowNotify("路径设置失败，请打开设置面板查看详细信息。");
                        }

                        // 开机启动
                        IsAutoOpen = readConfig.IsAutoOpen;
                    }
                }
            }
            catch
            {
                // 报错，重置数据
                config = new Config();
                InitCmdData();
                InitHotkeyData();
                SaveConfig();
            }

        }

        /// <summary>
        /// 设置开机启动
        /// </summary>
        /// <param name="isAutoStart"></param>
        /// <returns></returns>
        public bool SetAutoStartup(bool isAutoStart)
        {
            // 当前操作是否成功
            var result = true;
            //开启启动
            string name = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name.ToString();
            string directory = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            string shortcutPath = Path.Combine(directory, string.Format("{0}.lnk", name));
            if (isAutoStart)
            {
                //文件不存在则重新创建
                if (!File.Exists(shortcutPath))
                {
                    string path = Process.GetCurrentProcess().MainModule.FileName;
                    if (CreateStartup(name, path))
                    {
                        config.IsAutoOpen = true;
                    }
                    else
                    {
                        result = false;
                    }
                }
                else
                {
                    config.IsAutoOpen = true;
                }
            }
            else
            {
                if(File.Exists(shortcutPath))
                {
                    File.Delete(shortcutPath);
                }
                config.IsAutoOpen = false;
            }
            return result;
        }

        //在开始菜单创建快捷方式启动
        bool CreateStartup(string shortcutName, string targetPath, string description = null, string iconLocation = null)
        {
            // 获取全局 开始 文件夹位置
            //string directory = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup);
            // 获取当前登录用户的 开始 文件夹位置
            string directory = Environment.GetFolderPath(Environment.SpecialFolder.Startup);

            try
            {
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                //IWshRuntimeLibrary:添加引用 Com 中搜索 Windows Script Host Object Model
                string shortcutPath = Path.Combine(directory, string.Format("{0}.lnk", shortcutName));
                IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShell();
                IWshRuntimeLibrary.IWshShortcut shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(shortcutPath);//创建快捷方式对象
                shortcut.TargetPath = targetPath;//指定目标路径
                shortcut.WorkingDirectory = Path.GetDirectoryName(targetPath);//设置起始位置
                shortcut.WindowStyle = 1;//设置运行方式，默认为常规窗口
                shortcut.Description = description;//设置备注
                shortcut.IconLocation = string.IsNullOrWhiteSpace(iconLocation) ? targetPath : iconLocation;//设置图标路径
                shortcut.Save();//保存快捷方式

                return true;
            }
            catch
            { }
            return false;
        }

        /// <summary>
        /// 注册热键
        /// </summary>
        public bool RegHotkey(string funkey , string showName, uint normalKey, uint combineFlag)
        {
            // 注册当前热键
            uint atom = 0;
            var isSuccess = HotkeyTool.Instance.RegisterHotkey(normalKey, combineFlag, out atom);
            if (isSuccess)
            {
                atomToHotkeyidDic[atom] = funkey;

                var hotkeyData = config.hotkeyDic[funkey];
                hotkeyData.key = normalKey;
                hotkeyData.keyFlag = combineFlag;
                hotkeyData.KeyBinding = showName;
                hotkeyData.atom = atom;
            }

            return isSuccess;
        }

        /// <summary>
        /// 注销热键
        /// </summary>
        /// <param name="key"></param>
        public void UnregHotkey(string key)
        {
            HotkeyData hotkeyData = null;
            if (config.hotkeyDic.TryGetValue(key, out hotkeyData))
            {
                if (hotkeyData.atom > 0)
                {
                    atomToHotkeyidDic.Remove(hotkeyData.atom);
                    HotkeyTool.Instance.UnregisterHotkey(hotkeyData.atom);

                    hotkeyData.key = 0;
                    hotkeyData.keyFlag = 0;
                    hotkeyData.atom = 0;
                    hotkeyData.KeyBinding = string.Empty;
                }
            }
        }

        #endregion  

        #region 系统方法

        /// <summary>
        /// 打开设置面板
        /// </summary>
        public void OpenMainPanel()
        {
            if(mainWindow != null)
            {
                mainWindow.Show();
                mainWindow.WindowState = System.Windows.WindowState.Normal;
                mainWindow.Activate();
            }
        }

        /// <summary>
        /// 显示托盘通知
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="tipType"></param>
        public void ShowNotify(string msg, Forms.ToolTipIcon tipType = Forms.ToolTipIcon.Info)
        {
            mainWindow.notifyIcon.ShowBalloonTip(500, APPLICATION_NAME, msg, tipType);
        }

        /// <summary>
        /// 退出应用
        /// </summary>
        public void Quit()
        {
            mainWindow.notifyIcon.Visible = false;
            System.Windows.Application.Current.Shutdown();
        }

        /// <summary>
        /// 打开cmd
        /// </summary>
        public void OpenCmdWindow()
        {
            if(cmdWin == null)
            {
                cmdWin = new CmdWindow();
            }
            cmdWin.Show();
            cmdWin.Activate();
        }

        public void CloseCmdWindow()
        {
            if (cmdWin != null && cmdWin.IsLoaded && cmdWin.IsVisible)
            {
                cmdWin.Close();
                cmdWin = null;
            }
        }

        /// <summary>
        /// 截屏
        /// </summary>
        public void OpenClipScreenWindow()
        {
            ClipTool.Instance.ShowMaskScreen();
        }

        public void CloseAllClipScreenWindow()
        {
            ClipTool.Instance.CloseAllClipScreen();
        }

        /// <summary>
        /// 打开指定文件或文件夹
        /// </summary>
        /// <param name="arg"></param>
        public void OpenFile(string arg)
        {
            if (File.Exists(arg))
            {
                System.Diagnostics.Process.Start(arg);
            }
            else if (Directory.Exists(arg))
            {
                System.Diagnostics.Process.Start("explorer.exe", arg);
            }
            else
            {
                throw new Exception("路径不存在");
            }
        }

        #endregion
    }
}