using SharpHook.Native;

namespace KeyboardNinja.Mappings.Window;

public record class WindowDockDown() : MappingRule("Window", "Dock window down", KeyCode.VcW, KeyCode.VcJ)
{
    public override Task ExecutePressAsync() => KeyPressAndRelease(KeyCode.VcDown, windows: true);
}
