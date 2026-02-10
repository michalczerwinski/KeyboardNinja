using SharpHook.Native;

namespace KeyboardNinja.Mappings.Window;

public record class WindowDockUp() : MappingRule("Window", "Dock window up", KeyCode.VcW, KeyCode.VcK)
{
    public override Task ExecutePressAsync() => KeyPressAndRelease(KeyCode.VcUp, windows: true);
}
