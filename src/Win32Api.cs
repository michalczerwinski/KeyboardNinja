using System.Runtime.InteropServices;
using System.Text;

namespace KeyboardNinja;
internal static class Win32Api
{
    [DllImport("user32.dll")]
    internal static extern bool SwitchDesktop(IntPtr hDesktop);

    [DllImport("user32.dll", SetLastError = false)]
    internal static extern IntPtr GetDesktopWindow();

    [DllImport("user32.dll", SetLastError = false)]
    internal static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
}
