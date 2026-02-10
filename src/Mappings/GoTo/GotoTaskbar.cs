using SharpHook.Native;

namespace KeyboardNinja.Mappings.Tasks;

public record class GotoTaskbar() : MappingRule("Go to", "Go to taskbar", KeyCode.VcF, KeyCode.VcSpace)
{
    public override Task ExecutePressAsync() => MultipleKeyPressAndRelease(
        [
            new (KeyCode.VcT, Windows: true),
            //new (KeyCode.VcEnd),
        ]
    );
}