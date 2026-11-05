namespace BossMod.Stormblood.Ultimate.UCOB;

class P3GrandOctet(BossModule module) : Components.GenericAOEs(module)
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

    private readonly List<WPos> Towers = [];
    private BitMask Stackers;

    private static readonly AOEShapeRect _shapeNaelTwin = new(60, 4);
    private static readonly AOEShapeRect _shapeBahamut = new(60, 6);
    private static readonly AOEShapeRect _shapeDrake = new(52, 10);

    private readonly List<AOEInstance> _portedAOEView = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _portedAOEView.Clear();
        _portedAOEView.AddRange(AOEs);
        return CollectionsMarshal.AsSpan(_portedAOEView);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if ((uint)slot < PartyState.MaxPartySize && BaitOrder[slot] >= NextBaitOrder)
            hints.Add($"Bait {BaitOrder[slot]}", false);
        base.AddHints(slot, actor, hints);
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (DiveOrder != 0)
            hints.Add($"Move {(DiveOrder < 0 ? "CW" : "CCW")}");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        // before nael has picked a target, everyone stand mid
        if (NumCasts == 0 && AOEs.Count == 0)
        {
            hints.AddForbiddenZone(new SDInvertedCircle(Arena.Center, 1));
            return;
        }

        if (DiveOrder == 0 || (uint)slot >= PartyState.MaxPartySize)
            return;

        WPos? baitSource = null;

        var order = BaitOrder[slot];

        if (order >= NextBaitOrder && order <= Casters.Count)
            // bait impending
            baitSource = Casters[order - 1].Position;
        else if (order == 0 && NumBaitsAssigned == 6)
            // one unassigned player gets bahamut
            baitSource = _baha?.Position;
        else if (order > 0 && order < NextBaitOrder)
        {
            // bait done, go back to mid so we can easily move to stack spot
            hints.GoalZones.Add(AIHints.GoalSingleTarget(Arena.Center, 5));
            return;
        }

        if (baitSource == null)
            return;

        var ctr = Arena.Center;
        var baitAngle = (baitSource.Value - ctr).ToAngle();

        // twin bait can stand relatively close so that they have the most time to get to the tower or to the stack destination
        if (order == 8)
        {
            if (NumCasts < 7)
            {
                // baha dive might cover our spot
                hints.GoalZones.Add(AIHints.GoalProximity(baitSource.Value, 20, 5));
            }
            else
            {
                baitAngle += (DiveOrder * 15).Degrees();
                var closeBait = Arena.Center + baitAngle.ToDirection() * 21.5f;

                if (Stackers.Any() && !Stackers[slot])
                {
                    // if not stacking, the plan is to preposition as close to our desired tower as possible, ensuring that nobody else's tower gets hit by twin
                    var myTower = Towers.MinBy(t => (t - closeBait).LengthSq());
                    foreach (var t in Towers)
                    {
                        hints.AddForbiddenZone(new SDCone(baitSource.Value, 100, (t - baitSource.Value).ToAngle(), MathF.Atan2(7, (t - baitSource.Value).Length()).Radians()));
                    }
                    // additionally make sure we hide on the correct side of the arena, since the stackers are using the other side
                    hints.AddForbiddenZone(new SDHalfPlane(baitSource.Value, baitAngle.ToDirection().Rotate((90 * DiveOrder).Degrees())));
                    // goal zone for tower
                    hints.GoalZones.Add(AIHints.GoalProximity(myTower, 20, 5));
                }
                else
                {
                    // exact position isn't that important, but since we already use this one for tower assignments, just stand there
                    hints.PathfindMapBounds = UCOB.PathfindHugBorderBounds;
                    hints.AddForbiddenZone(new SDPrecisePosition(closeBait, new(0, 1), 0.5f, actor.Position, 0.1f));
                }
            }

            return;
        }

        baitAngle += (DiveOrder * 34).Degrees(); // approximation, i ain't doing no trigonometry
        var goalEdge = AIHints.GoalProximity(ctr + baitAngle.ToDirection() * 22, 20, 5);
        hints.GoalZones.Add(p => p.InCircle(ctr, 19) ? 0 : goalEdge(p));
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        // draw safespot
        if (NumCasts == 0 && AOEs.Count <= 1 && _initialSafespot != default)
            Arena.AddCircleUnfilled(_initialSafespot, 1, Colors.Safe);

        // draw bait
        var order = BaitOrder[pcSlot];
        if (order >= NextBaitOrder && order <= Casters.Count)
        {
            var source = Casters[order - 1];
            Arena.ActorInsideBounds(source.Position, source.Rotation, Colors.Object);
            BaitShape(order).Outline(Arena, source.Position, Angle.FromDirection(pc.Position - source.Position));
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.Firehorn or OID.Iceclaw or OID.Thunderwing or OID.TailOfDarkness or OID.FangOfLight && !Casters.Contains(actor))
        {
            Casters.Add(actor);
            InitIfReady();
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var shape = CastShape(spell.Action);
        if (shape != null)
        {
            AOEs.Add(new(shape, caster.Position, spell.Rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID));
        }

        if ((AID)spell.Action.ID == AID.MegaflareTower)
            Towers.Add(spell.LocXZ);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        var shape = CastShape(spell.Action);
        if (shape != null)
        {
            if (AOEs.RemoveAll(aoe => aoe.ActorID == caster.InstanceID) > 0)
                ++NumCasts;
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (!Raid.TryFindSlot(actor.InstanceID, out var slot))
            return;

        switch ((IconID)iconID)
        {
            case IconID.LunarDive: // this happens at the same time (so arbitrary order) as first cauterize
                BaitOrder[slot] = 1;
                break;
            case IconID.Cauterize:
                BaitOrder[slot] = ++NumBaitsAssigned;
                break;
            case IconID.MegaflareDive:
                BaitOrder[slot] = ++NumBaitsAssigned;
                if (NumBaitsAssigned == 7)
                    for (var i = 0; i < BaitOrder.Length; ++i)
                        if (BaitOrder[i] == 0)
                            BaitOrder[i] = 8; // twintania bait
                break;
            case IconID.MegaflareStack:
                Stackers.Set(slot);
                break;
        }
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID == OID.NaelDeusDarnus && id == 0x1E43)
        {
            _nael = actor;
            InitIfReady();
        }
        else if ((OID)actor.OID == OID.Twintania && id == 0x1E44)
        {
            Twintania = actor;
            InitIfReady();
        }
        else if ((OID)actor.OID == OID.BahamutPrime && id == 0x1E43)
        {
            _baha = actor;
            InitIfReady();
        }
    }

    private void InitIfReady()
    {
        if (DiveOrder != 0 || _nael == null || Twintania == null || _baha == null || Casters.Count != 5)
            return;

        // at this point NextCasters should contain 5 drakes, order is not yet known
        var dirToNael = Angle.FromDirection(_nael.Position - Module.Center);
        var dirToTwin = Angle.FromDirection(Twintania.Position - Module.Center);
        var dirToBaha = Angle.FromDirection(_baha.Position - Module.Center);

        // bahamut on cardinal => CCW dive order
        // bahamut on intercardinal => CW dive order
        var bahamutIntercardinal = ((int)MathF.Round(dirToBaha.Deg / 45) & 1) != 0;
        DiveOrder = bahamutIntercardinal ? -1 : +1;
        var orders = Casters.Select(c => DiveOrder * CCWDirection(Angle.FromDirection(c.Position - Module.Center), dirToBaha)).ToList();
        MemoryExtensions.Sort(orders.AsSpan(), Casters.AsSpan());
        Casters.Insert(0, _nael);
        Casters.Add(_baha);
        Casters.Add(Twintania);

        // safespot is opposite of bahamut; if nael is there - adjusted 45 degrees
        var dirToSafespot = dirToBaha + 180.Degrees();
        if (dirToSafespot.AlmostEqual(dirToNael, 0.1f))
            dirToSafespot += DiveOrder * 45.Degrees();
        _initialSafespot = Module.Center + 20 * dirToSafespot.ToDirection();
    }

    private float CCWDirection(Angle direction, Angle reference)
    {
        var ccwDist = (direction - reference).Normalized().Deg;
        if (ccwDist < -5f)
            ccwDist += 360;
        return ccwDist;
    }

    private int NextBaitOrder => AOEs.Count + NumCasts + 1;
    private AOEShapeRect BaitShape(int order) => order switch
    {
        1 or 8 => _shapeNaelTwin,
        7 => _shapeBahamut,
        _ => _shapeDrake
    };

    private AOEShapeRect? CastShape(ActionID aid) => (AID)aid.ID switch
    {
        AID.Cauterize1 => _shapeDrake,
        AID.Cauterize2 => _shapeDrake,
        AID.Cauterize3 => _shapeDrake,
        AID.Cauterize4 => _shapeDrake,
        AID.Cauterize5 => _shapeDrake,
        AID.LunarDive => _shapeNaelTwin,
        AID.TwistingDive => _shapeNaelTwin,
        AID.MegaflareDive => _shapeBahamut,
        _ => null
    };
}

class P3GrandOctetTower : P3MegaflareTower
{
    readonly P3GrandOctet go;
    BitMask _stackTargets;
    bool _assigned;
    readonly int[] _qmtOrder = Utils.MakeArray(8, -1);

    public P3GrandOctetTower(BossModule module) : base(module)
    {
        EnableHints = false;
        go = module.FindComponent<P3GrandOctet>()!;
        foreach (var (slot, group) in Service.Config.Get<UCOBConfig>().P3QuickmarchTrioAssignments.Resolve(Raid))
            if ((uint)slot < PartyState.MaxPartySize)
                _qmtOrder[slot] = group;
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.MegaflareStack)
        {
            var slot = Raid.FindSlot(actor.InstanceID);
            if ((uint)slot >= PartyState.MaxPartySize)
                return;
            _stackTargets.Set(slot);
            foreach (ref var t in Towers.AsSpan())
                t.ForbiddenSoakers.Set(slot);
            AssignTowers();
        }
    }

    void AssignTowers()
    {
        if (_assigned || _stackTargets.NumSetBits() != 4 || go == null || go.Twintania == null || Towers.Count != 4)
            return;

        var twinAngle = (go.Twintania.Position - Arena.Center).ToAngle();
        var twinBaiterPos = Arena.Center + (twinAngle + (go.DiveOrder * 15).Degrees()).ToDirection() * 21.5f;

        Towers.SortBy(t => (t.Position - twinBaiterPos).LengthSq());

        var allowedPlayers = Raid.WithSlot().ExcludedFromMask(_stackTargets).OrderBy(s => _qmtOrder[s.Item1] >= 0 ? (ulong)_qmtOrder[s.Item1] : s.Item2.InstanceID).ToList();

        if (allowedPlayers.Count < Towers.Count)
            return;

        var twinBait = allowedPlayers.FindIndex(p => go.BaitOrder[p.Item1] == 8);
        if (twinBait >= 0)
        {
            var old = allowedPlayers[twinBait];
            allowedPlayers.RemoveAt(twinBait);
            allowedPlayers.Insert(0, old);
        }

        for (var i = 0; i < Towers.Count; i++)
            Towers.Ref(i).ForbiddenSoakers = ~BitMask.Build(allowedPlayers[i].Item1);

        _assigned = true;
    }
}
