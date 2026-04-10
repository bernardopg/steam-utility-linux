namespace SteamUtility.Core.Models;

public sealed class StatData
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public int Permission { get; set; }

    public object? MinValue { get; set; }

    public object? MaxValue { get; set; }

    public object? DefaultValue { get; set; }

    public object? Value { get; set; }

    public bool IncrementOnly { get; set; }
}
