using KeyboardNinja.Configuration;
using KeyboardNinja.Helpers;

namespace KeyboardNinja;

internal sealed class KeyboardNinjaApplicationContext : ApplicationContext
{
	private readonly IServiceProvider _serviceProvider;
	private readonly ShortcutConfigurationService _shortcutConfigurationService;
	private readonly IUiDispatcher _uiDispatcher;
	private readonly NotifyIcon _notifyIcon;

	public KeyboardNinjaApplicationContext(IServiceProvider serviceProvider, ShortcutConfigurationService shortcutConfigurationService, IUiDispatcher uiDispatcher)
	{
		_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
		_shortcutConfigurationService = shortcutConfigurationService ?? throw new ArgumentNullException(nameof(shortcutConfigurationService));
		_uiDispatcher = uiDispatcher ?? throw new ArgumentNullException(nameof(uiDispatcher));
		_shortcutConfigurationService.ConfigurationReloaded += ShortcutConfigurationService_ConfigurationReloaded;
		_shortcutConfigurationService.ConfigurationReloadFailed += ShortcutConfigurationService_ConfigurationReloadFailed;
		_notifyIcon = new NotifyIcon
		{
			Icon = IconHelper.CreateNinjaIcon(),
			Text = Application.ProductName,
			Visible = true,
			ContextMenuStrip = BuildContextMenu(),
		};
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			_shortcutConfigurationService.ConfigurationReloaded -= ShortcutConfigurationService_ConfigurationReloaded;
			_shortcutConfigurationService.ConfigurationReloadFailed -= ShortcutConfigurationService_ConfigurationReloadFailed;
			_notifyIcon.Visible = false;
			_notifyIcon.Dispose();
		}

		base.Dispose(disposing);
	}

	private ContextMenuStrip BuildContextMenu()
	{
		var contextMenu = new ContextMenuStrip();

		var showHelpMenuItem = new ToolStripMenuItem("Show Help");
		showHelpMenuItem.Click += (sender, e) => FormHelper.ToggleForm<FrmHelp>(_serviceProvider);
		contextMenu.Items.Add(showHelpMenuItem);

		var configurationMenuItem = new ToolStripMenuItem("Configuration");
		configurationMenuItem.Click += ConfigurationMenuItem_Click;
		contextMenu.Items.Add(configurationMenuItem);

		var exitMenuItem = new ToolStripMenuItem("Exit");
		exitMenuItem.Click += (sender, e) => ExitThread();
		contextMenu.Items.Add(exitMenuItem);

		return contextMenu;
	}

	private void ConfigurationMenuItem_Click(object? sender, EventArgs e)
	{
		try
		{
			_shortcutConfigurationService.OpenConfigurationEditor();
		}
		catch (InvalidOperationException ex)
		{
			MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
	}

	private void ShortcutConfigurationService_ConfigurationReloaded(object? sender, ShortcutConfigurationNotificationEventArgs e)
	{
		_uiDispatcher.Post(() => _notifyIcon.ShowBalloonTip(1000, Application.ProductName ?? nameof(KeyboardNinja), e.Message, ToolTipIcon.Info));
	}

	private void ShortcutConfigurationService_ConfigurationReloadFailed(object? sender, ShortcutConfigurationNotificationEventArgs e)
	{
		_uiDispatcher.Post(() => _notifyIcon.ShowBalloonTip(2000, Application.ProductName ?? nameof(KeyboardNinja), $"Configuration reload failed: {e.Message}", ToolTipIcon.Error));
	}
}
