namespace KeyboardNinja.Actions;

internal interface IShortcutActionRegistry
{
	IShortcutAction Resolve(string name);
}
