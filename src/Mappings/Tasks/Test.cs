using SharpHook.Native;

namespace KeyboardNinja.Mappings.Tasks;

public record class Test() : MappingRule("Test", "Test", KeyCode.VcT, KeyCode.VcO)
{
	public override Task ExecutePressAsync() => Task.Run(() =>
	{
		var outputPath = Path.Combine(AppContext.BaseDirectory, $"screen_analysis_{DateTime.Now:yyyyMMdd_HHmmss}.png");
		var regions = Helpers.ScreenAnalyzer.DetectRegionsAndSave(outputPath);
		if (regions.Count == 0) return;
		Forms.FrmNavigationOverlay.ShowAndNavigate(regions);
	});
}
