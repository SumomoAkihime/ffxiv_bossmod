namespace BossMod.RealmReborn.Alliance.A13Thanatos;

public enum OID : uint
{
    Boss = 0x92E, // R3.000, x1
    Helper = 0x92F, // R0.500, x1
    Nemesis = 0x930, // R2.000, x0 (spawn during fight)
    Sandman = 0x983, // R1.800, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 1461, // Boss/Nemesis->player, no cast, single-target
    BlightedGloom = 759, // Boss->self, no cast, range 10+R circle
    BlackCloud = 758, // Boss->location, no cast, range 6 circle
    Cloudscourge = 760, // Helper->location, 3.0s cast, range 6 circle
    Knout = 763, // Boss->self, no cast, single-target
    CrepusculeBlade = 762, // Boss->self, 3.0s cast, range 8+R 120?-degree cone
    VoidFireII = 1829, // Nemesis->location, 3.0s cast, range 5 circle
}

public enum SID : uint
{
    AstralRealignment = 398, // none->player, extra=0x0
    Leaden = 67, // none->931, extra=0x50
}

class Cloudscourge(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Cloudscourge, 6);
class VoidFireII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VoidFireII, 5);
class CrepusculeBlade(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CrepusculeBlade, new AOEShapeCone(11, 60.Degrees()));
class CrepusculeInterrupt(BossModule module) : Components.CastInterruptHint(module, (uint)AID.CrepusculeBlade);

class Adds(BossModule module) : Components.AddsMulti(module, [(uint)OID.Nemesis, (uint)OID.Sandman]);

class AstralRealignment(BossModule module) : Components.GenericInvincible(module)
{
    private BitMask _playerStates;

    protected override ReadOnlySpan<Actor> ForbiddenTargets(int slot, Actor actor)
        => !_playerStates[slot] ? new Actor[] { Module.PrimaryActor } : Array.Empty<Actor>();

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        hints.SetPriority(Module.PrimaryActor, _playerStates[slot] ? 5 : AIHints.Enemy.PriorityInvincible);
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if ((SID)status.ID == SID.AstralRealignment && Raid.TryFindSlot(actor.InstanceID, out var slot))
            _playerStates.Set(slot);
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if ((SID)status.ID == SID.AstralRealignment && Raid.TryFindSlot(actor.InstanceID, out var slot))
            _playerStates.Clear(slot);
    }
}

class A13ThanatosStates : StateMachineBuilder
{
    public A13ThanatosStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Adds>()
            .ActivateOnEnter<AstralRealignment>()
            .ActivateOnEnter<CrepusculeBlade>()
            .ActivateOnEnter<CrepusculeInterrupt>()
            .ActivateOnEnter<Cloudscourge>()
            .ActivateOnEnter<VoidFireII>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 92, NameID = 710)]
public class A13Thanatos(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, ThanatosBounds)
{
    public static readonly WPos ArenaCenter = new(440.4f, 280);
    public static readonly ArenaBoundsCustom ThanatosBounds = MakeBounds();

    private static ArenaBoundsCustom MakeBounds() => new(
        [
            new Circle(ArenaCenter, 27.5f),
            new Rectangle(ArenaCenter + new WDir(34, 0), 10, 8),
            new Rectangle(ArenaCenter + new WDir(34, 0).Rotate(120.Degrees()), 10, 8, 120.Degrees()),
            new Rectangle(ArenaCenter + new WDir(34, 0).Rotate(-120.Degrees()), 10, 8, -120.Degrees())
        ],
        MapResolution: 1);
}
