namespace BossMod.QuestBattle.Endwalker.MSQ;

class ImperialAI(WorldState ws) : UnmanagedRotation(ws, 3f)
{
    protected override void Exec(Actor? primaryTarget)
    {
        if (Player.FindStatus(2736u) != null)
        {
            var someTarget = Hints.PotentialTargets.FirstOrDefault(x => x.Actor.Position.InRect(new WPos(68f, -222f), new WDir(52f, default), 16f) && (x.Actor.Position - Player.Position).LengthSq() <= 900f);
            if (someTarget != null)
            {
                UseAction(Roleplay.AID.DiffractiveMagitekCannonIFTC, someTarget.Actor, 0, someTarget.Actor.PosRot.XYZ());
                UseAction(Roleplay.AID.MagitekCannonIFTC, someTarget.Actor, 0, someTarget.Actor.PosRot.XYZ());
            }
            // mount rotation
            return;
        }

        if (Player.HPMP.CurHP < Player.HPMP.MaxHP * 0.75f && World.Client.DutyActions[0].CurCharges > 0)
        {
            UseAction(Roleplay.AID.MedicalKit, Player, -50f);
        }

        if (primaryTarget is not { IsAlly: false })
        {
            return;
        }

        if (Player.InCombat)
        {
            UseAction(Roleplay.AID.RampartIFTC, Player, -50f);
        }

        switch (ComboAction)
        {
            case Roleplay.AID.RiotBladeIFTC:
                UseAction(Roleplay.AID.FightOrFlightIFTC, Player, -50f);
                UseAction(Roleplay.AID.RageOfHaloneIFTC, primaryTarget);
                break;
            case Roleplay.AID.FastBladeIFTC:
                UseAction(Roleplay.AID.FightOrFlightIFTC, Player, -50f);
                UseAction(Roleplay.AID.RiotBladeIFTC, primaryTarget);
                break;
        }

        UseAction(Roleplay.AID.FastBladeIFTC, primaryTarget);
    }
}

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 793u)]
internal class InFromTheCold(WorldState ws) : QuestBattle(ws)
{
    private readonly ImperialAI _ai = new(ws);
    private readonly Dictionary<ulong, (WPos Position, DateTime Time, WDir Velocity)> _motion = [];
    private bool _sneak;

    public override void AddQuestAIHints(Actor player, AIHints hints)
    {
        foreach (var h in hints.PotentialTargets.Where(p => p.Actor.Position.InCircle(player.Position, 40f)))
        {
            if (!h.Actor.InCombat && !h.Actor.Position.AlmostEqual(new(111f, -317f), 10f))
            {
                var sight = (h.Actor.OID == 0x3515 ? 11f : 8.5f) + h.Actor.HitboxRadius;
                hints.AddForbiddenZone(new SDCone(h.Actor.Position, sight, h.Actor.Rotation, 45f.Degrees()), source: h.Actor.InstanceID);
                hints.AddForbiddenZone(new SDCircle(h.Actor.Position, h.Actor.HitboxRadius), source: h.Actor.InstanceID);

                var velocity = Velocity(h.Actor);
                if (velocity.LengthSq() > 0.25f)
                    hints.AddForbiddenZone(new SDCone(h.Actor.Position + velocity * 1.5f, sight, h.Actor.Rotation, 45f.Degrees()), source: h.Actor.InstanceID);
            }
        }

        if (_sneak)
            SneakAlongRoute(player, hints);
        _sneak = false;

        _ai.Execute(player, hints);
    }

    private WDir Velocity(Actor actor)
    {
        if (!_motion.TryGetValue(actor.InstanceID, out var motion))
        {
            _motion[actor.InstanceID] = (actor.Position, World.CurrentTime, default);
            return default;
        }
        var dt = (float)(World.CurrentTime - motion.Time).TotalSeconds;
        if (dt >= 0.5f)
        {
            motion = (actor.Position, World.CurrentTime, (actor.Position - motion.Position) / dt);
            _motion[actor.InstanceID] = motion;
        }
        return motion.Velocity;
    }

    private void SneakAlongRoute(Actor player, AIHints hints)
    {
        if (hints.ForcedMovement is not { } route || player.InCombat)
            return;
        if (hints.InteractWithTarget is { } target && player.DistanceToHitbox(target) < 20)
        {
            hints.ForcedMovement = null;
            return;
        }

        var dir = new WDir(route.X, route.Z);
        var len = dir.Length();
        if (len < 0.5f)
            return;
        var next = player.Position + dir;
        var step = dir / len;
        for (var d = 1f; d <= Math.Min(len, 8); d += 1)
        {
            var point = player.Position + step * d;
            if (hints.ForbiddenZones.Any(z => z.shapeDistance.Contains(point)))
            {
                hints.ForcedMovement = null;
                hints.GoalZones.Add(AIHints.GoalProximity(next, Math.Max(40, len + 10), 1));
                return;
            }
        }
    }

    private bool HasFuel => World.Party.Player()?.FindStatus(404u) != null;
    private bool FuelInNearWreckage => World.Actors.Any(a => a.OID == 0x1EB69E && a.IsTargetable);

    private void RefuelHints(Actor player, AIHints hints)
    {
        _sneak = true;
        InteractWithFirst(player, hints, HasFuel ? 0x1EB4F1u : 0x1EB56Fu);
    }

    private void InteractWithFirst(Actor player, AIHints hints, params uint[] oids)
    {
        if (player.InCombat)
            return;
        foreach (var oid in oids)
        {
            hints.InteractWithOID(World, oid);
            if (hints.InteractWithTarget != null)
                return;
        }
    }

    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .Named("Combat")
            .Hints((player, hints) => hints.ForcedMovement = new(default, default, 1f))
            .With(obj => {
                obj.Update += () => obj.CompleteIf(World.Party.Player()?.PosRot.Z > -320f);
            }),

        new QuestObjective(ws)
            .Named("Medkit")
            .Hints((player, hints) => {
                hints.PrioritizeTargetsByOID([0x3506, 0x3507]);
                InteractWithFirst(player, hints, 0x3507u, 0x3506u);
            })
            .With(obj => obj.OnDutyActionsChange += (op) => obj.CompleteIf(op.Slots[0].Action.ID == 27315u)),

        new QuestObjective(ws)
            .Named("F")
            .Hints((player, hints) => InteractWithFirst(player, hints, 0x3507u, 0x3506u))
            .With(obj => obj.OnEventObjectAnimation += (act, p1, p2) => obj.CompleteIf(act.OID == 0x1EA1A1u && p1 == 4 && p2 == 8)),

        new QuestObjective(ws)
            .Named("Reaper 1")
            .MoveHint(new WPos(134, -290), 0.1f)
            .MoveHint(new WPos(126, -264), 0.2f)
            .MoveHint(new WPos(123, -238), 0.3f)
            .WithInteract(0x1EB456u)
            .With(obj => obj.OnDirectorUpdate += (diru) => obj.CompleteIf(diru.UpdateID == 0x10000002u && diru.Param1 == 0x76DFu)),

        new QuestObjective(ws)
            .Named("Wounded Imperial")
            .Hints((player, hints) => {
                hints.GoalZones.Add(AIHints.GoalSingleTarget(new WPos(105, -259), 3));
                if (player.Position.AlmostEqual(new WPos(111.218f, -257.802f), 2)) { hints.WantJump = true; } })
            .With(obj => obj.OnDirectorUpdate += (diru) => obj.CompleteIf(diru.UpdateID == 0x10000002u && diru.Param1 == 0x76E0u)),

        new QuestObjective(ws)
            .Named("Key")
            .WithInteract(0xFE20Bu)
            .With(obj => obj.OnDirectorUpdate += (diru) => obj.CompleteIf(diru.UpdateID == 0x10000002u && diru.Param1 == 0x76E2u)),

        new QuestObjective(ws)
            .Named("Leave church")
            .WithConnections(new Waypoint(V3(108.5f, 10.8f, -258.2f), false), new Waypoint(V3(116, 10.8f, -259.3f), false))
            .Hints((player, hints) => {
                if (player.Position.AlmostEqual(new WPos(109, -257.263f), 2f)) { hints.WantJump = true; } })
            .With(obj => obj.OnNavigationComplete += () => obj.CompleteIf(true)),

        new QuestObjective(ws)
            .Named("Fuel (near wreckage)")
            .WithConnections(V3(124.5f, 10.8f, -232.9f), V3(175.5f, 10.8f, -227.7f))
            .Hints((player, hints) => {
                _sneak = true;
                InteractWithFirst(player, hints, 0x1EB69Eu);
            })
            .With(obj => obj.Update += () => obj.CompleteIf(HasFuel || !FuelInNearWreckage)),

        new QuestObjective(ws)
            .Named("Fuel (far wreckage)")
            .WithConnections(V3(124.5f, 10.8f, -232.9f), V3(175.5f, 10.8f, -227.7f), V3(188.4f, 10.8f, -256.8f), V3(225.1f, 10.8f, -286.9f), V3(268.8f, 10.8f, -325.2f), V3(295.5f, 10.8f, -366.3f))
            .Hints((player, hints) => {
                _sneak = true;
                InteractWithFirst(player, hints, 0x1EB452u);
            })
            .With(obj => obj.Update += () => obj.CompleteIf(HasFuel || FuelInNearWreckage)),

        new QuestObjective(ws)
            .Named("Back from far wreckage")
            .WithConnections(V3(268.8f, 10.8f, -325.2f), V3(225.1f, 10.8f, -286.9f), V3(188.4f, 10.8f, -256.8f))
            .Hints(RefuelHints)
            .With(obj => {
                obj.OnNavigationComplete += () => obj.CompleteIf(true);
                obj.Update += () => obj.CompleteIf(FuelInNearWreckage);
            }),

        new QuestObjective(ws)
            .Named("Refuel")
            .WithConnections(V3(175.5f, 10.8f, -227.7f), V3(130.3f, 10.7f, -230.5f))
            .Hints(RefuelHints)
            .With(obj => obj.OnDirectorUpdate += (diru) => obj.CompleteIf(diru.UpdateID == 0x10000002u && diru.Param1 == 0x76C1u)),

        new QuestObjective(ws)
            .Named("Mount")
            .WithInteract(0x1EB45B)
            .With(obj => obj.OnStatusGain += (act, st) => obj.CompleteIf(act.OID == default && st.ID == 2736u)),

        new QuestObjective(ws)
            .Named("Combat")
            .With(obj => obj.OnDirectorUpdate += (diru) => obj.CompleteIf(diru.UpdateID == 0x10000002u && diru.Param1 == 0x76C2u)),

        new QuestObjective(ws)
            .Named("Help the townspeople")
            .Hints((player, hints) => {
                hints.WantDismount = true;
                hints.GoalZones.Add(AIHints.GoalSingleTarget(new WPos(12f, -148f), 5f, 0.5f));
                hints.GoalZones.Add(AIHints.GoalSingleTarget(new WPos(-81f, -180f), 5f, 0.75f));
            })
            .With(obj => {
                obj.OnStatusGain += (act, st) => obj.CompleteIf(act.OID == default && st.ID == 2737u);
            }),

        new QuestObjective(ws)
            .Named("Leave")
            .Hints((player, hints) => hints.ForcedMovement = new(-0.33f, default, 0.67f))
    ];
}
