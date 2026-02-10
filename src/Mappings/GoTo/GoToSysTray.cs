using SharpHook.Native;

namespace KeyboardNinja.Mappings.Tasks;

public record class GoToSysTray() : MappingRule("Go to", "Go to systray", KeyCode.VcG, KeyCode.VcO)
{
    public override Task ExecutePressAsync() => KeyPressAndRelease(KeyCode.VcB, windows: true);
}