using KeyboardNinja.Helpers;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.WindowsAndMessaging;

namespace KeyboardNinja.Actions;

internal sealed class SwitchTaskAction : IShortcutAction
{
	public string Name => "switch-task";

	public Task ExecutePressAsync(ShortcutActionContext context)
	{
		var parameters = context.GetParameters<SwitchTaskParameters>();
		return Task.Run(() =>
		{
			var currentWindow = PInvoke.GetForegroundWindow();
			if (currentWindow == HWND.Null)
			{
				return;
			}

			if (string.Equals(parameters.Scope, "adjacent-monitor", StringComparison.OrdinalIgnoreCase))
			{
				SwitchToAdjacentMonitorTask(currentWindow, parameters.Direction);
				return;
			}

			SwitchToTaskOnCurrentMonitor(currentWindow, parameters.Direction);
		});
	}

	private static void SwitchToTaskOnCurrentMonitor(HWND currentWindow, string direction)
	{
		var windows = DesktopHelper.GetTaskbarWindows();
		if (windows.Count == 0)
		{
			return;
		}

		var currentIndex = windows.IndexOf(currentWindow);
		var targetIndex = GetTargetIndex(windows.Count, currentIndex, direction);
		DesktopHelper.SwitchActiveTask(windows[targetIndex]);
	}

	private static void SwitchToAdjacentMonitorTask(HWND currentWindow, string direction)
	{
		var monitorToWindows = DesktopHelper.GetWindowsGroupByMonitors();
		if (monitorToWindows.Count == 0)
		{
			return;
		}

		var currentMonitor = PInvoke.MonitorFromWindow(currentWindow, MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST);
		var monitors = monitorToWindows.Keys.ToList();
		var currentIndex = monitors.IndexOf(currentMonitor);
		var targetIndex = GetTargetIndex(monitors.Count, currentIndex, direction);
		DesktopHelper.SwitchActiveTask(monitorToWindows[monitors[targetIndex]]);
	}

	private static int GetTargetIndex(int count, int currentIndex, string direction)
	{
		if (count == 0)
		{
			throw new InvalidOperationException("The switch-task action requires at least one window.");
		}

		var resolvedCurrentIndex = currentIndex >= 0 ? currentIndex : 0;
		return string.Equals(direction, "previous", StringComparison.OrdinalIgnoreCase)
			? (count + resolvedCurrentIndex - 1) % count
			: (resolvedCurrentIndex + 1) % count;
	}

	private sealed class SwitchTaskParameters
	{
		public required string Direction { get; init; }

		public string Scope { get; init; } = "current-monitor";
	}
}
