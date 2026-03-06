using KeyboardNinja.Helpers;

namespace KeyboardNinja.Actions;

internal sealed class SendKeysAction : IShortcutAction
{
	public string Name => "send-keys";

	public Task ExecutePressAsync(ShortcutActionContext context)
	{
		var parameters = context.GetParameters<SendKeysParameters>();
		if (parameters.Keys.Count == 0)
		{
			throw new InvalidOperationException("The send-keys action requires at least one key definition.");
		}

		var keys = parameters.Keys
			.Select(key => new KeySet(KeyCodeParser.Parse(key.Key), key.Shift, key.Windows, key.Control, key.Alt))
			.ToArray();

		return KeyboardSimulationHelper.SimulateKeySequenceAsync(keys);
	}

	private sealed class SendKeysParameters
	{
		public List<SendKeysItem> Keys { get; init; } = [];
	}

	private sealed class SendKeysItem
	{
		public required string Key { get; init; }

		public bool Shift { get; init; }

		public bool Windows { get; init; }

		public bool Control { get; init; }

		public bool Alt { get; init; }
	}
}
