namespace BossMod.Dawntrail.Trial.T05Necron;

sealed class NeutronRing(BossModule module) : Components.RaidwideCastDelay(module, AID.NeutronRingVisual, AID.NeutronRing, 2.6f);

sealed class GrandCrossArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    private static readonly AOEShapeDonut donut = new(9f, 60f);
    private bool active;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ArenaChangeVisual)
        {
            _aoe = [new(donut, Necron.ArenaCenter, default, Module.CastFinishAt(spell, 1.1f))];
        }
    }

    public override void OnEventDirectorUpdate(uint updateID, uint param1, uint param2, uint param3, uint param4)
    {
        if (updateID == 0x8000000D)
        {
            if (param1 == 0x2u)
            {
                Arena.Bounds = Necron.CircleArena;
                Arena.Center = Necron.ArenaCenter;
                _aoe = [];
                active = true;
            }
            else if (param1 == 0x01u)
            {
                Arena.Bounds = new ArenaBoundsRect(18f, 15f);
                Arena.Center = Necron.ArenaCenter;
                active = false;
            }
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        base.DrawArenaBackground(pcSlot, pc);
        if (!active || pc.PosRot.Y < -100f)
        {
            return;
        }
        else if (Arena.Bounds.Radius != 9f)
        {
            Arena.Bounds = Necron.CircleArena;
            Arena.Center = Necron.ArenaCenter;
        }
    }
}

sealed class GrandCrossBait(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GrandCrossBait, 3f);

sealed class GrandCrossRect(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(2);
    private static readonly AOEShapeRect rectPredict = new(50f, 2.25f, 50f), rect = new(100f, 2f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        var oid = source.OID;
        var offset = oid switch
        {
            (uint)OID.AzureAether1 => 41f.Degrees(),
            (uint)OID.AzureAether2 => -153f.Degrees(),
            _ => default
        };
        if (offset != default)
        {
            var center = Arena.Center;
            _aoes.Add(new(rectPredict, center, Angle.FromDirection(center - source.Position) + offset, WorldState.FutureTime(oid == (uint)OID.AzureAether1 ? 7.6f : 5.6f)));
            if (_aoes.Count > 1)
            {
                ref var aoe1 = ref _aoes.Ref(0);
                ref var aoe2 = ref _aoes.Ref(1);
                if (aoe1.Activation > aoe2.Activation)
                {
                    (aoe1, aoe2) = (aoe2, aoe1);
                }
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID == (uint)AID.GrandCrossRect) // replace prediction with actual AOE
        {
            ref var aoe = ref _aoes.Ref(0);
            aoe = new(rect, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.GrandCrossRect)
        {
            ++NumCasts;
            if (_aoes.Count != 0)
            {
                _aoes.RemoveAt(0);
            }
        }
    }
}

sealed class GrandCrossProx(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GrandCrossProximity, new AOEShapeRect(100f, 5f));

