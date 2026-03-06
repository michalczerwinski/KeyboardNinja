namespace KeyboardNinja.Actions;

internal sealed class ShortcutActionRegistry(IEnumerable<IShortcutAction> actions) : IShortcutActionRegistry
{
	private readonly Dictionary<string, IShortcutAction> _actions = actions.ToDictionary(action => action.Name, StringComparer.OrdinalIgnoreCase);

	public IShortcutAction Resolve(string name)
	{
		if (!_actions.TryGetValue(name, out var action))
		{
			throw new InvalidOperationException($"The action '{name}' is not registered.");
		}

		return action;
	}
}
