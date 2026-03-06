using System.Text.Json;
using KeyboardNinja.Actions;
using SharpHook.Native;

namespace KeyboardNinja.Configuration;

internal sealed class ShortcutBinding(string category, string description, KeyCode primaryKey, KeyCode secondaryKey, IShortcutAction action, JsonElement parameters)
{
	public string Category { get; } = category;

	public string Description { get; } = description;

	public KeyCode PrimaryKey { get; } = primaryKey;

	public KeyCode SecondaryKey { get; } = secondaryKey;

	public IShortcutAction Action { get; } = action;

	public JsonElement Parameters { get; } = parameters;

	public int UsageCount { get; set; }

	public Task ExecutePressAsync() => Action.ExecutePressAsync(new ShortcutActionContext(this, Parameters));

	public Task ExecuteReleaseAsync() => Action.ExecuteReleaseAsync(new ShortcutActionContext(this, Parameters));
}
