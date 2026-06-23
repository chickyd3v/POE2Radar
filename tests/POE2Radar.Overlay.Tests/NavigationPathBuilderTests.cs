using POE2Radar.Overlay.Navigation;
using NumVec2 = System.Numerics.Vector2;
using Xunit;

namespace POE2Radar.Overlay.Tests;

public sealed class NavigationPathBuilderTests
{
    [Fact]
    public void BuildForwardPath_SkipsWaypointsBehindPlayerOnGoalBearing()
    {
        var player = new NumVec2(28f, 0f);
        var waypoints = new List<(int x, int y)> { (0, 0), (10, 0), (20, 0), (30, 0), (40, 0) };

        var fwd = NavigationPathBuilder.BuildForwardPath(player, waypoints, (40, 0));

        Assert.Equal((30, 0), fwd[0]);
        Assert.Equal((40, 0), fwd[^1]);
        Assert.DoesNotContain((0, 0), fwd);
        Assert.DoesNotContain((10, 0), fwd);
    }

    [Fact]
    public void BuildForwardPath_WalkingTowardGoal_DoesNotFoldBackward()
    {
        // Player at 25 heading to goal at 40; path still lists vertices behind — must not include them.
        var player = new NumVec2(25f, 0f);
        var waypoints = new List<(int x, int y)> { (0, 0), (10, 0), (20, 0), (30, 0), (40, 0) };

        var fwd = NavigationPathBuilder.BuildForwardPath(player, waypoints, (40, 0));

        Assert.DoesNotContain((0, 0), fwd);
        Assert.DoesNotContain((10, 0), fwd);
        Assert.DoesNotContain((20, 0), fwd);
        Assert.Equal((40, 0), fwd[^1]);
    }

    [Fact]
    public void BuildForwardPath_AllVerticesBehind_GoesStraightToGoal()
    {
        var player = new NumVec2(50f, 0f);
        var waypoints = new List<(int x, int y)> { (0, 0), (10, 0), (20, 0) };

        var fwd = NavigationPathBuilder.BuildForwardPath(player, waypoints, (60, 0));

        Assert.Single(fwd);
        Assert.Equal((60, 0), fwd[0]);
    }

    [Fact]
    public void BuildForwardPath_PlayerToGoal_WhenNoWaypointsYet()
    {
        var fwd = NavigationPathBuilder.BuildForwardPath(new NumVec2(5, 5), [], (12, 18));

        Assert.Single(fwd);
        Assert.Equal((12, 18), fwd[0]);
    }

    [Fact]
    public void HasDrawablePath_TrueWithOnlyLiveGoal()
        => Assert.True(NavigationPathBuilder.HasDrawablePath([], (1, 2)));
}
