using System.Text.Json;

namespace KeyboardNinja.Configuration;

internal static class ShortcutConfigurationJson
{
	public static readonly JsonSerializerOptions SerializerOptions = new()
	{
		PropertyNameCaseInsensitive = true,
		ReadCommentHandling = JsonCommentHandling.Skip,
		AllowTrailingCommas = true,
		WriteIndented = true,
	};
}
