namespace KeyboardNinja.Configuration;

internal interface IShortcutCatalog
{
	IReadOnlyList<ShortcutBinding> CurrentShortcuts { get; }

	string ConfigurationFilePath { get; }

	event EventHandler? ShortcutsChanged;

	void OpenConfigurationEditor();
}
