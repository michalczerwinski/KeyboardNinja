using SharpHook.Native;

namespace KeyboardNinja.Mappings.Selection;

public record class SelectHome() : MappingRule("Selection", "Select until beginning", KeyCode.VcS, KeyCode.VcN)
{
    public override Task ExecutePressAsync() => KeyPressAndRelease(KeyCode.VcHome, shift: true);
}
