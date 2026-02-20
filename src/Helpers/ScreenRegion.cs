namespace KeyboardNinja.Helpers;

/// <summary>A large screen region identified by flood fill, with an assigned two-letter navigation label and badge color.</summary>
internal sealed record ScreenRegion(string Label, Point Center, Color BadgeColor);
