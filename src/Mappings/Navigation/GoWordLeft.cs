using SharpHook.Native;

namespace KeyboardNinja.Mappings.Navigation;

public record class GoWordLeft() : MappingRule("Navigation", "Go to one word left", KeyCode.VcF, KeyCode.VcComma)
{
    public override Task ExecutePressAsync() => KeyPressAndRelease(KeyCode.VcLeft, control: true);
}
