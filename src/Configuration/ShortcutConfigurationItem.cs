using System.Text.Json;
using System.Text.Json.Serialization;

namespace KeyboardNinja.Configuration;

internal sealed class ShortcutConfigurationItem
{
	[JsonPropertyName("name")]
	public required string Name { get; init; }

	[JsonPropertyName("trigger")]
	public required string Trigger { get; init; }

	[JsonPropertyName("action")]
	public required ShortcutActionConfiguration Action { get; init; }
}

internal sealed class ShortcutActionConfiguration
{
	[JsonPropertyName("name")]
	public required string Name { get; init; }

	[JsonExtensionData]
	public IDictionary<string, JsonElement>? Parameters { get; init; }
}
