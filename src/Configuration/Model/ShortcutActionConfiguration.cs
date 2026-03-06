using System.Text.Json;
using System.Text.Json.Serialization;

namespace KeyboardNinja.Configuration.Model;

internal sealed class ShortcutActionConfiguration
{
	[JsonPropertyName("name")]
	public required string Name { get; init; }

	[JsonExtensionData]
	public IDictionary<string, JsonElement>? Parameters { get; init; }
}
