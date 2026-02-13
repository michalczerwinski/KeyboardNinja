using KeyboardNinja.Helpers;
using KeyboardNinja.Mappings.Clipboard;
using KeyboardNinja.Mappings.Delete;
using KeyboardNinja.Mappings.Desktop;
using KeyboardNinja.Mappings.Navigation;
using KeyboardNinja.Mappings.Selection;
using KeyboardNinja.Mappings.Tasks;
using KeyboardNinja.Mappings.Window;
using SharpHook;
using SharpHook.Native;
using System.Text;

namespace KeyboardNinja;

internal static class Program
{
	/// <summary>
	/// The main entry point for the application.
	/// </summary>
	[STAThread]
	static void Main()
	{
		Task.Run(HandleShortcuts);

		ApplicationConfiguration.Initialize();

		var icon = new NotifyIcon
		{
			Icon = IconHelper.CreateNinjaIcon(),
			Text = Application.ProductName,
			Visible = true,
			ContextMenuStrip = BuildContextMenu()
		};

		//HandleHintForm();
		Application.Run();
	}

	private static void HandleHintForm()
	{
		var x = new System.Windows.Forms.Timer();
		x.Tick += (s, e) =>
		{
			var lastKeyPressed = _pressingStarted.Any() ? _pressingStarted.Values.Max() : (DateTimeOffset?)null;

			string GetKeyDescription(KeyCode keyCode) => keyCode.ToString().Replace("Vc", string.Empty);

			if (_hintForm == null)
			{
				if (DateTime.UtcNow - lastKeyPressed > TimeSpan.FromMilliseconds(1200))
				{
					StringBuilder help = new();
					var applicableRules = MappingRules.Where(r => _pressingStarted.Keys.Contains(r.PrimaryKey)).OrderByDescending(r => r.UsageCount).Take(10);
					foreach (var rule in applicableRules)
					{
						help.AppendLine($"{GetKeyDescription(rule.PrimaryKey)}+{GetKeyDescription(rule.SecondaryKey)}: {rule.Description}");
					}
					_hintForm = NotificationHelper.ShowToast(help.ToString(), null);
				}
			}
			else if (lastKeyPressed == null)
			{
				_hintForm.Close();
				_hintForm.Dispose();
				_hintForm = null;
			}
		};
		x.Interval = 800;
		x.Start();
	}

	private static ContextMenuStrip BuildContextMenu()
	{
		var contextMenu = new ContextMenuStrip();

		var showHelpMenuItem = new ToolStripMenuItem("Show Help");
		showHelpMenuItem.Click += (sender, e) => { Helpers.FormHelper.ToggleForm<FrmHelp>(); };
		contextMenu.Items.Add(showHelpMenuItem);

		var exitMenuItem = new ToolStripMenuItem("Exit");
		exitMenuItem.Click += (sender, e) => { Application.Exit(); };
		contextMenu.Items.Add(exitMenuItem);
		return contextMenu;
	}

	public static List<MappingRule> MappingRules = [
		new VimKeysSelectUp(),
		new VimKeysSelectDown(),
		new VimKeysSelectLeft(),
		new VimKeysSelectRight(),
		new SelectWordLeft(),
		new SelectWordRight(),
		new SelectHome(),
		new SelectEnd(),
		new SelectAll(),
		new ClipboardCopy(),
		new ClipboardPaste(),
		new ClipboardHistory(),
		new ClipboardCopyLine(),
		new ClipboardUndo(),
		new ClipboardRedo(),
		new VimKeysUp(),
		new VimKeysDown(),
		new VimKeysLeft(),
		new VimKeysRight(),
		new GoWordLeft(),
		new GoWordRight(),
		new GoToNextDesktop(),
		new GoToPreviousDesktop(),
		new GoToHome(),
		new GoToEnd(),
		new ShowContextmenu(),
		new SwitchToNextTask(),
		new SwitchToPreviousTask(),
		new SwitchToNextMonitorTask(),
		new SwitchToPreviousMonitorTask(),
		new ShowHelp(),
		new MoveWindowLeft(),
		new MoveWindowRight(),
		new WindowDockDown(),
		new WindowDockUp(),
		new WindowDockRight(),
		new WindowDockLeft(),
		new CloseWindow(),
		new PositionWindow(),
		new GoToSysTray(),
		new GotoTaskbar(),
		new DeleteWordLeft(),
		new DeleteWordRight(),
		new DeleteLine(),

		new QPressCtlEnd(),
		new QPressCtlHome(),
		new QPressEnd(),
		new QPressHome(),

		new Test(),
	];

	private static void HandleShortcuts()
	{
		using var hook = new SimpleGlobalHook(GlobalHookType.Keyboard);
		hook.KeyPressed += Hook_KeyPressed;
		hook.KeyReleased += Hook_KeyReleased;
		hook.Run();
	}

	private static Dictionary<KeyCode, DateTimeOffset> _pressingStarted = new();
	private static HashSet<KeyCode> _primaryUsed = new();
	private static Form? _hintForm = null;

	private static void Hook_KeyPressed(object? sender, KeyboardHookEventArgs e)
	{
		if (e.IsEventSimulated)
		{
			return;
		}

		//Trace.WriteLine($"Press: {e.Data.KeyCode.ToString()}");

		bool isShiftPressed = (e.RawEvent.Mask & ModifierMask.LeftShift) > 0 || (e.RawEvent.Mask & ModifierMask.RightShift) > 0;
		bool isControlPressed = (e.RawEvent.Mask & ModifierMask.LeftCtrl) > 0 || (e.RawEvent.Mask & ModifierMask.RightCtrl) > 0;
		bool isAltPressed = (e.RawEvent.Mask & ModifierMask.LeftAlt) > 0 || (e.RawEvent.Mask & ModifierMask.RightAlt) > 0;
		bool isWindowPressed = (e.RawEvent.Mask & ModifierMask.Meta) > 0;

		if (isShiftPressed || isControlPressed || isAltPressed || isWindowPressed)
		{
			return;
		}

		var rule = MappingRules.FirstOrDefault(m => m.SecondaryKey == e.Data.KeyCode && _pressingStarted.ContainsKey(m.PrimaryKey));

		if (rule != null)
		{
			var betweenKeysDelay = DateTimeOffset.UtcNow - _pressingStarted[rule.PrimaryKey];

			if (betweenKeysDelay > TimeSpan.FromMilliseconds(100))
			{
				e.SuppressEvent = true;
				_primaryUsed.Add(rule.PrimaryKey);
				Task.Run(rule.ExecutePressAsync);
				rule.UsageCount++;
			}
			else
			{
				e.SuppressEvent = true;

				Task.Run(() =>
				{
					var simulator = new EventSimulator();

					foreach (var pair in _pressingStarted.OrderBy(p => p.Value))
					{
						simulator.SimulateKeyPress(pair.Key);
					}

					_pressingStarted.Clear();
					_primaryUsed.Clear();
					simulator.SimulateKeyPress(e.Data.KeyCode);
				});
			}
		}
		else if (_pressingStarted.Any() && !_pressingStarted.ContainsKey(e.Data.KeyCode))
		{
			var simulator = new EventSimulator();

			foreach (var pair in _pressingStarted.OrderBy(p => p.Value))
			{
				simulator.SimulateKeyPress(pair.Key);
			}

			_pressingStarted.Clear();
			_primaryUsed.Clear();
		}

		if (MappingRules.Any(m => m.PrimaryKey == e.Data.KeyCode))
		{
			e.SuppressEvent = true;

			if (!_pressingStarted.TryGetValue(e.Data.KeyCode, out var last))
			{
				_pressingStarted[e.Data.KeyCode] = DateTimeOffset.UtcNow;
				_primaryUsed.Clear();
			}
		}
	}

	private static void Hook_KeyReleased(object? sender, KeyboardHookEventArgs e)
	{
		if (e.IsEventSimulated)
		{
			return;
		}

		//Trace.WriteLine($"Release: {e.Data.KeyCode.ToString()}");

		bool isShiftPressed = (e.RawEvent.Mask & ModifierMask.LeftShift) > 0 || (e.RawEvent.Mask & ModifierMask.RightShift) > 0;
		bool isControlPressed = (e.RawEvent.Mask & ModifierMask.LeftCtrl) > 0 || (e.RawEvent.Mask & ModifierMask.RightCtrl) > 0;
		bool isAltPressed = (e.RawEvent.Mask & ModifierMask.LeftAlt) > 0 || (e.RawEvent.Mask & ModifierMask.RightAlt) > 0;
		bool isWindowPressed = (e.RawEvent.Mask & ModifierMask.Meta) > 0;

		if (isShiftPressed || isControlPressed || isAltPressed || isWindowPressed)
		{
			return;
		}

		var rule = MappingRules.FirstOrDefault(m => m.SecondaryKey == e.Data.KeyCode && _pressingStarted.ContainsKey(m.PrimaryKey));

		if (rule != null)
		{
			e.SuppressEvent = true;
			Task.Run(rule.ExecuteReleaseAsync);
		}

		if (MappingRules.Any(m => m.PrimaryKey == e.Data.KeyCode) && _pressingStarted.ContainsKey(e.Data.KeyCode))
		{
			e.SuppressEvent = true;

			var delay = DateTimeOffset.UtcNow - _pressingStarted[e.Data.KeyCode];

			if (delay < TimeSpan.FromMilliseconds(400) && !_primaryUsed.Contains(e.Data.KeyCode))
			{
				EventSimulator eventSimulator = new();
				eventSimulator.SimulateKeyPress(e.Data.KeyCode);
				eventSimulator.SimulateKeyRelease(e.Data.KeyCode);
			}

			_pressingStarted.Remove(e.Data.KeyCode);
		}
	}
}
