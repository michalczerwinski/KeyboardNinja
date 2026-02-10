using SharpHook;
using SharpHook.Native;

namespace KeyboardNinja;

public abstract record class MappingRule(string Category, string Description, KeyCode PrimaryKey, KeyCode SecondaryKey)
{
    public int UsageCount { get; set; } = 0;

    public virtual Task ExecutePressAsync() => Task.CompletedTask;

    public virtual Task ExecuteReleaseAsync() => Task.CompletedTask;

    public Task MultipleKeyPressAndRelease(KeySet[] keys)
        => Task.Run(() =>
        {
            var simulator = new EventSimulator();

            foreach(var key in keys)
            {
                SimulateKeyPressAndRelease(key.KeyCode, key.Shift, key.Windows, key.Control, key.Alt, simulator);
            }
        });

    protected Task KeyPressAndRelease(KeyCode keyCode, bool shift = false, bool windows = false, bool control = false, bool alt = false) => Task.Run(() =>
    {
        var simulator = new EventSimulator();

        SimulateKeyPressAndRelease(keyCode, shift, windows, control, alt, simulator);
    });

    private static void SimulateKeyPressAndRelease(KeyCode keyCode, bool shift, bool windows, bool control, bool alt, EventSimulator simulator)
    {
        if (shift)
        {
            simulator.SimulateKeyPress(KeyCode.VcLeftShift);
        }

        if (windows)
        {
            simulator.SimulateKeyPress(KeyCode.VcLeftMeta);
        }

        if (control)
        {
            simulator.SimulateKeyPress(KeyCode.VcLeftControl);
        }

        if (alt)
        {
            simulator.SimulateKeyPress(KeyCode.VcLeftAlt);
        }

        simulator.SimulateKeyPress(keyCode);
        simulator.SimulateKeyRelease(keyCode);

        if (shift)
        {
            simulator.SimulateKeyRelease(KeyCode.VcLeftShift);
        }

        if (windows)
        {
            simulator.SimulateKeyRelease(KeyCode.VcLeftMeta);
        }

        if (control)
        {
            simulator.SimulateKeyRelease(KeyCode.VcLeftControl);
        }

        if (alt)
        {
            simulator.SimulateKeyRelease(KeyCode.VcLeftAlt);
        }
    }
}
