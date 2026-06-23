using NumVec2 = System.Numerics.Vector2;

namespace POE2Radar.Overlay.Navigation;

/// <summary>
/// Render-rate navigation path assembly — unit-tested, no game reads.
/// Trims stale waypoints behind the player (toward-goal test) so the line never folds backward.
/// </summary>
public static class NavigationPathBuilder
{
    public static (int x, int y) PlayerCell(NumVec2 playerGrid)
        => ((int)MathF.Round(playerGrid.X), (int)MathF.Round(playerGrid.Y));

    /// <summary>True when there is anything to draw for this target.</summary>
    public static bool HasDrawablePath(IReadOnlyList<(int x, int y)> waypoints, (int x, int y)? liveGoal)
        => waypoints.Count > 0 || liveGoal is not null;

    /// <summary>
    /// First waypoint index that is not behind the player relative to the goal bearing.
    /// Returns <c>waypoints.Count</c> when every stored vertex is behind (draw straight to goal).
    /// </summary>
    public static int FindForwardWaypointIndex(
        NumVec2 playerGrid,
        IReadOnlyList<(int x, int y)> waypoints,
        (int x, int y)? liveGoal)
    {
        if (waypoints.Count == 0) return 0;

        var goal = liveGoal.HasValue ? ToVec(liveGoal.Value) : ToVec(waypoints[^1]);
        var toGoal = goal - playerGrid;
        if (toGoal.LengthSquared() < 1e-4f) return waypoints.Count - 1;

        var goalDir = NumVec2.Normalize(toGoal);
        const float behindCells = 2f; // grid cells of backward tolerance at corners
        for (var i = 0; i < waypoints.Count; i++)
        {
            var ahead = NumVec2.Dot(ToVec(waypoints[i]) - playerGrid, goalDir);
            if (ahead > -behindCells) return i;
        }

        return waypoints.Count;
    }

    /// <summary>
    /// Waypoints ahead of the player plus optional live goal. Never includes vertices behind the
    /// player on the goal bearing (prevents zigzag while walking toward the target).
    /// </summary>
    public static List<(int x, int y)> BuildForwardPath(
        NumVec2 playerGrid,
        IReadOnlyList<(int x, int y)> waypoints,
        (int x, int y)? liveGoal)
    {
        var result = new List<(int x, int y)>();
        if (waypoints.Count > 0)
        {
            var start = FindForwardWaypointIndex(playerGrid, waypoints, liveGoal);
            for (var i = start; i < waypoints.Count; i++)
            {
                if (result.Count > 0 && result[^1] == waypoints[i]) continue;
                result.Add(waypoints[i]);
            }
        }

        if (liveGoal is { } g && (result.Count == 0 || result[^1] != g))
            result.Add(g);

        return result;
    }

    /// <summary>Grid polyline for map projection: live player cell + forward path.</summary>
    public static List<(int x, int y)> BuildDrawPolyline(
        NumVec2 playerGrid,
        IReadOnlyList<(int x, int y)> waypoints,
        (int x, int y)? liveGoal)
    {
        var fwd = BuildForwardPath(playerGrid, waypoints, liveGoal);
        if (fwd.Count == 0 && liveGoal is null) return fwd;

        var poly = new List<(int x, int y)>(fwd.Count + 1) { PlayerCell(playerGrid) };
        foreach (var p in fwd)
        {
            if (poly[^1] == p) continue;
            poly.Add(p);
        }
        return poly;
    }

    private static NumVec2 ToVec((int x, int y) c) => new(c.x, c.y);
}
