namespace BossMod.Stormblood.Ultimate.UWU;

class Mesohigh(BossModule module) : Components.CastCounter(module, (uint)AID.Mesohigh)
{
    protected readonly List<Actor> _sisters = [.. module.Enemies((uint)OID.GarudaSister)];
    private const float _radius = 3;
    protected DateTime Activation;

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.Mesohigh && Activation == default)
            Activation = WorldState.FutureTime(5.1f);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var s in EnumerateTetherSources())
        {
            var tetherTarget = WorldState.Actors.Find(s.Tether.Target);
            if (tetherTarget != null)
            {
                Arena.AddLine(s.Position, tetherTarget.Position, Colors.Danger);
                Arena.ZoneCircleOutline(tetherTarget.Position, _radius, Colors.Danger);
            }
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        return _sisters.Any(s => s.Tether.Target == player.InstanceID) ? PlayerPriority.Danger : PlayerPriority.Normal;
    }

    protected IEnumerable<Actor> EnumerateTetherSources()
    {
        foreach (var s in _sisters.Tethered(TetherID.Mesohigh))
            yield return s;
        if (Module.PrimaryActor.Tether.ID == (uint)TetherID.Mesohigh)
            yield return Module.PrimaryActor;
    }
}

class P1Mesohigh(BossModule module) : Mesohigh(module)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_sisters.Count != 2)
            return;

        Actor? desiredSource = null;
        if (actor.Role == Role.Tank && actor.FindStatus((uint)SID.ThermalLow)?.Extra == 2)
            desiredSource = _sisters.MinBy(s => s.Position.X);
        else if (actor == Raid.WithoutSlot(false, true, true).Where(a => a.Class.IsDD() && a.FindStatus((uint)SID.ThermalLow)?.Extra == 1).MinBy(a => a.InstanceID))
            desiredSource = _sisters.MaxBy(s => s.Position.X);

        if (desiredSource != null)
        {
            var curTarget = WorldState.Actors.Find(desiredSource.Tether.Target);
            if (curTarget == actor)
            {
                var destination = new WPos(desiredSource.Position.X > 100 ? 106 : 94, 100);
                hints.AddForbiddenZone(new SDInvertedCircle(destination, 1), Activation);
            }
            else if (curTarget != null)
            {
                var intercept = WPos.Lerp(desiredSource.Position, curTarget.Position, 0.5f);
                hints.AddForbiddenZone(new SDPrecisePosition(intercept, new(0, 1), Arena.Bounds.MapResolution, actor.Position, 0.1f));
            }
        }
        else if (Module.PrimaryActor.TargetID != actor.InstanceID)
        {
            hints.AddForbiddenZone(new SDInvertedCircle(new(100, 103), 1), Activation);
        }
    }
}

class P4Mesohigh(BossModule module) : Mesohigh(module);
