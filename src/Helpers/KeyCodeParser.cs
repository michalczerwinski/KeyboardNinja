using SharpHook.Native;

namespace KeyboardNinja.Helpers;

internal static class KeyCodeParser
{
	private static readonly Dictionary<string, string> Aliases = new(StringComparer.OrdinalIgnoreCase)
	{
		[","] = "Comma",
		["."] = "Period",
		["/"] = "Slash",
		["-"] = "Minus",
		[" "] = "Space",
	};

	public static KeyCode Parse(string value)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			throw new InvalidOperationException("Shortcut keys must not be empty.");
		}

		var normalized = Normalize(value);
		if (Enum.TryParse($"Vc{normalized}", ignoreCase: true, out KeyCode keyCode))
		{
			return keyCode;
		}

		throw new InvalidOperationException($"The key '{value}' is not supported.");
	}

	private static string Normalize(string value)
	{
		var trimmedValue = value.Trim();
		if (Aliases.TryGetValue(trimmedValue, out var alias))
		{
			return alias;
		}

		if (trimmedValue.Length == 1 && char.IsLetterOrDigit(trimmedValue[0]))
		{
			return char.ToUpperInvariant(trimmedValue[0]).ToString();
		}

		return trimmedValue.Replace(" ", string.Empty, StringComparison.Ordinal);
	}
}
