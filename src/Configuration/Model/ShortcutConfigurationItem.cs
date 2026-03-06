using System.Text.Json.Serialization;

namespace KeyboardNinja.Configuration.Model;

internal sealed class ShortcutConfigurationItem
{
  [JsonPropertyName("name")]
	public required string Name { get; init; }

   [JsonPropertyName("trigger")]
	public required string Trigger { get; init; }

	[JsonPropertyName("action")]
	public required ShortcutActionConfiguration Action { get; init; }
}
