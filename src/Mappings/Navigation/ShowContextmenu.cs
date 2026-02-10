using SharpHook.Native;

namespace KeyboardNinja.Mappings.Navigation;

public record class ShowContextmenu() : MappingRule("Navigation", "Show context	menu", KeyCode.VcF, KeyCode.VcP)
{
	public override Task ExecutePressAsync() => KeyPressAndRelease(KeyCode.VcF10, shift: true);
}
