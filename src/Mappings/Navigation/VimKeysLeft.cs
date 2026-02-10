using SharpHook.Native;

namespace KeyboardNinja.Mappings.Navigation;

public record class VimKeysLeft() : MappingRule("Navigation", "Press the cursor left key", KeyCode.VcF, KeyCode.VcH)
{
    public override Task ExecutePressAsync() => KeyPressAndRelease(KeyCode.VcLeft);
}
