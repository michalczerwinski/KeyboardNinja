using KeyboardNinja.Helpers;
using SharpHook.Native;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace KeyboardNinja.Mappings.Tasks;

public record class SwitchToNextTask() : MappingRule("Tasks", "Go to next task", KeyCode.VcT, KeyCode.VcPeriod)
{
	public override Task ExecutePressAsync() => Task.Run(() =>
	{
		var currentWindow = PInvoke.GetForegroundWindow();

		if (currentWindow == HWND.Null)
		{
			return;
		}

		var windows = DesktopHelper.GetTaskbarWindows();

		if (windows.Count == 0)
		{
			return;
		}

		var currentIndex = windows.IndexOf(currentWindow);
		var targetIndex = (windows.Count + currentIndex - 1) % windows.Count;
		DesktopHelper.SwitchActiveTask(windows[targetIndex]);
	});
}
