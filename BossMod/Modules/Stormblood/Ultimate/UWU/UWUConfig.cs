namespace BossMod.Stormblood.Ultimate.UWU;

[ConfigDisplay(Order = 0x210, Parent = typeof(StormbloodConfig))]
public class UWUConfig() : ConfigNode()
{
    [PropertyDisplay("Beyond Limits puddle assignments")]
    [GroupDetails(["Garuda", "Ifrit", "Titan"])]
    [GroupPreset("H1 -> R2 -> M1", [-1, -1, 0, -1, 2, -1, -1, 1])]
    public GroupAssignment3 BeyondLimits = new() { Assignments = [-1, -1, 0, -1, 2, -1, -1, 1] };

    [PropertyDisplay("P1: Thermal Low cleanse order")]
    [GroupDetails(["1", "2", "3"])]
    [GroupPreset("M1 -> M2 -> OT", [-1, 2, -1, -1, 0, 1, -1, -1])]
    public GroupAssignment3 P1Friction = new() { Assignments = [-1, 2, -1, -1, 0, 1, -1, -1] };

    [PropertyDisplay("P1: Show hints for fast awaken (during Friction)")]
    public bool P1FastAwaken = false;

    [PropertyDisplay("Titan gaols priorities (close < far)")]
    [GroupDetails(["0", "1", "2", "3", "4", "5", "6", "7"])]
    public GroupAssignmentUnique P3GaolPriorities = GroupAssignmentUnique.Default();

    [PropertyDisplay("Show fixed location for Mistral Song 2")]
    public bool P1MistralSongFixedLocation = true;
}

public class GroupAssignment3 : GroupAssignment
{
    public override bool Validate()
    {
        if (Assignments == null || Assignments.Length != 8)
            return false;
        Span<int> counts = stackalloc int[3];
        counts.Clear();
        foreach (var group in Assignments)
        {
            if (group < -1 || group >= counts.Length)
                return false;
            if (group >= 0)
                ++counts[group];
        }
        return counts[0] == 1 && counts[1] == 1 && counts[2] == 1;
    }
}
