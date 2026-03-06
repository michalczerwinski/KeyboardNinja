using KeyboardNinja.Helpers;

namespace KeyboardNinja.Actions;

internal sealed class ShowHelpAction(IServiceProvider serviceProvider, IUiDispatcher uiDispatcher) : IShortcutAction
{
	private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
	private readonly IUiDispatcher _uiDispatcher = uiDispatcher ?? throw new ArgumentNullException(nameof(uiDispatcher));

	public string Name => "show-help";

	public Task ExecutePressAsync(ShortcutActionContext context)
	{
		_uiDispatcher.Invoke(() => FormHelper.ToggleForm<FrmHelp>(_serviceProvider));
		return Task.CompletedTask;
	}
}
