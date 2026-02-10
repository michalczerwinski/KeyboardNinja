using SharpHook.Native;

namespace KeyboardNinja.Mappings.Selection;

public record class SelectWordLeft() : MappingRule("Selection", "Select the word on the left", KeyCode.VcS, KeyCode.VcComma)
{
    public override Task ExecutePressAsync() => KeyPressAndRelease(KeyCode.VcLeft, shift: true, control: true);
}
