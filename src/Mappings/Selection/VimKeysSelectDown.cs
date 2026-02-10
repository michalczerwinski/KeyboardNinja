using SharpHook.Native;

namespace KeyboardNinja.Mappings.Selection;

public record class VimKeysSelectDown() : MappingRule("Selection", "Press the cursor down key with shift", KeyCode.VcS, KeyCode.VcJ)
{
    public override Task ExecutePressAsync() => KeyPressAndRelease(KeyCode.VcDown, shift: true);
}
