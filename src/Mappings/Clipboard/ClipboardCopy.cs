using SharpHook.Native;

namespace KeyboardNinja.Mappings.Clipboard;

public record class ClipboardCopy() : MappingRule("Clipboard", "Copy (yank) to clipboard", KeyCode.VcC, KeyCode.VcY)
{
    public override Task ExecutePressAsync() => KeyPressAndRelease(KeyCode.VcC, control: true);
}
