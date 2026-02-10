using SharpHook.Native;

namespace KeyboardNinja.Mappings.Selection;

public record class SelectEnd() : MappingRule("Selection", "Select until end", KeyCode.VcS, KeyCode.VcM)
{
    public override Task ExecutePressAsync() => KeyPressAndRelease(KeyCode.VcEnd, shift: true);
}

public record class SelectAll() : MappingRule("Selection", "Select all", KeyCode.VcS, KeyCode.VcO)
{
    public override Task ExecutePressAsync() => KeyPressAndRelease(KeyCode.VcA, control: true);
}
