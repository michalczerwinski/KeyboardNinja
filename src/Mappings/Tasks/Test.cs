using KeyboardNinja.Helpers;
using SharpHook.Native;

namespace KeyboardNinja.Mappings.Tasks;

public record class Test() : MappingRule("Test", "Test", KeyCode.VcT, KeyCode.VcO)
{
	public override Task ExecutePressAsync() => Task.Run(() =>
	{
		NotificationHelper.ShowToast("This is a test notification.\n\nAdditional lines with info\nOne more line\nAnd one more", 2000);
	});
}
