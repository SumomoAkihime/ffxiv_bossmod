using System.Numerics;
using BossMod;
using BossMod.Components;

static class UpstreamOmega20261024Tests
{
    public static void Run(Action<bool, string> check)
    {
        VerifyRegistrations(check);
        VerifyKnockbacks(check);
        VerifyDischargerSequence(check);
        VerifyGhostsAndStack(check);
        VerifyVoidzoneCleanup(check);
        VerifyAdditionalInterfaces(check);
        VerifyBoulderLifecycle(check);
        VerifyO3Batches(check);
        VerifyO8Upstream(check);
        foreach (var aid in new uint[] { 9483, 20304, 10270, 12911 })
            check(ActionDefinitions.Instance[new(ActionType.Spell, aid)] != null, $"欧米茄任务动作 {aid} 已注册，可供动作队列执行");
    }

    private static void VerifyRegistrations(Action<bool, string> check)
    {
        (uint oid, uint cfc, uint name)[] cases =
        [
            (0x1A6F, 252, 5629), (0x1E5D, 253, 5631), (0x1E14, 254, 5633),
            (0x1E77, 255, 6052), (0x213E, 286, 7127), (0x21D1, 287, 7097),
            (0x20CB, 288, 7107), (0x2153, 289, 7131), (0x2435, 587, 7691),
            (0x2448, 588, 7641), (0x2477, 589, 7695), (0x247B, 590, 7633)
        ];
        foreach (var item in cases)
        {
            using var f = new Fixture();
            using var m = f.Module(item.oid, item.name);
            check(m.Info is { GroupType: BossModuleInfo.GroupType.CFC, Maturity: BossModuleInfo.Maturity.Contributed } info
                && info.PrimaryActorOID == item.oid && info.GroupID == item.cfc && info.NameID == item.name,
                $"欧米茄 CFC {item.cfc} 的主 OID、NameID 和默认成熟度一致");
            m.StateMachine.Start(f.World.CurrentTime);
            check(m.StateMachine.ActiveState != null && m.Components.Count > 0, $"欧米茄 CFC {item.cfc} 实际激活组件");
            foreach (var component in m.Components.OfType<GenericAOEs>())
                _ = component.ActiveAOEs(0, f.Player).Length;
        }
    }

    private static void VerifyKnockbacks(Action<bool, string> check)
    {
        (uint oid, uint aid, string component, WPos safe, WPos danger)[] cases =
        [
            (0x1A6F, (uint)BossMod.Stormblood.Raid.O1NAlteRoite.AID.BreathWing, "BreathWing", new(0, 0), new(0, 17)),
            (0x1E77, (uint)BossMod.Stormblood.Raid.O4NExdeath.AID.VacuumWave, "VacuumWave", new(1, 0), new(18, 0)),
            (0x20CB, 10086, "Shockwave", new(1, 1), new(18, 0)),
            (0x2435, 12653, "CycloneKnockback", new(1, 0), new(18, 0)),
            (0x2153, 10532, "AeroAssault", new(1, 0), new(18, 0)),
            (0x2153, 10535, "Shockwave", new(0, 0), new(0, 17))
        ];
        foreach (var item in cases)
        {
            using var f = new Fixture();
            using var m = f.Module(item.oid);
            m.StateMachine.Start(f.World.CurrentTime);
            var component = Component<GenericKnockback>(m, item.component);
            var cast = f.StartCast(m.PrimaryActor, item.aid, m.Arena.Center, 5);
            var hints = new AIHints();
            component.AddAIHints(0, f.Player, default, hints);
            check(hints.ForbiddenZones.Count == 1, $"{item.oid:X}/{item.component} 读条生成击退约束");
            var zone = hints.ForbiddenZones[0];
            check(zone.activation == m.CastFinishAt(cast), $"{item.component} 禁区与实际读条结算时间一致");
            check(!zone.shapeDistance.Contains(m.Arena.Center + item.safe.ToWDir())
                && zone.shapeDistance.Contains(m.Arena.Center + item.danger.ToWDir()), $"{item.oid:X}/{item.component} 区分安全落点和场外落点");
            if (component is SimpleKnockbacks { Shape: not null })
                check(!zone.shapeDistance.Contains(m.Arena.Center + new WDir(200, 200)), $"{item.component} 不限制击退范围外的候选点");
            f.Status(0, new((uint)BossMod.WAR.SID.ArmsLength, 0, f.World.FutureTime(60), f.Player.InstanceID));
            hints.Clear();
            component.AddAIHints(0, f.Player, default, hints);
            check(hints.ForbiddenZones.Count == 0, $"{item.component} 抗击退覆盖结算时不添加禁区");
            f.Status(0, default);
            hints.Clear();
            component.AddAIHints(0, f.Player, default, hints);
            check(hints.ForbiddenZones.Count == 1, $"{item.component} 抗击退消失后恢复约束");
            f.FinishCast(m.PrimaryActor);
            hints.Clear();
            component.AddAIHints(0, f.Player, default, hints);
            check(hints.ForbiddenZones.Count == 0 && component.ActiveKnockbacks(0, f.Player).Length == 0, $"{item.component} 取消读条后无残留");
        }
    }

    private static void VerifyDischargerSequence(Action<bool, string> check)
    {
        using var f = new Fixture();
        using var m = f.Module(0x247B);
        m.StateMachine.Start(f.World.CurrentTime);
        var component = Component<GenericKnockback>(m, "Discharger");
        f.StartCast(m.PrimaryActor, 13041, m.Arena.Center, 2);
        f.Event(m.PrimaryActor, 13075, m.Arena.Center + new WDir(-15, 0));
        var sources = component.ActiveKnockbacks(0, f.Player);
        check(sources.Length == 2 && sources[0].Activation < sources[1].Activation, "O12 位移来源按实际结算先后排序");
        var hints = new AIHints();
        component.AddAIHints(0, f.Player, default, hints);
        check(hints.ForbiddenZones.Count == 2 && !hints.ForbiddenZones[0].shapeDistance.Contains(m.Arena.Center)
            && hints.ForbiddenZones[1].shapeDistance.Contains(m.Arena.Center), "O12 第二段从第一段落点继续计算，排除仅首段安全的位置");
        check(hints.ForbiddenZones.All(z => !z.shapeDistance.Contains(m.Arena.Center + new WDir(-14.9f, 0))), "O12 联合约束保留两段均安全的起点");
        var later = sources[1].Activation;
        f.Status(0, new((uint)BossMod.WAR.SID.ArmsLength, 0, f.World.FutureTime(10), f.Player.InstanceID));
        hints.Clear();
        component.AddAIHints(0, f.Player, default, hints);
        check(hints.ForbiddenZones.Count == 1 && hints.ForbiddenZones[0].activation == later, "O12 按每段生效时刻分别判断抗击退");
        f.Status(0, default);
        f.Event(m.PrimaryActor, 13046, m.Arena.Center);
        f.Event(m.PrimaryActor, 13046, m.Arena.Center);
        check(component.ActiveKnockbacks(0, f.Player).Length == 0, "O12 放电结算及重复事件均不留下来源");
        f.StartCast(m.PrimaryActor, 13041, m.Arena.Center, 2);
        f.World.Execute(new ActorState.OpDestroy(m.PrimaryActor.InstanceID));
        check(component.ActiveKnockbacks(0, f.Player).Length == 0, "O12 跟随来源销毁后清理预测");
    }

    private static void VerifyGhostsAndStack(Action<bool, string> check)
    {
        using var f = new Fixture();
        using var m = f.Module(0x213E);
        m.StateMachine.Start(f.World.CurrentTime);
        var ghost = f.Create(0x213F, new(2, 4));
        var dead = f.Create(0x213F, new(-6, 4));
        dead.IsDead = true;
        var component = Component<GenericAOEs>(m, "ChasingGhost");
        var active = component.ActiveAOEs(0, f.Player);
        check(active.Length == 1 && active[0].Origin == ghost.Position, "O5 只把存活幽灵作为危险来源");
        f.Status(0, new(938, 0, f.World.FutureTime(60), f.Player.InstanceID));
        var hints = new AIHints();
        component.AddAIHints(0, f.Player, default, hints);
        check(component.ActiveAOEs(0, f.Player).Length == 0 && hints.GoalZones.Count == 1
            && hints.GoalZones[0](ghost.Position) > hints.GoalZones[0](dead.Position)
            && hints.TemporaryObstacles.Count == 0, "O5 Throttle 引导接触存活幽灵且允许进入其范围");
        f.World.Execute(new ActorState.OpEventState(ghost.InstanceID, 7));
        hints.Clear();
        component.AddAIHints(0, f.Player, default, hints);
        check(hints.GoalZones.Count == 0, "O5 已退场的幽灵不再作为接触目标");
        f.Status(0, default);
        var tank = f.Create(0, new(0, 4), ActorType.Player, Class.PLD);
        m.PrimaryActor.TargetID = tank.InstanceID;
        f.Player.PosRot = new(0, 0, 4, 0);
        f.StartCast(m.PrimaryActor, 10406, m.Arena.Center, 5);
        var stack = Component<GenericAOEs>(m, "DiabolicHeadlamp");
        f.Player.PosRot = new(0, 0, -12, 0);
        var text = new BossComponent.TextHints();
        stack.AddHints(0, f.Player, text);
        check(text.Count == 0, "O5 直线分摊内不提示离开安全线");
        hints.Clear();
        stack.AddAIHints(0, f.Player, default, hints);
        check(hints.ForbiddenZones.Count == 1 && !hints.ForbiddenZones[0].shapeDistance.Contains(f.Player.Position)
            && hints.ForbiddenZones[0].shapeDistance.Contains(new(8, -12)), "O5 分摊文字和 AI 使用相同的线内安全区");
        f.Player.PosRot = new(8, 0, -12, 0);
        stack.AddHints(0, f.Player, text);
        check(text.Count == 1 && text[0].Item1 == "Stack in the line", "O5 直线分摊外明确提示进线");
        f.FinishCast(m.PrimaryActor);
        check(stack.ActiveAOEs(0, f.Player).Length == 0, "O5 分摊读条结束后清理");
    }

    private static void VerifyVoidzoneCleanup(Action<bool, string> check)
    {
        foreach (var item in new (uint boss, uint zone, string component)[] { (0x1E77, (uint)BossMod.Stormblood.Raid.O4NExdeath.OID.BlackHole, "BlackHoles"), (0x2477, (uint)BossMod.Stormblood.Raid.O11NOmega.OID.BurnPuddle, "BallisticPuddle") })
        {
            using var f = new Fixture();
            using var m = f.Module(item.boss);
            m.StateMachine.Start(f.World.CurrentTime);
            var zone = f.Create(item.zone, m.Arena.Center);
            var component = Component<Voidzone>(m, item.component);
            check(component.ActiveAOEs(0, f.Player).Length == 1, $"{item.component} 活跃实体显示持续范围");
            f.World.Execute(new ActorState.OpEventState(zone.InstanceID, 7));
            var hints = new AIHints();
            component.AddAIHints(0, f.Player, default, hints);
            check(component.ActiveAOEs(0, f.Player).Length == 0 && hints.TemporaryObstacles.Count == 0, $"{item.component} 退场后立即移除范围与障碍");
            f.World.Execute(new ActorState.OpDestroy(zone.InstanceID));
            check(component.ActiveAOEs(0, f.Player).Length == 0, $"{item.component} 销毁路径无残留");
        }
    }

    private static void VerifyAdditionalInterfaces(Action<bool, string> check)
    {
        using (var f = new Fixture())
        using (var m = f.Module(0x213E))
        {
            m.StateMachine.Start(f.World.CurrentTime);
            f.Player.PosRot = new(-30.6f, 0, 1, 0);
            m.Update();
            check(m.Arena.Center.AlmostEqual(new(-30.6f, 51), 0.001f)
                && m.Arena.InBounds(new(-30.6f, 1)) && m.Arena.InBounds(new(-30.6f, 90))
                && !m.Arena.InBounds(new(-30.6f, 40)), "O5 车顶世界坐标保留两段平台和中间空隙");
            f.Player.PosRot = new(0, 0, 4, 0);
            m.Update();
            check(m.Arena.Center == new WPos(0, 4) && m.Arena.InBounds(f.Player.Position), "O5 返回主车厢恢复场地中心与边界");
        }
        using (var f = new Fixture())
        using (var m = f.Module(0x1E77))
        {
            m.StateMachine.Start(f.World.CurrentTime);
            var movement = Component<StayMove>(m, "VoidElements");
            f.Status(0, new(960, 0, f.World.FutureTime(5), f.Player.InstanceID));
            check(movement.PlayerStates[0].Requirement == StayMove.Requirement.Stay, "O4 实际状态回调接入热病停步提示");
            f.Status(0, default);
            check(movement.PlayerStates[0].Requirement == StayMove.Requirement.None, "O4 热病消失事件清理停步要求");
        }
        using (var f = new Fixture())
        using (var m = f.Module(0x247B))
        {
            m.StateMachine.Start(f.World.CurrentTime);
            var female = f.Create(0x247C, m.Arena.Center + new WDir(5, 0));
            var filter = Component<BossComponent>(m, "PacketFilter");
            var hints = new AIHints();
            f.Status(0, new(1661, 0, f.World.FutureTime(30), f.Player.InstanceID));
            filter.AddAIHints(0, f.Player, default, hints);
            check(hints.ForcedTarget == m.PrimaryActor, "O12 PacketFilterF 状态通过真实回调选择男欧米茄");
            f.Status(0, new(1660, 0, f.World.FutureTime(30), f.Player.InstanceID));
            hints.Clear();
            filter.AddAIHints(0, f.Player, default, hints);
            check(hints.ForcedTarget == female, "O12 PacketFilterM 状态通过真实回调选择女欧米茄");
            f.Status(0, default);
            hints.Clear();
            filter.AddAIHints(0, f.Player, default, hints);
            check(hints.ForcedTarget == null, "O12 状态消失后解除强制目标");
        }
        using (var f = new Fixture())
        using (var m = f.Module(0x2448))
        {
            m.StateMachine.Start(f.World.CurrentTime);
            var helper = f.Create(0x233C, m.Arena.Center);
            var exaflare = Component<GenericAOEs>(m, "Exaflare");
            f.StartCast(helper, 13871, m.Arena.Center, 4);
            exaflare.Update();
            check(exaflare.ActiveAOEs(0, f.Player).Length == 5, "O10 构造连续爆炸线并刷新五段缓存");
            for (var i = 0; i < 5; ++i)
            {
                helper.PosRot = new(m.Arena.Center.X, 0, m.Arena.Center.Z + i * 8, 0);
                f.Event(helper, i == 0 ? 13871u : 13869u, helper.Position);
                exaflare.Update();
            }
            check(exaflare.ActiveAOEs(0, f.Player).Length == 0, "O10 最后一段爆炸结束后移除线及原点缓存");
        }
    }

    private static void VerifyBoulderLifecycle(Action<bool, string> check)
    {
        foreach (var late in new[] { false, true })
        {
            using var f = new Fixture();
            using var m = f.Module(0x21D1);
            m.StateMachine.Start(f.World.CurrentTime);
            var component = Component<GenericLineOfSightAOE>(m, "BoulderShadow");
            var painting = new WPos(-16, -8.5f);
            var rockPosition = new WPos(-11.37f, -3.68f);
            var helper = f.Create(0x18D6, painting);
            var rock = f.Create(0x1EA1A1, rockPosition);
            f.StartCast(helper, 10243, painting, 10);
            if (late)
                f.StartCast(m.PrimaryActor, 10287, m.Arena.Center, 5);
            component.OnActorEAnim(rock, 0);
            component.OnActorEAnim(rock, 0);
            if (!late)
                f.StartCast(m.PrimaryActor, 10287, m.Arena.Center, 5);
            var safe = rockPosition + (rockPosition - painting).Normalized() * 5;
            var danger = painting + new WDir(-2, 0);
            check(component.ActiveAOEs(0, f.Player).Length == 1 && component.ActiveAOEs(0, f.Player)[0].Check(safe), $"O6 岩石{(late ? "晚" : "先")}出现时重建唯一躲藏区");
            var hints = new AIHints();
            component.AddAIHints(0, f.Player, default, hints);
            check(hints.ForbiddenZones.Count == 1 && !hints.ForbiddenZones[0].shapeDistance.Contains(safe)
                && hints.ForbiddenZones[0].shapeDistance.Contains(danger), "O6 实际躲藏区可被 AI 使用");
            m.PrimaryActor.CastInfo!.ElapsedTime = m.PrimaryActor.CastInfo.TotalTime;
            f.FinishCast(m.PrimaryActor);
            check(component.ActiveAOEs(0, f.Player).Length == 1, "O6 正常视觉读条结束后等实际伤害清理");
            f.Event(helper, 10289, painting);
            check(component.ActiveAOEs(0, f.Player).Length == 0, "O6 实际伤害事件清理安全区");
            f.StartCast(helper, 10243, painting, 10);
            component.OnActorEAnim(rock, 0);
            f.StartCast(m.PrimaryActor, 10287, m.Arena.Center, 5);
            f.Event(m.PrimaryActor, 10287, m.Arena.Center);
            f.FinishCast(m.PrimaryActor);
            check(component.ActiveAOEs(0, f.Player).Length == 1, "O6 动作已经结算时不把客户端剩余读条误判为取消");
            f.Event(helper, 10289, painting);
            check(component.ActiveAOEs(0, f.Player).Length == 0, "O6 时序交错后仍由实际伤害清理");
            f.StartCast(helper, 10243, painting, 10);
            component.OnActorEAnim(rock, 0);
            f.StartCast(m.PrimaryActor, 10287, m.Arena.Center, 5);
            f.FinishCast(m.PrimaryActor);
            check(component.ActiveAOEs(0, f.Player).Length == 0, "O6 提前取消读条不留下躲藏区");
            component.OnActorEAnim(rock, 0);
            check(component.ActiveAOEs(0, f.Player).Length == 0, "O6 取消后重复岩石动画不恢复旧预测");
            f.StartCast(helper, 10243, painting, 10);
            component.OnActorEAnim(rock, 0);
            f.Event(helper, 10244, painting);
            check(component.ActiveAOEs(0, f.Player).Length == 0, "O6 解除附身信号清理安全区");
            f.StartCast(helper, 10243, painting, 10);
            component.OnActorEAnim(rock, 0);
            f.World.Execute(new ActorState.OpDestroy(rock.InstanceID));
            check(component.ActiveAOEs(0, f.Player).Length == 0, "O6 岩石实体销毁清理安全区");
        }
    }

    private static void VerifyO3Batches(Action<bool, string> check)
    {
        using var f = new Fixture();
        using var m = f.Module(0x1E14);
        m.StateMachine.Start(f.World.CurrentTime);
        var waltz = Component<GenericAOEs>(m, "TheQueensWaltz2");
        f.StartCast(m.PrimaryActor, 9329, m.Arena.Center, 5);
        f.StartCast(m.PrimaryActor, 9329, m.Arena.Center, 5);
        check(waltz.ActiveAOEs(0, f.Player).Length == 24, "O3 上游女王之舞保留两批范围");
        f.Event(m.PrimaryActor, 9329, m.Arena.Center);
        check(waltz.ActiveAOEs(0, f.Player).Length == 12, "O3 女王之舞一次结算只清理一批");
        f.Event(m.PrimaryActor, 9329, m.Arena.Center);
        check(waltz.ActiveAOEs(0, f.Player).Length == 0, "O3 女王之舞全部结算后清空");
        var game = Component<GenericAOEs>(m, "TheGame");
        f.StartCast(m.PrimaryActor, 9325, m.Arena.Center, 5);
        f.StartCast(m.PrimaryActor, 9325, m.Arena.Center, 5);
        check(game.ActiveAOEs(0, f.Player).Length == 31, "O3 上游游戏仅当前批次按坦克安全格过滤");
        f.Event(m.PrimaryActor, 9325, m.Arena.Center);
        check(game.ActiveAOEs(0, f.Player).Length == 15, "O3 游戏按一批十六格逐组清理");
        f.Player.Class = Class.DRG;
        check(game.ActiveAOEs(0, f.Player).Length == 14, "O3 输出角色保留两个安全格");
        f.Event(m.PrimaryActor, 9325, m.Arena.Center);
        check(game.ActiveAOEs(0, f.Player).Length == 0, "O3 游戏第二批结算无残留");
    }

    private static void VerifyO8Upstream(Action<bool, string> check)
    {
        using var f = new Fixture();
        using var m = f.Module(0x2153);
        m.StateMachine.Start(f.World.CurrentTime);
        check(Component<StackWithCastTargets>(m, "FlagrantFireStack").MinStackSize == 2, "O8 按用户决定采用上游分摊人数");
        check(Component<SimpleAOEs>(m, "WaveCannon").Shape is AOEShapeRect { LengthFront: 70.5f }
            && Component<SimpleAOEs>(m, "ThrummingThunderReal").Shape is AOEShapeRect { LengthFront: 40.5f }, "O8 采用上游直线长度");
        var helper = f.Create(0x18D6, m.Arena.Center);
        var ruin = Component<GenericAOEs>(m, "RevoltingRuin");
        f.StartCast(helper, 10530, new(3, 0), 4);
        check(ruin.ActiveAOEs(0, f.Player).Length == 0, "O8 上游阈值忽略五米内传送");
        f.StartCast(helper, 10530, new(6, 0), 4);
        check(ruin.ActiveAOEs(0, f.Player).Length == 1 && ruin.ActiveAOEs(0, f.Player)[0].Origin == new WPos(6, 0), "O8 超过五米按传送落点预测脱逃");
        var hints = new AIHints();
        ruin.AddAIHints(0, f.Player, default, hints);
        check(hints.ForbiddenZones.Count == 1 && hints.GoalZones.Count == 1 && hints.GoalZones[0](new(6, -4)) == 10, "O8 采用上游身后安全站位引导");
        f.Event(m.PrimaryActor, 10539, m.Arena.Center);
        check(ruin.ActiveAOEs(0, f.Player).Length == 0, "O8 圣母颂分支清理脱逃预测");
    }

    private static T Component<T>(BossModule module, string name) where T : BossComponent
        => (T)module.Components.Single(c => c.GetType().Name == name);

    private sealed class Fixture : IDisposable
    {
        public readonly WorldState World = new(10000000, "upstream-omega-20261024-test");
        public readonly Actor Player;
        private ulong _nextID = 1;

        public Fixture()
        {
            World.Frame = new(DateTime.UnixEpoch.AddHours(1), 0, 0, 0, 0, 1);
            Player = Create(0, default, ActorType.Player, Class.PLD);
            World.Execute(new PartyState.OpModify(0, new(1, Player.InstanceID, false, "player")));
        }

        public Actor Create(uint oid, WPos position, ActorType type = ActorType.Enemy, Class cls = Class.None, uint nameID = 0)
        {
            var id = _nextID++;
            World.Execute(new ActorState.OpCreate(id, oid, (int)id * 2, 0, "fixture", nameID, type, cls, 100,
                new(position.X, 0, position.Z, 0), 0.5f, new(1000, 1000, 0, 10000, 10000), true, type == ActorType.Player, 0, 0, 0));
            return World.Actors.Find(id)!;
        }

        public BossModule Module(uint oid, uint nameID = 0)
        {
            var boss = Create(oid, default, nameID: nameID);
            var module = BossModuleRegistry.CreateModuleForActor(World, boss, BossModuleInfo.Maturity.Contributed)
                ?? throw new InvalidOperationException($"未注册欧米茄主 OID {oid:X}");
            boss.PosRot = new(module.Arena.Center.X, 0, module.Arena.Center.Z, 0);
            Player.PosRot = boss.PosRot;
            return module;
        }

        public ActorCastInfo StartCast(Actor caster, uint aid, WPos location, float duration)
        {
            World.Execute(new ActorState.OpCastInfo(caster.InstanceID, new ActorCastInfo
            {
                Action = new(ActionType.Spell, aid), TargetID = caster.InstanceID,
                Location = new(location.X, 0, location.Z), TotalTime = duration
            }));
            return caster.CastInfo!;
        }

        public void FinishCast(Actor caster) => World.Execute(new ActorState.OpCastInfo(caster.InstanceID, null));
        public void Status(int index, ActorStatus status) => World.Execute(new ActorState.OpStatus(Player.InstanceID, index, status));
        public void Event(Actor caster, uint aid, WPos position) => World.Execute(new ActorState.OpCastEvent(caster.InstanceID,
            new(new(ActionType.Spell, aid), Player.InstanceID, 0, 0, new(position.X, 0, position.Z), 0, 0, default)));
        public void Dispose() { }
    }
}
