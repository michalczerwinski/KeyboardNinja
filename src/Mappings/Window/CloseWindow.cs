using SharpHook.Native;

namespace KeyboardNinja.Mappings.Window;

public record class CloseWindow() : MappingRule("Window", "Close current window (application)", KeyCode.VcW, KeyCode.VcC)
{
    public override Task ExecutePressAsync() => KeyPressAndRelease(KeyCode.VcF4, alt: true);
}
