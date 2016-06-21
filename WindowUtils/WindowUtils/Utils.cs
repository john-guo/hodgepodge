using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace WindowUtils
{
    public static class Utils
    {
        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowLongPtr(IntPtr hWnd, WindowLongIndex nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowLongPtr(IntPtr hWnd, WindowLongIndex nIndex);

        [DllImport("user32.dll")]
        public static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

        [DllImport("user32.dll")]
        public static extern IntPtr WindowFromPoint(System.Drawing.Point p);

        [DllImport("user32.dll")]
        public static extern bool AttachThreadInput(int idAttach, int idAttachTo, bool fAttach);

        [DllImport("user32.dll")]
        public static extern int GetWindowThreadProcessId(IntPtr hWnd, out int ProcessId);

        [DllImport("user32.dll")]
        public static extern bool SetSystemCursor(IntPtr hcur, OCRId id);

        [DllImport("user32.dll")]
        public static extern bool SystemParametersInfo(SystemParametersInfoAction uiAction, uint uiParam, ref uint pvParam, uint fWinIni);

        public enum WindowLongIndex : int
        {
            GWL_EXSTYLE = -20,
            GWL_STYLE = -16,
            GWL_WNDPROC = -4,
            GWL_HINSTANCE = -6,
            GWL_HWNDPARENT = -8,
            GWL_ID = -12,
            GWL_USERDATA = -21,
            DWL_DLGPROC = 4,
            DWL_MSGRESULT = 0,
            DWL_USER = 8,
        }

        [Flags]
        public enum ExtendedWindowStyles : long
        {
            WS_EX_ACCEPTFILES = 0x00000010,

            WS_EX_APPWINDOW = 0x00040000,

            WS_EX_CLIENTEDGE = 0x00000200,

            WS_EX_COMPOSITED = 0x02000000,

            WS_EX_CONTEXTHELP = 0x00000400,

            WS_EX_CONTROLPARENT = 0x00010000,

            WS_EX_DLGMODALFRAME = 0x00000001,

            WS_EX_LAYERED = 0x00080000,

            WS_EX_LAYOUTRTL = 0x00400000,

            WS_EX_LEFT = 0x00000000,

            WS_EX_LEFTSCROLLBAR = 0x00004000,

            WS_EX_LTRREADING = 0x00000000,

            WS_EX_MDICHILD = 0x00000040,

            WS_EX_NOACTIVATE = 0x08000000,

            WS_EX_NOINHERITLAYOUT = 0x00100000,

            WS_EX_NOPARENTNOTIFY = 0x00000004,

            WS_EX_OVERLAPPEDWINDOW = WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE,

            WS_EX_PALETTEWINDOW = WS_EX_WINDOWEDGE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST,

            WS_EX_RIGHT = 0x00001000,

            WS_EX_RIGHTSCROLLBAR = 0x00000000,

            WS_EX_RTLREADING = 0x00002000,

            WS_EX_STATICEDGE = 0x00020000,

            WS_EX_TOOLWINDOW = 0x00000080,

            WS_EX_TOPMOST = 0x00000008,

            WS_EX_TRANSPARENT = 0x00000020,

            WS_EX_WINDOWEDGE = 0x00000100

        }

        public enum SystemParametersInfoAction : uint
        {
            SPI_GETBEEP = 0x0001,
            SPI_SETBEEP = 0x0002,
            SPI_GETMOUSE = 0x0003,
            SPI_SETMOUSE = 0x0004,
            SPI_GETBORDER = 0x0005,
            SPI_SETBORDER = 0x0006,
            SPI_GETKEYBOARDSPEED = 0x000A,
            SPI_SETKEYBOARDSPEED = 0x000B,
            SPI_LANGDRIVER = 0x000C,
            SPI_ICONHORIZONTALSPACING = 0x000D,
            SPI_GETSCREENSAVETIMEOUT = 0x000E,
            SPI_SETSCREENSAVETIMEOUT = 0x000F,
            SPI_GETSCREENSAVEACTIVE = 0x0010,
            SPI_SETSCREENSAVEACTIVE = 0x0011,
            SPI_GETGRIDGRANULARITY = 0x0012,
            SPI_SETGRIDGRANULARITY = 0x0013,
            SPI_SETDESKWALLPAPER = 0x0014,
            SPI_SETDESKPATTERN = 0x0015,
            SPI_GETKEYBOARDDELAY = 0x0016,
            SPI_SETKEYBOARDDELAY = 0x0017,
            SPI_ICONVERTICALSPACING = 0x0018,
            SPI_GETICONTITLEWRAP = 0x0019,
            SPI_SETICONTITLEWRAP = 0x001A,
            SPI_GETMENUDROPALIGNMENT = 0x001B,
            SPI_SETMENUDROPALIGNMENT = 0x001C,
            SPI_SETDOUBLECLKWIDTH = 0x001D,
            SPI_SETDOUBLECLKHEIGHT = 0x001E,
            SPI_GETICONTITLELOGFONT = 0x001F,
            SPI_SETDOUBLECLICKTIME = 0x0020,
            SPI_SETMOUSEBUTTONSWAP = 0x0021,
            SPI_SETICONTITLELOGFONT = 0x0022,
            SPI_GETFASTTASKSWITCH = 0x0023,
            SPI_SETFASTTASKSWITCH = 0x0024,
            SPI_SETDRAGFULLWINDOWS = 0x0025,
            SPI_GETDRAGFULLWINDOWS = 0x0026,
            SPI_GETNONCLIENTMETRICS = 0x0029,
            SPI_SETNONCLIENTMETRICS = 0x002A,
            SPI_GETMINIMIZEDMETRICS = 0x002B,
            SPI_SETMINIMIZEDMETRICS = 0x002C,
            SPI_GETICONMETRICS = 0x002D,
            SPI_SETICONMETRICS = 0x002E,
            SPI_SETWORKAREA = 0x002F,
            SPI_GETWORKAREA = 0x0030,
            SPI_SETPENWINDOWS = 0x0031,
            SPI_GETHIGHCONTRAST = 0x0042,
            SPI_SETHIGHCONTRAST = 0x0043,
            SPI_GETKEYBOARDPREF = 0x0044,
            SPI_SETKEYBOARDPREF = 0x0045,
            SPI_GETSCREENREADER = 0x0046,
            SPI_SETSCREENREADER = 0x0047,
            SPI_GETANIMATION = 0x0048,
            SPI_SETANIMATION = 0x0049,
            SPI_GETFONTSMOOTHING = 0x004A,
            SPI_SETFONTSMOOTHING = 0x004B,
            SPI_SETDRAGWIDTH = 0x004C,
            SPI_SETDRAGHEIGHT = 0x004D,
            SPI_SETHANDHELD = 0x004E,
            SPI_GETLOWPOWERTIMEOUT = 0x004F,
            SPI_GETPOWEROFFTIMEOUT = 0x0050,
            SPI_SETLOWPOWERTIMEOUT = 0x0051,
            SPI_SETPOWEROFFTIMEOUT = 0x0052,
            SPI_GETLOWPOWERACTIVE = 0x0053,
            SPI_GETPOWEROFFACTIVE = 0x0054,
            SPI_SETLOWPOWERACTIVE = 0x0055,
            SPI_SETPOWEROFFACTIVE = 0x0056,
            SPI_SETCURSORS = 0x0057,
            SPI_SETICONS = 0x0058,
            SPI_GETDEFAULTINPUTLANG = 0x0059,
            SPI_SETDEFAULTINPUTLANG = 0x005A,
            SPI_SETLANGTOGGLE = 0x005B,
            SPI_GETWINDOWSEXTENSION = 0x005C,
            SPI_SETMOUSETRAILS = 0x005D,
            SPI_GETMOUSETRAILS = 0x005E,
            SPI_SETSCREENSAVERRUNNING = 0x0061,
            SPI_SCREENSAVERRUNNING = SPI_SETSCREENSAVERRUNNING,
            SPI_GETFILTERKEYS = 0x0032,
            SPI_SETFILTERKEYS = 0x0033,
            SPI_GETTOGGLEKEYS = 0x0034,
            SPI_SETTOGGLEKEYS = 0x0035,
            SPI_GETMOUSEKEYS = 0x0036,
            SPI_SETMOUSEKEYS = 0x0037,
            SPI_GETSHOWSOUNDS = 0x0038,
            SPI_SETSHOWSOUNDS = 0x0039,
            SPI_GETSTICKYKEYS = 0x003A,
            SPI_SETSTICKYKEYS = 0x003B,
            SPI_GETACCESSTIMEOUT = 0x003C,
            SPI_SETACCESSTIMEOUT = 0x003D,
            SPI_GETSERIALKEYS = 0x003E,
            SPI_SETSERIALKEYS = 0x003F,
            SPI_GETSOUNDSENTRY = 0x0040,
            SPI_SETSOUNDSENTRY = 0x0041,
            SPI_GETSNAPTODEFBUTTON = 0x005F,
            SPI_SETSNAPTODEFBUTTON = 0x0060,
            SPI_GETMOUSEHOVERWIDTH = 0x0062,
            SPI_SETMOUSEHOVERWIDTH = 0x0063,
            SPI_GETMOUSEHOVERHEIGHT = 0x0064,
            SPI_SETMOUSEHOVERHEIGHT = 0x0065,
            SPI_GETMOUSEHOVERTIME = 0x0066,
            SPI_SETMOUSEHOVERTIME = 0x0067,
            SPI_GETWHEELSCROLLLINES = 0x0068,
            SPI_SETWHEELSCROLLLINES = 0x0069,
            SPI_GETMENUSHOWDELAY = 0x006A,
            SPI_SETMENUSHOWDELAY = 0x006B,
            SPI_GETWHEELSCROLLCHARS = 0x006C,
            SPI_SETWHEELSCROLLCHARS = 0x006D,
            SPI_GETSHOWIMEUI = 0x006E,
            SPI_SETSHOWIMEUI = 0x006F,
            SPI_GETMOUSESPEED = 0x0070,
            SPI_SETMOUSESPEED = 0x0071,
            SPI_GETSCREENSAVERRUNNING = 0x0072,
            SPI_GETDESKWALLPAPER = 0x0073,
            SPI_GETAUDIODESCRIPTION = 0x0074,
            SPI_SETAUDIODESCRIPTION = 0x0075,
            SPI_GETSCREENSAVESECURE = 0x0076,
            SPI_SETSCREENSAVESECURE = 0x0077,
            SPI_GETHUNGAPPTIMEOUT = 0x0078,
            SPI_SETHUNGAPPTIMEOUT = 0x0079,
            SPI_GETWAITTOKILLTIMEOUT = 0x007A,
            SPI_SETWAITTOKILLTIMEOUT = 0x007B,
            SPI_GETWAITTOKILLSERVICETIMEOUT = 0x007C,
            SPI_SETWAITTOKILLSERVICETIMEOUT = 0x007D,
            SPI_GETMOUSEDOCKTHRESHOLD = 0x007E,
            SPI_SETMOUSEDOCKTHRESHOLD = 0x007F,
            SPI_GETPENDOCKTHRESHOLD = 0x0080,
            SPI_SETPENDOCKTHRESHOLD = 0x0081,
            SPI_GETWINARRANGING = 0x0082,
            SPI_SETWINARRANGING = 0x0083,
            SPI_GETMOUSEDRAGOUTTHRESHOLD = 0x0084,
            SPI_SETMOUSEDRAGOUTTHRESHOLD = 0x0085,
            SPI_GETPENDRAGOUTTHRESHOLD = 0x0086,
            SPI_SETPENDRAGOUTTHRESHOLD = 0x0087,
            SPI_GETMOUSESIDEMOVETHRESHOLD = 0x0088,
            SPI_SETMOUSESIDEMOVETHRESHOLD = 0x0089,
            SPI_GETPENSIDEMOVETHRESHOLD = 0x008A,
            SPI_SETPENSIDEMOVETHRESHOLD = 0x008B,
            SPI_GETDRAGFROMMAXIMIZE = 0x008C,
            SPI_SETDRAGFROMMAXIMIZE = 0x008D,
            SPI_GETSNAPSIZING = 0x008E,
            SPI_SETSNAPSIZING = 0x008F,
            SPI_GETDOCKMOVING = 0x0090,
            SPI_SETDOCKMOVING = 0x0091,
            SPI_GETACTIVEWINDOWTRACKING = 0x1000,
            SPI_SETACTIVEWINDOWTRACKING = 0x1001,
            SPI_GETMENUANIMATION = 0x1002,
            SPI_SETMENUANIMATION = 0x1003,
            SPI_GETCOMBOBOXANIMATION = 0x1004,
            SPI_SETCOMBOBOXANIMATION = 0x1005,
            SPI_GETLISTBOXSMOOTHSCROLLING = 0x1006,
            SPI_SETLISTBOXSMOOTHSCROLLING = 0x1007,
            SPI_GETGRADIENTCAPTIONS = 0x1008,
            SPI_SETGRADIENTCAPTIONS = 0x1009,
            SPI_GETKEYBOARDCUES = 0x100A,
            SPI_SETKEYBOARDCUES = 0x100B,
            SPI_GETMENUUNDERLINES = SPI_GETKEYBOARDCUES,
            SPI_SETMENUUNDERLINES = SPI_SETKEYBOARDCUES,
            SPI_GETACTIVEWNDTRKZORDER = 0x100C,
            SPI_SETACTIVEWNDTRKZORDER = 0x100D,
            SPI_GETHOTTRACKING = 0x100E,
            SPI_SETHOTTRACKING = 0x100F,
            SPI_GETMENUFADE = 0x1012,
            SPI_SETMENUFADE = 0x1013,
            SPI_GETSELECTIONFADE = 0x1014,
            SPI_SETSELECTIONFADE = 0x1015,
            SPI_GETTOOLTIPANIMATION = 0x1016,
            SPI_SETTOOLTIPANIMATION = 0x1017,
            SPI_GETTOOLTIPFADE = 0x1018,
            SPI_SETTOOLTIPFADE = 0x1019,
            SPI_GETCURSORSHADOW = 0x101A,
            SPI_SETCURSORSHADOW = 0x101B,
            SPI_GETMOUSESONAR = 0x101C,
            SPI_SETMOUSESONAR = 0x101D,
            SPI_GETMOUSECLICKLOCK = 0x101E,
            SPI_SETMOUSECLICKLOCK = 0x101F,
            SPI_GETMOUSEVANISH = 0x1020,
            SPI_SETMOUSEVANISH = 0x1021,
            SPI_GETFLATMENU = 0x1022,
            SPI_SETFLATMENU = 0x1023,
            SPI_GETDROPSHADOW = 0x1024,
            SPI_SETDROPSHADOW = 0x1025,
            SPI_GETBLOCKSENDINPUTRESETS = 0x1026,
            SPI_SETBLOCKSENDINPUTRESETS = 0x1027,
            SPI_GETUIEFFECTS = 0x103E,
            SPI_SETUIEFFECTS = 0x103F,
            SPI_GETDISABLEOVERLAPPEDCONTENT = 0x1040,
            SPI_SETDISABLEOVERLAPPEDCONTENT = 0x1041,
            SPI_GETCLIENTAREAANIMATION = 0x1042,
            SPI_SETCLIENTAREAANIMATION = 0x1043,
            SPI_GETCLEARTYPE = 0x1048,
            SPI_SETCLEARTYPE = 0x1049,
            SPI_GETSPEECHRECOGNITION = 0x104A,
            SPI_SETSPEECHRECOGNITION = 0x104B,
            SPI_GETFOREGROUNDLOCKTIMEOUT = 0x2000,
            SPI_SETFOREGROUNDLOCKTIMEOUT = 0x2001,
            SPI_GETACTIVEWNDTRKTIMEOUT = 0x2002,
            SPI_SETACTIVEWNDTRKTIMEOUT = 0x2003,
            SPI_GETFOREGROUNDFLASHCOUNT = 0x2004,
            SPI_SETFOREGROUNDFLASHCOUNT = 0x2005,
            SPI_GETCARETWIDTH = 0x2006,
            SPI_SETCARETWIDTH = 0x2007,
            SPI_GETMOUSECLICKLOCKTIME = 0x2008,
            SPI_SETMOUSECLICKLOCKTIME = 0x2009,
            SPI_GETFONTSMOOTHINGTYPE = 0x200A,
            SPI_SETFONTSMOOTHINGTYPE = 0x200B,
            SPI_GETFONTSMOOTHINGCONTRAST = 0x200C,
            SPI_SETFONTSMOOTHINGCONTRAST = 0x200D,
            SPI_GETFOCUSBORDERWIDTH = 0x200E,
            SPI_SETFOCUSBORDERWIDTH = 0x200F,
            SPI_GETFOCUSBORDERHEIGHT = 0x2010,
            SPI_SETFOCUSBORDERHEIGHT = 0x2011,
            SPI_GETFONTSMOOTHINGORIENTATION = 0x2012,
            SPI_SETFONTSMOOTHINGORIENTATION = 0x2013,
            SPI_GETMINIMUMHITRADIUS = 0x2014,
            SPI_SETMINIMUMHITRADIUS = 0x2015,
            SPI_GETMESSAGEDURATION = 0x2016,
            SPI_SETMESSAGEDURATION = 0x2017,
        }


        public enum OCRId : uint
        {
            OCR_APPSTARTING = 32650,
            OCR_NORMAL = 32512,
            OCR_CROSS = 32515,
            OCR_HAND = 32649,
            OCR_HELP = 32651,
            OCR_IBEAM = 32513,
            OCR_NO = 32648,
            OCR_SIZEALL = 32646,
            OCR_SIZENESW = 32643,
            OCR_SIZENS = 32645,
            OCR_SIZENWSE = 32642,
            OCR_SIZEWE = 32644,
            OCR_UP = 32516,
            OCR_WAIT = 32514
        }

        public static IntPtr AddWindowExStyle(IntPtr hwnd, ExtendedWindowStyles styles)
        {
            var oldStyles = GetWindowLongPtr(hwnd, WindowLongIndex.GWL_EXSTYLE);
            return SetWindowLongPtr(hwnd, WindowLongIndex.GWL_EXSTYLE, new IntPtr((long)styles | oldStyles.ToInt64()));
        }

        public static IntPtr RemoveWindowExStyle(IntPtr hwnd, ExtendedWindowStyles styles)
        {
            var oldStyles = Utils.GetWindowLongPtr(hwnd, WindowLongIndex.GWL_EXSTYLE);
            return SetWindowLongPtr(hwnd, WindowLongIndex.GWL_EXSTYLE, new IntPtr(~(long)styles & oldStyles.ToInt64()));
        }

        /// <summary>

        /// Changes the size, position, and Z order of a child, pop-up or top-level window.

        /// </summary>

        /// <param name="hWnd">A handle to the window.</param>

        /// <param name="hWndInsertAfter">A handle to the window to precede the positioned window in the Z order. (HWND value)</param>

        /// <param name="X">The new position of the left side of the window, in client coordinates.</param>

        /// <param name="Y">The new position of the top of the window, in client coordinates.</param>

        /// <param name="W">The new width of the window, in pixels.</param>

        /// <param name="H">The new height of the window, in pixels.</param>

        /// <param name="uFlags">The window sizing and positioning flags. (SWP value)</param>

        /// <returns>Nonzero if function succeeds, zero if function fails.</returns>

        [DllImport("user32.dll", SetLastError = true)]

        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int W, int H, uint uFlags);



        /// <summary>

        /// HWND values for hWndInsertAfter

        /// </summary>

        public static class HWND

        {

            public static readonly IntPtr

            NOTOPMOST = new IntPtr(-2),

            BROADCAST = new IntPtr(0xffff),

            TOPMOST = new IntPtr(-1),

            TOP = new IntPtr(0),

            BOTTOM = new IntPtr(1);

        }



        /// <summary>
        /// SetWindowPos Flags
        /// </summary>
        public static class SWP
        {

            public static readonly uint

            NOSIZE = 0x0001,

            NOMOVE = 0x0002,

            NOZORDER = 0x0004,

            NOREDRAW = 0x0008,

            NOACTIVATE = 0x0010,

            DRAWFRAME = 0x0020,

            FRAMECHANGED = 0x0020,

            SHOWWINDOW = 0x0040,

            HIDEWINDOW = 0x0080,

            NOCOPYBITS = 0x0100,

            NOOWNERZORDER = 0x0200,

            NOREPOSITION = 0x0200,

            NOSENDCHANGING = 0x0400,

            DEFERERASE = 0x2000,

            ASYNCWINDOWPOS = 0x4000;

        }

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }
    }
}
