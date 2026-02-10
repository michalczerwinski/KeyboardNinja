using SharpHook.Native;

namespace KeyboardNinja.Mappings.Navigation;

public record class GoToHome() : MappingRule("Navigation", "Go to the beginning of the line", KeyCode.VcF, KeyCode.VcN)
{
    public override Task ExecutePressAsync() => KeyPressAndRelease(KeyCode.VcHome);
}
