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
        // Additional fixed pull distances from the upstream enemy behavior table.
        [0x2212] = 0, // Garuda
        [0x2215] = 0, // Satin Plume
        [0x2216] = 0, // Spiny Plume
        [0x221A] = 0, // Ifrit
        [0x221E] = 0, // The Ultima Weapon
        [0x4CBF] = 7, // Queen Hawk Piece
    };

    public static bool TryGet(uint oid, out float distance) => Known.TryGetValue(oid, out distance);
}
