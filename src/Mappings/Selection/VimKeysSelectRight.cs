using SharpHook.Native;

namespace KeyboardNinja.Mappings.Selection;

public record class VimKeysSelectRight() : MappingRule("Selection", "Press the cursor right key with shift", KeyCode.VcS, KeyCode.VcL)
{
    public override Task ExecutePressAsync() => KeyPressAndRelease(KeyCode.VcRight, shift: true);
}
