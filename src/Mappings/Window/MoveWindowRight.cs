using SharpHook.Native;

namespace KeyboardNinja.Mappings.Window;

public record class MoveWindowRight() : MappingRule("Window", "Move window right (another monitor)", KeyCode.VcW, KeyCode.VcPeriod)
{
    public override Task ExecutePressAsync() => KeyPressAndRelease(KeyCode.VcRight, windows: true, shift:true);
}
