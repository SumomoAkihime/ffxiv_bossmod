using System.Reflection;
using BossMod;
using BossMod.Components;
using Hobbes = BossMod.Shadowbringers.Alliance.A12Hobbes.A12Hobbes;
using Goliath = BossMod.Shadowbringers.Alliance.A13GoliathTank.A13GoliathTank;
using Fortress = BossMod.Shadowbringers.Alliance.A15WalkingFortress.A15WalkingFortress;
using Thanatos = BossMod.RealmReborn.Alliance.A13Thanatos.A13Thanatos;
using Adelphel = BossMod.Heavensward.Dungeon.D04TheVault.D041SerAdelphel.D041SerAdelphel;
using ThunderGod = BossMod.Stormblood.Alliance.A33ThunderGod.A33ThunderGod;

static class RadarDrawingTests
{
    const BindingFlags All = BindingFlags.Public | BindingFlags.Instance;
    static readonly MethodInfo ActivateMethod = typeof(BossModule).GetMethod(nameof(BossModule.ActivateComponent), All)!;

    public static void Run(Action<bool, string> check)
    {
        PhlegethonCenter(check);
        HobbesCenter(check);
        GoliathEnergyRing(check);
        WalkingFortressExaflare(check);
        WalkingFortressTurrets(check);
        WalkingFortressCancelledStacks(check);
        ThanatosCenter(check);
        AdelphelCenter(check);
        ThunderGodCenter(check);
        ThunderGodColosseum(check);
        MustadioSatelliteBeam(check);
        ThunderGodHallowedBolt(check);
    }

    static void PhlegethonCenter(Action<bool, string> check)
    {
        using var f = new Fixture();
        var center = new WPos(-110, 181.6f);
        var boss = f.Create(0x938, center);
        using var module = new BossMod.RealmReborn.Alliance.A16Phlegethon.A16Phlegethon(f.World, boss);
        Center(module, check, "提坦");
        var inside = true;
        for (var x = -31; x <= 31; ++x)
            for (var z = -31; z <= 31; ++z)
                if (x * x + z * z < 32 * 32)
                    inside &= module.Arena.InBounds(center + new WDir(x, z));
        check(inside, "提坦主场圆与南侧扇区重叠时不抵消边界");
        check(module.Arena.InBounds(new(-148.65f, 191.975f)) && module.Arena.InBounds(new(-110, 221.59f))
            && module.Arena.InBounds(new(-71.35f, 191.975f)) && !module.Arena.InBounds(center + new WDir(0, -40)), "提坦三踏板可进入，北侧场外仍不可进入");
        var flare = Activate<GenericAOEs>(module, "BossMod.RealmReborn.Alliance.A16Phlegethon.AncientFlareVoidzone");
        flare.OnCastStarted(boss, Cast(1730, center, default, 7));
        check(flare.ActiveAOEs(0, f.Player)[0].Origin == center, "提坦古代耀星仍使用实际机制圆心");
        var border = Activate<GenericAOEs>(module, "BossMod.RealmReborn.Alliance.A16Phlegethon.DynamicArenaBorder");
        border.OnActorEAnim(f.Create(0x1E8894, center), 0x00040008);
        check(border.ActiveAOEs(0, f.Player)[0].Origin == center, "提坦动态外圈仍使用实际机制圆心");
    }

    static void HobbesCenter(Action<bool, string> check)
    {
        using var f = new Fixture();
        var boss = f.Create(0x2C2B, new(-805, -240));
        using var module = new Hobbes(f.World, boss);
        Registered(module, typeof(Hobbes), 0x2C2B, check, "霍布斯");
        Center(module, check, "霍布斯");
        check(module.Arena.InBounds(new(-831, -225)) && module.Arena.InBounds(new(-805, -270)) && module.Arena.InBounds(new(-779, -225)) && !module.Arena.InBounds(new(-831, -204)), "霍布斯三平台及平台外世界坐标正确");

        var fire = Activate<GenericAOEs>(module, "BossMod.Shadowbringers.Alliance.A12Hobbes.FireResistanceTest");
        fire.OnMapEffect(7, 0x00100010);
        var aoes = fire.ActiveAOEs(0, f.Player);
        var platform = new WPos(-831, -225);
        var mechanicCenter = new WPos(-805, -240);
        var expected = mechanicCenter + (mechanicCenter - platform).Normalized() * 15;
        check(aoes.Length == 1 && aoes[0].Origin.AlmostEqual(expected, 0.01f), "霍布斯抗火预警使用固定机制中心");
    }

    static void GoliathEnergyRing(Action<bool, string> check)
    {
        using var f = new Fixture();
        var boss = f.Create(0x2C7E, new(-780, 555));
        var helper = f.Create(0x233C, new(-770, 555));
        using var module = new Goliath(f.World, boss);
        Registered(module, typeof(Goliath), 0x2C7E, check, "巨人战车");
        var ring = Activate<ConcentricAOEs>(module, "BossMod.Shadowbringers.Alliance.A13GoliathTank.EnergyRing");
        ring.OnCastStarted(boss, Cast(18743, boss.Position, default, 4));
        ring.OnCastStarted(helper, Cast(18743, helper.Position, default, 4));
        ring.Update();
        var initial = ring.ActiveAOEs(0, f.Player);
        check(initial.Length == 2 && initial[0].Shape is AOEShapeCircle { Radius: 12 } && initial[1].Shape is AOEShapeCircle { Radius: 12 }, "巨人战车能同时绘制交叠首环");
        ring.OnEventCast(boss, Event(18743));
        ring.Update();
        check(ring.ActiveAOEs(0, f.Player).ToArray().Single(a => a.Origin.AlmostEqual(boss.Position, 0.01f)).Shape is AOEShapeDonut { InnerRadius: 12, OuterRadius: 24 }, "巨人战车首击后推进至第二环");
        ring.OnEventCast(boss, Event(18744));
        ring.Update();
        check(ring.ActiveAOEs(0, f.Player).ToArray().Single(a => a.Origin.AlmostEqual(boss.Position, 0.01f)).Shape is AOEShapeDonut { InnerRadius: 24, OuterRadius: 36 }, "巨人战车第二击后推进至第三环");
        ring.OnEventCast(boss, Event(18745));
        ring.OnEventCast(boss, Event(18746));
        ring.Update();
        check(ring.ActiveAOEs(0, f.Player).Length == 1, "巨人战车能量环按伤害步骤推进并保留另一条序列");
        for (var i = 0; i < 4; ++i)
        {
            ring.OnEventCast(helper, Event((uint)(18743 + i)));
            ring.Update();
        }
        check(ring.ActiveAOEs(0, f.Player).Length == 0, "巨人战车能量环末次伤害后清空");
        ring.OnCastStarted(boss, Cast(18743, boss.Position, default, 4));
        f.World.Execute(new ActorState.OpDead(boss.InstanceID, true));
        ring.Update();
        check(ring.ActiveAOEs(0, f.Player).Length == 0, "巨人战车死亡时清空能量环绘制缓存");
    }

    static void WalkingFortressExaflare(Action<bool, string> check)
    {
        VerifyExaflareLine(check, "南向", new(920, 397), -0.0000479221f.Radians(), 18649, 0.7f,
            [new(920, 397), new(920, 406.359985f), new(920, 415.075012f), new(920, 423.760010f), new(920, 432.399994f), new(920, 441.084991f), new(920, 449.799988f)]);
        VerifyExaflareLine(check, "东向", new(870, 427), 1.5703885555f.Radians(), 18649, 0.7f,
            [new(870, 427), new(879.344971f, 427.003235f), new(888.059998f, 427.006226f), new(896.744995f, 427.009247f), new(905.459961f, 427.012238f), new(914.145020f, 427.015228f), new(922.844971f, 427.018250f)]);

        using var f = new Fixture();
        var boss = f.Create(0x2C74, new(900, 427));
        var caster = f.Create(0x233C, new(880, 397));
        using var module = new Fortress(f.World, boss);
        Registered(module, typeof(Fortress), 0x2C74, check, "移动要塞");
        var exa = Activate<Exaflare>(module, "BossMod.Shadowbringers.Alliance.A15WalkingFortress.BallisticExaImpact");
        exa.OnCastStarted(caster, Cast(18650, caster.Position, default, 3.2f));
        exa.Update();
        check(exa.ActiveAOEs(0, f.Player).Length == 0, "移动要塞延后读条在接近结算前不提前绘制");
        f.Advance(1);
        exa.Update();
        check(exa.ActiveAOEs(0, f.Player).Length != 0, "移动要塞延后读条进入显示窗口后绘制");
    }

    static void VerifyExaflareLine(Action<bool, string> check, string direction, WPos origin, Angle rotation, uint aid, float duration, WPos[] impacts)
    {
        using var f = new Fixture();
        var boss = f.Create(0x2C74, new(900, 427));
        var caster = f.Create(0x233C, origin, rotation);
        using var module = new Fortress(f.World, boss);
        var exa = Activate<Exaflare>(module, "BossMod.Shadowbringers.Alliance.A15WalkingFortress.BallisticExaImpact");
        exa.OnCastStarted(caster, Cast(aid, origin, rotation, duration));
        exa.Update();
        check(exa.ActiveAOEs(0, f.Player).Length != 0, $"移动要塞{direction}实录首击开始绘制");
        for (var i = 0; i < impacts.Length; ++i)
        {
            f.Move(caster, impacts[i], rotation);
            exa.OnEventCast(caster, Event(18652));
            exa.Update();
            check((exa.ActiveAOEs(0, f.Player).Length != 0) == (i + 1 < impacts.Length), $"移动要塞{direction}第 {i + 1} 击推进与清理");
        }
    }

    static void WalkingFortressTurrets(Action<bool, string> check)
    {
        using var f = new Fixture();
        var boss = f.Create(0x2C74, new(900, 427));
        var left = f.Create(0x2C77, new(884.9774f, 441.9774f));
        var right = f.Create(0x2C77, new(914.9767f, 411.9783f));
        var first = f.CreatePlayer(new(879.6547f, 450.8438f));
        var second = f.CreatePlayer(new(904.2791f, 442.1372f));
        left.TargetID = first.InstanceID;
        right.TargetID = second.InstanceID;
        using var module = new Fortress(f.World, boss);
        var turrets = Activate<GenericAOEs>(module, "BossMod.Shadowbringers.Alliance.A15WalkingFortress.GoliathTankLaserTurret");
        turrets.OnEventIcon(first, 164, first.InstanceID);
        turrets.OnEventIcon(second, 164, second.InstanceID);
        var aoes = turrets.ActiveAOEs(0, f.Player).ToArray();
        check(aoes.Length == 2 && aoes.Any(a => a.Origin.AlmostEqual(left.Position, 0.01f)) && aoes.Any(a => a.Origin.AlmostEqual(right.Position, 0.01f)), "移动要塞双标记按战车目标分配，不按最近战车合并");
        f.Move(first, new(870, 470), default);
        f.Move(second, new(930, 400), default);
        left.TargetID = second.InstanceID;
        right.TargetID = first.InstanceID;
        aoes = turrets.ActiveAOEs(0, f.Player).ToArray();
        check(aoes.Single(a => a.Origin.AlmostEqual(left.Position, 0.01f)).Rotation.AlmostEqual(left.AngleTo(first), 0.01f) && aoes.Single(a => a.Origin.AlmostEqual(right.Position, 0.01f)).Rotation.AlmostEqual(right.AngleTo(second), 0.01f), "移动要塞激光跟随原标记目标移动，换仇恨不改分配");
        turrets.OnEventCast(left, Event(18662));
        check(turrets.ActiveAOEs(0, f.Player).Length == 1, "移动要塞左战车结算只清理自身预警");
        turrets.OnEventCast(right, Event(18662));
        check(turrets.ActiveAOEs(0, f.Player).Length == 0, "移动要塞右战车结算清理剩余预警");
    }

    static void WalkingFortressCancelledStacks(Action<bool, string> check)
    {
        using var f = new Fixture();
        var boss = f.Create(0x2C74, new(900, 427));
        var caster1 = f.Create(0x233C, new(900, 412));
        var caster2 = f.Create(0x233C, new(900, 412));
        var target1 = f.CreatePlayer(new(919.462891f, 443.198242f));
        var target2 = f.CreatePlayer(new(883.451538f, 438.345947f));
        using var module = new Fortress(f.World, boss);
        var stacks = Activate<GenericStackSpread>(module, "BossMod.Shadowbringers.Alliance.A15WalkingFortress.ShrapnelImpact");
        var cast1 = Cast(18675, caster1.Position, default, 4.7f); cast1.TargetID = target1.InstanceID;
        var cast2 = Cast(18675, caster2.Position, default, 4.7f); cast2.TargetID = target2.InstanceID;
        f.World.Execute(new ActorState.OpCastInfo(caster1.InstanceID, cast1));
        f.World.Execute(new ActorState.OpCastInfo(caster2.InstanceID, cast2));
        check(stacks.Stacks.Count == 2, "移动要塞实录分摊读条显示两组分摊");
        f.World.Execute(new ActorState.OpCastInfo(caster1.InstanceID, null));
        f.World.Execute(new ActorState.OpCastInfo(caster2.InstanceID, null));
        check(stacks.Stacks.Count == 0, "移动要塞无伤害的取消读条会清理分摊预警");
    }

    static void ThanatosCenter(Action<bool, string> check)
    {
        using var f = new Fixture();
        var boss = f.Create(0x92E, new(440.4f, 280));
        using var module = new Thanatos(f.World, boss);
        Registered(module, typeof(Thanatos), 0x92E, check, "塔纳托斯");
        Center(module, check, "塔纳托斯");
        check(module.Arena.InBounds(new(440.4f, 280)) && !module.Arena.InBounds(new(440.4f, 230)), "塔纳托斯中心圆及圆外世界坐标正确");
        var cloud = Activate<GenericAOEs>(module, "BossMod.RealmReborn.Alliance.A13Thanatos.Cloudscourge");
        cloud.OnCastStarted(boss, Cast(760, new(440.4f, 280), default, 3));
        check(cloud.ActiveAOEs(0, f.Player).Length == 1, "塔纳托斯激活后的世界坐标 AOE 可见");
    }

    static void AdelphelCenter(Action<bool, string> check)
    {
        using var f = new Fixture();
        var boss = f.Create(0x1051, new(15, -100));
        using var module = new Adelphel(f.World, boss);
        Registered(module, typeof(Adelphel), 0x1051, check, "圣阿德尔菲尔");
        Center(module, check, "圣阿德尔菲尔");
        check(module.Arena.InBounds(new(0, -100)) && !module.Arena.InBounds(new(0, -120)) && !module.Arena.InBounds(new(21, -100)), "圣阿德尔菲尔裁边内外世界坐标正确");
        var blade = Activate<GenericAOEs>(module, "BossMod.Heavensward.Dungeon.D04TheVault.D041SerAdelphel.ShiningBlade");
        blade.OnActorNpcYell(boss, 2523);
        var aoes = blade.ActiveAOEs(0, f.Player);
        check(aoes.Length == 4 && aoes[0].Origin.AlmostEqual(boss.Position, 0.01f) && aoes[0].Rotation.AlmostEqual(Angle.FromDirection(new WPos(-18.509f, -100.023f) - boss.Position), 0.01f), "圣阿德尔菲尔闪耀之刃保留固定机制中心");
    }

    static void ThunderGodCenter(Action<bool, string> check)
    {
        using var f = new Fixture();
        var boss = f.Create(0x25D7, new(-600, -600));
        using var module = new ThunderGod(f.World, boss);
        Registered(module, typeof(ThunderGod), 0x25D7, check, "雷神");
        Center(module, check, "雷神");
        check(module.Arena.InBounds(new(-612.5f, -578.4f)) && !module.Arena.InBounds(new(-600, -600)) && !module.Arena.InBounds(new(-600, -570)), "雷神平台、中央空洞及场外世界坐标正确");
        var bolt = Activate<GenericAOEs>(module, "BossMod.Stormblood.Alliance.A33ThunderGod.HallowedBolt");
        bolt.OnCastStarted(boss, Cast(14182, boss.Position, default, 3));
        bolt.Update();
        check(bolt.ActiveAOEs(0, f.Player).Length == 1, "雷神激活后的中心 AOE 使用修正场地");
    }

    static void ThunderGodColosseum(Action<bool, string> check)
    {
        using var f = new Fixture();
        var boss = f.Create(0x25D7, new(-600, -600));
        using var module = new ThunderGod(f.World, boss);
        var arena = Activate<BossComponent>(module, "BossMod.Stormblood.Alliance.A33ThunderGod.Colosseum");
        var lower = module.Bounds;
        var lowerCenter = module.Center;
        arena.OnEventCast(boss, Event(14178));
        check(module.Bounds is ArenaBoundsCircle && module.Center.AlmostEqual(new(-600, -600), 0.001f), "雷神竞技场进入完整圆形并切换正确中心");
        check(module.Arena.InBounds(new(-600, -600)) && module.Arena.InBounds(new(-567.544f, -608.598f)), "雷神上层包含中心及最新回放的最远有效站位");
        check(module.Arena.InBounds(new(-565.1f, -600)) && !module.Arena.InBounds(new(-564.9f, -600)), "雷神上层按游戏模型使用35米圆形边界");
        arena.OnEventCast(boss, Event(14186));
        check(module.Bounds is ArenaBoundsCircle, "雷神上层终结演出开始时保留上层场地");
        arena.OnEventCast(boss, Event(14187));
        check(ReferenceEquals(module.Bounds, lower) && module.Center == lowerCenter && !module.Arena.InBounds(new(-600, -600)), "雷神返回时恢复六平台边界和绘图中心");
        arena.OnEventCast(boss, Event(14187));
        check(ReferenceEquals(module.Bounds, lower), "雷神重复返回事件保持边界稳定");
    }

    static void Registered(BossModule module, Type type, uint oid, Action<bool, string> check, string name)
        => check(BossModuleRegistry.FindByOID(oid)?.ModuleType == type && BossModuleRegistry.FindByType(type) != null && module.Info?.ModuleType == type, $"{name}模块已注册并以真实主实体构造");

    static void Center(BossModule module, Action<bool, string> check, string name)
    {
        var bounds = (ArenaBoundsCustom)module.Bounds;
        check(module.Arena.Center.AlmostEqual(bounds.Center, 0.01f), $"{name}场地中心取组合边界中心");
    }

    static T Activate<T>(BossModule module, string typeName) where T : BossComponent
    {
        var type = typeof(BossModule).Assembly.GetType(typeName, true)!;
        ActivateMethod.MakeGenericMethod(type).Invoke(module, null);
        return (T)module.Components.Single(c => c.GetType() == type);
    }

    static void MustadioSatelliteBeam(Action<bool, string> check)
    {
        using var f = new Fixture();
        var boss = f.Create(0x25B7, new(600, 290));
        var turret = f.Create(0x25B8, new(621.2f, 290), 135f.Degrees());
        using var module = new BossMod.Stormblood.Alliance.A31Mustadio.A31Mustadio(f.World, boss);
        var beam = Activate<GenericAOEs>(module, "BossMod.Stormblood.Alliance.A31Mustadio.SatelliteBeam");
        beam.OnCastStarted(turret, Cast(14145, new(610.590f, 300.588f), turret.Rotation, 1.7f));
        var aoes = beam.ActiveAOEs(0, f.Player);
        check(aoes.Length == 1 && aoes[0].Origin.AlmostEqual(turret.Position, 0.01f), "姆斯塔迪奥卫星射线使用机械兵坐标，不使用后移15米的读条目标点");
        check(aoes[0].Check(turret.Position + turret.Rotation.ToDirection() * 29) && !aoes[0].Check(turret.Position - turret.Rotation.ToDirection()), "姆斯塔迪奥卫星射线朝机械兵前方覆盖四分之一场地");
        beam.OnEventCast(turret, Event(14145));
        check(beam.ActiveAOEs(0, f.Player).IsEmpty, "姆斯塔迪奥卫星射线结算清除");
        beam.OnCastStarted(turret, Cast(14145, new(610.590f, 300.588f), turret.Rotation, 1.7f));
        beam.OnActorDestroyed(turret);
        check(beam.ActiveAOEs(0, f.Player).IsEmpty, "姆斯塔迪奥机械兵提前销毁清除预警");
    }

    static void ThunderGodHallowedBolt(Action<bool, string> check)
    {
        using var f = new Fixture();
        var boss = f.Create(0x25D7, new(-600, -600));
        using var module = new ThunderGod(f.World, boss);
        var bolt = Activate<GenericAOEs>(module, "BossMod.Stormblood.Alliance.A33ThunderGod.HallowedBolt");
        foreach (var circleFirst in new[] { true, false })
        {
            var first = f.Create(0x233C, new(-608.44037f, -620.14124f));
            var second = f.Create(0x233C, first.Position);
            var cast1 = Cast(circleFirst ? 14182u : 14183u, first.Position, default, 4.7f);
            var cast2 = Cast(circleFirst ? 14183u : 14182u, second.Position, default, 4.7f);
            bolt.OnCastStarted(first, cast1);
            f.Advance(2);
            bolt.OnCastStarted(second, cast2);
            var initial = bolt.ActiveAOEs(0, f.Player);
            check(initial.Length == 1 && (initial[0].Shape is AOEShapeCircle) == circleFirst, "雷神两种圆环顺序均只显示当前段");
            bolt.OnCastFinished(first, cast1);
            var next = bolt.ActiveAOEs(0, f.Player);
            check(next.Length == 1 && (next[0].Shape is AOEShapeCircle) != circleFirst, "雷神当前段结束立即显示实际第二段");
            bolt.OnCastFinished(second, cast2);
            check(bolt.ActiveAOEs(0, f.Player).IsEmpty, "雷神两种圆环顺序结算后均无残留");
        }
        var helper = f.Create(0x233C, new(-580.1771f, -617.84082f));
        var cancelled = Cast(14182, helper.Position, default, 4.7f);
        bolt.OnCastStarted(helper, cancelled);
        bolt.OnCastFinished(helper, cancelled);
        check(bolt.ActiveAOEs(0, f.Player).IsEmpty, "雷神取消读条不生成虚构的下一环");
        bolt.OnCastStarted(helper, cancelled);
        bolt.OnActorDestroyed(helper);
        check(bolt.ActiveAOEs(0, f.Player).IsEmpty, "雷神辅助实体销毁清除范围");
        bolt.OnCastStarted(helper, cancelled);
        bolt.OnEventCast(boss, Event(14187));
        check(bolt.ActiveAOEs(0, f.Player).IsEmpty, "雷神返回下层时清除上层预警");
    }

    static ActorCastInfo Cast(uint aid, WPos location, Angle rotation, float total) => new()
    {
        Action = new(ActionType.Spell, aid),
        Location = new(location.X, 0, location.Z),
        Rotation = rotation,
        TotalTime = total
    };

    static ActorCastEvent Event(uint aid) => new(new(ActionType.Spell, aid), 0, 0, 0, default, 0, 0, default);

    sealed class Fixture : IDisposable
    {
        public readonly WorldState World = new(10000000, "radar-test");
        public readonly Actor Player;
        ulong _nextID = 10;

        public Fixture()
        {
            World.Frame = new(DateTime.UnixEpoch.AddHours(1), 0, 0, 0, 0, 1);
            Player = CreatePlayer(default);
        }

        public Actor Create(uint oid, WPos position, Angle rotation = default)
        {
            var id = _nextID++;
            World.Execute(new ActorState.OpCreate(id, oid, (int)id, 0, "fixture", 0, ActorType.Enemy, Class.None, 100, new(position.X, 0, position.Z, rotation.Rad), .5f, new(1000, 1000, 0, 10000, 10000), true, false, 0, 0, 0));
            return World.Actors.Find(id)!;
        }

        public Actor CreatePlayer(WPos position)
        {
            var id = _nextID++;
            World.Execute(new ActorState.OpCreate(id, 0, (int)id, 0, "player", 0, ActorType.Player, Class.PLD, 100, new(position.X, 0, position.Z, 0), .5f, new(1000, 1000, 0, 10000, 10000), true, true, 0, 0, 0));
            return World.Actors.Find(id)!;
        }

        public void Move(Actor actor, WPos position, Angle rotation) => World.Execute(new ActorState.OpMove(actor.InstanceID, new(position.X, 0, position.Z, rotation.Rad)));
        public void Advance(double seconds) => World.Frame.Timestamp = World.CurrentTime.AddSeconds(seconds);
        public void Dispose() { }
    }
}
