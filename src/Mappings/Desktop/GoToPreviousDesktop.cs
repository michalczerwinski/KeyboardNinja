using SharpHook.Native;

namespace KeyboardNinja.Mappings.Desktop;

public record class GoToPreviousDesktop() : MappingRule("Desktop", "Go to previous desktop", KeyCode.VcD, KeyCode.VcH)
{
    public override Task ExecutePressAsync() => KeyPressAndRelease(KeyCode.VcLeft, control: true, windows: true);
}
