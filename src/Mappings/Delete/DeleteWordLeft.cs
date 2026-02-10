using SharpHook.Native;

namespace KeyboardNinja.Mappings.Delete;

public record class DeleteWordLeft() : MappingRule("Delete", "Delete word on the left", KeyCode.VcD, KeyCode.VcComma)
{
    public override Task ExecutePressAsync() => KeyPressAndRelease(KeyCode.VcBackspace, control: true);
}
