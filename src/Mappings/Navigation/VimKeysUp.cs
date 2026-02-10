using SharpHook.Native;

namespace KeyboardNinja.Mappings.Navigation;

public record class VimKeysUp() : MappingRule("Navigation", "Press the cursor up key", KeyCode.VcF, KeyCode.VcK)
{
    public override Task ExecutePressAsync() => KeyPressAndRelease(KeyCode.VcUp);
}

