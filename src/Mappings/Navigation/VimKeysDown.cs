using SharpHook.Native;

namespace KeyboardNinja.Mappings.Navigation;

public record class VimKeysDown() : MappingRule("Navigation", "Press the cursor down key", KeyCode.VcF, KeyCode.VcJ)
{
    public override Task ExecutePressAsync() => KeyPressAndRelease(KeyCode.VcDown);
}
