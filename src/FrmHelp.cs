
using SharpHook.Native;
using System.Text;

namespace KeyboardNinja;

public partial class FrmHelp : Form
{
	public FrmHelp()
	{
		InitializeComponent();
		BuildHelp();
	}

	private static string GetKeyDescription(KeyCode keyCode) => keyCode.ToString().Replace("Vc", string.Empty);

	private void BuildHelp()
	{
		StringBuilder helpText = new();
		helpText.Append(@"{\rtf1\ansi\deff0
{\fonttbl{\f0 Arial;}}
{\colortbl;\red0\green0\blue0;\red30\green144\blue255;\red105\green105\blue105;}
\pard\sa200\sl276\slmult1\f0\fs20");

		foreach (var group in Program.MappingRules.GroupBy(m => m.Category))
		{
			helpText.Append($"\\b\\fs24\\cf2 {group.Key} \\b0\\fs20\\par\\cf1\n");
			foreach (var mapping in group)
			{
				var usageInfo = mapping.UsageCount switch { 0 => string.Empty, 1 => "[Used 1 time]", _ => $"[Used {mapping.UsageCount} times]" };
				helpText.Append($"\\tab {mapping.Description} (\\b {GetKeyDescription(mapping.PrimaryKey)} + {GetKeyDescription(mapping.SecondaryKey)}\\b0 ) \\cf3 {usageInfo} \\cf1\\par\n"); // Description, bold keys, gray usage info
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
}
