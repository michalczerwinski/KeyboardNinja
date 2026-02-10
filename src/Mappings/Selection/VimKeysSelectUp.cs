using SharpHook.Native;

namespace KeyboardNinja.Mappings.Selection;

public record class VimKeysSelectUp() : MappingRule("Selection", "Press the cursor up key with shift", KeyCode.VcS, KeyCode.VcK)
{
    public override Task ExecutePressAsync() => KeyPressAndRelease(KeyCode.VcUp, shift: true);
}
