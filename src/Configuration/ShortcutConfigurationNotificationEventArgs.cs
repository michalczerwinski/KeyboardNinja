namespace KeyboardNinja.Configuration;

internal sealed class ShortcutConfigurationNotificationEventArgs(string message) : EventArgs
{
	public string Message { get; } = message;
}
