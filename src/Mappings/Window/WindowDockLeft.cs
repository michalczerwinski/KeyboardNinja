using SharpHook.Native;

namespace KeyboardNinja.Mappings.Window;

public record class WindowDockLeft() : MappingRule("Window", "Dock window up", KeyCode.VcW, KeyCode.VcH)
{
    public override Task ExecutePressAsync() => KeyPressAndRelease(KeyCode.VcLeft, windows: true);
}
