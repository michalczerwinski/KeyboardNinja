using System.Runtime.ExceptionServices;
using System.Threading;

namespace KeyboardNinja.Helpers;

internal interface IUiDispatcher
{
	void Invoke(Action action);

	void Post(Action action);
}

internal sealed class UiDispatcher(SynchronizationContext synchronizationContext) : IUiDispatcher
{
	private readonly SynchronizationContext _synchronizationContext = synchronizationContext ?? throw new ArgumentNullException(nameof(synchronizationContext));

	public void Invoke(Action action)
	{
		ArgumentNullException.ThrowIfNull(action);

		if (ReferenceEquals(SynchronizationContext.Current, _synchronizationContext))
		{
			action();
			return;
		}

		Exception? exception = null;
		_synchronizationContext.Send(_ =>
		{
			try
			{
				action();
			}
			catch (Exception ex)
			{
				exception = ex;
			}
		}, null);

		if (exception != null)
		{
			ExceptionDispatchInfo.Capture(exception).Throw();
		}
	}

	public void Post(Action action)
	{
		ArgumentNullException.ThrowIfNull(action);

		_synchronizationContext.Post(_ => action(), null);
	}
}
