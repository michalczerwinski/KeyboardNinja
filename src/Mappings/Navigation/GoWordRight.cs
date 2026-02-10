using SharpHook.Native;

namespace KeyboardNinja.Mappings.Navigation;

public record class GoWordRight() : MappingRule("Navigation", "Go to one word right", KeyCode.VcF, KeyCode.VcPeriod)
{
	public override Task ExecutePressAsync() => KeyPressAndRelease(KeyCode.VcRight, control: true);
}
