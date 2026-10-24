namespace BossMod.Stormblood.Raid.O3NHalicarnassus;

public enum OID : uint
{
    Boss = 0x1E14, // R0.5
    SoulReaper = 0x1E15, // R2.0
    GreatDragon = 0x1E16, // R6.0
    Helper = 0x186D
}

public enum AID : uint
{
    AutoAttackDragon = 970, // GreatDragon->player, no cast, single-target
    AutoAttack = 871, // Boss->player, no cast, single-target

    SpellbladeHoly = 8943, // Boss->self, 3.0s cast, single-target
    SpellbladeBlizzardIII = 9313, // Boss->self, 3.0s cast, range 6, point blank AOE
    SpellbladeFireIII = 9312, // Boss->self, 3.0s cast, range ?-15 AOE Donut
    SpellbladeThunderIII = 9314, // Boss->self, 2.5s cast, range 60, width 6, frontal line AOE
    HolyBlur = 9316, //Helper->player, no cast, range 6
    Ribbit = 9318, //Boss->none, 4.0s cast, range 17, 95-degree cone, frontal AOE cone

    TheQueensWaltz1 = 9327, // Boss->self, 3.0s cast
    SwordDance = 9328, // Helper->self, 3.0s cast, range 34, 20-degree cone
    TheQueensWaltz2 = 9329, // Boss->self, 5.0s cast
    EarthlyDance = 9330, // Helper->self, no cast, range 80
    Uplift = 9331, // Helper->self, no cast, range 80

    ThePlayingField = 8960, // Boss->self, 3.0s cast
    PanelSwap = 8964, // Boss->self, 3.0s cast
    TheGame = 9325, // Boss->self, 5.0s cast, range 80

    PlaceToken = 8955, // Boss->self, 3.0s cast, spawns GreatDragon
    PlaceDarkToken = 8956, // Boss->self 3.0s cast, spawns SoulReapers
    CrossReaper = 9323, // SoulReaper->self, 8.5s cast, range8, point blank AOE
    FrostBreath = 9334, // GreatDragon->self, no cast, range 15, 8.5-degree cone, frontal cone AoE
}

class Ribbit(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Ribbit, new AOEShapeCone(19.5f, 75.Degrees()));
class SpellbladeThunderIII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SpellbladeThunderIII, new AOEShapeRect(60, 3));
class SpellbladeBlizzardIII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SpellbladeBlizzardIII, new AOEShapeCircle(9));
class SpellbladeFireIII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SpellbladeFireIII, new AOEShapeDonut(4, 15));
class CrossReaper(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CrossReaper, new AOEShapeCircle(10));
class TheQueensWaltz1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TheQueensWaltz1, new AOEShapeCone(34, 10.Degrees()));
class TheQueensWaltz2(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeRect rect = new(5, 5, 5);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var result = new List<AOEInstance>(_aoes.Count);
        for (var i = 12; i < _aoes.Count; ++i)
            result.Add(_aoes[i]);
        for (var i = 0; i < Math.Min(12, _aoes.Count); ++i)
        {
            var aoe = _aoes[i];
            aoe.Color = Colors.AOE;
            result.Add(aoe);
        }
        return CollectionsMarshal.AsSpan(result);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.TheQueensWaltz2)
        {
            int[] xOffsets = [-15, -5, 5, 15];
            int[] zOffsets = [-15, -5, 5, 15];

            foreach (var zOffset in zOffsets)
            {
                foreach (var xOffset in xOffsets)
                {
                    if (xOffset != zOffset)
                    {
                        _aoes.Add(new(rect, new(xOffset, zOffset)));
                    }
                }
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count > 0 && (AID)spell.Action.ID == AID.TheQueensWaltz2)
        {
            _aoes.RemoveRange(0, Math.Min(12, _aoes.Count));
        }
    }
}

class TheGame(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeRect rect = new(5, 5, 5);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count == 0)
            return [];

        int[] safeTiles = actor.Role switch
        {
            Role.Tank => [6],
            Role.Healer => [9],
            Role.Melee or Role.Ranged => [5, 10],
            _ => []
        };
        var result = new List<AOEInstance>(_aoes.Count);
        for (var i = 16; i < _aoes.Count; ++i)
            result.Add(_aoes[i]);
        for (var i = 0; i < Math.Min(16, _aoes.Count); ++i)
        {
            if (safeTiles.Contains(i))
                continue;
            var aoe = _aoes[i];
            aoe.Color = Colors.AOE;
            result.Add(aoe);
        }
        return CollectionsMarshal.AsSpan(result);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.TheGame)
        {
            int[] xOffsets = [-15, -5, 5, 15];
            int[] zOffsets = [-15, -5, 5, 15];

            foreach (var zOffset in zOffsets)
            {
                foreach (var xOffset in xOffsets)
                {
                    _aoes.Add(new(rect, new(xOffset, zOffset)));
                }
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count > 0 && (AID)spell.Action.ID == AID.TheGame)
        {
            _aoes.RemoveRange(0, Math.Min(16, _aoes.Count));
        }
    }
}
class O3NHalicarnassusStates : StateMachineBuilder
{
    public O3NHalicarnassusStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Ribbit>()
            .ActivateOnEnter<SpellbladeThunderIII>()
            .ActivateOnEnter<SpellbladeBlizzardIII>()
            .ActivateOnEnter<SpellbladeFireIII>()
            .ActivateOnEnter<CrossReaper>()
            .ActivateOnEnter<TheQueensWaltz1>()
            .ActivateOnEnter<TheQueensWaltz2>()
            .ActivateOnEnter<TheGame>();

    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "VeraNala", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 254, NameID = 5633)]
public class O3NHalicarnassus(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 0), new ArenaBoundsSquare(20));
