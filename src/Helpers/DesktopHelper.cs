using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Dwm;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.WindowsAndMessaging;

namespace KeyboardNinja.Helpers;

internal static class DesktopHelper
{
	[ComImport]
	[Guid("a5cd92ff-29be-454c-8d04-d82879fb3f1b")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	private interface IVirtualDesktopManager
	{
		bool IsWindowOnCurrentVirtualDesktop(IntPtr topLevelWindow);
		Guid GetWindowDesktopId(IntPtr topLevelWindow);
		void MoveWindowToDesktop(IntPtr topLevelWindow, ref Guid desktopId);
	}

	[ComImport]
	[Guid("aa509086-5ca9-4c25-8f95-589d3c07b48a")]
	private class VirtualDesktopManager { }

	public static List<HWND> GetTaskbarWindows(HMONITOR? monitor = null)
	{
		if (!monitor.HasValue)
		{
			var foreground = PInvoke.GetForegroundWindow();
			if (foreground != HWND.Null)
			{
				monitor = PInvoke.MonitorFromWindow(foreground, MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST);
			}
		}

		List<HWND> windows = GetDesktopWindows(monitor);

		return windows;
	}

	public static Dictionary<HMONITOR, HWND> GetWindowsGroupByMonitors()
	{
		var windows = GetDesktopWindows(null);

		return windows.GroupBy(g => PInvoke.MonitorFromWindow(g, MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST))
					  .ToDictionary(g => g.Key, g => g.First());
	}

	private static List<HWND> GetDesktopWindows(HMONITOR? monitor)
	{
		IVirtualDesktopManager? desktopManager = null;
		try
		{
			desktopManager = (IVirtualDesktopManager)new VirtualDesktopManager();
		}
		catch { }

		var windows = new List<HWND>();

		PInvoke.EnumWindows((hWnd, _) =>
		{
			if (!PInvoke.IsWindowVisible(hWnd))
			{
				return true;
			}

			if (PInvoke.GetWindowTextLength(hWnd) == 0)
			{
				return true;
			}

			if (PInvoke.GetWindow(hWnd, GET_WINDOW_CMD.GW_OWNER) != HWND.Null)
			{
				return true;
			}

			var exStyle = (WINDOW_EX_STYLE)PInvoke.GetWindowLong(hWnd, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE);
			if (exStyle.HasFlag(WINDOW_EX_STYLE.WS_EX_TOOLWINDOW) && !exStyle.HasFlag(WINDOW_EX_STYLE.WS_EX_APPWINDOW))
			{
				return true;
			}

			unsafe
			{
				int cloaked;
				if (PInvoke.DwmGetWindowAttribute(hWnd, DWMWINDOWATTRIBUTE.DWMWA_CLOAKED, &cloaked, sizeof(int)).Succeeded && cloaked != 0)
				{
					return true;
				}
			}

			if (desktopManager != null)
			{
				try
				{
					if (!desktopManager.IsWindowOnCurrentVirtualDesktop(hWnd))
					{
						return true;
					}
				}
				catch { }
			}

			if (monitor.HasValue && PInvoke.MonitorFromWindow(hWnd, MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST) != monitor.Value)
			{
				return true;
			}

			windows.Add(hWnd);
			return true;
		}, IntPtr.Zero);

		windows.Sort((a, b) => ((nint)a).CompareTo((nint)b));
		return windows;
	}

	public static void SwitchActiveTask(HWND window)
	{
		if (PInvoke.IsIconic(window))
		{
			PInvoke.ShowWindow(window, SHOW_WINDOW_CMD.SW_RESTORE);
		}

		PInvoke.SetForegroundWindow(window);
	}
}
