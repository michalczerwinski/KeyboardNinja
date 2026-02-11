using KeyboardNinja.Helpers;
using SharpHook.Native;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace KeyboardNinja.Mappings.Tasks;

public record class SwitchToNextMonitorTask() : MappingRule("Tasks", "Go to task on next monitor", KeyCode.VcT, KeyCode.VcL)
{
	public override Task ExecutePressAsync() => Task.Run(() =>
	{
		var monitorToWindows = DesktopHelper.GetWindowsGroupByMonitors();

		if (monitorToWindows.Count == 0)
		{
			return;
		}

		var currentWindow = PInvoke.GetForegroundWindow();

		if (currentWindow == HWND.Null)
		{
			return;
		}

		var currentMonitor = monitorToWindows.FirstOrDefault(g => g.Value == currentWindow).Key;
		var monitors = monitorToWindows.Keys.ToList();
		var currentIndex = monitors.IndexOf(currentMonitor);
		var targetIndex = (currentIndex + 1) % monitors.Count;
		var firstWindowOnTargetMonitor = monitorToWindows[monitors[targetIndex]];
		DesktopHelper.SwitchActiveTask(firstWindowOnTargetMonitor);
	});
}
