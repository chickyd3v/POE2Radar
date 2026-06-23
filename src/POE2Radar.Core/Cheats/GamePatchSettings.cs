namespace POE2Radar.Core.Cheats;

/// <summary>
/// User-controlled byte-patch toggles (AOB-scanned game instruction patches). Persisted inside
/// <c>radar_settings.json</c> as the <c>patches</c> object. All default off — opt-in only.
/// </summary>
public sealed class GamePatchSettings
{
    public bool NoAtlasFog { get; set; }
    public bool InfiniteZoom { get; set; }
    public bool PlayerLightRadius { get; set; }
    public float PlayerLightRadiusValue { get; set; } = 2000f;

    public bool IsEnabled(string cheatName) => cheatName switch
    {
        "NoAtlasFog" => NoAtlasFog,
        "InfiniteZoom" => InfiniteZoom,
        "PlayerLightRadius" => PlayerLightRadius,
        _ => false,
    };
}
