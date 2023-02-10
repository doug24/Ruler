using System.Runtime.InteropServices;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;

namespace Ruler
{
    [ComVisible(false)]
    internal sealed class NativeMethods
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 4)]
        internal class MONITORINFOEX
        {
            internal uint cbSize = (uint)Marshal.SizeOf(typeof(MONITORINFOEX));
            internal RECT rcMonitor = new();
            internal RECT rcWork = new();
            internal uint dwFlags = 0;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)] internal char[] szDevice = new char[32];
        }

        // cannot get GetMonitorInfo to work using CsWin32 in any form,
        // so keeping this as-is
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern bool GetMonitorInfo(HMONITOR hmonitor, [In, Out] MONITORINFOEX info);
    }
}
