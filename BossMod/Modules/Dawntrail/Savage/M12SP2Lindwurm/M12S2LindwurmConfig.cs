namespace BossMod.Dawntrail.Savage.M12S2Lindwurm;

[ConfigDisplay(Order = 0x160, Parent = typeof(DawntrailConfig))]
public sealed class M12S2LindwurmConfig : ConfigNode
{
    public enum Replication1Strategy
    {
        [PropertyDisplay("Clone Relative")]
        CloneRelative,

        [PropertyDisplay("DN")]
        DN
    }

    public enum Replication2Strategy
    {
        [PropertyDisplay("DN (true north)")]
        DN,

        [PropertyDisplay("Banana Codex (west = north)")]
        BananaCodex
    }

    [PropertyDisplay("Replication 1 strategy")]
    public Replication1Strategy Rep1Strategy = Replication1Strategy.CloneRelative;

    [PropertyDisplay("Replication 2 strategy")]
    public Replication2Strategy Rep2Strategy = Replication2Strategy.DN;

    [PropertyDisplay("Show Replication 3 role hints")]
    public bool ShowReplication3Hints = false;
}
