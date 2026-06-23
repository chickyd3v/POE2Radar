namespace POE2Radar.Core.Cheats;

/// <summary>Applies <see cref="GamePatchSettings"/> to a live <see cref="CheatManager"/>.</summary>
public static class CheatPatchSync
{
    public static void Apply(GamePatchSettings settings, ICheatPatchTarget target)
    {
        foreach (var def in CheatDefinition.All())
        {
            if (def.Type == CheatType.PatchConstant)
            {
                if (settings.PlayerLightRadius)
                {
                    var value = Math.Clamp(
                        settings.PlayerLightRadiusValue,
                        def.ConstantMin,
                        def.ConstantMax);
                    if (!target.IsActive(def.Name) || NeedsConstantRefresh(target, def.Name, value))
                        target.SetConstantValue(def.Name, value);
                }
                else if (target.IsActive(def.Name))
                {
                    target.SetEnabled(def.Name, false);
                }
                continue;
            }

            var want = settings.IsEnabled(def.Name);
            var active = target.IsActive(def.Name);
            if (want == active) continue;
            target.SetEnabled(def.Name, want);
        }
    }

    private static bool NeedsConstantRefresh(ICheatPatchTarget target, string name, float value) =>
        target is CheatManager mgr
        && mgr.TryGetCurrentValue(name, out var current)
        && Math.Abs(current - value) > 0.01f;
}
