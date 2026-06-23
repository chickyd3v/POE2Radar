using POE2Radar.Overlay.Navigation;
using NumVec2 = System.Numerics.Vector2;
using Xunit;

namespace POE2Radar.Overlay.Tests;

public sealed class RouteTrackerTests
{
    [Fact]
    public void Maintain_AdvancesCursorAsPlayerWalks()
    {
        var tracker = new RouteTracker();
        tracker.ApplyResult(
            [(0, 0), (10, 0), (20, 0), (30, 0), (40, 0)],
            new NumVec2(40, 0));

        tracker.Maintain(new NumVec2(25, 0));

        Assert.True(tracker.CurrentPoints.Count < 5);
        Assert.Equal((30, 0), tracker.CurrentPoints[0]);
    }

    [Fact]
    public void ShouldReplan_FiresWhenEmpty()
    {
        var tracker = new RouteTracker();
        Assert.True(tracker.ShouldReplan(new NumVec2(0, 0), new NumVec2(10, 10)));
    }
}
