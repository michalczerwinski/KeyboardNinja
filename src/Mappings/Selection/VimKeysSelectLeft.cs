using SharpHook.Native;

namespace KeyboardNinja.Mappings.Selection;

public record class VimKeysSelectLeft() : MappingRule("Selection", "Press the cursor left key with shift", KeyCode.VcS, KeyCode.VcH)
{
    public override Task ExecutePressAsync() => KeyPressAndRelease(KeyCode.VcLeft, shift: true);
}
