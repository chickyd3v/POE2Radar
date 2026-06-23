using POE2Radar.Core.Cheats;
using Xunit;

namespace POE2Radar.Core.Tests;

public sealed class CheatPatchSyncTests
{
    private sealed class FakeTarget : ICheatPatchTarget
    {
        public readonly List<(string Op, string Name, object? Arg)> Calls = new();
        private readonly Dictionary<string, bool> _active = new(StringComparer.Ordinal);

        public bool SetEnabled(string name, bool enabled)
        {
            Calls.Add((enabled ? "enable" : "disable", name, null));
            _active[name] = enabled;
            return true;
        }

        public bool SetConstantValue(string name, float value)
        {
            Calls.Add(("constant", name, value));
            _active[name] = true;
            return true;
        }

        public bool IsActive(string name) => _active.TryGetValue(name, out var on) && on;
    }

    [Fact]
    public void Apply_EnablesRequestedPatches()
    {
        var target = new FakeTarget();
        var settings = new GamePatchSettings
        {
            NoAtlasFog = true,
            InfiniteZoom = false,
            PlayerLightRadius = false,
        };

        CheatPatchSync.Apply(settings, target);

        Assert.Contains(("enable", "NoAtlasFog", null), target.Calls);
        Assert.DoesNotContain(target.Calls, c => c.Name == "InfiniteZoom");
        Assert.DoesNotContain(target.Calls, c => c.Name == "PlayerLightRadius");
    }

    [Fact]
    public void Apply_DisablesPreviouslyActivePatch()
    {
        var target = new FakeTarget();
        target.SetEnabled("InfiniteZoom", true);

        CheatPatchSync.Apply(new GamePatchSettings { InfiniteZoom = false }, target);

        Assert.Contains(("disable", "InfiniteZoom", null), target.Calls);
    }

    [Fact]
    public void Apply_PlayerLightRadiusEnabled_SetsClampedValue()
    {
        var target = new FakeTarget();
        var settings = new GamePatchSettings
        {
            PlayerLightRadius = true,
            PlayerLightRadiusValue = 999999f,
        };

        CheatPatchSync.Apply(settings, target);

        Assert.Contains(("constant", "PlayerLightRadius", 50000f), target.Calls);
    }

    [Fact]
    public void Apply_SkipsUnchangedState()
    {
        var target = new FakeTarget();
        var settings = new GamePatchSettings { NoAtlasFog = true };

        CheatPatchSync.Apply(settings, target);
        var firstCount = target.Calls.Count;

        CheatPatchSync.Apply(settings, target);

        Assert.Equal(firstCount, target.Calls.Count);
    }
}
