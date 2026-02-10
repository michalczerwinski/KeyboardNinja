using SharpHook.Native;

namespace KeyboardNinja.Mappings.Navigation;

public record class VimKeysRight() : MappingRule("Navigation", "Press the cursor right key", KeyCode.VcF, KeyCode.VcL)
{
    public override Task ExecutePressAsync() => KeyPressAndRelease(KeyCode.VcRight);
}
