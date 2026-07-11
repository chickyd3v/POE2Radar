using POE2Radar.Core.Game;
using POE2Radar.Core.Pathfinding;
using Xunit;

namespace POE2Radar.Core.Tests;

public sealed class BotwFlaskLogicTests
{
    [Fact]
    public void Poe2Live_ExposesTryBotwBuffReadContract()
    {
        var method = typeof(Poe2Live).GetMethod(nameof(Poe2Live.TryBotwFlaskBuffTimeLeft));

        Assert.NotNull(method);
        Assert.Equal(typeof(bool), method.ReturnType);
        var parameters = method.GetParameters();
        Assert.Equal(2, parameters.Length);
        Assert.Equal(typeof(nint), parameters[0].ParameterType);
        Assert.True(parameters[1].IsOut);
        Assert.Equal(typeof(float?).MakeByRefType(), parameters[1].ParameterType);
    }

    [Fact]
    public void GridRadius_DefaultScale_IsFourMetresInGrid()
    {
        var expected = (4f * 1f * 10f) / GridConstants.GridToWorld;
        Assert.Equal(expected, BotwFlaskLogic.PresenceGridRadius(1f), precision: 4);
    }

    [Theory]
    [InlineData(null, true)]          // buff absent
    [InlineData(0.5f, true)]          // under 1s
    [InlineData(1.0f, false)]         // exactly 1s — do not refresh yet
    [InlineData(5.0f, false)]
    public void BuffNeedsRefresh(float? timeLeft, bool expected)
        => Assert.Equal(expected, BotwFlaskLogic.BuffNeedsRefresh(timeLeft));

    [Fact]
    public void ShouldFire_RequiresEnabledAndRefreshAndCombatGate()
    {
        Assert.False(BotwFlaskLogic.ShouldFire(enabled: false, buffTimeLeft: null, hasRareInPresence: true, rage: 30));
        Assert.False(BotwFlaskLogic.ShouldFire(enabled: true, buffTimeLeft: 5f, hasRareInPresence: true, rage: 30));
        Assert.False(BotwFlaskLogic.ShouldFire(enabled: true, buffTimeLeft: null, hasRareInPresence: false, rage: 14));
        Assert.True(BotwFlaskLogic.ShouldFire(enabled: true, buffTimeLeft: null, hasRareInPresence: true, rage: 0));
        Assert.True(BotwFlaskLogic.ShouldFire(enabled: true, buffTimeLeft: 0.2f, hasRareInPresence: false, rage: 15));
    }

    [Fact]
    public void FireReason_PrefersRareOverRage()
    {
        Assert.Equal("botw@rare", BotwFlaskLogic.FireReason(hasRareInPresence: true, rage: 30));
        Assert.Equal("botw@rage15", BotwFlaskLogic.FireReason(hasRareInPresence: false, rage: 15));
        Assert.Null(BotwFlaskLogic.FireReason(hasRareInPresence: false, rage: 0));
    }

    [Theory]
    [InlineData("unique_flask_blood_of_the_karui", true)]
    [InlineData("flask_effect_life_not_removed_when_full", true)]
    [InlineData("flask_effect_life", true)]
    [InlineData("flask_effect_mana", false)]
    [InlineData("", false)]
    public void IsBotwFlaskBuff(string name, bool expected)
        => Assert.Equal(expected, BotwFlaskLogic.IsBotwFlaskBuff(name));

    [Fact]
    public void RageStatKey_IsLiveBaseResistsKey()
        => Assert.Equal(0x2B9B, Poe2.StatsComponent.RageStatKey);

    [Theory]
    [InlineData("HideoutCave", false)]
    [InlineData("HideoutBlankDesert", false)]
    [InlineData("P1_Town", false)]
    [InlineData("G_Endgame_Town", false)]
    [InlineData("", false)]
    [InlineData("G1_1", true)]
    [InlineData("MapAtlasSomething", true)]
    public void IsCombatArea_BlocksTownAndHideout(string code, bool expected)
        => Assert.Equal(expected, BotwFlaskLogic.IsCombatArea(code));

    [Fact]
    public void IsHostileRareInRange_FiltersCorrectly()
    {
        var player = new System.Numerics.Vector2(100, 100);
        var r = 5f;
        var rareNear = MakeDot(Poe2Live.Rarity.Rare, friendly: false, alive: true, grid: new(103, 100));
        var magicNear = MakeDot(Poe2Live.Rarity.Magic, friendly: false, alive: true, grid: new(103, 100));
        var rareFar = MakeDot(Poe2Live.Rarity.Unique, friendly: false, alive: true, grid: new(200, 100));
        var rareFriendly = MakeDot(Poe2Live.Rarity.Rare, friendly: true, alive: true, grid: new(103, 100));
        var rareDead = MakeDot(Poe2Live.Rarity.Rare, friendly: false, alive: false, grid: new(103, 100));

        Assert.True(BotwFlaskLogic.IsHostileRareInRange(rareNear, player, r));
        Assert.False(BotwFlaskLogic.IsHostileRareInRange(magicNear, player, r));
        Assert.False(BotwFlaskLogic.IsHostileRareInRange(rareFar, player, r));
        Assert.False(BotwFlaskLogic.IsHostileRareInRange(rareFriendly, player, r));
        Assert.False(BotwFlaskLogic.IsHostileRareInRange(rareDead, player, r));
    }

    private static Poe2Live.EntityDot MakeDot(Poe2Live.Rarity rarity, bool friendly, bool alive, System.Numerics.Vector2 grid)
        => new(1, 0, grid, default, Poe2Live.EntityCategory.Monster, "Metadata/Monsters/X",
            alive ? 10 : 0, alive ? 10 : 10, false, friendly ? (byte)1 : (byte)0, rarity, false);
}
