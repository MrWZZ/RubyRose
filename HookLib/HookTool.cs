using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace HookLib
{
    public sealed class HookTool
    {
        #region 单例

        private HookTool() { }
        private static HookTool instance;
        public static HookTool Instance
        {
            get
            {
                if(instance == null)
                {
                    instance = new HookTool();
                }
                return instance;
            }
        }

        ~HookTool()
        {
            ClearHook();
        }

        #endregion

        #region 模块注册

        // 钩子对应的结构
        public delegate int HookProc(int nCode, int wParam, int lParam);
        // 设置钩子  
        [DllImport("user32.dll")]
        static extern int SetWindowsHookEx(int idHook, HookProc lpfn, int hInstance, int threadId);

        // 取消钩子
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        static extern bool UnhookWindowsHookEx(int idHook);

        // 调用下一个钩子  
        [DllImport("user32.dll")]
        static extern int CallNextHookEx(int idHook, int nCode, int wParam, int lParam);

        // 获取模块
        [DllImport("kernel32.dll")]
        static extern int GetModuleHandle(string name);

        // 键盘或模拟键盘输入时调用
        const int WH_KEYBOARD_LL = 13;
        // 鼠标或模拟鼠标输入时调用
        const int WH_MOUSE_LL = 14;

        // 按下非系统键时
        const int WM_KEYDOWN = 0x0100;
        // 释放非系统键时
        const int WM_KEYUP = 0x0101;
        // 按下系统键时
        const int WM_SYSKEYDOWN = 0x0104;
        // 释放系统键时
        const int WM_SYSKEYUP = 0x0105;

        #endregion

        // 钩子句柄
        int hHook;
        // 系统触发键盘输入事件时调用
        HookProc KeyBoardHookProcedure;
        event Action<KeyStatus, Keys> onKeyInput;
        public enum KeyStatus
        {
            KeyDown,
            KeyUp,
        }

        /// <summary>
        /// 设置键盘钩子 
        /// </summary>
        /// <remarks>在程序结束前一定要调用ClearHook释放注册成功的钩子</remarks>
        /// <param name="hookHandler"></param>
        public void SetHook(Action<KeyStatus, Keys> keyInputHandler)
        {
            // 安装键盘钩子  
            if (hHook == 0)
            {
                KeyBoardHookProcedure = new HookProc(KeyBoardHookProc);

                hHook = SetWindowsHookEx(
                    WH_KEYBOARD_LL,
                    KeyBoardHookProcedure,
                    GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName),
                    0);

                //如果设置钩子失败.  
                if (hHook == 0)
                {
                    throw new Exception("键盘钩子设置失败");
                }
            }

            onKeyInput += keyInputHandler;
        }

        /// <summary>
        /// 设置键盘钩子 
        /// </summary>
        /// <remarks>在程序结束前一定要调用ClearHook释放注册成功的钩子</remarks>
        /// <param name="hookHandler"></param>
        public void SetHook(HookProc hookHandler)
        {
            if (hHook == 0)
            {
                KeyBoardHookProcedure = new HookProc(KeyBoardHookProc);

                hHook = SetWindowsHookEx(
                    WH_KEYBOARD_LL,
                    hookHandler,
                    GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName), 
                    0);

                //如果设置钩子失败.  
                if (hHook == 0)
                {
                    throw new Exception("键盘钩子设置失败");
                }
            }
        }

        /// <summary>
        /// 释放键盘钩子
        /// </summary>
        public void ClearHook()
        {
            if (hHook != 0)
            {
                if(!UnhookWindowsHookEx(hHook))
                {
                    throw new Exception("键盘钩子取消失败");
                }
                else
                {
                    hHook = 0;
                }
            }
            onKeyInput = null;
        }

        int KeyBoardHookProc(int nCode, int wParam, int lParam)
        {
            if (nCode >= 0)
            {
                var kbh = (KeyBoardHookStruct)Marshal.PtrToStructure((IntPtr)lParam, typeof(KeyBoardHookStruct));
                var keyStatus = wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN ? KeyStatus.KeyDown : KeyStatus.KeyUp;
                onKeyInput(keyStatus, (Keys)kbh.vkCode);
            }

            return CallNextHookEx(hHook, nCode, wParam, lParam);
        }

        [StructLayout(LayoutKind.Sequential)]
        public class KeyBoardHookStruct
        {
            public int vkCode;
            public int scanCode;
            public int flags;
            public int time;
            public int dwExtraInfo;
        }
    }
}
