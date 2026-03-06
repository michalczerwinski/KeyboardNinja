
﻿using KeyboardNinja.Configuration;
using SharpHook.Native;
using System.Text;

namespace KeyboardNinja;

internal partial class FrmHelp : Form
{
	private readonly IShortcutCatalog _shortcutCatalog;

	public FrmHelp()
		: this(EmptyShortcutCatalog.Instance)
	{
	}

	public FrmHelp(IShortcutCatalog shortcutCatalog)
	{
		_shortcutCatalog = shortcutCatalog;
		InitializeComponent();
		_shortcutCatalog.ShortcutsChanged += ShortcutCatalog_ShortcutsChanged;
		FormClosed += FrmHelp_FormClosed;
		BuildHelp();
	}

	private static string GetKeyDescription(KeyCode keyCode) => keyCode.ToString().Replace("Vc", string.Empty);

	private void BuildHelp()
	{
		StringBuilder helpText = new();
		helpText.Append(@"{\rtf1\ansi\deff0
{\fonttbl{\f0 Arial;}}
{\colortbl;\red0\green0\blue0;\red30\green144\blue255;\red105\green105\blue105;}
\pard\sa200\sl276\slmult1\f0\fs20\tx440\tx29880");

		foreach (var group in _shortcutCatalog.CurrentShortcuts.GroupBy(m => m.Category))
		{
			helpText.Append($"\\b\\fs24\\cf2 {group.Key} \\b0\\fs20\\par\\cf1\n");
			foreach (var mapping in group)
			{
				var usageInfo = mapping.UsageCount switch { 0 => string.Empty, 1 => "[Used 1 time]", _ => $"[Used {mapping.UsageCount} times]" };
				helpText.Append($"\\tab {mapping.Description} (\\b {GetKeyDescription(mapping.PrimaryKey)} + {GetKeyDescription(mapping.SecondaryKey)}\\b0 ) \\cf3 \\tab \\tab {usageInfo} \\cf1\\par\n"); // Description, bold keys, gray usage info
			}
			helpText.Append("\\par\n"); // Extra line break after each category group
		}

		helpText.Append(@"}");
		richTextBox1.Rtf = helpText.ToString();
	}

	private void FrmHelp_Shown(object sender, EventArgs e)
	{
		TopMost = true;
		TopLevel = true;
		BringToFront();
		Activate();
	}

	private void FrmHelp_FormClosed(object? sender, FormClosedEventArgs e)
	{
		_shortcutCatalog.ShortcutsChanged -= ShortcutCatalog_ShortcutsChanged;
	}

	private void ShortcutCatalog_ShortcutsChanged(object? sender, EventArgs e)
	{
		if (IsDisposed)
		{
			return;
		}

		if (InvokeRequired)
		{
			BeginInvoke(BuildHelp);
			return;
		}

		BuildHelp();
	}

	private sealed class EmptyShortcutCatalog : IShortcutCatalog
	{
		public static EmptyShortcutCatalog Instance { get; } = new();

		public IReadOnlyList<ShortcutBinding> CurrentShortcuts { get; } = Array.Empty<ShortcutBinding>();

		public string ConfigurationFilePath => string.Empty;

		public event EventHandler? ShortcutsChanged
		{
			add { }
			remove { }
		}

		public void OpenConfigurationEditor()
		{
			throw new InvalidOperationException("The design-time shortcut catalog cannot open the configuration editor.");
		}
	}
}
