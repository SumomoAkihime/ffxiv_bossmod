namespace BossMod.Stormblood.Ultimate.UCOB;

sealed class P1Plummet(BossModule module) : Components.Cleave(module, (uint)AID.Plummet, new AOEShapeCone(12f, 60f.Degrees()), [(uint)OID.Twintania])
{
    public bool Soak;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var originsandtargets = CollectionsMarshal.AsSpan(OriginsAndTargets());
        var len = originsandtargets.Length;
        for (var i = 0; i < len; ++i)
        {
            ref var ot = ref originsandtargets[i];
            var origin = ot.origin;
            var originE = hints.FindEnemy(origin);
            originE?.CanMove = false;

            if (actor != ot.target)
            {
                var pos = origin.Position.Quantized();
                var shape = Soak && IsSoaker(assignment) ? Shape.InvertedDistance(pos, ot.angle) : Shape.Distance(pos, ot.angle);
                hints.AddForbiddenZone(shape, NextExpected);

                // non-tanks preposition away from where tank might face boss
                if (originE?.DesiredRotation is Angle rot)
                {
                    var predicted = Soak && IsSoaker(assignment) ? Shape.InvertedDistance(pos, rot) : Shape.Distance(pos, rot);
                    hints.AddForbiddenZone(predicted, DateTime.MaxValue);
                }
            }
        }
    }

    static bool IsSoaker(PartyRolesConfig.Assignment ass) => Service.Config.Get<UCOBConfig>().P1PlummetTargets[(int)ass];
}

sealed class P2BahamutsClaw(BossModule module) : Components.CastCounter(module, (uint)AID.BahamutsClaw);
sealed class P3FlareBreath(BossModule module) : Components.Cleave(module, (uint)AID.FlareBreath, new AOEShapeCone(29.2f, 45f.Degrees()), [(uint)OID.BahamutPrime]); // TODO: verify angle
sealed class P5MornAfah(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.MornAfah, 4f, 8, 8); // TODO: verify radius

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.Twintania, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 280u, NameID = 3210u, PlanLevel = 70)]
public sealed class UCOB(WorldState ws, Actor primary) : BossModule(ws, primary, default, new ArenaBoundsCircle(21f))
{
    private Actor? _nael;
    private Actor? _bahamutPrime;

    public Actor? Twintania() => PrimaryActor.IsDestroyed ? null : PrimaryActor;
    public Actor? Nael() => _nael;
    public Actor? BahamutPrime() => _bahamutPrime;

    public override bool ShouldPrioritizeAllEnemies => true;

    protected override void UpdateModule()
    {
        _nael ??= GetActor((uint)OID.NaelDeusDarnus);
        _bahamutPrime ??= GetActor((uint)OID.BahamutPrime);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // Apply encounter tank positioning after every mechanic has contributed its hints.
        foreach (var enemy in hints.PotentialTargets)
        {
            if (enemy.Actor.OID is not ((uint)OID.Twintania or (uint)OID.NaelDeusDarnus or (uint)OID.BahamutPrime)
                || enemy.Actor.TargetID != actor.InstanceID || !enemy.Actor.IsTargetable)
                continue;

            enemy.TankDistance = 0f;
            var position = enemy.Actor.Position;
            var rotation = enemy.DesiredRotation;
            hints.GoalZones.Add(p => p.InRect(position, rotation, 100f, 0f, 1f) ? 0.5f : 0f);
            if (enemy.CanMove)
                hints.GoalZones.Add(hints.PullTargetToLocation(enemy.Actor, enemy.DesiredPosition, actor, WorldState.Client.Cooldowns[ActionDefinitions.GCDGroup].Remaining, 0.5f));
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(Twintania());
        Arena.Actor(Nael());
        Arena.Actor(BahamutPrime());
    }
}
