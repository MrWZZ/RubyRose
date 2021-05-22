using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows;

namespace HotkeyLib
{
    public class HotkeyTool
    {
        #region 单例

        private HotkeyTool() { }
        private static HotkeyTool instance;
        public static HotkeyTool Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new HotkeyTool();
                }
                return instance;
            }
        }

        ~HotkeyTool()
        {
            Clear();
        }

        #endregion

        private IntPtr hWnd;
        private HwndSource source;
        private event Action<uint> onHotkey;

        [DllImport("user32.dll")]
        public static extern uint RegisterHotKey(IntPtr hWnd, uint id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        public static extern uint UnregisterHotKey(IntPtr hWnd, uint id);

        [DllImport("kernel32.dll")]
        public static extern uint GlobalAddAtom(String lpString);

        [DllImport("kernel32.dll")]
        public static extern uint GlobalDeleteAtom(uint nAtom);

        List<uint> atomList = new List<uint>();

        public void Init(Window window, Action<uint> onHotkeyHandler)
        {
            onHotkey += onHotkeyHandler;
            hWnd = new WindowInteropHelper(window).Handle;
            source = HwndSource.FromHwnd(hWnd);
            source = PresentationSource.FromVisual(window) as HwndSource;
            source.AddHook(Listener);
        }

        /// <summary>
        /// 注册快捷键
        /// </summary>
        /// <param name="key">实体键</param>
        /// <param name="keyflag">功能键</param>
        /// <param name="hotkeyid">该快捷键对应的全局ID，用于判断用户当前按下的是什么组合键</param>
        /// <returns>是否注册成功</returns>
        public bool RegisterHotkey(uint key, uint keyflag, out uint hotkeyid)
        {
            hotkeyid = GlobalAddAtom(Guid.NewGuid().ToString());
            var result = RegisterHotKey(hWnd, hotkeyid, keyflag, key);
            var isSuccess = result > 0;

            if(isSuccess)
            {
                atomList.Add(hotkeyid);
            }
            else
            {
                GlobalDeleteAtom(hotkeyid);
            }

            return isSuccess;
        }

        /// <summary>
        /// 注销快捷键
        /// </summary>
        /// <param name="key">快捷键对应的唯一ID</param>
        /// <returns>是否删除成功</returns>
        public bool UnregisterHotkey(uint hotkeyid)
        {
            GlobalDeleteAtom(hotkeyid);
            atomList.Remove(hotkeyid);

            var result = UnregisterHotKey(hWnd, hotkeyid);
            return result > 0;
        }

        /// <summary>
        /// 该窗口绑定的快捷键触发后的系统响应
        /// </summary>
        public IntPtr Listener(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handle)
        {
            uint hotkeyid = (uint)wParam.ToInt32();
            onHotkey(hotkeyid);
            return IntPtr.Zero;
        }

        void Clear()
        {
            source.RemoveHook(Listener);
            foreach (var item in atomList)
            {
                GlobalDeleteAtom(item);
            }
            atomList.Clear();
        }
    }
}
