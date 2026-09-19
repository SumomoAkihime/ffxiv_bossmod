namespace BossMod.Stormblood.Ultimate.UCOB;

sealed class P4AddsPositioning(BossModule module) : BossComponent(module)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var spawned = false;

        if (hints.FindEnemy(((UCOB)Module).Twintania()) is { } twintania)
        {
            spawned = true;
            twintania.DesiredPosition = new(11.5f, -5.5f);
            twintania.DesiredRotation = 180f.Degrees();
        }

        if (hints.FindEnemy(((UCOB)Module).Nael()) is { } nael)
        {
            spawned = true;
            nael.DesiredPosition = new(13f, -1f);
            nael.DesiredRotation = 90f.Degrees();
        }

        if (!spawned)
        {
            switch (actor.Class.GetRole())
            {
                case Role.Melee:
                    hints.GoalZones.Add(AIHints.GoalSingleTarget(new WPos(0f, -11f), 4f));
                    break;
                case Role.Healer:
                case Role.Ranged:
                    hints.GoalZones.Add(AIHints.GoalSingleTarget(Arena.Center, 4f));
                    break;
            }
        }
    }
}
