namespace BossMod.Dawntrail.Alliance.A10Despot;

public enum OID : uint
{
    Boss = 0x4692,
    Flamingo1 = 0x4713,
    Flamingo2 = 0x4693
}

public enum AID : uint
{
    AutoAttack1 = 870,
    AutoAttack2 = 872,
    WingCutter = 41672,
    ScraplineStorm = 40650,
    Scrapline = 41393,
    Typhoon = 41902,
    IsleDrop = 41699,
    Peck = 41695,
    Panzerfaust = 41698,
    PanzerfaustRepeats = 41353
}

sealed class IsleDrop(BossModule module) : Components.SimpleAOEs(module, (uint)AID.IsleDrop, new AOEShapeCircle(6f));
sealed class WingCutter(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WingCutter, new AOEShapeCone(6f, 60f.Degrees()));
sealed class PanzerfaustHint(BossModule module) : Components.CastInterruptHint(module, AID.Panzerfaust, showNameInHint: true);
sealed class Panzerfaust(BossModule module) : Components.SingleTargetCast(module, AID.Panzerfaust);

sealed class ScraplineStorm(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.ScraplineStorm, 10f, kind: Kind.TowardsOrigin)
{
    private readonly ScraplineTyphoon _aoe = module.FindComponent<ScraplineTyphoon>()!;

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        if (_aoe.AOEs.Count != 0)
            return _aoe.AOEs[0].Check(pos);
        return false;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    { }
}

sealed class ScraplineTyphoon(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = new(2);
    private static readonly AOEShapeCircle circle = new(10f);
    private static readonly AOEShapeDonut donut = new(8f, 40f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (AOEs.Count != 0)
            yield return AOEs[0];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ScraplineStorm)
        {
            AddAOE(circle, 2.1f);
            AddAOE(donut, 5.6f);

            void AddAOE(AOEShape shape, float delay) => AOEs.Add(new(shape, spell.LocXZ, default, Module.CastFinishAt(spell, delay)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (AOEs.Count != 0 && spell.Action.ID is (uint)AID.Scrapline or (uint)AID.Typhoon)
            AOEs.RemoveAt(0);
    }
}

public sealed class A10DespotStates : StateMachineBuilder
{
    public A10DespotStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<IsleDrop>()
            .ActivateOnEnter<WingCutter>()
            .ActivateOnEnter<ScraplineTyphoon>()
            .ActivateOnEnter<ScraplineStorm>()
            .ActivateOnEnter<Panzerfaust>()
            .ActivateOnEnter<PanzerfaustHint>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1015, NameID = 13608, SortOrder = 6)]
public sealed class A10Despot(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, DefaultBounds)
{
    public static readonly WPos ArenaCenter = new(-545, -585);
    public static readonly ArenaBoundsCircle DefaultBounds = new(40);
}
