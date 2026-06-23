using POE2Radar.Core.Cheats;
using Xunit;

namespace POE2Radar.Core.Tests;

public sealed class CheatDefinitionTests
{
    [Fact]
    public void All_ReturnsThreeBytePatchCheats()
    {
        var defs = CheatDefinition.All();

        Assert.Equal(3, defs.Count);
        Assert.Contains(defs, d => d.Name == "NoAtlasFog");
        Assert.Contains(defs, d => d.Name == "InfiniteZoom");
        Assert.Contains(defs, d => d.Name == "PlayerLightRadius");
        Assert.DoesNotContain(defs, d => d.Name == "RevealMap");
        Assert.DoesNotContain(defs, d => d.Name == "EnemyHealthBars");
    }

    [Fact]
    public void PlayerLightRadius_IsPatchConstantType()
    {
        var light = CheatDefinition.All().Single(d => d.Name == "PlayerLightRadius");

        Assert.Equal(CheatType.PatchConstant, light.Type);
        Assert.Equal(2000f, light.ConstantDefault);
        Assert.Equal(100f, light.ConstantMin);
        Assert.Equal(50000f, light.ConstantMax);
    }
}
