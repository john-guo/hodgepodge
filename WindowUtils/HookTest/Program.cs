using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EasyHook;
using System.Runtime.Remoting;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace HookTest
{
    public class IpcHelper : MarshalByRefObject
    {
        public void IsInstalled(Int32 InClientPID)
        {
            Console.WriteLine("I have been installed in target {0}.", InClientPID);
        }

        public void OnMethodCall(string something)
        {
            Console.WriteLine("Call! {0}", something);
        }

        public void Exception(Exception ex)
        {
            Console.WriteLine(ex);
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }
    }



    namespace BeepHook
    {
        class Program
        {
            // The matching delegate for MessageBeep
            [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
            delegate bool MessageBeepDelegate(uint uType);

            // Import the method so we can call it
            [DllImport("user32.dll")]
            static extern bool MessageBeep(uint uType);


            [DllImport("user32.dll")]
            public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int W, int H, uint uFlags);

            [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
            public delegate bool SetWindowPosDelegate(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int W, int H, uint uFlags);


            static bool MySetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int W, int H, uint uFlags)
            {
                //var me = (Class1)HookRuntimeInfo.Callback;
                //me.bag.Add(string.Format("{0} {1} {2} {3} {4} {5} {6}", hWnd, hWndInsertAfter, X, Y, W, H, uFlags));
                //me.ipc.OnMethodCall(string.Format("{0} {1} {2} {3} {4} {5} {6}", hWnd, hWndInsertAfter, X, Y, W, H, uFlags));
                //return SetWindowPos(hWnd, hWndInsertAfter, X, Y, W, H, uFlags);
                Console.WriteLine("MySetWindowPos");
                return true;
            }

            /// <summary>
            /// Our MessageBeep hook handler
            /// </summary>
            static private bool MessageBeepHook(uint uType)
            {
                // We aren't going to call the original at all
                // but we could using: return MessageBeep(uType);
                Console.Write("...intercepted...");
                return false;
            }

            /// <summary>
            /// Plays a beep using the native MessageBeep method
            /// </summary>
            static private void PlayMessageBeep()
            {
                Console.Write("    MessageBeep(BeepType.Asterisk) return value: ");
                Console.WriteLine(MessageBeep((uint)BeepType.Asterisk));
            }

            static void Main(string[] args)
            {
                Console.WriteLine("Calling MessageBeep with no hook.");
                PlayMessageBeep();

                Console.Write("\nPress <enter> to call MessageBeep while hooked by MessageBeepHook:");
                Console.ReadLine();

                Console.WriteLine("\nInstalling local hook for user32!MessageBeep");
                // Create the local hook using our MessageBeepDelegate and MessageBeepHook function
                using (var hook = EasyHook.LocalHook.Create(
                        EasyHook.LocalHook.GetProcAddress("user32.dll", "MessageBeep"),
                        new MessageBeepDelegate(MessageBeepHook),
                        null))
                {
                    // Only hook this thread (threadId == 0 == GetCurrentThreadId)
                    hook.ThreadACL.SetInclusiveACL(new int[] { 0 });

                    PlayMessageBeep();

                    Console.Write("\nPress <enter> to disable hook for current thread:");
                    Console.ReadLine();
                    Console.WriteLine("\nDisabling hook for current thread.");
                    // Exclude this thread (threadId == 0 == GetCurrentThreadId)
                    hook.ThreadACL.SetExclusiveACL(new int[] { 0 });
                    PlayMessageBeep();

                    Console.Write("\nPress <enter> to uninstall hook and exit.");
                    Console.ReadLine();
                } // hook.Dispose() will uninstall the hook for us
            }

            public enum BeepType : uint
            {
                /// <summary>
                /// A simple windows beep
                /// </summary>            
                SimpleBeep = 0xFFFFFFFF,
                /// <summary>
                /// A standard windows OK beep
                /// </summary>
                OK = 0x00,
                /// <summary>
                /// A standard windows Question beep
                /// </summary>
                Question = 0x20,
                /// <summary>
                /// A standard windows Exclamation beep
                /// </summary>
                Exclamation = 0x30,
                /// <summary>
                /// A standard windows Asterisk beep
                /// </summary>
                Asterisk = 0x40,
            }

        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            try  
            {
                string ChannelName = null;
                //Config.Register("TestHook", "Wrench.exe", "HookDll.dll");  
  
                RemoteHooking.IpcCreateServer<IpcHelper>(ref ChannelName, WellKnownObjectMode.Singleton);

                var p = Process.GetProcessesByName("Wrench").FirstOrDefault();

                if (p == null)
                {
                    Console.WriteLine("Program not found.");
                    return;
                }

                //int pid;
                //RemoteHooking.CreateAndInject("Wrench.exe", "", 0, InjectionOptions.DoNotRequireStrongName,
                //    "HookDll.dll",
                //    "HookDll.dll",
                //    out pid,
                //    ChannelName);
                RemoteHooking.Inject(
                    p.Id,
                    InjectionOptions.Default,
                    "HookDll.dll",
                    "HookDll.dll",
                    p.Id, ChannelName);

                Console.ReadLine();  
            }  
            catch (Exception ExtInfo)  
            {  
                Console.WriteLine("There was an error while connecting to target:\r\n{0}", ExtInfo.ToString());  
            }  

        }
    }
}
