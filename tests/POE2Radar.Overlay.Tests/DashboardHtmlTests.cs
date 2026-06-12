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
}
