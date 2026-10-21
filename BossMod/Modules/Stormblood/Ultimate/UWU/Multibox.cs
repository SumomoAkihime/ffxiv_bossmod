using BossMod.Autorotation;

namespace BossMod.Stormblood.Ultimate.UWU;

sealed class Multibox(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    public static RotationModuleDefinition Definition()
    {
        return new RotationModuleDefinition("Auto-UWU", "Auto-food, auto-wall, prepull position", "AI", "xan", RotationModuleQuality.WIP, new(~0ul), 70, 70, RelatedBossModule: typeof(UWU));
    }

    private readonly PartyRolesConfig partyRolesConfig = Service.Config.Get<PartyRolesConfig>();

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        if (Bossmods.ActiveModule is not UWU module)
            return;

        if (module.StateMachine.ActiveState != null
            && World.Party.WithoutSlot(true, true, true).Any(p => p.IsDead || p.FindStatus(235u, DateTime.MaxValue) != null)
            && module.StateMachine.ActivePhaseIndex < 4)
        {
            Hints.ForcedMovement = -Player.DirectionTo(new WPos(100f, 100f)).ToVec3();
        }

        var playerAssignment = partyRolesConfig[World.Party.Members[0].ContentId];
        _ = playerAssignment;

        if (!Player.InCombat && World.Client.CountdownRemaining > 0f)
        {
            if (!(Player.FindStatus(48u, DateTime.MaxValue)?.ExpireAt > World.FutureTime(1020d)))
                Hints.ActionsToExecute.Push(ActionDefinitions.IDPopcorn, Player, ActionQueue.Priority.High);

            if (primaryTarget != null)
            {
                var destination = primaryTarget.Position + primaryTarget.DirectionTo(Player) * (14f + primaryTarget.HitboxRadius + 0.5f);
                var shape = new SDPrecisePosition(destination, new(0f, 1f), 0.5f, Player.Position, 0.1f);
                Hints.GoalZones.Add(p => shape.Distance(p) > 0f ? 5f : 0f);
            }
        }
    }
}
