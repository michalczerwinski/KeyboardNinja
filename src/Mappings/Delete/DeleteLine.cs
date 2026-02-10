using SharpHook.Native;

namespace KeyboardNinja.Mappings.Delete;

public record class DeleteLine() : MappingRule("Delete", "Delete current line of text to clipboard", KeyCode.VcD, KeyCode.VcMinus)
{
	public override Task ExecutePressAsync() => MultipleKeyPressAndRelease([
			new (KeyCode.VcHome),
			new (KeyCode.VcHome),
			new (KeyCode.VcEnd, Shift: true),
			new (KeyCode.VcSpace),
			new (KeyCode.VcBackspace),
			new (KeyCode.VcBackspace),
		]);
}

