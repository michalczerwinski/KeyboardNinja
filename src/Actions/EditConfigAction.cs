using KeyboardNinja.Helpers;
using System.ComponentModel;
using System.Diagnostics;

namespace KeyboardNinja.Actions;

internal sealed class EditConfigAction : IShortcutAction
{
	private static readonly string ConfigurationFilePath = Path.Combine(AppContext.BaseDirectory, "keyboard-config.json");

	public string Name => "edit-config";

	public Task ExecutePressAsync(ShortcutActionContext context) => Task.Run(() =>
	{
		try
		{
			Process.Start(new ProcessStartInfo
			{
				FileName = ConfigurationFilePath,
				UseShellExecute = true,
			});
		}
		catch (Win32Exception)
		{
			NotificationHelper.ShowToast("KeyboardNinja could not open the configuration file in the default editor.", 3000);
		}
	});
}
