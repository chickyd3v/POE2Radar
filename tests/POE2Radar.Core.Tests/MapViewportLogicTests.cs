using POE2Radar.Core.Game;
using Xunit;

namespace POE2Radar.Core.Tests;

public sealed class MapViewportLogicTests
{
    private const int W = 1920, H = 1080;

    [Fact]
    public void TrySelectLocalVisibleMini_PicksSmallestLocallyVisible()
    {
        var reads = new[]
        {
            new MapViewportLogic.MiniElementRead(false, 0, -20, 0.5f, 0, 0, W, H),
            new MapViewportLogic.MiniElementRead(true, 1, -20, 0.5f, 1600, 800, 1820, 1020),
        };

        Assert.True(MapViewportLogic.TrySelectLocalVisibleMini(reads, out var pick));
        Assert.Equal(1600f, pick.ScreenLeft);
    }

    [Fact]
    public void TrySelectLocalVisibleMini_IgnoresTogglerHistory_BothEverToggled()
    {
        var reads = new[]
        {
            new MapViewportLogic.MiniElementRead(false, 0, -20, 0.5f, 0, 0, W, H),
            new MapViewportLogic.MiniElementRead(true, 2, -20, 0.5f, 1650, 850, 1870, 1070),
        };

        Assert.True(MapViewportLogic.TrySelectLocalVisibleMini(reads, out var pick));
        Assert.True(pick.LocalVisible);
        Assert.True(pick.ScreenLeft > 1600f);
    }

    [Fact]
    public void IsTabMapOpen_CornerLocalVisibleMeansTabOpen()
    {
        Assert.True(MapViewportLogic.IsTabMapOpen(cornerLocalVisible: true));
        Assert.False(MapViewportLogic.IsTabMapOpen(cornerLocalVisible: false));
    }

    [Fact]
    public void ClassifyByIntrinsicSize_LargerElementIsTabMap()
    {
        MapViewportLogic.ClassifyByIntrinsicSize(800, 600, 250, 250, out var firstIsLarge);
        Assert.True(firstIsLarge);

        MapViewportLogic.ClassifyByIntrinsicSize(250, 250, 800, 600, out firstIsLarge);
        Assert.False(firstIsLarge);
    }

    [Fact]
    public void IsPlausibleMinimapRect_RejectsFullscreenLayoutRect()
    {
        Assert.True(MapViewportLogic.IsPlausibleMinimapRect(1650, 850, 1870, 1070, W, H));
        Assert.False(MapViewportLogic.IsPlausibleMinimapRect(171, 72, 3267, 1368, W, H));
    }

    [Fact]
    public void IsTopRightMinimapRect_RejectsMidScreenLayoutRect()
    {
        // Live --map-probe on 3440×1440: corner widget parent-chain rect, not the visible minimap.
        Assert.False(MapViewportLogic.IsTopRightMinimapRect(1719.9f, 720f, 1944.9f, 945f, 3440, 1440));
        Assert.True(MapViewportLogic.IsTopRightMinimapRect(3200f, 90f, 3420f, 310f, 3440, 1440));
    }

    [Fact]
    public void ResolveMinimapClipRect_UsesTopRightFallbackForBadLayoutRect()
    {
        var (l, t, r, b) = MapViewportLogic.ResolveMinimapClipRect(
            1719.9f, 720f, 1944.9f, 945f, 3440, 1440, 1440 / 1600f);

        Assert.True(l > 3000f);
        Assert.True(t < 1440 * 0.25f);
    }

    [Fact]
    public void TopRightMinimapRect_IsTopRightAndScaled()
    {
        var uiScale = H / 1600f;
        var (l, t, r, b) = MapViewportLogic.TopRightMinimapRect(W, H, uiScale);
        var side = MapViewportLogic.DefaultMinimapSide(H, uiScale);

        Assert.InRange(r - l, side - 1f, side + 1f);
        Assert.InRange(b - t, side - 1f, side + 1f);
        Assert.True(l > W * 0.5f);
        Assert.True(t < H * 0.35f);
    }

    [Fact]
    public void DefaultMinimapSide_ScalesWithClientHeight()
    {
        Assert.InRange(MapViewportLogic.DefaultMinimapSide(1440, 0.9f), 270f, 278f);
        Assert.InRange(MapViewportLogic.DefaultMinimapSide(1080, 0.675f), 200f, 210f);
    }

    [Fact]
    public void MapProjectionCenter_MinimapIgnoresPanShiftAndDefaultShift()
    {
        var (x, y) = MapViewportLogic.MapProjectionCenter(
            W, H, shiftX: 14f, shiftY: 173f, offsetX: 0f, offsetY: 0f,
            minimapClip: true, clipLeft: 3070f, clipTop: 9f, clipRight: 3432f, clipBottom: 371f);

        Assert.Equal(3251f, x, 0);
        Assert.Equal(190f, y, 0);
    }

    [Fact]
    public void MapProjectionCenter_LargeMapAppliesDefaultShiftY()
    {
        var (x, y) = MapViewportLogic.MapProjectionCenter(
            W, H, shiftX: 14f, shiftY: 173f, offsetX: 0f, offsetY: 0f,
            minimapClip: false, 0, 0, 0, 0);

        Assert.Equal(W * 0.5f + 14f, x, 0);
        Assert.Equal(H * 0.5f + 173f + MapViewportLogic.MapDefaultShiftY, y, 0);
    }

    [Fact]
    public void TrySelectMinimapFrameRect_PicksLargestTopRightSquareSibling()
    {
        // Live --map-scan-frames @ 3440×1440: 402×402 frame (362px) beats smaller HUD icons.
        var candidates = new[]
        {
            new MapViewportLogic.MinimapFrameCandidate(89, 89, 3092, 178, 3172, 258, true),
            new MapViewportLogic.MinimapFrameCandidate(402, 402, 3070, 9, 3432, 371, true),
            new MapViewportLogic.MinimapFrameCandidate(693, 230, 2817, 380, 3440, 587, true),
        };

        Assert.True(MapViewportLogic.TrySelectMinimapFrameRect(candidates, 3440, 1440,
            out var l, out var t, out var r, out var b));

        Assert.Equal(3070f, l, 0);
        Assert.Equal(9f, t, 0);
        Assert.Equal(3432f, r, 0);
        Assert.Equal(371f, b, 0);
    }

    [Fact]
    public void ClampScreenRect_ScalesAndClampsToWindow()
    {
        var (l, t, r, b) = MapViewportLogic.ClampScreenRect(100, 200, 250, 250, 0.675f, W, H);

        Assert.True(MapViewportLogic.HasArea(l, t, r, b));
        Assert.Equal(100 * 0.675f, l, 3);
        Assert.Equal(200 * 0.675f, t, 3);
    }
}
