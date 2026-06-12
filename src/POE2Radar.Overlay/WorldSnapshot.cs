using POE2Radar.Core.Game;
using NumVec2 = System.Numerics.Vector2;

namespace POE2Radar.Overlay;

public readonly record struct HpBarSpec(nint Entity, float Width, uint Fill, float BorderWidth, uint Border);

/// <summary>
/// Immutable world-state published by the background world reader (~30 Hz). The render thread reads
/// a volatile reference each frame without locking; a new snapshot replaces the prior one wholesale.
/// </summary>
public sealed record WorldSnapshot(
    bool InGame,
    uint AreaHash,
    int AreaLevel,
    string AreaCode,
    int CharLevel,
    IReadOnlyList<Poe2Live.EntityDot> Entities,
    IReadOnlyList<Poe2Live.Landmark> Landmarks,
    Poe2Live.TerrainData? Terrain,
    IReadOnlyList<NavTarget> NavTargets,
    IReadOnlyList<HpBarSpec> HpSpecs,
    IReadOnlyList<LegendEntry> Legend,
    HashSet<string> SelectedIds)
{
    public static readonly WorldSnapshot Empty = new(
        false, 0, 0, "", 0,
        new List<Poe2Live.EntityDot>(),
        new List<Poe2Live.Landmark>(),
        null,
        new List<NavTarget>(),
        new List<HpBarSpec>(),
        new List<LegendEntry>(),
        new HashSet<string>(StringComparer.Ordinal));
}
