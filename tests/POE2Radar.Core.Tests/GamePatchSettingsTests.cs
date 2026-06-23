using System.Text.Json;
using POE2Radar.Core.Cheats;
using Xunit;

namespace POE2Radar.Core.Tests;

public sealed class GamePatchSettingsTests
{
    private static readonly JsonSerializerOptions Json = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    [Fact]
    public void Defaults_AreAllDisabled()
    {
        var p = new GamePatchSettings();

        Assert.False(p.NoAtlasFog);
        Assert.False(p.InfiniteZoom);
        Assert.False(p.PlayerLightRadius);
        Assert.Equal(2000f, p.PlayerLightRadiusValue);
    }

    [Fact]
    public void RoundTripsThroughJson()
    {
        var original = new GamePatchSettings
        {
            NoAtlasFog = true,
            InfiniteZoom = true,
            PlayerLightRadius = true,
            PlayerLightRadiusValue = 3500f,
        };

        var json = JsonSerializer.Serialize(original, Json);
        var loaded = JsonSerializer.Deserialize<GamePatchSettings>(json, Json)!;

        Assert.Equal(original.NoAtlasFog, loaded.NoAtlasFog);
        Assert.Equal(original.InfiniteZoom, loaded.InfiniteZoom);
        Assert.Equal(original.PlayerLightRadiusValue, loaded.PlayerLightRadiusValue);
    }
}
