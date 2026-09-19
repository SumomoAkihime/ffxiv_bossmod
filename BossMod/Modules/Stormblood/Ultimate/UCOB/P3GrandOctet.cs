namespace BossMod.Stormblood.Ultimate.UCOB;

sealed class P3GrandOctet(BossModule module) : Components.GenericAOEs(module)
{
    public List<Actor> Casters = [];
    private Actor? _nael;
    public Actor? Twintania { get; private set; }
    private Actor? _baha;
    public List<AOEInstance> AOEs = [];
    public int DiveOrder; // 0 if not yet known, +1 if CCW, -1 if CW
    private WPos _initialSafespot;
    public readonly int[] BaitOrder = new int[PartyState.MaxPartySize];
    public int NumBaitsAssigned = 1; // reserve for lunar dive

    private readonly AOEShapeRect _shapeNaelTwin = new(63.96f, 4f);
    private readonly AOEShapeRect _shapeBahamut = new(64.2f, 6f);
    private readonly AOEShapeRect _shapeDrake = new(52f, 10f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(AOEs);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (BaitOrder[slot] >= NextBaitOrder)
        {
            hints.Add($"Bait {BaitOrder[slot]}", false);
        }
        base.AddHints(slot, actor, hints);
    }

    public override void AddGlobalHints(Actor actor, GlobalHints hints)
    {
        if (DiveOrder != 0)
        {
            hints.Add($"Move {(DiveOrder < 0 ? "CW" : "CCW")}");
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (NumCasts == 0 && AOEs.Count == 0)
        {
            hints.AddForbiddenZone(new SDInvertedCircle(Arena.Center, 1f));
            return;
        }

        if (DiveOrder == 0 || (uint)slot >= PartyState.MaxPartySize)
        {
            return;
        }

        WPos? baitSource = null;
        var order = BaitOrder[slot];
        if (order >= NextBaitOrder && order <= Casters.Count)
        {
            baitSource = Casters[order - 1].Position;
        }
        else if (order == 0 && NumBaitsAssigned == 6)
        {
            baitSource = _baha?.Position;
        }
        else if (order > 0 && order < NextBaitOrder)
        {
            hints.GoalZones.Add(AIHints.GoalSingleTarget(Arena.Center, 5f));
            return;
        }

        if (baitSource is { } source)
        {
            var direction = (source - Arena.Center).ToAngle() + (DiveOrder * 34f).Degrees();
            var edgeGoal = AIHints.GoalProximity(Arena.Center + direction.ToDirection() * 21f, 20f, 5f);
            hints.GoalZones.Add(p => p.InCircle(Arena.Center, 19f) ? 0f : edgeGoal(p));
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        // draw safespot
        if (NumCasts == 0 && AOEs.Count <= 1 && _initialSafespot != default)
        {
            Arena.ZoneCircleOutline(_initialSafespot, 1f, Colors.Safe);
        }

        // draw bait
        var order = BaitOrder[pcSlot];
        if (order >= NextBaitOrder && order <= Casters.Count)
        {
            var source = Casters[order - 1];
            Arena.Actor(source, Colors.Object, true, true);
            BaitShape(order).Outline(Arena, source.Position, Angle.FromDirection(pc.Position - source.Position));
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID is (uint)OID.Firehorn or (uint)OID.Iceclaw or (uint)OID.Thunderwing or (uint)OID.TailOfDarkness or (uint)OID.FangOfLight)
        {
            Casters.Add(actor);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var shape = CastShape(spell.Action);
        if (shape != null)
        {
            AOEs.Add(new(shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        var shape = CastShape(spell.Action);
        if (shape != null)
        {
            var count = AOEs.Count;
            var id = caster.InstanceID;
            ++NumCasts;
            var aoes = CollectionsMarshal.AsSpan(AOEs);
            for (var i = 0; i < count; ++i)
            {
                ref var aoe = ref aoes[i];
                if (aoe.ActorID == id)
                {
                    AOEs.RemoveAt(i);
                    return;
                }
            }
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        var slot = Raid.FindSlot(actor.InstanceID);
        if (slot < 0)
        {
            return;
        }

        switch (iconID)
        {
            case (uint)IconID.LunarDive: // this happens at the same time (so arbitrary order) as first cauterize
                BaitOrder[slot] = 1;
                break;
            case (uint)IconID.Cauterize:
                BaitOrder[slot] = ++NumBaitsAssigned;
                break;
            case (uint)IconID.MegaflareDive:
                BaitOrder[slot] = ++NumBaitsAssigned;
                if (NumBaitsAssigned == 7)
                {
                    var len = BaitOrder.Length;
                    for (var i = 0; i < len; ++i)
                    {
                        if (BaitOrder[i] == 0)
                        {
                            BaitOrder[i] = 8; // twintania bait
                        }
                    }
                }
                break;
        }
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        var oid = actor.OID;
        if (oid == (uint)OID.NaelDeusDarnus && id == 0x1E43)
        {
            _nael = actor;
            InitIfReady();
        }
        else if (oid == (uint)OID.Twintania && id == 0x1E44)
        {
            Twintania = actor;
            InitIfReady();
        }
        else if (oid == (uint)OID.BahamutPrime && id == 0x1E43)
        {
            _baha = actor;
            InitIfReady();
        }
    }

    private void InitIfReady()
    {
        if (_nael == null || Twintania == null || _baha == null)
            return;

        // at this point NextCasters should contain 5 drakes, order is not yet known
        var center = Arena.Center;
        var dirToNael = Angle.FromDirection(_nael.Position - center);
        var dirToBaha = Angle.FromDirection(_baha.Position - center);

        // bahamut on cardinal => CCW dive order
        // bahamut on intercardinal => CW dive order
        var bahamutIntercardinal = ((int)MathF.Round(dirToBaha.Deg / 45f) & 1) != 0;
        DiveOrder = bahamutIntercardinal ? -1 : +1;

        var count = Casters.Count;
        var orders = new float[count];
        for (var i = 0; i < count; ++i)
        {
            var c = Casters[i];
            orders[i] = DiveOrder * CCWDirection(Angle.FromDirection(c.Position - center), dirToBaha);
        }

        MemoryExtensions.Sort(orders, Casters.AsSpan());
        Casters.Insert(0, _nael);
        Casters.Add(_baha);
        Casters.Add(Twintania);

        // safespot is opposite of bahamut; if nael is there - adjusted 45 degrees
        var dirToSafespot = dirToBaha + 180f.Degrees();
        if (dirToSafespot.AlmostEqual(dirToNael, 0.1f))
        {
            dirToSafespot += DiveOrder * 45f.Degrees();
        }
        _initialSafespot = center + 20f * dirToSafespot.ToDirection();
    }

    private float CCWDirection(Angle direction, Angle reference)
    {
        var ccwDist = (direction - reference).Normalized().Deg;
        if (ccwDist < -5f)
        {
            ccwDist += 360f;
        }
        return ccwDist;
    }

    private int NextBaitOrder => AOEs.Count + NumCasts + 1;
    private AOEShapeRect BaitShape(int order) => order switch
    {
        1 or 8 => _shapeNaelTwin,
        7 => _shapeBahamut,
        _ => _shapeDrake
    };

    private AOEShapeRect? CastShape(ActionID aid) => aid.ID switch
    {
        (uint)AID.Cauterize1 => _shapeDrake,
        (uint)AID.Cauterize2 => _shapeDrake,
        (uint)AID.Cauterize3 => _shapeDrake,
        (uint)AID.Cauterize4 => _shapeDrake,
        (uint)AID.Cauterize5 => _shapeDrake,
        (uint)AID.LunarDive => _shapeNaelTwin,
        (uint)AID.TwistingDive => _shapeNaelTwin,
        (uint)AID.MegaflareDive => _shapeBahamut,
        _ => null
    };
}

sealed class P3GrandOctetTower : Components.CastTowers
{
    private readonly P3GrandOctet _octet;
    private readonly int[] _quickmarchOrder = Utils.MakeArray(PartyState.MaxPartySize, -1);
    private BitMask _stackTargets;
    private bool _assigned;

    public P3GrandOctetTower(BossModule module) : base(module, (uint)AID.MegaflareTower, 3f)
    {
        EnableHints = false;
        _octet = module.FindComponent<P3GrandOctet>()!;
        foreach (var (slot, group) in Service.Config.Get<UCOBConfig>().P3QuickmarchTrioAssignments.Resolve(Raid))
        {
            if ((uint)slot < PartyState.MaxPartySize)
            {
                _quickmarchOrder[slot] = group;
            }
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID != (uint)IconID.MegaflareStack)
        {
            return;
        }

        var slot = Raid.FindSlot(actor.InstanceID);
        if (slot < 0)
        {
            return;
        }

        _stackTargets.Set(slot);
        var towers = CollectionsMarshal.AsSpan(Towers);
        for (var i = 0; i < towers.Length; ++i)
        {
            towers[i].ForbiddenSoakers.Set(slot);
        }
        AssignTowers();
    }

    private void AssignTowers()
    {
        if (_assigned || _stackTargets.NumSetBits() != 4 || _octet.Twintania == null || Towers.Count != 4)
        {
            return;
        }

        var twintaniaPosition = _octet.Twintania.Position;
        Towers.Sort((a, b) => (a.Position - twintaniaPosition).LengthSq().CompareTo((b.Position - twintaniaPosition).LengthSq()));

        var allowed = Raid.WithSlot(true, true, true)
            .ExcludedFromMask(_stackTargets)
            .OrderBy(p => _quickmarchOrder[p.Item1] >= 0 ? (ulong)_quickmarchOrder[p.Item1] : p.Item2.InstanceID)
            .ToList();
        if (allowed.Count < Towers.Count)
        {
            return;
        }

        var twinBait = allowed.FindIndex(p => _octet.BaitOrder[p.Item1] == 8);
        if (twinBait > 0)
        {
            var baiter = allowed[twinBait];
            allowed.RemoveAt(twinBait);
            allowed.Insert(0, baiter);
        }

        for (var i = 0; i < Towers.Count; ++i)
        {
            Towers.Ref(i).ForbiddenSoakers = ~BitMask.Build(allowed[i].Item1);
        }
        _assigned = true;
    }
}
