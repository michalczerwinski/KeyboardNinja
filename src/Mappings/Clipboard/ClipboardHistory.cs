using SharpHook.Native;

namespace KeyboardNinja.Mappings.Clipboard;

public record class ClipboardHistory() : MappingRule("Clipboard", "Paste from history", KeyCode.VcC, KeyCode.VcH)
{
    public override Task ExecutePressAsync() => KeyPressAndRelease(KeyCode.VcV, windows: true);
}
