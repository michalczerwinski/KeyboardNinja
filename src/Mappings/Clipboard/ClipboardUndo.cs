using SharpHook.Native;

namespace KeyboardNinja.Mappings.Clipboard;

public record class ClipboardUndo() : MappingRule("Clipboard", "Undo", KeyCode.VcC, KeyCode.VcU)
{
    public override Task ExecutePressAsync() => KeyPressAndRelease(KeyCode.VcZ, control: true);
}
