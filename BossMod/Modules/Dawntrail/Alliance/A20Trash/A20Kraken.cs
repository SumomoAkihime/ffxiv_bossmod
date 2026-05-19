namespace BossMod.Dawntrail.Alliance.A20Trash;

public enum OID3 : uint
{
    Kraken = 0x4885,
    Clipper = 0x4888,
    Acrophies = 0x488A,
    LightElemental = 0x4887,
    GreaterPugil = 0x4886,
    Banshee = 0x4889,
    GiantAscetic = 0x488D,
    Alkyoneus = 0x488B,
    GiantRanger = 0x488C
}

public enum AID3 : uint
{
    Banishga = 43562,
    Sucker = 43791,
    Flood = 43792,
    ParalyzeIII = 43570,
    ImpactRoar = 43566,
    Catapult1 = 43567,
    Catapult2 = 43568,
    PowerAttack = 43569,
    MightyStrikes = 43575
}

sealed class MightyStrikes(BossModule module) : Components.CastInterruptHint(module, AID3.MightyStrikes, showNameInHint: true);
sealed class PowerAttack(BossModule module) : Components.SimpleAOEs(module, (uint)AID3.PowerAttack, new AOEShapeCone(20f, 60f.Degrees()));
sealed class CatapultParalyzeIIIBanishga(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID3.Catapult1, (uint)AID3.Catapult2, (uint)AID3.Banishga, (uint)AID3.ParalyzeIII], new AOEShapeCircle(6f));
sealed class ImpactRoar(BossModule module) : Components.RaidwideCast(module, AID3.ImpactRoar);
sealed class Sucker(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID3.Sucker, 15f, kind: Kind.TowardsOrigin);
sealed class Flood(BossModule module) : Components.SimpleAOEs(module, (uint)AID3.Flood, new AOEShapeCircle(8f));

sealed class A20KrakenStates : StateMachineBuilder
{
    public A20KrakenStates(A20Kraken module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MightyStrikes>()
            .ActivateOnEnter<PowerAttack>()
            .ActivateOnEnter<CatapultParalyzeIIIBanishga>()
            .ActivateOnEnter<ImpactRoar>()
            .ActivateOnEnter<Flood>()
            .ActivateOnEnter<Sucker>()
            .Raw.Update = () => module.Enemies(OID3.Kraken).All(e => e.IsDeadOrDestroyed)
                && module.Enemies(OID3.Clipper).All(e => e.IsDeadOrDestroyed)
                && module.Enemies(OID3.Acrophies).All(e => e.IsDeadOrDestroyed)
                && module.Enemies(OID3.LightElemental).All(e => e.IsDeadOrDestroyed)
                && module.Enemies(OID3.GreaterPugil).All(e => e.IsDeadOrDestroyed)
                && module.Enemies(OID3.Banshee).All(e => e.IsDeadOrDestroyed)
                && module.Enemies(OID3.Alkyoneus).All(e => e.IsDeadOrDestroyed)
                && module.Enemies(OID3.GiantAscetic).All(e => e.IsDeadOrDestroyed)
                && module.Enemies(OID3.GiantRanger).All(e => e.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team (Malediktus), adapted by Codex", PrimaryActorOID = (uint)OID3.Kraken, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1058u, NameID = 14071u, Category = BossModuleInfo.Category.Alliance, Expansion = BossModuleInfo.Expansion.Dawntrail, SortOrder = 5)]
public sealed class A20Kraken(WorldState ws, Actor primary) : BossModule(ws, primary, new(-847f, -852f), arena)
{
    private static readonly ArenaBoundsSquare arena = new(25f);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies(OID3.Kraken), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID3.Clipper), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID3.Acrophies), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID3.LightElemental), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID3.GreaterPugil), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID3.Banshee), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID3.Alkyoneus), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID3.GiantAscetic), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID3.GiantRanger), ArenaColor.Enemy);
    }
}
