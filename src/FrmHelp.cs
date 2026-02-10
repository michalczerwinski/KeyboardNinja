
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
        helpText.Append(@"{\rtf1\ansi {\colortbl;\red0\green0\blue0;\red0\green0\blue255;}");

        foreach (var group in Program.MappingRules.GroupBy(m => m.Category))
        {
            helpText.Append($"\\b {group.Key} \\b0 \\line ");

            foreach (var mapping in group)
            {
                var usageInfo = mapping.UsageCount switch { 0 => string.Empty, 1=>"[Used 1 time]", _ => $"[Used {mapping.UsageCount} times]" };
                helpText.Append($"{mapping.Description} ({GetKeyDescription(mapping.PrimaryKey)} + {GetKeyDescription(mapping.SecondaryKey)}) \\cf2 {usageInfo} \\cf \\line ");
            }

            helpText.Append(" \\line ");
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
