using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace sm
{

    public class WinApi
    {
        public const Int32 CCHDEVICENAME = 32;
        public const Int32 CCHFORMNAME = 32;

        public enum DEVMODE_SETTINGS
        {
            ENUM_CURRENT_SETTINGS = (-1),
            ENUM_REGISTRY_SETTINGS = (-2)
        }
        [Flags()]
        public enum DisplayDeviceStateFlags : int
        {
            /// <summary>The device is part of the desktop.</summary>
            AttachedToDesktop = 0x1,
            MultiDriver = 0x2,
            /// <summary>The device is part of the desktop.</summary>
            PrimaryDevice = 0x4,
            /// <summary>Represents a pseudo device used to mirror application drawing for remoting or other purposes.</summary>
            MirroringDriver = 0x8,
            /// <summary>The device is VGA compatible.</summary>
            VGACompatible = 0x10,
            /// <summary>The device is removable; it cannot be the primary display.</summary>
            Removable = 0x20,
            /// <summary>The device has more display modes than its output devices support.</summary>
            ModesPruned = 0x8000000,
            Remote = 0x4000000,
            Disconnect = 0x2000000
        }

        public enum Display_Device_Stateflags
        {
            DISPLAY_DEVICE_ATTACHED_TO_DESKTOP = 0x1,
            DISPLAY_DEVICE_MIRRORING_DRIVER = 0x8,
            DISPLAY_DEVICE_MODESPRUNED = 0x8000000,
            DISPLAY_DEVICE_MULTI_DRIVER = 0x2,
            DISPLAY_DEVICE_PRIMARY_DEVICE = 0x4,
            DISPLAY_DEVICE_VGA_COMPATIBLE = 0x10
        }

        public enum DeviceFlags
        {
            CDS_FULLSCREEN = 0x4,
            CDS_GLOBAL = 0x8,
            CDS_NORESET = 0x10000000,
            CDS_RESET = 0x40000000,
            CDS_SET_PRIMARY = 0x10,
            CDS_TEST = 0x2,
            CDS_UPDATEREGISTRY = 0x1,
            CDS_VIDEOPARAMETERS = 0x20,
        }

        public enum DEVMODE_Flags
        {
            DM_BITSPERPEL = 0x40000,
            DM_DISPLAYFLAGS = 0x200000,
            DM_DISPLAYFREQUENCY = 0x400000,
            DM_PELSHEIGHT = 0x100000,
            DM_PELSWIDTH = 0x80000,
            DM_POSITION = 0x20
        }

        public enum DisplaySetting_Results
        {
            DISP_CHANGE_BADFLAGS = -4,
            DISP_CHANGE_BADMODE = -2,
            DISP_CHANGE_BADPARAM = -5,
            DISP_CHANGE_FAILED = -1,
            DISP_CHANGE_NOTUPDATED = -3,
            DISP_CHANGE_RESTART = 1,
            DISP_CHANGE_SUCCESSFUL = 0
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINTL
        {
            [MarshalAs(UnmanagedType.I4)]
            public int x;
            [MarshalAs(UnmanagedType.I4)]
            public int y;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct DEVMODE
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmDeviceName;

            [MarshalAs(UnmanagedType.U2)]
            public UInt16 dmSpecVersion;

            [MarshalAs(UnmanagedType.U2)]
            public UInt16 dmDriverVersion;

            [MarshalAs(UnmanagedType.U2)]
            public UInt16 dmSize;

            [MarshalAs(UnmanagedType.U2)]
            public UInt16 dmDriverExtra;

            [MarshalAs(UnmanagedType.U4)]
            public DEVMODE_Flags dmFields;

            public POINTL dmPosition;

            [MarshalAs(UnmanagedType.U4)]
            public UInt32 dmDisplayOrientation;

            [MarshalAs(UnmanagedType.U4)]
            public UInt32 dmDisplayFixedOutput;

            [MarshalAs(UnmanagedType.I2)]
            public Int16 dmColor;

            [MarshalAs(UnmanagedType.I2)]
            public Int16 dmDuplex;

            [MarshalAs(UnmanagedType.I2)]
            public Int16 dmYResolution;

            [MarshalAs(UnmanagedType.I2)]
            public Int16 dmTTOption;

            [MarshalAs(UnmanagedType.I2)]
            public Int16 dmCollate;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmFormName;

            [MarshalAs(UnmanagedType.U2)]
            public UInt16 dmLogPixels;

            [MarshalAs(UnmanagedType.U4)]
            public UInt32 dmBitsPerPel;

            [MarshalAs(UnmanagedType.U4)]
            public UInt32 dmPelsWidth;

            [MarshalAs(UnmanagedType.U4)]
            public UInt32 dmPelsHeight;

            [MarshalAs(UnmanagedType.U4)]
            public UInt32 dmDisplayFlags;

            [MarshalAs(UnmanagedType.U4)]
            public UInt32 dmDisplayFrequency;

            [MarshalAs(UnmanagedType.U4)]
            public UInt32 dmICMMethod;

            [MarshalAs(UnmanagedType.U4)]
            public UInt32 dmICMIntent;

            [MarshalAs(UnmanagedType.U4)]
            public UInt32 dmMediaType;

            [MarshalAs(UnmanagedType.U4)]
            public UInt32 dmDitherType;

            [MarshalAs(UnmanagedType.U4)]
            public UInt32 dmReserved1;

            [MarshalAs(UnmanagedType.U4)]
            public UInt32 dmReserved2;

            [MarshalAs(UnmanagedType.U4)]
            public UInt32 dmPanningWidth;

            [MarshalAs(UnmanagedType.U4)]
            public UInt32 dmPanningHeight;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct DISPLAY_DEVICE
        {
            [MarshalAs(UnmanagedType.U4)]
            public int cb;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DeviceName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceString;
            [MarshalAs(UnmanagedType.U4)]
            public DisplayDeviceStateFlags StateFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceID;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceKey;
        }

        public class User_32
        {
            [DllImport("user32.dll")]
            public static extern int ChangeDisplaySettings(ref DEVMODE devMode, int flags);

            //[DllImport("user32.dll")]
            //public static extern int ChangeDisplaySettingsEx(ref DEVMODE devMode, int flags);

            [DllImport("user32.dll")]
            public static extern int ChangeDisplaySettingsEx(string lpszDeviceName, [In] ref DEVMODE lpDevMode, IntPtr hwnd, int dwFlags, IntPtr lParam);

            [DllImport("user32.dll")]
            public static extern bool EnumDisplayDevices(string lpDevice, int iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, int dwFlags);

            [DllImport("user32.dll")]
            public static extern int EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);
        }

    }

    public class Screen
    {
        public WinApi.DEVMODE DeviceMode;
        public WinApi.DISPLAY_DEVICE ScreenDevice;
        public string DeviceName;
        public bool IsPrimary;
        public int ScreenWidth;
        public int ScreenHeight;
        public int XPosition;
        public int YPosition;
    }

    public static class ScreenHelper
    {
        public static List<Screen> GetAllScreen()
        {
            List<WinApi.DISPLAY_DEVICE> devices = new List<WinApi.DISPLAY_DEVICE>();
            List<Screen> screens = new List<Screen>();
            bool error = false;
            //Here I am listing all DisplayDevices (Monitors)
            for (int devId = 0; !error; devId++)
            {
                try
                {
                    WinApi.DISPLAY_DEVICE device = new WinApi.DISPLAY_DEVICE();
                    device.cb = Marshal.SizeOf(typeof(WinApi.DISPLAY_DEVICE));
                    error = !WinApi.User_32.EnumDisplayDevices(null, devId, ref device, 0);
                    if ((device.StateFlags & WinApi.DisplayDeviceStateFlags.AttachedToDesktop) == WinApi.DisplayDeviceStateFlags.AttachedToDesktop)
                    {
                        devices.Add(device);
                    }
                }
                catch (Exception)
                {
                    error = true;
                }
            }

            devices.ForEach(d =>
            {
                WinApi.DEVMODE ndm = NewDevMode();
                WinApi.User_32.EnumDisplaySettings(d.DeviceName, (int)WinApi.DEVMODE_SETTINGS.ENUM_REGISTRY_SETTINGS, ref ndm);
                screens.Add(new Screen()
                {
                    DeviceMode = ndm,
                    ScreenDevice = d,
                    DeviceName = d.DeviceName,
                    IsPrimary = ((d.StateFlags & WinApi.DisplayDeviceStateFlags.PrimaryDevice) == WinApi.DisplayDeviceStateFlags.PrimaryDevice),
                    ScreenWidth = (int)ndm.dmPelsWidth,
                    ScreenHeight = (int)ndm.dmPelsHeight,
                    XPosition = (int)ndm.dmPosition.x,
                    YPosition = (int)ndm.dmPosition.y
                });
            });

            return screens;
        }

        public static void SetPrimaryScreen(string deviceName)
        {
            List<Screen> screenList = GetAllScreen();
            Screen primaryScreen = GetPrimaryScreen(screenList);
            if (primaryScreen.ScreenDevice.DeviceName == deviceName)
                return;

            Screen newPrimaryScreen = GetScreen(deviceName);

            SwitchPrimaryScreen(newPrimaryScreen, primaryScreen);

        }

        public static Screen GetPrimaryScreen(List<Screen> devices)
        {
            foreach (Screen d in devices)
            {
                if ((d.ScreenDevice.StateFlags & WinApi.DisplayDeviceStateFlags.PrimaryDevice) == WinApi.DisplayDeviceStateFlags.PrimaryDevice)
                {
                    return d;
                }
            }
            return null;
        }

        public static List<Screen> GetUnPrimaryScreen(List<Screen> devices)
        {
            List<Screen> dList = new List<Screen>();

            foreach (Screen d in devices)
            {
                if ((d.ScreenDevice.StateFlags & WinApi.DisplayDeviceStateFlags.PrimaryDevice) != WinApi.DisplayDeviceStateFlags.PrimaryDevice)
                {
                    dList.Add(d);
                }
            }
            return dList;
        }

        public static Screen GetScreen(string deviceName)
        {
            List<Screen> screenList = GetAllScreen();
            return screenList.Where(p => p.ScreenDevice.DeviceName == deviceName).FirstOrDefault();
        }

        private static void SwitchPrimaryScreen(Screen newPrimary, Screen oldPrimary)
        {
            MoveOldPrimary(newPrimary, oldPrimary);
            MoveNewPrimary(newPrimary, oldPrimary);
            CommitChange(newPrimary, oldPrimary);
        }

        private static void MoveOldPrimary(Screen newPrimary, Screen oldPrimary)
        {
            WinApi.DEVMODE ndm3 = NewDevMode();
            ndm3.dmFields = WinApi.DEVMODE_Flags.DM_POSITION;
            ndm3.dmPosition.x = (int)newPrimary.DeviceMode.dmPelsWidth;
            ndm3.dmPosition.y = 0;

            WinApi.User_32.ChangeDisplaySettingsEx(oldPrimary.ScreenDevice.DeviceName, ref ndm3, (IntPtr)null, (int)WinApi.DeviceFlags.CDS_UPDATEREGISTRY | (int)WinApi.DeviceFlags.CDS_NORESET, IntPtr.Zero);

        }

        private static void MoveNewPrimary(Screen newPrimary, Screen oldPrimary)
        {
            WinApi.DEVMODE ndm4 = NewDevMode();
            ndm4.dmFields = WinApi.DEVMODE_Flags.DM_POSITION;
            ndm4.dmPosition.x = 0;
            ndm4.dmPosition.y = 0;
            WinApi.User_32.ChangeDisplaySettingsEx(newPrimary.ScreenDevice.DeviceName, ref ndm4, (IntPtr)null, (int)WinApi.DeviceFlags.CDS_SET_PRIMARY | (int)WinApi.DeviceFlags.CDS_UPDATEREGISTRY | (int)WinApi.DeviceFlags.CDS_NORESET, IntPtr.Zero);
        }

        private static void CommitChange(Screen newPrimary, Screen oldPrimary)
        {
            WinApi.DEVMODE ndm5 = NewDevMode();
            WinApi.User_32.ChangeDisplaySettingsEx(oldPrimary.ScreenDevice.DeviceName, ref ndm5, (IntPtr)null, (int)WinApi.DeviceFlags.CDS_UPDATEREGISTRY, (IntPtr)null);

            WinApi.DEVMODE ndm6 = NewDevMode();
            WinApi.User_32.ChangeDisplaySettingsEx(newPrimary.ScreenDevice.DeviceName, ref ndm6, (IntPtr)null, (int)WinApi.DeviceFlags.CDS_SET_PRIMARY | (int)WinApi.DeviceFlags.CDS_UPDATEREGISTRY, IntPtr.Zero);
        }


        private static WinApi.DEVMODE NewDevMode()
        {
            WinApi.DEVMODE dm = new WinApi.DEVMODE();
            dm.dmDeviceName = new String(new char[31]);
            dm.dmFormName = new String(new char[31]);
            dm.dmSize = (ushort)Marshal.SizeOf(dm);
            return dm;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                var list = ScreenHelper.GetAllScreen();
                foreach (var s in list)
                {
                    Console.WriteLine("{0} {1}", s.DeviceName, s.IsPrimary ? "Primary" : "");
                }
                return;
            }

            try
            {
                ScreenHelper.SetPrimaryScreen(args[0]);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }

            if (args.Length >= 2)
            {
                int ms = 0;
                int.TryParse(args[1], out ms);
                if (ms > 0)
                {
                    Thread.Sleep(ms);
                }
            }
        }
    }
}
