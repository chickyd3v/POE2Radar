using POE2Radar.Core.Pathfinding;

namespace POE2Radar.Core.Game;

/// <summary>Pure helpers for Blood of the Warrior auto-flask (no memory I/O).</summary>
public static class BotwFlaskLogic
{
    public const float RefreshSeconds = 1f;
    public const int RageThreshold = 15;
    public const float PresenceBaseMetres = 4f;
    public const float MetresToWorld = 10f;

    public static readonly string[] FlaskBuffNames =
    [
        "unique_flask_blood_of_the_karui", // ✓ live BotW-specific buff id
        "flask_effect_life_not_removed_when_full",
        "flask_effect_life",
    ];

    public static float PresenceGridRadius(float aoeScale)
        => (PresenceBaseMetres * aoeScale * MetresToWorld) / GridConstants.GridToWorld;

    public static bool IsBotwFlaskBuff(string name)
    {
        foreach (var n in FlaskBuffNames)
            if (string.Equals(name, n, StringComparison.Ordinal)) return true;
        return false;
    }

    public static bool BuffNeedsRefresh(float? timeLeft)
        => timeLeft is null || timeLeft < RefreshSeconds;

    public static bool ShouldFire(bool enabled, float? buffTimeLeft, bool hasRareInPresence, int rage)
        => enabled
           && BuffNeedsRefresh(buffTimeLeft)
           && (hasRareInPresence || rage >= RageThreshold);

    /// <summary>
    /// BotW is combat-only: hideouts/towns leave rage high while the flask buff often never sticks,
    /// which would spam the life key every cooldown.
    /// </summary>
    public static bool IsCombatArea(string areaCode)
    {
        if (string.IsNullOrEmpty(areaCode)) return false;
        if (areaCode.StartsWith("Hideout", StringComparison.OrdinalIgnoreCase)) return false;
        if (areaCode.Contains("Town", StringComparison.OrdinalIgnoreCase)) return false;
        if (ZoneGuide.Shared.Area(areaCode) is { Town: true }) return false;
        return true;
    }

    public static string? FireReason(bool hasRareInPresence, int rage)
    {
        if (hasRareInPresence) return "botw@rare";
        if (rage >= RageThreshold) return "botw@rage15";
        return null;
    }

    public static bool IsHostileRareInRange(Poe2Live.EntityDot e, System.Numerics.Vector2 playerGrid, float presenceGridRadius)
    {
        if (e.Category != Poe2Live.EntityCategory.Monster) return false;
        if (e.IsFriendly || !e.IsAlive) return false;
        if (e.Rarity < Poe2Live.Rarity.Rare) return false;
        var r2 = presenceGridRadius * presenceGridRadius;
        var dx = e.Grid.X - playerGrid.X;
        var dy = e.Grid.Y - playerGrid.Y;
        return dx * dx + dy * dy <= r2;
    }
}
