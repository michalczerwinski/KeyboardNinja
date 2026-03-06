using KeyboardNinja.Helpers;

namespace KeyboardNinja.Actions;

internal sealed class ShowToastAction : IShortcutAction
{
	public string Name => "show-toast";

	public Task ExecutePressAsync(ShortcutActionContext context)
	{
		var parameters = context.GetParameters<ShowToastParameters>();
		return Task.Run(() => NotificationHelper.ShowToast(parameters.Message, parameters.DurationMs));
	}

	private sealed class ShowToastParameters
	{
		public required string Message { get; init; }

		public int? DurationMs { get; init; }
	}
}
