namespace POE2Radar.Core.Cheats;

public enum CheatType
{
    NopInstruction,
    ReplaceBytes,
    PatchConstant,
}

public sealed record CheatDefinition(
    string Name,
    string ShortName,
    byte?[] Pattern,
    int TargetOffset,
    byte[] PatchBytes,
    CheatType Type,
    int RipInstrLen = 0,
    int RipDispOffset = 0,
    float ConstantDefault = 0f,
    float ConstantMin = 0f,
    float ConstantMax = 100f)
{
    public static IReadOnlyList<CheatDefinition> All() =>
    [
        new("NoAtlasFog", "Fog",
            [0xF3, 0x0F, 0x59, 0x51, null, 0xF3, 0x0F, 0x58, 0xC1],
            TargetOffset: 0,
            PatchBytes: [0x90, 0x90, 0x90, 0x90, 0x90],
            CheatType.NopInstruction),

        new("InfiniteZoom", "Zoom",
            [0xF3, 0x0F, 0x5F, 0xC8, 0xF3, 0x0F, 0x5D, 0x0D, null, null, null, null, 0xF3, 0x0F, 0x11, 0x8E],
            TargetOffset: 4,
            PatchBytes: [0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90],
            CheatType.NopInstruction),

        new("PlayerLightRadius", "Light",
            [0xF3, 0x44, 0x0F, 0x58, 0xC6, 0xF3, 0x44, 0x0F, 0x59, 0x3D],
            TargetOffset: 5,
            PatchBytes: [],
            CheatType.PatchConstant,
            RipInstrLen: 9, RipDispOffset: 5,
            ConstantDefault: 2000.0f, ConstantMin: 100.0f, ConstantMax: 50000.0f),
    ];
}
