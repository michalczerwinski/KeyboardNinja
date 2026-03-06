namespace KeyboardNinja.Configuration;

internal interface IShortcutConfigurationService
{
	IReadOnlyList<ShortcutBinding> CurrentShortcuts { get; }

	string ConfigurationFilePath { get; }

	event EventHandler? ShortcutsChanged;

	void OpenConfigurationEditor();
}
