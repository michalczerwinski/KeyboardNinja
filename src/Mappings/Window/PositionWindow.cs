using SharpHook.Native;

namespace KeyboardNinja.Mappings.Window;

public record class PositionWindow() : MappingRule("Window", "Position", KeyCode.VcW, KeyCode.VcP)
{
    public override Task ExecutePressAsync() => KeyPressAndRelease(KeyCode.VcZ, windows: true);
}
