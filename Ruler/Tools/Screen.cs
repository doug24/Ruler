using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Interop;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.HiDpi;

namespace Ruler
{
    public class Screen
    {
        private readonly IntPtr hMonitor;

        private Screen(HMONITOR monitor)
        {
            var info = new NativeMethods.MONITORINFOEX();
            NativeMethods.GetMonitorInfo(monitor, info);

            _ = PInvoke.GetDpiForMonitor(monitor, dpiType: MONITOR_DPI_TYPE.MDT_EFFECTIVE_DPI, out uint dpiX, out uint dpiY);
            ScaleX = dpiX / 96.0;
            ScaleY = dpiY / 96.0;

            BoundsPix = new Rect(
                info.rcMonitor.left, info.rcMonitor.top,
                info.rcMonitor.right - info.rcMonitor.left,
                info.rcMonitor.bottom - info.rcMonitor.top);

            BoundsDip = new(
                BoundsPix.Left / ScaleX, BoundsPix.Top / ScaleY,
                BoundsPix.Width / ScaleX, BoundsPix.Height / ScaleY);

            WorkingAreaPix = new Rect(
                info.rcWork.left, info.rcWork.top,
                info.rcWork.right - info.rcWork.left,
                info.rcWork.bottom - info.rcWork.top);

            WorkingAreaDip = new(
                WorkingAreaPix.Left / ScaleX, WorkingAreaPix.Top / ScaleY,
                WorkingAreaPix.Width / ScaleX, WorkingAreaPix.Height / ScaleY);

            Primary = (info.dwFlags & (uint)MONITOR_FROM_FLAGS.MONITOR_DEFAULTTOPRIMARY) != 0;

            DeviceName = new string(info.szDevice).TrimEnd((char)0);

            hMonitor = monitor;
        }

        public override bool Equals(object? obj)
        {
            return obj is Screen screen &&
                   EqualityComparer<IntPtr>.Default.Equals(hMonitor, screen.hMonitor);
        }

        public override int GetHashCode()
        {
            return -1250308577 + hMonitor.GetHashCode();
        }

        public override string ToString()
        {
            return $"{DeviceName} [{WorkingAreaDip}]";
        }

        public double ScaleX { get; } = 1.0;
        public double ScaleY { get; } = 1.0;

        public Rect BoundsPix { get; }

        public Rect BoundsDip { get; }

        public Rect WorkingAreaPix { get; }

        public Rect WorkingAreaDip { get; }

        public string DeviceName { get; }

        public bool Primary { get; }

        internal static Screen FromHandle(IntPtr handle)
        {
            return new Screen(PInvoke.MonitorFromWindow(new(handle), MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST));
        }

        public static Screen FromPoint(Point point)
        {
            return new(PInvoke.MonitorFromPoint(new((int)point.X, (int)point.Y), MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST));
        }

        public static Screen FromWindow(Window window)
        {
            return new Screen(PInvoke.MonitorFromWindow(
                new(new WindowInteropHelper(window).Handle),
                MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST));
        }

        unsafe public static IEnumerable<Screen> AllScreens
        {
            get
            {
                List<Screen> screens = new();

                PInvoke.EnumDisplayMonitors(new HDC(), (RECT?)null,
                    delegate (HMONITOR hMonitor, HDC hdcMonitor, RECT* lprcMonitor, LPARAM dwData)
                    {
                        screens.Add(new Screen(hMonitor));
                        return true;

                    },
                    IntPtr.Zero);

                return screens;
            }
        }
    }
}
