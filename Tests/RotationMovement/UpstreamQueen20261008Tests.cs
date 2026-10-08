using System.Reflection;
using BossMod;
using BossMod.Components;
using BossMod.Pathfinding;
using Queen = BossMod.Dawntrail.Extreme.Ex3QueenEternal.Ex3QueenEternal;
using QueenAID = BossMod.Dawntrail.Extreme.Ex3QueenEternal.AID;

static class UpstreamQueen20261008Tests
{
    const BindingFlags All = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
    const string Namespace = "BossMod.Dawntrail.Extreme.Ex3QueenEternal.";

    public static void Run(Action<bool, string> check)
    {
        Transition(check);
        EarthLayers(check);
        IceBridges(check);
        LegitimateForce(check);
        EddaDonut(check);
    }

    static void Transition(Action<bool, string> check)
    {
        var f = new Fixture();
        using var module = new Queen(f.World, f.Boss);
        module.StateMachine.Start(f.World.CurrentTime);
        var arena = module.Components.Single(c => c.GetType().Name == "ArenaChanges");
        var phase1 = module.StateMachine.Phases[0];
        f.Boss.HPMP.CurHP = 1;
        check(!phase1.Update!(), "女王 HP=1 但转场读条未开始时保持 P1");
        var cast = Cast((uint)QueenAID.AuthorityEternal, f.Boss.Position, 10);
        arena.OnCastStarted(f.Boss, cast);
        arena.OnCastFinished(f.Boss, cast);
        check(f.Boss.CastInfo == null && phase1.Update!(), "女王转场读条结束后 HP=1 仍可进入 P2");
        module.StateMachine.Update(f.World.CurrentTime);
        check(module.StateMachine.ActivePhaseIndex == 1 && module.Components.Contains(arena), "女王转场标志跨阶段保留");
        var intermission = module.StateMachine.Phases[1].InitialState;
        check(intermission.Update!(0) == 0 && intermission.NextStates![0].Update!(0) == 0, "P2 转场等待实际开始与结束标志，读条已结束也能推进");
        f.Boss.IsDestroyed = true;
        check(!module.StateMachine.Phases[1].Update!(), "P2 主体尚未生成时不提前结束战斗");
        var boss2 = f.Create(0x4678, Queen.ArenaCenter);
        typeof(Queen).GetMethod("UpdateModule", All)!.Invoke(module, null);
        check(!module.StateMachine.Phases[1].Update!(), "P2 主体存活时保持战斗");
        boss2.IsDead = true;
        check(module.StateMachine.Phases[1].Update!(), "两阶段主体均死亡或销毁后结束战斗");
    }

    static void EarthLayers(Action<bool, string> check)
    {
        var f = new Fixture();
        using var module = new Queen(f.World, f.Boss);
        var changes = Activate<BossComponent>(module, Namespace + "ArenaChanges");
        var earth = Activate<BossComponent>(module, Namespace + "VirtualShiftEarth");
        changes.OnEventDirectorUpdate(0x8000000D, 4, 0, 0, 0);
        var bounds = (ArenaBoundsCustom)module.Bounds;
        check(bounds.WorldProjectionLayers is { Length: 2 } && module.Center == Queen.ArenaCenter, "女王首轮大地建立同中心的浮空与地面双层");
        check(module.ResolveArenaProjectionLayer(f.Player) == 1, "女王有效地面队员选择分裂层");
        var npc = f.Create(0x233C, Queen.ArenaCenter);
        var status = new ActorStatus(3770, 0, DateTime.MaxValue, 0);
        earth.OnStatusGain(npc, ref status);
        check(Get<BitMask>(earth, "Flying").None() && module.ResolveArenaProjectionLayer(npc) == 0, "非队员浮空状态不读写队员掩码，按几何回退判层");
        var savedProjection = MiniArena.Config.ProjectRadarInto3DWorld;
        MiniArena.Config.ProjectRadarInto3DWorld = false;
        try
        {
            VerifyLayer(1, false);
            earth.OnStatusGain(f.Player, ref status);
            VerifyLayer(0, true);
            earth.OnStatusLose(f.Player, ref status);
            VerifyLayer(1, false);

            // 独立 MiniArena 未提供回调时继续保留几何判层。
            var standalone = new MiniArena(module.Center, bounds);
            standalone.Begin(default, f.Boss, f.Player, false);
            check(standalone.CurrentArenaProjectionLayer == 0, "MiniArena 可选回调保留独立调用方的几何选择");
            standalone.End();
        }
        finally { MiniArena.Config.ProjectRadarInto3DWorld = savedProjection; }

        changes.OnEventDirectorUpdate(0x8000000D, 1, 0, 0, 0);
        check(module.ResolveArenaProjectionLayer(f.Player) == null, "大地组件仍活动时普通场地不遗留层编号");
        changes.OnEventDirectorUpdate(0x8000000D, 4, 0, 0, 0);
        check(ReferenceEquals(module.Bounds, Queen.EarthBounds), "后续大地保持原有分裂场地");

        void VerifyLayer(int layer, bool centerAccessible)
        {
            module.Arena.Begin(default, f.Boss, f.Player, false);
            check(module.Arena.CurrentArenaProjectionLayer == layer && module.ResolveArenaProjectionLayer(f.Player) == layer,
                $"女王第 {layer} 层的实际 Begin 回调与模块判层一致");
            var presentation = Get<RelSimplifiedComplexPolygon>(module.Arena, "_frameArenaProjectionShape");
            check(presentation.Contains(default) == centerAccessible && presentation.Contains(new(-8, -6)),
                $"女王第 {layer} 层迷你图正确包含平台及排除地面中央空洞");
            var worldLayer = (int)typeof(MiniArena).GetMethod("SelectDefaultWorldProjectionLayer", All)!.Invoke(module.Arena, [bounds, default(WDir), 0f])!;
            check(worldLayer == layer, $"女王第 {layer} 层 3D 默认投影跟随同一帧选择");
            var hints = new AIHints();
            module.CalculateAIHints(0, f.Player, default, hints);
            var map = new Map();
            hints.InitPathfindMap(map);
            check(hints.PathfindMapArenaProjectionLayer == layer && Accessible(map, Queen.ArenaCenter) == centerAccessible && Accessible(map, new(92, 94)),
                $"女王第 {layer} 层 AI 地图与绘图使用同一可站立区域");
            var probe = new LayerAOE(module);
            probe.AddAIHints(0, f.Player, default, hints);
            check(hints.ForbiddenZones.Count == (layer == 1 ? 1 : 0), $"女王第 {layer} 层 AOE 过滤使用真实模块回调");
            var text = new BossComponent.TextHints();
            probe.AddHints(0, f.Player, text);
            check((text.Count != 0) == (layer == 1), $"女王第 {layer} 层 AOE 文字与 AI 一致");
            probe.DrawArenaBackground(0, f.Player);
            check(module.Arena.CurrentArenaProjectionLayer == layer, "AOE 绘图作用域结束后保留当前队员层");
            module.Arena.End();
        }
    }

    static void IceBridges(Action<bool, string> check)
    {
        var f = new Fixture();
        using var module = new Queen(f.World, f.Boss);
        var changes = Activate<BossComponent>(module, Namespace + "ArenaChanges");
        changes.OnEventDirectorUpdate(0x8000000D, 8, 0, 0, 0);
        var ice = Activate<GenericAOEs>(module, Namespace + "VirtualShiftIce");
        foreach (var index in new byte[] { 4, 5, 6, 7 })
        {
            var bridge = Queen.ArenaCenter + new WDir(index < 6 ? -5 : 5, index % 2 == 0 ? -4 : 4);
            ice.OnMapEffect(index, 0x00020001);
            check(module.Arena.InBounds(bridge), $"冰桥 {index} 重生后纳入全部冰区边界");
            ice.OnMapEffect(index, 0x00200010);
            check(ice.ActiveAOEs(0, f.Player).Length == 1 && ice.ActiveAOEs(0, f.Player)[0].Origin == bridge, $"冰桥 {index} 损坏时生成正确危险区");
            ice.OnMapEffect(index, 0x00800004);
            check(!module.Arena.InBounds(bridge) && ice.ActiveAOEs(0, f.Player).IsEmpty, $"冰桥 {index} 摧毁后扣除边界并清除危险区");
        }
    }

    static void LegitimateForce(Action<bool, string> check)
    {
        foreach (var rightFirst in new[] { true, false })
        {
            var f = new Fixture();
            using var module = new Queen(f.World, f.Boss);
            var force = Activate<GenericAOEs>(module, Namespace + "LegitimateForce");
            var aid = rightFirst ? QueenAID.LegitimateForceFirstR : QueenAID.LegitimateForceFirstL;
            var cast = Cast((uint)aid, f.Boss.Position, 8);
            cast.Rotation = 180f.Degrees();
            force.OnCastStarted(f.Boss, cast);
            var initial = force.ActiveAOEs(0, f.Player).ToArray();
            check(initial.Length == 2 && initial[0].Origin.AlmostEqual(new(rightFirst ? 85 : 115, 90), 0.031f) && initial[1].Origin.AlmostEqual(new(rightFirst ? 115 : 85, 90), 0.031f)
                && initial.All(a => a.Shape is AOEShapeRect { LengthFront: 60, HalfWidth: 15 } && a.Rotation == cast.Rotation), "合法武力左右两种顺序使用偏移原点及长 60 半宽 15 矩形");
            check(initial[0].Risky && !initial[1].Risky && Math.Abs((initial[1].Activation - initial[0].Activation).TotalSeconds - 3.1) < 0.001, "合法武力两段风险及 3.1 秒间隔保留");
            force.OnEventCast(f.Boss, Event((uint)aid));
            var next = force.ActiveAOEs(0, f.Player);
            check(next.Length == 1 && next[0].Origin == initial[1].Origin && next[0].Risky, "合法武力首段结算后晋级第二段");
            force.OnEventCast(f.Boss, Event((uint)(rightFirst ? QueenAID.LegitimateForceSecondL : QueenAID.LegitimateForceSecondR)));
            check(force.ActiveAOEs(0, f.Player).IsEmpty && force.NumCasts == 2, "合法武力第二段结算后清除范围");
        }
    }

    static void EddaDonut(Action<bool, string> check)
    {
        var f = new Fixture();
        var boss = f.Create(0x16C6, new(300, 374));
        using var module = new BossMod.Heavensward.DeepDungeon.PalaceOfTheDead.DD50EddaBlackbosom.DD50EddaBlackbosom(f.World, boss);
        var donut = Activate<SimpleAOEs>(module, "BossMod.Heavensward.DeepDungeon.PalaceOfTheDead.DD50EddaBlackbosom.InHeathDonut");
        check(donut.Shape is AOEShapeDonut { InnerRadius: 3, OuterRadius: 51.5f }, "死宫 50 健康月环使用内径 3、外径 51.5");
    }

    static bool Accessible(Map map, WPos position)
    {
        var (x, y) = map.WorldToGrid(position);
        return x >= 0 && y >= 0 && x < map.Width && y < map.Height && map.PixelMaxG[y * map.Width + x] >= 0;
    }

    static T Activate<T>(BossModule module, string name) where T : BossComponent
    {
        var type = typeof(BossModule).Assembly.GetType(name, true)!;
        typeof(BossModule).GetMethod(nameof(BossModule.ActivateComponent), All)!.MakeGenericMethod(type).Invoke(module, null);
        return (T)module.Components.Single(c => c.GetType() == type);
    }
    static T Get<T>(object instance, string name) => (T)instance.GetType().GetField(name, All)!.GetValue(instance)!;
    static ActorCastInfo Cast(uint aid, WPos position, float duration) => new() { Action = new(ActionType.Spell, aid), Location = new(position.X, 0, position.Z), TotalTime = duration };
    static ActorCastEvent Event(uint aid) => new(new(ActionType.Spell, aid), 0, 0, 0, default, 0, 0, default);

    sealed class LayerAOE(BossModule module) : GenericAOEs(module)
    {
        readonly AOEInstance[] _aoes = [new(new AOEShapeCircle(30), Queen.ArenaCenter, arenaProjectionLayer: 1, restrictToArenaProjectionLayer: true)];
        public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;
    }

    sealed class Fixture
    {
        public readonly WorldState World = new(10000000, "queen-upstream-test");
        public readonly Actor Player;
        public readonly Actor Boss;
        ulong _nextID = 10;
        public Fixture()
        {
            World.Frame = new(DateTime.UnixEpoch.AddHours(1), 0, 0, 0, 0, 1);
            Player = Create(0, new(92, 94), ActorType.Player);
            World.Execute(new PartyState.OpModify(0, new(1, Player.InstanceID, false, "player")));
            Boss = Create(0x4677, Queen.ArenaCenter);
        }
        public Actor Create(uint oid, WPos position, ActorType type = ActorType.Enemy)
        {
            var id = _nextID++;
            World.Execute(new ActorState.OpCreate(id, oid, (int)id, 0, "fixture", 0, type, type == ActorType.Player ? Class.PLD : Class.None, 100,
                new(position.X, 0, position.Z, 0), .5f, new(1000, 1000, 0, 10000, 10000), true, type == ActorType.Player, 0, 0, 0));
            return World.Actors.Find(id)!;
        }
    }
}
