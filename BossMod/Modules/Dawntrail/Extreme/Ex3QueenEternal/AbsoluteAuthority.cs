namespace BossMod.Dawntrail.Extreme.Ex3QueenEternal;

sealed class AbsoluteAuthorityPuddles(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AbsoluteAuthorityPuddlesAOE, 8f);

sealed class AbsoluteAuthorityExpansionBoot(BossModule module) : Components.UniformStackSpread(module, 6f, 15f, 4)
{
    public int NumCasts;
    private readonly Ex3QueenEternalConfig _config = Service.Config.Get<Ex3QueenEternalConfig>();

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.AuthoritysExpansion:
                if (!_config.AbsoluteAuthorityIgnoreFlares)
                    AddSpread(actor, status.ExpireAt);
                break;
            case SID.AuthoritysBoot:
                AddStack(actor, status.ExpireAt);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.AbsoluteAuthorityExpansion or AID.AbsoluteAuthorityBoot)
        {
            ++NumCasts;
            Spreads.Clear();
            Stacks.Clear();
        }
    }
}

sealed class AbsoluteAuthorityHeel(BossModule module) : Components.GenericStackSpread(module)
{
    public int NumCasts;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID.AuthoritysHeel && Stacks.Count == 0)
            Stacks.Add(new(actor, 1.5f, 8, 8, activation: WorldState.FutureTime(5.1f)));
    }

    public override void Update()
    {
        if (Stacks.Count == 0)
            return;

        var player = Raid.Player()!;
        var party = Raid.WithoutSlot(false, true, true);
        var closestActor = default(Actor);
        var closestDistance = float.MaxValue;

        foreach (var p in party)
        {
            if (p == player)
                continue;

            var dist = (player.Position - p.Position).LengthSq();
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closestActor = p;
            }
        }

        Stacks.Ref(0).Target = closestActor ?? player;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.AbsoluteAuthorityHeel or AID.AbsoluteAuthorityHeelFail)
        {
            ++NumCasts;
            Stacks.Clear();
        }
    }
}

sealed class AbsoluteAuthorityKnockback(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.AbsoluteAuthorityKnockback, 30f, kind: Components.Knockback.Kind.DirForward);
