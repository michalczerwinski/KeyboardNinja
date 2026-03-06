using SharpHook;
﻿using KeyboardNinja.Helpers;
using SharpHook.Native;

namespace KeyboardNinja;

public abstract record class MappingRule(string Category, string Description, KeyCode PrimaryKey, KeyCode SecondaryKey)
{
    public int UsageCount { get; set; } = 0;

    public virtual Task ExecutePressAsync() => Task.CompletedTask;

    public virtual Task ExecuteReleaseAsync() => Task.CompletedTask;

    public Task MultipleKeyPressAndRelease(KeySet[] keys) => KeyboardSimulationHelper.SimulateKeySequenceAsync(keys);

    protected Task KeyPressAndRelease(KeyCode keyCode, bool shift = false, bool windows = false, bool control = false, bool alt = false)
        => KeyboardSimulationHelper.SimulateKeySequenceAsync([new KeySet(keyCode, shift, windows, control, alt)]);
}
