using SharpHook;
using SharpHook.Native;

namespace KeyboardNinja.Configuration;

internal sealed class ShortcutHookService(IShortcutCatalog shortcutCatalog) : IDisposable
{
	private readonly IShortcutCatalog _shortcutCatalog = shortcutCatalog ?? throw new ArgumentNullException(nameof(shortcutCatalog));
	private readonly Dictionary<KeyCode, DateTimeOffset> _pressingStarted = [];
	private readonly HashSet<KeyCode> _primaryUsed = [];
	private readonly object _stateSyncRoot = new();
	private SimpleGlobalHook? _hook;
	private Task? _hookTask;

	public void Start()
	{
		_hookTask ??= Task.Run(RunHook);
	}

	public void Dispose()
	{
		_hook?.Dispose();
	}

	private void RunHook()
	{
		using var hook = new SimpleGlobalHook(GlobalHookType.Keyboard);
		_hook = hook;
		hook.KeyPressed += Hook_KeyPressed;
		hook.KeyReleased += Hook_KeyReleased;
		hook.Run();
	}

	private void Hook_KeyPressed(object? sender, KeyboardHookEventArgs e)
	{
		if (e.IsEventSimulated || HasModifiers(e.RawEvent.Mask))
		{
			return;
		}

		lock (_stateSyncRoot)
		{
			var shortcuts = _shortcutCatalog.CurrentShortcuts;
			var binding = shortcuts.FirstOrDefault(shortcut => shortcut.SecondaryKey == e.Data.KeyCode && _pressingStarted.ContainsKey(shortcut.PrimaryKey));

			if (binding != null)
			{
				var betweenKeysDelay = DateTimeOffset.UtcNow - _pressingStarted[binding.PrimaryKey];
				e.SuppressEvent = true;

				if (betweenKeysDelay > TimeSpan.FromMilliseconds(100))
				{
					_primaryUsed.Add(binding.PrimaryKey);
					binding.UsageCount++;
					Task.Run(binding.ExecutePressAsync);
				}
				else
				{
					ReplaySuppressedKeys(e.Data.KeyCode);
				}
			}
			else if (_pressingStarted.Count > 0 && !_pressingStarted.ContainsKey(e.Data.KeyCode))
			{
				ReplaySuppressedKeys(null);
			}

			if (shortcuts.Any(shortcut => shortcut.PrimaryKey == e.Data.KeyCode))
			{
				e.SuppressEvent = true;
				if (!_pressingStarted.ContainsKey(e.Data.KeyCode))
				{
					_pressingStarted[e.Data.KeyCode] = DateTimeOffset.UtcNow;
					_primaryUsed.Clear();
				}
			}
		}
	}

	private void Hook_KeyReleased(object? sender, KeyboardHookEventArgs e)
	{
		if (e.IsEventSimulated || HasModifiers(e.RawEvent.Mask))
		{
			return;
		}

		lock (_stateSyncRoot)
		{
			var shortcuts = _shortcutCatalog.CurrentShortcuts;
			var binding = shortcuts.FirstOrDefault(shortcut => shortcut.SecondaryKey == e.Data.KeyCode && _pressingStarted.ContainsKey(shortcut.PrimaryKey));
			if (binding != null)
			{
				e.SuppressEvent = true;
				Task.Run(binding.ExecuteReleaseAsync);
			}

			if (shortcuts.Any(shortcut => shortcut.PrimaryKey == e.Data.KeyCode) && _pressingStarted.ContainsKey(e.Data.KeyCode))
			{
				e.SuppressEvent = true;
				var delay = DateTimeOffset.UtcNow - _pressingStarted[e.Data.KeyCode];

				if (delay < TimeSpan.FromMilliseconds(400) && !_primaryUsed.Contains(e.Data.KeyCode))
				{
					var eventSimulator = new EventSimulator();
					eventSimulator.SimulateKeyPress(e.Data.KeyCode);
					eventSimulator.SimulateKeyRelease(e.Data.KeyCode);
				}

				_pressingStarted.Remove(e.Data.KeyCode);
			}
		}
	}

	private static bool HasModifiers(ModifierMask modifierMask)
	{
		var isShiftPressed = (modifierMask & ModifierMask.LeftShift) > 0 || (modifierMask & ModifierMask.RightShift) > 0;
		var isControlPressed = (modifierMask & ModifierMask.LeftCtrl) > 0 || (modifierMask & ModifierMask.RightCtrl) > 0;
		var isAltPressed = (modifierMask & ModifierMask.LeftAlt) > 0 || (modifierMask & ModifierMask.RightAlt) > 0;
		var isWindowPressed = (modifierMask & ModifierMask.Meta) > 0;
		return isShiftPressed || isControlPressed || isAltPressed || isWindowPressed;
	}

	private void ReplaySuppressedKeys(KeyCode? trailingKeyCode)
	{
		var simulator = new EventSimulator();

		foreach (var pair in _pressingStarted.OrderBy(pair => pair.Value))
		{
			simulator.SimulateKeyPress(pair.Key);
		}

		_pressingStarted.Clear();
		_primaryUsed.Clear();

		if (trailingKeyCode.HasValue)
		{
			simulator.SimulateKeyPress(trailingKeyCode.Value);
		}
	}
}
