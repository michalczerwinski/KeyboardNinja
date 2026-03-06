using System.Text.Json;
using KeyboardNinja.Configuration;

namespace KeyboardNinja.Actions;

internal sealed class ShortcutActionContext(ShortcutBinding binding, JsonElement parameters)
{
	public ShortcutBinding Binding { get; } = binding ?? throw new ArgumentNullException(nameof(binding));

	public JsonElement Parameters { get; } = parameters;

	public T GetParameters<T>()
	{
		return JsonSerializer.Deserialize<T>(Parameters.GetRawText(), ShortcutConfigurationJson.SerializerOptions)
			?? throw new InvalidOperationException($"The '{Binding.Action.Name}' action parameters could not be parsed.");
	}
}
