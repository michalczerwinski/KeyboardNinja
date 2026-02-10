using KeyboardNinja.Helpers;
using SharpHook.Native;

namespace KeyboardNinja.Mappings.Tasks;

public record class Test() : MappingRule("Test", "Test", KeyCode.VcT, KeyCode.VcO)
{
	public override Task ExecutePressAsync() => Task.Run(() =>
	{
		ToastNotificationHelper.Show("This is a test notification.");
	});
}
