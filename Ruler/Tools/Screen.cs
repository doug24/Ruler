using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Interop;

namespace Ruler
{
    public class Screen
    {
        private readonly IntPtr hMonitor;

        private Screen(IntPtr monitor)
        {
            var info = new NativeMethods.MONITORINFOEX();
            NativeMethods.GetMonitorInfo(monitor, info);

            _ = NativeMethods.GetDpiForMonitor(monitor, NativeMethods.MonitorDpiType.MDT_EFFECTIVE_DPI, out uint dpiX, out uint dpiY);
            ScaleX = dpiX / 96.0;
            ScaleY = dpiY / 96.0;

            BoundsPix = new Rect(
                info.rcMonitor.Left, info.rcMonitor.Top,
                info.rcMonitor.Right - info.rcMonitor.Left,
                info.rcMonitor.Bottom - info.rcMonitor.Top);

            BoundsDip = new(
                BoundsPix.Left / ScaleX, BoundsPix.Top / ScaleY,
                BoundsPix.Width / ScaleX, BoundsPix.Height / ScaleY);

            WorkingAreaPix = new Rect(
                info.rcWork.Left, info.rcWork.Top,
                info.rcWork.Right - info.rcWork.Left,
                info.rcWork.Bottom - info.rcWork.Top);

            WorkingAreaDip = new(
                WorkingAreaPix.Left / ScaleX, WorkingAreaPix.Top / ScaleY,
                WorkingAreaPix.Width / ScaleX, WorkingAreaPix.Height / ScaleY);

            Primary = (info.dwFlags & NativeMethods.MONITOR_DEFAULTTOPRIMARY) != 0;

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
            return new Screen(NativeMethods.MonitorFromWindow(handle, NativeMethods.MONITOR_DEFAULTTONEAREST));
        }

        public static Screen FromPoint(Point point)
        {
            return new(NativeMethods.GetMonitor(point));
        }

        public static Screen FromWindow(Window window)
        {
            return new Screen(NativeMethods.MonitorFromWindow(
                new WindowInteropHelper(window).Handle,
                NativeMethods.MONITOR_DEFAULTTONEAREST));
        }

        public static IEnumerable<Screen> AllScreens
        {
            get
            {
                List<Screen> screens = new();

                NativeMethods.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero,
                    delegate (IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData)
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
