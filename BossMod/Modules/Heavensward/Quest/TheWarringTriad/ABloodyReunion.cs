namespace BossMod.Heavensward.Quest.WarringTriad.ABloodyReunion;

public enum OID : uint
{
    Boss = 0x161E,
    MagitekTurretI = 0x161F,
    MagitekTurretII = 0x1620,
    TerminusEst = 0x1621,
    Helper = 0x233C,
}

public enum AID : uint
{
    MagitekSlug = 6026,
    AetherochemicalGrenado = 6031,
    SelfDetonate = 6032,
    MagitekSpread = 6027,
}

class MagitekSlug(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MagitekSlug, new AOEShapeRect(60f, 2f));
class AetherochemicalGrenado(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AetherochemicalGrenado, 8f);
class SelfDetonate(BossModule module) : Components.CastHint(module, AID.SelfDetonate, "Kill turret before detonation!", true);
class MagitekSpread(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MagitekSpread, new AOEShapeCone(20.55f, 120f.Degrees()));

class RegulaVanHydrusStates : StateMachineBuilder
{
    public RegulaVanHydrusStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MagitekSlug>()
            .ActivateOnEnter<AetherochemicalGrenado>()
            .ActivateOnEnter<SelfDetonate>()
            .ActivateOnEnter<MagitekSpread>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, StatesType = typeof(RegulaVanHydrusStates), ObjectIDType = typeof(OID), ActionIDType = typeof(AID), GroupType = BossModuleInfo.GroupType.CFC, GroupID = 173, NameID = 3818)]
public class RegulaVanHydrus(WorldState ws, Actor primary) : BossModule(ws, primary, new(230f, 79f), new ArenaBoundsCircle(20.256f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        base.DrawEnemies(pcSlot, pc);
        Arena.Actors(Enemies((uint)OID.MagitekTurretI), ArenaColor.Enemy);
        Arena.Actors(Enemies((uint)OID.MagitekTurretII), ArenaColor.Enemy);
    }
}
