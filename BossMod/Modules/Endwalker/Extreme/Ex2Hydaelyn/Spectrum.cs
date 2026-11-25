namespace BossMod.Endwalker.Extreme.Ex2Hydaelyn;

class Spectrum(BossModule module) : Components.CastCounter(module, (uint)AID.BrightSpectrum)
{
    private const float _radius = 5;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        int tanksInRange = 0, nonTanksInRange = 0;
        var party = Raid.WithoutSlot(false, true, true);
        for (var i = 0; i < party.Length; ++i)
        {
            var other = party[i];
            if (other == actor || !other.Position.InCircle(actor.Position, _radius))
                continue;
            if (other.Role == Role.Tank)
                ++tanksInRange;
            else
                ++nonTanksInRange;
        }

        if (nonTanksInRange != 0 || actor.Role != Role.Tank && tanksInRange != 0)
            hints.Add("Spread!");

        if (actor.Role == Role.Tank && tanksInRange == 0)
            hints.Add("Stack with co-tank");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.ZoneCircleOutline(pc.Position, _radius, Colors.Danger);
        foreach (var player in Raid.WithoutSlot(false, true, true).Exclude(pc))
            Arena.Actor(player, player.Position.InCircle(pc.Position, _radius) ? Colors.PlayerInteresting : Colors.PlayerGeneric);
    }
}
