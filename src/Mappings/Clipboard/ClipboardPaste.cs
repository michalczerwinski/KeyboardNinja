using SharpHook.Native;
using System.Xml.Serialization;

namespace KeyboardNinja.Mappings.Clipboard;

public record class ClipboardPaste() : MappingRule("Clipboard", "Paste from clipboard", KeyCode.VcC, KeyCode.VcP)
{
    public override Task ExecutePressAsync() => KeyPressAndRelease(KeyCode.VcV, control: true);
}
