namespace KeyboardNinja.Actions;

internal interface IShortcutAction
{
	string Name { get; }

	Task ExecutePressAsync(ShortcutActionContext context);

	Task ExecuteReleaseAsync(ShortcutActionContext context) => Task.CompletedTask;
}
