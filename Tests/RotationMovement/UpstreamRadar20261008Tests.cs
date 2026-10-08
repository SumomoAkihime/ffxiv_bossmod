using BossMod;
using BossMod.Components;

static class UpstreamRadar20261008Tests
{
    public static void Run(Action<bool, string> check)
    {
        VerifyRegistrations(check);
        VerifyMultiBossLifecycles(check);
        VerifyAngraPlayerSpecificAOEs(check);
        VerifyCerberusInvertedAOEs(check);
        VerifyCloudChaseLifecycle(check);
        VerifyAtomosGeometry(check);
        VerifyTitanGeometryAndLineOfSight(check);
        VerifyRamuhKnockback(check);
        VerifyGarudaPullAndTransition(check);
        VerifyExistingKnockbackConstraints(check);
    }

    private static void VerifyRegistrations(Action<bool, string> check)
    {
        (uint oid, uint cfc, uint nameID, string type)[] cases =
        [
            (0x19D, 4, 1382, "D012CaptainMadison"), (0x1A0, 4, 1382, "D012CaptainMadisonSecond"),
            (0x112, 6, 427, "D062ManorSteward"),
            (0xD9C, 111, 3243, "A31Garm"), (0xE00, 111, 3231, "A31AngraMainyu"),
            (0xDA0, 111, 3247, "A33QueenScylla"), (0xDFC, 111, 3227, "A32FiveheadedDragon"),
            (0xDA4, 111, 3380, "A35Atomos"), (0xDF6, 111, 3234, "A33Cerberus"),
            (0xCFA, 111, 3240, "A34CloudofDarkness"),
            (0x298C, 689, 8350, "E04Titan"), (0x2D02, 715, 9281, "E05Ramuh"), (0x2D0E, 719, 9287, "E06Garuda")
        ];
        foreach (var item in cases)
        {
            using var fixture = new Fixture();
            using var module = fixture.Module(item.oid);
            var info = module.Info ?? throw new InvalidOperationException("缺少模块注册信息");
            check(module.GetType().Name == item.type && info.GroupType == BossModuleInfo.GroupType.CFC
                && info.GroupID == item.cfc && info.NameID == item.nameID && info.PrimaryActorOID == item.oid
                && info.Maturity == BossModuleInfo.Maturity.Contributed, $"{item.type} 保持唯一 OID、CFC、NameID 和 Contributed 注册");
            module.StateMachine.Start(fixture.World.CurrentTime);
            check(module.Components.Count > 0, $"{item.type} 激活机制组件");
        }
    }

    private static void VerifyMultiBossLifecycles(Action<bool, string> check)
    {
        using var fixture = new Fixture();
        using var garm = fixture.Module(0xD9C);
        garm.StateMachine.Start(fixture.World.CurrentTime);
        fixture.World.Execute(new ActorState.OpDead(garm.PrimaryActor.InstanceID, true));
        garm.StateMachine.Update(fixture.World.CurrentTime);
        check(garm.StateMachine.ActivePhaseIndex == 0, "加姆死亡且后续小怪未出现时继续保持活动");
        var dragon = fixture.Create(0xD9D, new(-77, 383));
        garm.StateMachine.Update(fixture.World.CurrentTime);
        check(garm.StateMachine.ActivePhaseIndex == 0, "加姆后续双头龙仍存活时保持活动");
        fixture.World.Execute(new ActorState.OpDead(dragon.InstanceID, true));
        garm.StateMachine.Update(fixture.World.CurrentTime);
        check(garm.StateMachine.ActivePhaseIndex != 0, "加姆与已出现的后续小怪全部死亡后结束");

        using var queen = fixture.Module(0xDA0);
        queen.StateMachine.Start(fixture.World.CurrentTime);
        var gate = fixture.Create(0xE73, new(130, 265));
        var xande = fixture.Create(0xDA3, new(130, 265));
        queen.StateMachine.Update(fixture.World.CurrentTime);
        fixture.World.Execute(new ActorState.OpDead(queen.PrimaryActor.InstanceID, true));
        queen.StateMachine.Update(fixture.World.CurrentTime);
        check(queen.StateMachine.ActivePhaseIndex == 0, "斯库拉死亡后等待赛安德和召唤门清理");
        fixture.World.Execute(new ActorState.OpDead(gate.InstanceID, true));
        fixture.World.Execute(new ActorState.OpDead(xande.InstanceID, true));
        queen.StateMachine.Update(fixture.World.CurrentTime);
        check(queen.StateMachine.ActivePhaseIndex != 0, "斯库拉的后续首领与召唤门全部清理后结束");
    }

    private static void VerifyAngraPlayerSpecificAOEs(Action<bool, string> check)
    {
        using var fixture = new Fixture();
        using var module = fixture.Module(0xE00);
        module.StateMachine.Start(fixture.World.CurrentTime);
        var player = fixture.Player(new(-147, 297));
        var vision = Component<GenericAOEs>(module, "DoubleVision");
        vision.OnCastStarted(module.PrimaryActor, Cast(3272, module.Arena.Center, 2.5f));
        vision.Update();
        check(vision.ActiveAOEs(0, player).Length == 0, "双重视线没有烙印时不显示个人危险半场");
        player.Statuses[0] = new(636, 1, fixture.World.FutureTime(10), 0);
        var first = vision.ActiveAOEs(0, player);
        if (first.Length == 0)
        {
            check(false, "双重视线有烙印时必须显示缓存");
            return;
        }
        var firstRotation = first[0].Rotation;
        check(first.Length == 1, "双重视线根据玩家烙印选择一侧缓存");
        player.Statuses[0] = new(637, 1, fixture.World.FutureTime(10), 0);
        var second = vision.ActiveAOEs(0, player);
        check(second.Length == 1 && MathF.Abs((second[0].Rotation - firstRotation).Normalized().Rad) > 3,
            "不同烙印的危险半场相反且不会沿用前个玩家缓存");
        vision.OnEventCast(module.PrimaryActor, Event(3273, module.Arena.Center));
        check(vision.ActiveAOEs(0, player).Length == 0, "双重视线结算后立即隐藏缓存");

        var roulette = Component<GenericAOEs>(module, "Roulette");
        roulette.OnEventCast(module.PrimaryActor, Event(3279, module.Arena.Center));
        roulette.Update();
        check(roulette.ActiveAOEs(0, player).Length == 1, "死亡轮盘结算后保留短暂危险扇区");
        fixture.Advance(3.1);
        roulette.Update();
        check(roulette.ActiveAOEs(0, player).Length == 0, "死亡轮盘显示期限结束后清理缓存");
    }

    private static void VerifyCerberusInvertedAOEs(Action<bool, string> check)
    {
        using var fixture = new Fixture();
        using var module = fixture.Module(0xDF6);
        module.StateMachine.Start(fixture.World.CurrentTime);
        var player = fixture.Player(new(0, -198));
        player.Class = Class.DRG;
        var juice = fixture.Create(0xDF9, new(0, -198));
        var mini = Component<GenericAOEs>(module, "Mini");
        var cast = Cast(3249, juice.Position, 3);
        mini.OnCastStarted(juice, cast);
        var aoes = mini.ActiveAOEs(0, player);
        check(aoes.Length == 1 && !aoes[0].Risky && aoes[0].Color == Colors.SafeFromAOE,
            "需要入腹且未缩小时 Mini 显示安全范围");
        var hints = new AIHints();
        mini.AddAIHints(0, player, default, hints);
        check(hints.ForbiddenZones.Count == 1 && !hints.ForbiddenZones[0].shapeDistance.Contains(juice.Position)
            && hints.ForbiddenZones[0].shapeDistance.Contains(juice.Position + new WDir(10, 0)), "Mini 反向禁区要求进入缩小范围");
        player.Statuses[0] = new(438, 10, fixture.World.FutureTime(10), 0);
        aoes = mini.ActiveAOEs(0, player);
        check(aoes.Length == 1 && aoes[0].Risky, "已经缩小后 Mini 恢复危险范围");
        var slabber = Component<GenericAOEs>(module, "Slabber");
        var zone = fixture.Create(0x1E968C, new(0, -190));
        slabber.Update();
        var zones = slabber.ActiveAOEs(0, player);
        check(zones.Length == 1 && !zones[0].Risky, "缩小玩家将吞食毒区显示为安全范围");
        player.PosRot.Y = -101;
        module.Components.Single(c => c.GetType().Name == "BellyArena").Update();
        check(mini.ActiveAOEs(0, player).Length == 0 && slabber.ActiveAOEs(0, player).Length == 0
            && module.Arena.Center.AlmostEqual(new(1, -200), 0.01f), "进入腹内切换场地并隐藏外部机制");
        player.PosRot.Y = 0;
        module.Components.Single(c => c.GetType().Name == "BellyArena").Update();
        check(module.Arena.Center.AlmostEqual(new(0, -198), 0.01f), "离开腹内恢复外部场地");
        mini.OnCastFinished(juice, cast);
        check(mini.ActiveAOEs(0, player).Length == 0, "Mini 读条结束清理范围");
    }

    private static void VerifyCloudChaseLifecycle(Action<bool, string> check)
    {
        using var fixture = new Fixture();
        using var module = fixture.Module(0xCFA);
        module.StateMachine.Start(fixture.World.CurrentTime);
        var player = fixture.Player(new(-300, -400));
        var chase = Component<StandardChasingAOEs>(module, "FeintParticleBeam");
        chase.OnCastStarted(module.PrimaryActor, Cast(3298, player.Position, 3.5f));
        var first = chase.ActiveAOEs(0, player);
        check(chase.Chasers.Count == 1 && first.Length == 1 && first[0].Shape is AOEShapeCircle { Radius: 8 },
            "暗黑之云无需图标自动选择追踪目标且首击半径为 8");
        for (var i = 0; i < 17; ++i)
            chase.OnEventCast(module.PrimaryActor, Event(i == 0 ? 3298u : 3299u, player.Position));
        check(chase.Chasers.Count == 0 && chase.ActiveAOEs(0, player).Length == 0, "十七击耗尽后清理追踪序列与缓存");
        chase.OnCastStarted(module.PrimaryActor, Cast(3298, player.Position, 3.5f));
        check(chase.Chasers.Count == 1, "追踪序列结束后同一目标可再次入选");
        fixture.World.Execute(new ActorState.OpDead(player.InstanceID, true));
        chase.Update();
        check(chase.Chasers.Count == 0 && chase.ActiveAOEs(0, player).Length == 0, "追踪目标死亡立即清理范围");
    }

    private static void VerifyAtomosGeometry(Action<bool, string> check)
    {
        using var fixture = new Fixture();
        using var module = fixture.Module(0xDA4);
        WPos[] centers = [new(-31, 49), new(0, -5), new(31, 49)];
        foreach (var center in centers)
            check(module.Arena.InBounds(center) && module.Arena.InBounds(center + new WDir(20, 0)), "阿托莫斯形状构造器保留上层圆台和下层环");
        check(!module.Arena.InBounds(new(0, 31)), "阿托莫斯平台间空隙不被错误填满");
        check(module.Arena.Bounds is ArenaBoundsCustom bounds && module.Arena.Center.AlmostEqual(bounds.Center, 0.01f),
            "阿托莫斯绘图中心使用组合边界实际中心");
    }

    private static void VerifyTitanGeometryAndLineOfSight(Action<bool, string> check)
    {
        using var fixture = new Fixture();
        using var module = fixture.Module(0x298C);
        module.StateMachine.Start(fixture.World.CurrentTime);
        var player = fixture.Player(new(100, 100));
        var earth = Component<SimpleAOEGroups>(module, "EvilEarth");
        var h1 = fixture.Create(0x233C, new(95, 95));
        var h2 = fixture.Create(0x233C, new(105, 105));
        var later = Cast(16820, h2.Position, 5);
        var earlier = Cast(16623, h1.Position, 3);
        earth.OnCastStarted(h2, later);
        earth.OnCastStarted(h1, earlier);
        var aoes = earth.ActiveAOEs(0, player);
        check(aoes.Length == 2 && aoes[0].Color == Colors.Danger && aoes[1].Color != Colors.Danger
            && aoes[0].Shape is AOEShapeRect { LengthFront: 10, HalfWidth: 5 }, "E04 地格保留全部波次并按首组时间高亮");
        earth.OnCastFinished(h1, earlier);
        earth.OnCastFinished(h2, later);
        check(earth.ActiveAOEs(0, player).Length == 0, "E04 地格全部结算后无残留");
        var rock = fixture.Create(0x298E, new(110, 100));
        var los = Component<GenericLineOfSightAOE>(module, "SeismicWave");
        los.OnCastFinished(rock, Cast(16626, rock.Position, 5));
        check(los.ActiveAOEs(0, player).Length == 1, "E04 巨石落地后生成提前避障安全区");
        los.OnEventCast(module.PrimaryActor, Event(16627, module.Arena.Center));
        check(los.ActiveAOEs(0, player).Length == 0, "E04 地震结算后清理避障缓存");
        check(Component<ProximityAOEs>(module, "CrumblingDown").Shape is AOEShapeCircle { Radius: 60 }, "E04 岩层崩落保持用户指定半径 60");
        var fault = Component<GenericKnockback>(module, "FaultZoneKnockback");
        fault.OnCastStarted(module.PrimaryActor, Cast(16642, new(100, 120), 3));
        fault.OnCastStarted(h1, Cast(16643, h1.Position, 3));
        var hints = new AIHints();
        fault.AddAIHints(0, player, default, hints);
        check(hints.ForbiddenZones.Count == 1 && hints.ForbiddenZones[0].shapeDistance.Contains(new(110, 110)), "E04 车击退预先限制场外落点");
        fault.OnCastFinished(h1, Cast(16643, h1.Position, 3));
        check(fault.ActiveKnockbacks(0, player).Length == 0, "E04 车击退结算后清理来源");
    }

    private static void VerifyRamuhKnockback(Action<bool, string> check)
    {
        using var fixture = new Fixture();
        using var module = fixture.Module(0x2D02);
        module.StateMachine.Start(fixture.World.CurrentTime);
        var player = fixture.Player(new(110, 100));
        var helper = fixture.Create(0x233C, new(100, 80));
        var knockback = Component<GenericKnockback>(module, "DeadlyDischargeKnockback");
        var cast = Cast(19347, helper.Position, 4.5f);
        knockback.OnCastStarted(helper, cast);
        var hints = new AIHints();
        knockback.AddAIHints(0, player, default, hints);
        check(hints.ForbiddenZones.Count == 1 && hints.ForbiddenZones[0].shapeDistance.Contains(new(110, 100))
            && !hints.ForbiddenZones[0].shapeDistance.Contains(new(103, 100)), "E05 从直线最近点击退并限制场外落点");
        knockback.OnCastFinished(helper, cast);
        check(knockback.ActiveKnockbacks(0, player).Length == 0, "E05 击退结算后清理来源");
    }

    private static void VerifyGarudaPullAndTransition(Action<bool, string> check)
    {
        using var fixture = new Fixture();
        using var module = fixture.Module(0x2D0E);
        module.StateMachine.Start(fixture.World.CurrentTime);
        var player = fixture.Player(new(108, 100));
        var helper = fixture.Create(0x233C, new(100, 100));
        var vacuum = Component<GenericAOEs>(module, "VacuumSlice");
        vacuum.OnCastStarted(module.PrimaryActor, Cast(19413, module.Arena.Center, 4));
        var pull = Component<GenericKnockback>(module, "IrresistiblePull");
        var cast = Cast(20025, helper.Position, 4);
        helper.CastInfo = cast;
        pull.OnCastStarted(helper, cast);
        var hints = new AIHints();
        pull.AddAIHints(0, player, default, hints);
        check(hints.ForbiddenZones.Count == 1 && hints.ForbiddenZones[0].shapeDistance.Contains(new(108, 100))
            && !hints.ForbiddenZones[0].shapeDistance.Contains(new(112, 100)), "E06 吸引按直线投影计算并避开吸引后危险线");
        var orb = fixture.Create(0x2D11, new(108, 100));
        var explosion = Component<GenericAOEs>(module, "Explosion");
        explosion.Update();
        check(explosion.ActiveAOEs(0, player).Length == 1 && explosion.ActiveAOEs(0, player)[0].Origin.AlmostEqual(new(103, 100), 0.01f),
            "E06 风球缓存随吸引预测位置更新");
        explosion.OnEventCast(orb, Event(19426, orb.Position));
        explosion.Update();
        check(explosion.ActiveAOEs(0, player).Length == 0, "E06 最后风球结算后清理缓存");
        pull.OnCastFinished(helper, cast);
        vacuum.OnMapEffect(1, 0x00080004);
        check(pull.ActiveKnockbacks(0, player).Length == 0 && vacuum.ActiveAOEs(0, player).Length == 0, "E06 吸引结束与地形清除后清理来源及危险线");
        var hands = Component<BaitAwayTethers>(module, "HandsOfHell");
        var tether = new ActorTetherInfo(106, helper.InstanceID);
        hands.OnTethered(player, tether);
        hands.Update();
        var firstActivation = hands.CurrentBaits[0].Activation;
        check(hands.CurrentBaits[0].Shape is AOEShapeRect rect && MathF.Abs(rect.LengthFront - 8) < 0.01f,
            "E06 地狱之手矩形长度随目标距离更新");
        hands.OnEventCast(helper, Event(19434, player.Position));
        check(hands.CurrentBaits.Count == 0, "E06 地狱之手结算后清理引导");
        fixture.Advance(5);
        hands.OnTethered(player, tether);
        check(hands.CurrentBaits.Count == 1 && hands.CurrentBaits[0].Activation > firstActivation,
            "E06 新一轮地狱之手不沿用旧引导时间");
        fixture.World.Execute(new ActorState.OpDead(module.PrimaryActor.InstanceID, true));
        module.StateMachine.Update(fixture.World.CurrentTime);
        check(module.StateMachine.ActivePhaseIndex == 0, "E06 迦楼罗死亡后等待融合阶段");
        var rakta = fixture.Create(0x2D10, new(100, 100));
        module.Update();
        fixture.World.Execute(new ActorState.OpDead(rakta.InstanceID, true));
        module.StateMachine.Update(fixture.World.CurrentTime);
        check(module.StateMachine.ActivePhaseIndex != 0, "E06 已出现的融合首领死亡后结束");
    }

    private static void VerifyExistingKnockbackConstraints(Action<bool, string> check)
    {
        (uint oid, uint aid, string component)[] cases = [(0x290C, 15941, "EmptyHateKnockback"), (0x296B, 16339, "TidalWave")];
        foreach (var item in cases)
        {
            using var fixture = new Fixture();
            using var module = fixture.Module(item.oid);
            module.StateMachine.Start(fixture.World.CurrentTime);
            var player = fixture.Player(new(100, 100));
            var helper = fixture.Create(0x233C, new(120, 100));
            var knockback = Component<GenericKnockback>(module, item.component);
            foreach (var source in new WPos[] { new(120, 100), new(80, 100) })
            {
                var cast = Cast(item.aid, source, 5);
                knockback.OnCastStarted(helper, cast);
                var hints = new AIHints();
                knockback.AddAIHints(0, player, default, hints);
                var unsafePoint = source.X > 100 ? new WPos(90, 100) : new WPos(110, 100);
                var safePoint = source.X > 100 ? new WPos(114, 100) : new WPos(86, 100);
                check(hints.ForbiddenZones.Count > 0 && hints.ForbiddenZones.Any(z => z.shapeDistance.Contains(unsafePoint))
                    && hints.ForbiddenZones.All(z => !z.shapeDistance.Contains(safePoint)), $"{item.component} 保留场地两侧的专用击退 AI 约束");
                knockback.OnCastFinished(helper, cast);
                check(knockback.ActiveKnockbacks(0, player).Length == 0, $"{item.component} 结算后无击退来源残留");
            }
        }
    }

    private static T Component<T>(BossModule module, string name) where T : BossComponent => module.Components.OfType<T>().Single(c => c.GetType().Name == name);

    private static ActorCastInfo Cast(uint aid, WPos location, float total) => new()
    {
        Action = new(ActionType.Spell, aid), Location = new(location.X, 0, location.Z), TotalTime = total
    };

    private static ActorCastEvent Event(uint aid, WPos target) => new(new(ActionType.Spell, aid), 0, 0, 0, new(target.X, 0, target.Z), 1, 0, default);

    private sealed class Fixture : IDisposable
    {
        public readonly WorldState World = new(10000000, "upstream-radar-20261008-test");
        private ulong _nextID = 1;

        public Fixture() => World.Frame = new(DateTime.UnixEpoch.AddHours(1), 0, 0, 0, 0, 1);

        public Actor Create(uint oid, WPos position, ActorType type = ActorType.Enemy)
        {
            var id = _nextID++;
            World.Execute(new ActorState.OpCreate(id, oid, (int)id, 0, "fixture", 0, type, Class.None, 100,
                new(position.X, 0, position.Z, 0), 0.5f, new(1000, 1000, 0, 10000, 10000), true, false, 0, 0, 0));
            return World.Actors.Find(id)!;
        }

        public Actor Player(WPos position)
        {
            var player = Create(0, position, ActorType.Player);
            World.Execute(new PartyState.OpModify(0, new(1, player.InstanceID, false, "fixture")));
            return player;
        }

        public BossModule Module(uint oid) => BossModuleRegistry.CreateModuleForActor(World, Create(oid, new(100, 100)), BossModuleInfo.Maturity.Contributed)
            ?? throw new InvalidOperationException($"未注册模块 OID {oid:X}");

        public void Advance(double seconds) => World.Frame = new(World.CurrentTime.AddSeconds(seconds), 0, 0, 0, 0, 1);
        public void Dispose() { }
    }
}
