namespace BossMod.Dawntrail.Trial.T03QueenEternal;

sealed class PowerfulGustKB(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.PowerfulGust, 20f, kind: Kind.DirForward, stopAfterWall: true)
{
}

sealed class DownburstKB(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.Downburst, 10f, stopAfterWall: true)
{
}

sealed class PowerfulGustDownburstRW(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.PowerfulGust, (uint)AID.Downburst]);

