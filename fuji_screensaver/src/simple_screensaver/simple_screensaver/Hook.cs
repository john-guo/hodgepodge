using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
namespace simple_screensaver
{
    ///   <summary> 
    ///   这个类可以让你得到一个在运行中程序的所有键盘或鼠标事件 
    ///   并且引发一个带KeyEventArgs参数的.NET事件以便你很容易使用这些信息 
    ///   </summary>
    public class KeyBordHook
    {
        private const int WM_KEYDOWN = 0x100;
        private const int WM_KEYUP = 0x101;
        private const int WM_SYSKEYDOWN = 0x104;
        private const int WM_SYSKEYUP = 0x105;
        //全局的事件 
        public event KeyEventHandler OnKeyDownEvent;
        public event KeyEventHandler OnKeyUpEvent;
        public event KeyPressEventHandler OnKeyPressEvent;
        static int hKeyboardHook = 0;   //键盘钩子句柄 
        //鼠标常量 
        public const int WH_KEYBOARD_LL = 13;   //keyboard   hook   constant   
        HookProc KeyboardHookProcedure;   //声明键盘钩子事件类型. 
        //声明键盘钩子的封送结构类型 
        [StructLayout(LayoutKind.Sequential)]
        public class KeyboardHookStruct
        {
            public int vkCode;   //表示一个在1到254间的虚似键盘码 
            public int scanCode;   //表示硬件扫描码 
            public int flags;
            public int time;
            public int dwExtraInfo;
        }
        //装置钩子的函数 
        [DllImport("user32.dll ", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);
        //卸下钩子的函数 
        [DllImport("user32.dll ", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern bool UnhookWindowsHookEx(int idHook);
        //下一个钩挂的函数 
        [DllImport("user32.dll ", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int CallNextHookEx(int idHook, int nCode, Int32 wParam, IntPtr lParam);
        [DllImport("user32 ")]
        public static extern int ToAscii(int uVirtKey, int uScanCode, byte[] lpbKeyState, byte[] lpwTransKey, int fuState);
        [DllImport("user32 ")]
        public static extern int GetKeyboardState(byte[] pbKeyState);
        public delegate int HookProc(int nCode, Int32 wParam, IntPtr lParam);
        ///   <summary> 
        ///   墨认的构造函数构造当前类的实例并自动的运行起来. 
        ///   </summary> 
        public KeyBordHook()
        {
            //Start();
        }
        //析构函数. 
        ~KeyBordHook()
        {
            Stop();
        }
        public void Start()
        {
            //安装键盘钩子   
            if (hKeyboardHook == 0)
            {
                KeyboardHookProcedure = new HookProc(KeyboardHookProc);
                hKeyboardHook = SetWindowsHookEx(WH_KEYBOARD_LL, KeyboardHookProcedure, Marshal.GetHINSTANCE(Assembly.GetExecutingAssembly().ManifestModule), 0);
                if (hKeyboardHook == 0)
                {
                    Stop();
                    throw new Exception("SetWindowsHookEx   ist   failed. ");
                }
            }
        }
        public void Stop()
        {
            bool retKeyboard = true;
            if (hKeyboardHook != 0)
            {
                retKeyboard = UnhookWindowsHookEx(hKeyboardHook);
                hKeyboardHook = 0;
            }
            //如果卸下钩子失败 
            if (!(retKeyboard)) throw new Exception("UnhookWindowsHookEx   failed. ");
        }
        private int KeyboardHookProc(int nCode, Int32 wParam, IntPtr lParam)
        {
            if ((nCode >= 0) && (OnKeyDownEvent != null || OnKeyUpEvent != null || OnKeyPressEvent != null))
            {
                KeyboardHookStruct MyKeyboardHookStruct = (KeyboardHookStruct)Marshal.PtrToStructure(lParam, typeof(KeyboardHookStruct));
                //引发OnKeyDownEvent 
                if (OnKeyDownEvent != null && (wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN))
                {
                    Keys keyData = (Keys)MyKeyboardHookStruct.vkCode;
                    KeyEventArgs e = new KeyEventArgs(keyData);
                    OnKeyDownEvent(this, e);
                }
                //引发OnKeyPressEvent 
                if (OnKeyPressEvent != null && wParam == WM_KEYDOWN)
                {
                    byte[] keyState = new byte[256];
                    GetKeyboardState(keyState);
                    byte[] inBuffer = new byte[2];
                    if (ToAscii(MyKeyboardHookStruct.vkCode,
                      MyKeyboardHookStruct.scanCode,
                      keyState,
                      inBuffer,
                      MyKeyboardHookStruct.flags) == 1)
                    {
                        KeyPressEventArgs e = new KeyPressEventArgs((char)inBuffer[0]);
                        OnKeyPressEvent(this, e);
                    }
                }
                //引发OnKeyUpEvent 
                if (OnKeyUpEvent != null && (wParam == WM_KEYUP || wParam == WM_SYSKEYUP))
                {
                    Keys keyData = (Keys)MyKeyboardHookStruct.vkCode;
                    KeyEventArgs e = new KeyEventArgs(keyData);
                    OnKeyUpEvent(this, e);
                }
            }
            return CallNextHookEx(hKeyboardHook, nCode, wParam, lParam);
        }
    }

    ///   <summary> 
    ///   这个类可以让你得到一个在运行中程序的所有鼠标事件 
    ///   并且引发一个带MouseEventArgs参数的.NET鼠标事件以便你很容易使用这些信息 
    ///   </summary> 
    public class MouseHook
    {
        private const int WM_MOUSEMOVE = 0x200;
        private const int WM_LBUTTONDOWN = 0x201;
        private const int WM_RBUTTONDOWN = 0x204;
        private const int WM_MBUTTONDOWN = 0x207;
        private const int WM_LBUTTONUP = 0x202;
        private const int WM_RBUTTONUP = 0x205;
        private const int WM_MBUTTONUP = 0x208;
        private const int WM_LBUTTONDBLCLK = 0x203;
        private const int WM_RBUTTONDBLCLK = 0x206;
        private const int WM_MBUTTONDBLCLK = 0x209;
        //全局的事件 
        public event MouseEventHandler OnMouseActivity;
        static int hMouseHook = 0;   //鼠标钩子句柄 
        //鼠标常量 
        public const int WH_MOUSE_LL = 14;   //mouse   hook   constant 
        HookProc MouseHookProcedure;   //声明鼠标钩子事件类型. 
        //声明一个Point的封送类型 
        [StructLayout(LayoutKind.Sequential)]
        public class POINT
        {
            public int x;
            public int y;
        }
        //声明鼠标钩子的封送结构类型 
        [StructLayout(LayoutKind.Sequential)]
        public class MouseHookStruct
        {
            public POINT pt;
            public int hWnd;
            public int wHitTestCode;
            public int dwExtraInfo;
        }
        //装置钩子的函数 
        [DllImport("user32.dll ", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);
        //卸下钩子的函数 
        [DllImport("user32.dll ", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern bool UnhookWindowsHookEx(int idHook);
        //下一个钩挂的函数 
        [DllImport("user32.dll ", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int CallNextHookEx(int idHook, int nCode, Int32 wParam, IntPtr lParam);
        public delegate int HookProc(int nCode, Int32 wParam, IntPtr lParam);
        ///   <summary> 
        ///   墨认的构造函数构造当前类的实例. 
        ///   </summary> 
        public MouseHook()
        {
            //Start(); 
        }
        //析构函数. 
        ~MouseHook()
        {
            Stop();
        }
        public void Start()
        {
            //安装鼠标钩子 
            if (hMouseHook == 0)
            {
                //生成一个HookProc的实例. 
                MouseHookProcedure = new HookProc(MouseHookProc);
                hMouseHook = SetWindowsHookEx(WH_MOUSE_LL, MouseHookProcedure, Marshal.GetHINSTANCE(Assembly.GetExecutingAssembly().GetModules()[0]), 0);
                //如果装置失败停止钩子 
                if (hMouseHook == 0)
                {
                    Stop();
                    throw new Exception("SetWindowsHookEx failed. ");
                }
            }
        }
        public void Stop()
        {
            bool retMouse = true;
            if (hMouseHook != 0)
            {
                retMouse = UnhookWindowsHookEx(hMouseHook);
                hMouseHook = 0;
            }
            //如果卸下钩子失败 
            if (!(retMouse)) throw new Exception("UnhookWindowsHookEx   failed. ");
        }
        private int MouseHookProc(int nCode, Int32 wParam, IntPtr lParam)
        {
            //如果正常运行并且用户要监听鼠标的消息 
            if ((nCode >= 0) && (OnMouseActivity != null))
            {
                MouseButtons button = MouseButtons.None;
                int clickCount = 0;
                switch (wParam)
                {
                    case WM_LBUTTONDOWN:
                        button = MouseButtons.Left;
                        clickCount = 1;
                        break;
                    case WM_LBUTTONUP:
                        button = MouseButtons.Left;
                        clickCount = 1;
                        break;
                    case WM_LBUTTONDBLCLK:
                        button = MouseButtons.Left;
                        clickCount = 2;
                        break;
                    case WM_RBUTTONDOWN:
                        button = MouseButtons.Right;
                        clickCount = 1;
                        break;
                    case WM_RBUTTONUP:
                        button = MouseButtons.Right;
                        clickCount = 1;
                        break;
                    case WM_RBUTTONDBLCLK:
                        button = MouseButtons.Right;
                        clickCount = 2;
                        break;
                }
                //从回调函数中得到鼠标的信息 
                MouseHookStruct MyMouseHookStruct = (MouseHookStruct)Marshal.PtrToStructure(lParam, typeof(MouseHookStruct));
                MouseEventArgs e = new MouseEventArgs(button, clickCount, MyMouseHookStruct.pt.x, MyMouseHookStruct.pt.y, 0);
                OnMouseActivity(this, e);
            }
            return CallNextHookEx(hMouseHook, nCode, wParam, lParam);
        }
    }
}