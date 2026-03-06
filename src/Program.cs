using KeyboardNinja.Actions;
using KeyboardNinja.Configuration;
using KeyboardNinja.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace KeyboardNinja;

internal static class Program
{
	[STAThread]
	private static void Main()
	{
		ApplicationConfiguration.Initialize();
		SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());

		var services = new ServiceCollection();
		ConfigureServices(services);

		var serviceProvider = services.BuildServiceProvider(validateScopes: true);

		try
		{
			serviceProvider.GetRequiredService<ShortcutConfigurationService>().Initialize();
			serviceProvider.GetRequiredService<ShortcutHookService>().Start();
			Application.Run(serviceProvider.GetRequiredService<KeyboardNinjaApplicationContext>());
		}
		finally
		{
			serviceProvider.Dispose();
		}
	}

	private static void ConfigureServices(IServiceCollection services)
	{
		var uiSynchronizationContext = SynchronizationContext.Current
			?? throw new InvalidOperationException("The Windows Forms synchronization context is not available.");

		services.AddSingleton<IUiDispatcher>(new UiDispatcher(uiSynchronizationContext));
		services.AddSingleton<IShortcutAction, SendKeysAction>();
		services.AddSingleton<IShortcutAction, ShowHelpAction>();
		services.AddSingleton<IShortcutAction, SwitchTaskAction>();
		services.AddSingleton<IShortcutAction, ShowToastAction>();
		services.AddSingleton<IShortcutAction, EditConfigAction>();
		services.AddSingleton<IShortcutActionRegistry, ShortcutActionRegistry>();
		services.AddSingleton<IShortcutConfigurationService, ShortcutConfigurationService>();
		services.AddSingleton(serviceProvider => (ShortcutConfigurationService)serviceProvider.GetRequiredService<IShortcutConfigurationService>());
		services.AddSingleton<ShortcutHookService>();
		services.AddTransient<FrmHelp>();
		services.AddSingleton<KeyboardNinjaApplicationContext>();
	}
}
