using SharpHook.Native;

namespace KeyboardNinja.Mappings.Delete;

public record class DeleteWordRight() : MappingRule("Delete", "Delete word on the left", KeyCode.VcD, KeyCode.VcPeriod)
{
    public override Task ExecutePressAsync() => KeyPressAndRelease(KeyCode.VcDelete, control: true);
}