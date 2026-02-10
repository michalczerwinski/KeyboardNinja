using SharpHook.Native;

namespace KeyboardNinja.Mappings.Navigation;

public record class GoToEnd() : MappingRule("Navigation", "Go to the end of the line", KeyCode.VcF, KeyCode.VcM)
{
	public override Task ExecutePressAsync() => KeyPressAndRelease(KeyCode.VcEnd);
}


public record class QPressCtlHome() : MappingRule("Navigation", "Go to the beginning of the document", KeyCode.VcQ, KeyCode.VcK)
{
	public override Task ExecutePressAsync() => KeyPressAndRelease(KeyCode.VcHome, control: true);
}

public record class QPressCtlEnd() : MappingRule("Navigation", "Go to the end of the document", KeyCode.VcQ, KeyCode.VcJ)
{
	public override Task ExecutePressAsync() => KeyPressAndRelease(KeyCode.VcEnd, control: true);
}

public record class QPressHome() : MappingRule("Navigation", "Go to the beginning of the line", KeyCode.VcQ, KeyCode.VcH)
{
	public override Task ExecutePressAsync() => KeyPressAndRelease(KeyCode.VcHome);
}

public record class QPressEnd() : MappingRule("Navigation", "Go to the end of the line", KeyCode.VcQ, KeyCode.VcL)
{
	public override Task ExecutePressAsync() => KeyPressAndRelease(KeyCode.VcEnd);
}
