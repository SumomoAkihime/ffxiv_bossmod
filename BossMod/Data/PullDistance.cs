namespace BossMod.Data;

public static class PullDistance
{
    // distance is between hitboxes
    private static readonly Dictionary<uint, float> Known = new()
    {
        // Twintania/Nael/Bahamut (UCOB)
        [0x1FDF] = 0,
        [0x1FE1] = 0,
        [0x1FE8] = 0,
        // Crucible of the Unbroken
        [0x4C90] = 5,
        [0x4C95] = 30,
        [0x4CDB] = 0,
        [0x4CDD] = 5,
        [0x4CFA] = 20,
        [0x4D06] = 5,
        [0x4D08] = 5,
        [0x4D18] = 0,
    };

    public static bool TryGet(uint oid, out float distance) => Known.TryGetValue(oid, out distance);
}
