using SharpHook.Native;

namespace KeyboardNinja.Mappings.Desktop;

public record class GoToNextDesktop() : MappingRule("Desktop", "Go to next desktop", KeyCode.VcD, KeyCode.VcL)
{
    public override Task ExecutePressAsync() => KeyPressAndRelease(KeyCode.VcRight, control: true, windows: true);
}
