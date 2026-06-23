using POE2Radar.Overlay.Config;
using Xunit;

namespace POE2Radar.Overlay.Tests;

public sealed class RadarSettingsMigrationTests
{
    [Fact]
    public void Migrate_FoldsLegacyShowPathIntoLayerToggles()
    {
        var s = new RadarSettings
        {
            ShowPath = true,
            PathTogglesMigrated = false,
            ShowPathWorld = false,
            ShowPathMap = false,
            ShowPathMinimap = false,
        };

        var changed = s.Migrate();

        Assert.True(changed);
        Assert.True(s.PathTogglesMigrated);
        Assert.True(s.ShowPathWorld);
        Assert.True(s.ShowPathMap);
        Assert.True(s.ShowPathMinimap);
    }

    [Fact]
    public void Migrate_LegacyShowPathOff_TurnsAllLayersOff()
    {
        var s = new RadarSettings { ShowPath = false, PathTogglesMigrated = false };

        s.Migrate();

        Assert.False(s.ShowPathWorld);
        Assert.False(s.ShowPathMap);
        Assert.False(s.ShowPathMinimap);
    }
}
