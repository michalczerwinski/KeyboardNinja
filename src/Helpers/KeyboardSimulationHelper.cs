using SharpHook;
using SharpHook.Native;

namespace KeyboardNinja.Helpers;

internal static class KeyboardSimulationHelper
{
	public static Task SimulateKeySequenceAsync(IEnumerable<KeySet> keys)
	{
		ArgumentNullException.ThrowIfNull(keys);

		return Task.Run(() =>
		{
			var simulator = new EventSimulator();
			foreach (var key in keys)
			{
				SimulateKeyPressAndRelease(key, simulator);
			}
		});
	}

	private static void SimulateKeyPressAndRelease(KeySet key, EventSimulator simulator)
	{
		if (key.Shift)
		{
			simulator.SimulateKeyPress(KeyCode.VcLeftShift);
		}

		if (key.Windows)
		{
			simulator.SimulateKeyPress(KeyCode.VcLeftMeta);
		}

		if (key.Control)
		{
			simulator.SimulateKeyPress(KeyCode.VcLeftControl);
		}

		if (key.Alt)
		{
			simulator.SimulateKeyPress(KeyCode.VcLeftAlt);
		}

		simulator.SimulateKeyPress(key.KeyCode);
		simulator.SimulateKeyRelease(key.KeyCode);

		if (key.Shift)
		{
			simulator.SimulateKeyRelease(KeyCode.VcLeftShift);
		}

		if (key.Windows)
		{
			simulator.SimulateKeyRelease(KeyCode.VcLeftMeta);
		}

		if (key.Control)
		{
			simulator.SimulateKeyRelease(KeyCode.VcLeftControl);
		}

		if (key.Alt)
		{
			simulator.SimulateKeyRelease(KeyCode.VcLeftAlt);
		}
	}
}
