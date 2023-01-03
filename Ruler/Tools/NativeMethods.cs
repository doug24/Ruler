using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace Ruler
{
    [ComVisible(false)]
    internal sealed class NativeMethods
    {
        [DllImport("user32.dll")]
        internal static extern bool LockWindowUpdate(IntPtr hWndLock);

        [DllImport("user32.dll")]
        internal static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        internal static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, int flags);

        [DllImport("user32.dll", EntryPoint = "CreateWindowExW", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        internal static extern IntPtr CreateWindow(int dwExStyle, string lpClassName, string lpWindowName, int dwStyle, int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        internal static extern IntPtr GetModuleHandle([MarshalAs(UnmanagedType.LPWStr)] string? modName);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref POINT pt);

        [DllImport("User32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        internal static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool InvalidateRect(IntPtr hWnd, IntPtr rect, [MarshalAs(UnmanagedType.Bool)] bool erase);


        #region Constants

        internal static readonly IntPtr HWND_TOPMOST = new(-1);
        internal static readonly IntPtr HTTRANSPARENT = new(-1);
        internal const int GWL_STYLE = -16;
        internal const int SM_CMONITORS = 80;
        internal const int WM_NCHITTEST = 0x0084;

        internal const int SWP_NOSIZE = 1;
        internal const int SWP_NOMOVE = 2;
        internal const int SWP_NOZORDER = 4;
        internal const int SWP_NOREDRAW = 8;
        internal const int SWP_NOACTIVATE = 0x10;
        internal const int SWP_FRAMECHANGED = 0x20;
        internal const int SWP_SHOWWINDOW = 0x40;
        internal const int SWP_HIDEWINDOW = 0x80;
        internal const int SWP_NOCOPYBITS = 0x100;
        internal const int SWP_NOOWNERZORDER = 0x200;
        internal const int SWP_NOSENDCHANGING = 0x400;

        internal const int WS_CHILD = 0x40000000;
        internal const int WS_MAXIMIZEBOX = 0x10000;
        internal const int WS_VISIBLE = 0x10000000;

        #endregion

        #region Structures

        [StructLayout(LayoutKind.Sequential)]
        internal struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public RECT(int left, int top, int right, int bottom)
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }
        }

        #endregion

        #region Screen 

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 4)]
        internal class MONITORINFOEX
        {
            internal int cbSize = Marshal.SizeOf(typeof(MONITORINFOEX));
            internal RECT rcMonitor = new();
            internal RECT rcWork = new();
            internal int dwFlags = 0;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)] internal char[] szDevice = new char[32];
        }

        internal static readonly uint MONITOR_DEFAULTTOPRIMARY = 0x00000001;
        internal static readonly uint MONITOR_DEFAULTTONEAREST = 0x00000002;

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern bool GetMonitorInfo(IntPtr hmonitor, [In, Out] MONITORINFOEX info);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr MonitorFromWindow(IntPtr handle, uint flags);

        [DllImport("user32.dll")]
        internal static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, EnumMonitorsDelegate lpfnEnum, IntPtr dwData);

        internal delegate bool EnumMonitorsDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData);

        [DllImport("Shcore.dll")]
        internal static extern uint GetDpiForMonitor(IntPtr hmonitor, MonitorDpiType dpiType, out uint dpiX, out uint dpiY);

        [DllImport("User32.dll", SetLastError = true)]
        private static extern IntPtr MonitorFromPoint(POINT pt, uint flags);

        internal static IntPtr GetMonitor(Rect bounds)
        {
            POINT pt = new((int)(bounds.Left + bounds.Width / 2),
                (int)(bounds.Top + bounds.Height / 2));

            return MonitorFromPoint(pt, MONITOR_DEFAULTTONEAREST);
        }

        internal static IntPtr GetMonitor(Point point)
        {
            POINT pt = new((int)point.X, (int)point.Y);

            return MonitorFromPoint(pt, MONITOR_DEFAULTTONEAREST);
        }

        internal enum MonitorDpiType
        {
            MDT_EFFECTIVE_DPI = 0,
            MDT_ANGULAR_DPI,
            MDT_RAW_DPI,
            MDT_DEFAULT
        };

        #endregion

        #region Magnification

        [StructLayout(LayoutKind.Sequential)]
        internal struct Transformation
        {
            public float m00;
            public float m10;
            public float m20;
            public float m01;
            public float m11;
            public float m21;
            public float m02;
            public float m12;
            public float m22;

            public Transformation(float magnificationFactor)
                : this()
            {
                m00 = magnificationFactor;
                m11 = magnificationFactor;
                m22 = 1.0f;
            }
        }

        [DllImport("Magnification.dll", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool MagInitialize();

        [DllImport("Magnification.dll", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool MagUninitialize();

        [DllImport("Magnification.dll", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool MagSetWindowSource(IntPtr hwnd, RECT rect);

        [DllImport("Magnification.dll", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool MagSetWindowTransform(IntPtr hwnd, ref Transformation pTransform);

        #endregion
    }
}
