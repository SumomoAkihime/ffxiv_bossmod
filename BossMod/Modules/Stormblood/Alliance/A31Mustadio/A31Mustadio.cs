namespace BossMod.Stormblood.Alliance.A31Mustadio;

class EnergyBurst(BossModule module) : Components.RaidwideCast(module, (uint)AID.EnergyBurst);
class ArmShot(BossModule module) : Components.SingleTargetCast(module, (uint)AID.ArmShot);
class LegShot(BossModule module) : Components.Voidzone(module, 6f, GetVoidzones)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.LegShotVoidzone);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.EventState != 7)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }
}

abstract class Handgonne(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeCone(30f, 105f.Degrees()));
class LeftHandgonne(BossModule module) : Handgonne(module, (uint)AID.LeftHandgonne);
class RightHandgonne(BossModule module) : Handgonne(module, (uint)AID.RightHandgonne);

class SatelliteBeam(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SatelliteBeam, new AOEShapeRect(30, 15)); // Satellite Beam and Compress can both be shown earleir through tether
class Compress(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Compress, new AOEShapeRect(100, 7.5f));

class BallisticSpread(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.BallisticImpact1, 6);

class Searchlight(BossModule module) : Components.GenericAOEs(module, (uint)AID.Searchlight2)
{
    private record class Target(Actor Actor, DateTime FreezeAt);

    // Icon appears about 3.2s before the 4s tracking period starts; explosion follows 4s after tracking stops.
    private const double _freezeDelay = 7.2d;
    private const double _activationDelay = 11.2d;
    private static readonly AOEShapeCircle _shape = new(5f);
    private readonly List<Target> _targets = [];
    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void Update()
    {
        var now = WorldState.CurrentTime;
        var count = _aoes.Count;
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        for (var i = 0; i < count; ++i)
        {
            if (now < _targets[i].FreezeAt)
                aoes[i].Origin = _targets[i].Actor.Position;
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Icon164 && WorldState.Actors.Find(targetID) is var target && target != null)
        {
            _targets.Add(new(target, WorldState.FutureTime(_freezeDelay)));
            _aoes.Add(new(_shape, target.Position, activation: WorldState.FutureTime(_activationDelay), actorID: target.InstanceID));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action.ID != WatchedAction || _aoes.Count == 0)
            return;

        var closest = 0;
        var closestDistance = float.MaxValue;
        var count = _aoes.Count;
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        for (var i = 0; i < count; ++i)
        {
            var distance = (aoes[i].Origin - caster.Position).LengthSq();
            if (distance < closestDistance)
            {
                closest = i;
                closestDistance = distance;
            }
        }
        _aoes.RemoveAt(closest);
        _targets.RemoveAt(closest);
    }
}

class LastTestament(BossModule module) : Components.CastWeakpoint(module, (uint)AID.LastTestament, new AOEShapeRect(100f, 30f), default, (uint)SID.BackUnseen, (uint)SID.LeftUnseen, (uint)SID.RightUnseen);

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 636, NameID = 7915)] // 7919 
public class A31Mustadio(WorldState ws, Actor primary) : BossModule(ws, primary, new(600, 290), new ArenaBoundsSquare(30, 45.Degrees()));
