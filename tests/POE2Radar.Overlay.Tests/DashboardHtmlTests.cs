using POE2Radar.Overlay.Web;
using Xunit;

namespace POE2Radar.Overlay.Tests;

public sealed class DashboardHtmlTests
{
    [Fact]
    public void SettingsTab_HasPerLayerPathToggles()
    {
        Assert.Contains("data-set=\"showPathWorld\"", DashboardHtml.Page);
        Assert.Contains("data-set=\"showPathMap\"", DashboardHtml.Page);
        Assert.Contains("data-set=\"showPathMinimap\"", DashboardHtml.Page);
        Assert.Contains("Path on minimap", DashboardHtml.Page);
        Assert.DoesNotContain("data-set=\"showPath\"", DashboardHtml.Page);
    }

    [Fact]
    public void SettingsTab_HasGamePatchToggles()
    {
        Assert.Contains("Game Patches", DashboardHtml.Page);
        Assert.Contains("data-patch=\"noAtlasFog\"", DashboardHtml.Page);
        Assert.Contains("data-patch=\"infiniteZoom\"", DashboardHtml.Page);
        Assert.Contains("data-patch=\"playerLightRadius\"", DashboardHtml.Page);
        Assert.Contains("data-patch=\"playerLightRadiusValue\"", DashboardHtml.Page);
        Assert.DoesNotContain("data-patch=\"revealMap\"", DashboardHtml.Page);
        Assert.DoesNotContain("data-patch=\"enemyHealthBars\"", DashboardHtml.Page);
    }

    [Fact]
    public void SettingsTab_HasBloodOfTheWarriorFlaskToggle()
    {
        Assert.Contains("data-set=\"bloodOfTheWarriorFlask\"", DashboardHtml.Page);
        Assert.Contains("Blood of the Warrior", DashboardHtml.Page);
    }
}
