using SharpHook.Native;

namespace KeyboardNinja.Mappings.Clipboard;

public record class ClipboardCopyLine() : MappingRule("Clipboard", "Copy current line of text to clipboard", KeyCode.VcC, KeyCode.VcL)
{
    public override Task ExecutePressAsync() => MultipleKeyPressAndRelease([
            new (KeyCode.VcHome),
            new (KeyCode.VcHome),
            new (KeyCode.VcEnd, Shift: true)
        ]);
}
