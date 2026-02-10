using SharpHook.Native;

namespace KeyboardNinja.Mappings.Window;

public record class MoveWindowLeft() : MappingRule("Window", "Move window left (another monitor)", KeyCode.VcW, KeyCode.VcComma)
{
    public override Task ExecutePressAsync() => KeyPressAndRelease(KeyCode.VcLeft, windows: true, shift: true);
}
