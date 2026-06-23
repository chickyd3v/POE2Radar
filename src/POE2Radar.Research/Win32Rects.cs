using System.Runtime.InteropServices;

namespace POE2Radar.Research;

internal static class Win32Rects
{
    [DllImport("user32.dll")]
    internal static extern bool GetClientRect(nint hWnd, out Rect lpRect);

    [StructLayout(LayoutKind.Sequential)]
    internal struct Rect
    {
        public int Left, Top, Right, Bottom;
        public int Width => Right - Left;
        public int Height => Bottom - Top;
    }
}
