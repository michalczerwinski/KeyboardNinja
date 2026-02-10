using SharpHook.Native;

namespace KeyboardNinja.Mappings.Selection;

public record class SelectWordRight() : MappingRule("Selection", "Select the word on the right", KeyCode.VcS, KeyCode.VcPeriod)
{
    public override Task ExecutePressAsync() => KeyPressAndRelease(KeyCode.VcRight, shift: true, control: true);
}
