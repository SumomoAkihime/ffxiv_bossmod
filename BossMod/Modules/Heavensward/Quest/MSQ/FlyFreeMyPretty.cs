namespace BossMod.Heavensward.Quest.MSQ.FlyFreeMyPretty;

public enum OID : uint
{
    Boss = 0x195E,
    GrynewahtP2 = 0x195F,
    ImperialColossus = 0x1966,
    ImperialHoplomachus = 0x1960,
    ImperialSecutor = 0x1963,
    ImperialEques = 0x1962,
    ImperialSagittarius = 0x1965,
    ImperialSignifer = 0x1964,
    ImperialLaquearius = 0x1961,
    FireVoidzone = 0x1E86DF
}

public enum AID : uint
{
    AugmentedUprising = 7608,
    AugmentedSuffering = 7607,
    Heartstopper = 866,
    Overpower = 720,
    GrandSword = 7615,
    MagitekRay = 7617,
    GrandStrike = 7616,
    ShrapnelShell = 7614,
    MagitekMissiles = 7612,
}

class MagitekMissiles(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MagitekMissiles, 15f);
class ShrapnelShell(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ShrapnelShell, 6f);
class Firebomb(BossModule module) : Components.Voidzone(module, 4f, m => m.Enemies((uint)OID.FireVoidzone).Where(e => e.EventState != 7));
class AugmentedUprising(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AugmentedUprising, new AOEShapeCone(8.5f, 60f.Degrees()));
class AugmentedSuffering(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AugmentedSuffering, 6.5f);
class Heartstopper(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Heartstopper, new AOEShapeRect(3.5f, 1.5f));
class Overpower(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Overpower, new AOEShapeCone(6f, 45f.Degrees()));
class GrandSword(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GrandSword, new AOEShapeCone(21f, 60f.Degrees()));
class MagitekRay(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MagitekRay, 6f);
class GrandStrike(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GrandStrike, new AOEShapeRect(48f, 2f));

class GrynewahtStates : StateMachineBuilder
{
    public GrynewahtStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AugmentedUprising>()
            .ActivateOnEnter<AugmentedSuffering>()
            .ActivateOnEnter<Overpower>()
            .ActivateOnEnter<Heartstopper>()
            .ActivateOnEnter<GrandSword>()
            .ActivateOnEnter<MagitekRay>()
            .ActivateOnEnter<GrandStrike>()
            .ActivateOnEnter<ShrapnelShell>()
            .ActivateOnEnter<Firebomb>()
            .ActivateOnEnter<MagitekMissiles>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, StatesType = typeof(GrynewahtStates), ObjectIDType = typeof(OID), ActionIDType = typeof(AID), GroupType = BossModuleInfo.GroupType.Quest, GroupID = 67894, NameID = 5576)]
public class Grynewaht(WorldState ws, Actor primary) : BossModule(ws, primary, default, new ArenaBoundsCircle(20f))
{
    private static readonly uint[] Adds = [(uint)OID.ImperialHoplomachus, (uint)OID.ImperialLaquearius, (uint)OID.ImperialEques, (uint)OID.ImperialSecutor, (uint)OID.ImperialSignifer, (uint)OID.ImperialSagittarius, (uint)OID.ImperialColossus, (uint)OID.GrynewahtP2];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var oid in Adds)
            Arena.Actors(Enemies(oid), ArenaColor.Enemy);
    }
}
