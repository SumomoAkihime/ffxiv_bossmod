namespace BossMod.Heavensward.Quest.MSQ.OneLifeForOneWorld;

public enum OID : uint
{
    Boss = 0x17CD,
    KnightOfDarkness = 0x17CE,
    BladeOfLight = 0x1EA19E,
}

public enum AID : uint
{
    Overpower = 6683,
    UnlitCyclone = 6684,
    UnlitCycloneAdds = 6685,
    Skydrive = 6686,
    UtterDestruction = 6690,
    RollingBladeCircle = 6691,
    RollingBladeCone = 6692
}

class Overpower(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Overpower, new AOEShapeCone(7f, 45f.Degrees()));
class UnlitCyclone(BossModule module) : Components.SimpleAOEs(module, (uint)AID.UnlitCyclone, 6f);
class UnlitCycloneAdds(BossModule module) : Components.SimpleAOEs(module, (uint)AID.UnlitCycloneAdds, 9f);
class Skydrive(BossModule module) : Components.SingleTargetCast(module, AID.Skydrive);
class SkydrivePuddle(BossModule module) : Components.Voidzone(module, 5f, m => m.Enemies(0x1EA19Cu).Where(x => x.EventState != 7));
class RollingBlade(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RollingBladeCircle, 7f);
class RollingBladeCone(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RollingBladeCone, new AOEShapeCone(60f, 15f.Degrees()));
class UtterDestruction(BossModule module) : Components.SimpleAOEs(module, (uint)AID.UtterDestruction, new AOEShapeDonut(10f, 20f));
class Adds(BossModule module) : Components.AddsMulti(module, [0x17CEu, 0x17CFu, 0x17D0u, 0x17D1u]);

class BladeOfLight(BossModule module) : BossComponent(module)
{
    public Actor? Blade => WorldState.Actors.FirstOrDefault(x => x.OID == (uint)OID.BladeOfLight && x.IsTargetable);

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Blade != null)
            Arena.Actor(Blade, ArenaColor.Vulnerable);
    }
}

class WarriorOfDarknessStates : StateMachineBuilder
{
    public WarriorOfDarknessStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Overpower>()
            .ActivateOnEnter<Adds>()
            .ActivateOnEnter<UnlitCyclone>()
            .ActivateOnEnter<UnlitCycloneAdds>()
            .ActivateOnEnter<Skydrive>()
            .ActivateOnEnter<SkydrivePuddle>()
            .ActivateOnEnter<BladeOfLight>()
            .ActivateOnEnter<RollingBlade>()
            .ActivateOnEnter<RollingBladeCone>()
            .ActivateOnEnter<UtterDestruction>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, StatesType = typeof(WarriorOfDarknessStates), ObjectIDType = typeof(OID), ActionIDType = typeof(AID), GroupType = BossModuleInfo.GroupType.Quest, GroupID = 67885, NameID = 5240)]
public class WarriorOfDarkness(WorldState ws, Actor primary) : BossModule(ws, primary, default, new ArenaBoundsCircle(20f));
