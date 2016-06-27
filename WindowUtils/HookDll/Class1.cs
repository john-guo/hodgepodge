using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using EasyHook;
using IContext = EasyHook.RemoteHooking.IContext;

using HookTest;
using System.Threading;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace HookDll
{
    //public interface IHook : IEntryPoint
    //{
    //    void Initialize(IContext context, string name);
    //    void Run(IContext context, string name);
    //}

    public class Class1 : IEntryPoint
    {
        IpcHelper ipc;
        ConcurrentBag<string> bag;

        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int W, int H, uint uFlags);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
        public delegate bool SetWindowPosDelegate(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int W, int H, uint uFlags);

        public Class1(IContext context, int pid, string name)
        {
            //var name = passThruArgs[0] as string;
            ipc = RemoteHooking.IpcConnectClient<IpcHelper>(name);
            bag = new ConcurrentBag<string>();
        }

        public void Run(IContext context, int pid, string name)
        {
            LocalHook hook;
            try
            {
                hook = LocalHook.Create(LocalHook.GetProcAddress("user32.dll", "SetWindowPos"),
                    new SetWindowPosDelegate(MySetWindowPos), this);

                hook.ThreadACL.SetExclusiveACL(new[] { 0 });
            }
            catch (Exception ex)
            {
                ipc.Exception(ex);
                return;
            }

            ipc.IsInstalled(RemoteHooking.GetCurrentProcessId());

            ipc.OnMethodCall(pid.ToString());

            // wait for host process termination...  


            //try
            //{
            //    while (true)
            //    {


            //        Thread.Sleep(500);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    ipc.Exception(ex);
            //}



            try
            {
                while (true)
                {
                    var p = Process.GetProcessById(pid);
                    if (p == null)
                    {
                        ipc.OnMethodCall("I'm return1.");
                        break;
                    }

                    p.Refresh();
                    if (p.HasExited)
                    {
                        ipc.OnMethodCall("I'm return2.");
                        break;
                    }

                    Thread.Sleep(500);
                    string result;
                    while (!bag.IsEmpty)
                    {
                        bag.TryTake(out result);
                        ipc.OnMethodCall(result);
                    }
                }
            }
            catch (Exception ex)
            {
                ipc.Exception(ex);
                // NET Remoting will raise an exception if host is unreachable  
            }

            hook.Dispose();
        }

        static bool MySetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int W, int H, uint uFlags)
        {
            var me = (Class1)HookRuntimeInfo.Callback;
            me.bag.Add(string.Format("{0} {1} {2} {3} {4} {5} {6}", hWnd, hWndInsertAfter, X, Y, W, H, uFlags));
            me.ipc.OnMethodCall(string.Format("{0} {1} {2} {3} {4} {5} {6}", hWnd, hWndInsertAfter, X, Y, W, H, uFlags));
            return SetWindowPos(hWnd, hWndInsertAfter, X, Y, W, H, uFlags);
            //return true;
        }
    }
}
