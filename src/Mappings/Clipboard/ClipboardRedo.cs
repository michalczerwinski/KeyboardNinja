using SharpHook.Native;

namespace KeyboardNinja.Mappings.Clipboard;

public record class ClipboardRedo() : MappingRule("Clipboard", "Undo", KeyCode.VcC, KeyCode.VcI)
{
    public override Task ExecutePressAsync() => KeyPressAndRelease(KeyCode.VcY, control: true);
}
