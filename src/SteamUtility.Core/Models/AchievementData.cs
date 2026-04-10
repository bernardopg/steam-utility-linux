namespace SteamUtility.Core.Models;

public sealed class AchievementData
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string IconNormal { get; set; } = string.Empty;

    public string IconLocked { get; set; } = string.Empty;

    public bool IsHidden { get; set; }

    public int Permission { get; set; }

    public bool Achieved { get; set; }

    public float Percent { get; set; }
}
