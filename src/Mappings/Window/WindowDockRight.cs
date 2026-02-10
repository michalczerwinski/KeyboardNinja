using SharpHook.Native;

namespace KeyboardNinja.Mappings.Window;

public record class WindowDockRight() : MappingRule("Window", "Dock window up", KeyCode.VcW, KeyCode.VcL)
{
    public override Task ExecutePressAsync() => KeyPressAndRelease(KeyCode.VcRight, windows: true);
}
