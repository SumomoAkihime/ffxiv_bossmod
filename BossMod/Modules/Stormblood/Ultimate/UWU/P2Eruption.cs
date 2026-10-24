namespace BossMod.Stormblood.Ultimate.UWU;

// TODO: can subsequent eruptions bait on other targets?
// casts are 3s long and 2s apart (overlapping)
class P2Eruption(BossModule module) : Components.SimpleAOEs(module, (uint)AID.EruptionAOE, 8)
{
    public int NumCastsStarted;
    private BitMask _baiters;

    public override void Update()
    {
        if (NumCastsStarted == 0)
        {
            var source = ((UWU)Module).Ifrit();
            if (source != null)
                _baiters = Raid.WithSlot(false, true, true).WhereActor(a => a.Class.IsDD()).SortedByRange(source.Position).TakeLast(2).Mask();
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_baiters[pcSlot])
            Arena.ZoneCircleOutline(pc.Position, 8, Colors.Danger);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        if (spell.Action.ID == (uint)AID.EruptionAOE)
        {
            if (NumCastsStarted < 2)
            {
                if (NumCastsStarted == 0)
                    _baiters.Reset();
                var (baiterSlot, baiter) = Raid.WithSlot(false, true, true).ExcludedFromMask(_baiters).Closest(spell.LocXZ);
                if (baiter != null)
                    _baiters.Set(baiterSlot);
            }
            ++NumCastsStarted;
            if (NumCastsStarted >= 8)
                _baiters.Reset();
        }
    }
}

class P2Eruption1 : P2Eruption
{
    private readonly List<Actor> BaitersPriorityOrder = [];
    private bool _fetters;
    private bool _howl;

    private bool Active => _fetters && _howl;

    public P2Eruption1(BossModule module) : base(module)
    {
        BaitersPriorityOrder.AddRange(Raid.WithoutSlot(false, true, true).OrderBy(s => (s.Role switch
        {
            Role.Ranged => 0,
            Role.Healer => 1,
            Role.Melee => 2,
            _ => 3,
        }, s.InstanceID)));
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        base.OnStatusGain(actor, ref status);
        if ((SID)status.ID == SID.InfernalFetters)
        {
            _fetters = true;
            BaitersPriorityOrder.Remove(actor);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        if ((AID)spell.Action.ID == AID.InfernoHowl)
        {
            _howl = true;
            BaitersPriorityOrder.RemoveAll(b => b.InstanceID == spell.TargetID);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (!Active || Module.FindComponent<P2Nails>() is not { } nails)
            return;

        var myOrder = BaitersPriorityOrder.IndexOf(actor);
        if (myOrder is < 0 or > 1)
            return;

        var sign = myOrder == 0 ? -1 : 1;
        var destination = NumCastsStarted switch
        {
            < 2 => Arena.Center + (nails.RelativeNorth + (120 * sign).Degrees()).ToDirection() * 18,
            < 4 => Arena.Center + (nails.RelativeNorth + (90 * sign).Degrees()).ToDirection() * 16.25f,
            < 6 => Arena.Center + (nails.RelativeNorth + (56 * sign).Degrees()).ToDirection() * 14.25f,
            < 8 => Arena.Center + (nails.RelativeNorth + (22.5f * sign).Degrees()).ToDirection() * 18.5f,
            _ => default
        };

        if (destination != default)
            hints.AddForbiddenZone(new SDPrecisePosition(destination, new(0, 1), Arena.Bounds.MapResolution, actor.Position, 0.1f));
    }
}
