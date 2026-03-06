using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json;
using KeyboardNinja.Actions;
using KeyboardNinja.Helpers;
using KeyboardNinja.Configuration.Model;

namespace KeyboardNinja.Configuration;

internal sealed class ShortcutConfigurationService : IShortcutCatalog, IDisposable
{
	private readonly IShortcutActionRegistry _actionRegistry;
	private readonly string _configurationFilePath = Path.Combine(AppContext.BaseDirectory, "keyboard-config.json");
	private readonly System.Threading.Timer _reloadTimer;
	private readonly object _reloadSyncRoot = new();
	private FileSystemWatcher? _watcher;
	private IReadOnlyList<ShortcutBinding> _currentShortcuts = [];
	private string? _lastLoadedConfigurationJson;

	public ShortcutConfigurationService(IShortcutActionRegistry actionRegistry)
	{
		_actionRegistry = actionRegistry ?? throw new ArgumentNullException(nameof(actionRegistry));
		_reloadTimer = new System.Threading.Timer(ReloadConfigurationCallback, this, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
	}

	public event EventHandler? ShortcutsChanged;

	public event EventHandler<ShortcutConfigurationNotificationEventArgs>? ConfigurationReloaded;

	public event EventHandler<ShortcutConfigurationNotificationEventArgs>? ConfigurationReloadFailed;

	public IReadOnlyList<ShortcutBinding> CurrentShortcuts => _currentShortcuts;

	public string ConfigurationFilePath => _configurationFilePath;

	public void Initialize()
	{
		EnsureConfigurationFileExists();
		LoadConfiguration(notifyOnSuccess: false);
		StartWatching();
	}

	public void OpenConfigurationEditor()
	{
		EnsureConfigurationFileExists();

		try
		{
			Process.Start(new ProcessStartInfo
			{
				FileName = _configurationFilePath,
				UseShellExecute = true,
			});
		}
		catch (Win32Exception ex)
		{
			throw new InvalidOperationException("KeyboardNinja could not open the configuration file in the default editor.", ex);
		}
	}

	public void Dispose()
	{
		_reloadTimer.Dispose();
		_watcher?.Dispose();
	}

	private static void ReloadConfigurationCallback(object? state)
	{
		if (state is not ShortcutConfigurationService service)
		{
			return;
		}

		try
		{
			service.LoadConfiguration(notifyOnSuccess: true);
		}
		catch (IOException ex)
		{
			service.OnConfigurationReloadFailed(ex.Message);
		}
		catch (JsonException ex)
		{
			service.OnConfigurationReloadFailed(ex.Message);
		}
		catch (InvalidOperationException ex)
		{
			service.OnConfigurationReloadFailed(ex.Message);
		}
		catch (NotSupportedException ex)
		{
			service.OnConfigurationReloadFailed(ex.Message);
		}
	}

	private void StartWatching()
	{
		var configurationDirectory = Path.GetDirectoryName(_configurationFilePath)
			?? throw new InvalidOperationException("The configuration directory could not be resolved.");

		var configurationFileName = Path.GetFileName(_configurationFilePath);
		_watcher = new FileSystemWatcher(configurationDirectory, configurationFileName)
		{
			NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName | NotifyFilters.CreationTime,
		};
		_watcher.Changed += Watcher_Changed;
		_watcher.Created += Watcher_Changed;
		_watcher.Renamed += Watcher_Renamed;
		_watcher.EnableRaisingEvents = true;
	}

	private void LoadConfiguration(bool notifyOnSuccess)
	{
		lock (_reloadSyncRoot)
		{
			var configurationText = ReadConfigurationTextWithRetry();
			var definitions = JsonSerializer.Deserialize<List<ShortcutConfigurationItem>>(configurationText, ShortcutConfigurationJson.SerializerOptions)
				?? throw new InvalidOperationException("The keyboard configuration file is empty or invalid.");
			var bindings = BuildBindings(definitions);

			_currentShortcuts = bindings;
			_lastLoadedConfigurationJson = configurationText;
		}

		ShortcutsChanged?.Invoke(this, EventArgs.Empty);

		if (notifyOnSuccess)
		{
			ConfigurationReloaded?.Invoke(this, new ShortcutConfigurationNotificationEventArgs("keyboard-config.json was reloaded."));
		}
	}

	private IReadOnlyList<ShortcutBinding> BuildBindings(IReadOnlyCollection<ShortcutConfigurationItem> definitions)
	{
		var bindings = new List<ShortcutBinding>(definitions.Count);
		var shortcutKeys = new HashSet<(SharpHook.Native.KeyCode Primary, SharpHook.Native.KeyCode Secondary)>();

		foreach (var definition in definitions)
		{
			var action = _actionRegistry.Resolve(definition.Action.Name);
			var (category, description) = ParseName(definition.Name);
			var (primaryKey, secondaryKey) = ParseTrigger(definition.Trigger);
			var parameters = definition.Action.Parameters is { Count: > 0 }
				? JsonSerializer.SerializeToElement(definition.Action.Parameters, ShortcutConfigurationJson.SerializerOptions)
				: JsonSerializer.SerializeToElement(new { }, ShortcutConfigurationJson.SerializerOptions);

			if (!shortcutKeys.Add((primaryKey, secondaryKey)))
			{
				throw new InvalidOperationException($"The trigger '{definition.Trigger}' is configured more than once.");
			}

			bindings.Add(new ShortcutBinding(category, description, primaryKey, secondaryKey, action, parameters));
		}

		return bindings;
	}

	private static (string Category, string Description) ParseName(string name)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			throw new InvalidOperationException("Each shortcut configuration item must define a non-empty name.");
		}

		var separatorIndex = name.IndexOf('/');
		if (separatorIndex <= 0 || separatorIndex == name.Length - 1)
		{
			throw new InvalidOperationException($"The name '{name}' must use the format 'Category/Description'.");
		}

		var category = name[..separatorIndex].Trim();
		var description = name[(separatorIndex + 1)..].Trim();

		if (category.Length == 0 || description.Length == 0)
		{
			throw new InvalidOperationException($"The name '{name}' must use the format 'Category/Description'.");
		}

		return (category, description);
	}

	private static (SharpHook.Native.KeyCode Primary, SharpHook.Native.KeyCode Secondary) ParseTrigger(string trigger)
	{
		if (string.IsNullOrWhiteSpace(trigger))
		{
			throw new InvalidOperationException("Each shortcut configuration item must define a non-empty trigger.");
		}

		var parts = trigger.Split('+', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
		if (parts.Length != 2)
		{
			throw new InvalidOperationException($"The trigger '{trigger}' must use the format 'Primary+Secondary'.");
		}

		return (KeyCodeParser.Parse(parts[0]), KeyCodeParser.Parse(parts[1]));
	}

	private string ReadConfigurationTextWithRetry()
	{
		Collection<Exception> failures = [];

		for (var attempt = 0; attempt < 5; attempt++)
		{
			try
			{
				return File.ReadAllText(_configurationFilePath);
			}
			catch (IOException ex)
			{
				failures.Add(ex);
			}
			catch (JsonException ex)
			{
				failures.Add(ex);
			}

			Thread.Sleep(150 * (attempt + 1));
		}

		throw new IOException($"KeyboardNinja could not read '{_configurationFilePath}' after multiple attempts.", failures.LastOrDefault());
	}

	private void EnsureConfigurationFileExists()
	{
		if (File.Exists(_configurationFilePath))
		{
			return;
		}

		if (string.IsNullOrEmpty(_lastLoadedConfigurationJson))
		{
			throw new InvalidOperationException($"The configuration file '{_configurationFilePath}' is missing.");
		}

		File.WriteAllText(_configurationFilePath, _lastLoadedConfigurationJson);
	}

	private void OnConfigurationReloadFailed(string message)
	{
		ConfigurationReloadFailed?.Invoke(this, new ShortcutConfigurationNotificationEventArgs(message));
	}

	private void Watcher_Changed(object sender, FileSystemEventArgs e)
	{
		_reloadTimer.Change(TimeSpan.FromMilliseconds(350), Timeout.InfiniteTimeSpan);
	}

	private void Watcher_Renamed(object sender, RenamedEventArgs e)
	{
		_reloadTimer.Change(TimeSpan.FromMilliseconds(350), Timeout.InfiniteTimeSpan);
	}
}
