namespace POE2Radar.Core.Cheats;

/// <summary>Abstraction over <see cref="CheatManager"/> for settings sync and unit tests.</summary>
public interface ICheatPatchTarget
{
    bool SetEnabled(string name, bool enabled);
    bool SetConstantValue(string name, float value);
    bool IsActive(string name);
}
