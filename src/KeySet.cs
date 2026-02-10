using SharpHook.Native;

namespace KeyboardNinja;

public record struct KeySet(KeyCode KeyCode, bool Shift = false, bool Windows = false, bool Control = false, bool Alt = false)
{
}