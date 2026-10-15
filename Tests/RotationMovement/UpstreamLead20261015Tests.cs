using System.Numerics;
using System.Reflection;
using BossMod;
using BossMod.Autorotation;
using BossMod.Components;
using ClassShared = BossMod.ClassShared;
using BossMod.Dawntrail.Trial.T04Zelenia;
using BossMod.Stormblood.Ultimate.UCOB;
using Role = BossMod.PartyRolesConfig.Assignment;

static class UpstreamLead20261015Tests
{
    const BindingFlags All = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
    static Type Type(string name) => typeof(AIHints).Assembly.GetType(name, true)!;
    static BossComponent Component(BossModule module, string name) => (BossComponent)Activator.CreateInstance(Type(name), All, null, [module], null)!;
    static ActionID ZAction(string name) => new(ActionType.Spell, (uint)Enum.Parse(Type("BossMod.Dawntrail.Trial.T04Zelenia.AID"), name));

    public static void Run(Action<bool, string> check)
    {
        Zelenia(check);
        BismarckAndOverlappingStacks(check);
        TwisterAndDash(check);
        Timeline(check);
        CountdownAndPlan(check);
        Mitigation(check);
    }

    static void Zelenia(Action<bool, string> check)
    {
        var f = new Fixture((uint)BossMod.Dawntrail.Trial.T04Zelenia.OID.Boss);
        using var module = new T04Zelenia(f.World, f.Boss);
        var aoes = (GenericAOEs)Component(module, "BossMod.Dawntrail.Trial.T04Zelenia.AlexandrianThunderIII");
        aoes.OnMapEffect(4, 0x00400100);
        aoes.OnMapEffect(5, 0x00400100);
        var a = f.Create(30, 0, ActorType.Enemy, new(100, 85));
        var b = f.Create(31, 0, ActorType.Enemy, new(113, 92.5f));
        var castA = new ActorCastInfo { Action = ZAction("AlexandrianThunderIII1"), Location = a.Position.ToVec3(), TotalTime = 4 };
        var castB = new ActorCastInfo { Action = ZAction("AlexandrianThunderIII2"), Location = b.Position.ToVec3(), TotalTime = 4 };
        aoes.OnCastStarted(a, castA);
        aoes.OnCastStarted(b, castB);
        for (var i = 0; i < 8; ++i)
        {
            var shown = aoes.ActiveAOEs(0, f.Player).ToArray();
            check(shown.Count(z => z.Shape is AOEShapeCircle) == 2 && shown.Count(z => z.Shape is AOEShapeCone) == 2,
                "普通泽莱尼亚：相邻连通地板去重而两名施法者圆圈均保留");
        }
        aoes.OnCastFinished(a, castA);
        var remaining = aoes.ActiveAOEs(0, f.Player).ToArray();
        check(remaining.Count(z => z.Shape is AOEShapeCircle) == 1 && remaining.Count(z => z.Shape is AOEShapeCone) == 2,
            "普通泽莱尼亚：首个施法者结束不清除另一个施法者的重叠范围");
        aoes.OnCastFinished(b, castB);
        check(aoes.ActiveAOEs(0, f.Player).Length == 0, "普通泽莱尼亚：最后一个施法结束清空去重缓存");
        var next = new ActorCastInfo { Action = ZAction("AlexandrianThunderIVCircle2"), TotalTime = 4 };
        aoes.OnCastStarted(a, next);
        check(aoes.ActiveAOEs(0, f.Player).Length == 2, "普通泽莱尼亚：下一组地板仍正常预警");
        aoes.OnCastFinished(a, next);
        check(aoes.ActiveAOEs(0, f.Player).Length == 2, "普通泽莱尼亚：双重结算首击保留范围");
        aoes.OnCastFinished(b, next);
        check(aoes.ActiveAOEs(0, f.Player).Length == 0, "普通泽莱尼亚：双重结算末击清理范围");
    }

    static void BismarckAndOverlappingStacks(Action<bool, string> check)
    {
        var f = new Fixture((uint)BossMod.Heavensward.Trial.T02Bismarck.OID.Boss);
        using (var module = new BossMod.Heavensward.Trial.T02Bismarck.T02Bismarck(f.World, f.Boss))
        {
            var deck = Component(module, "BossMod.Heavensward.Trial.T02Bismarck.DeckArena");
            deck.Update();
            check(module.Arena.Center == ((ArenaBoundsCustom)module.Arena.Bounds).Center
                && module.Arena.InBounds(new(-27, 4)) && !module.Arena.InBounds(new(5, -5)),
                "俾斯麦：甲板世界顶点不因边界自动重心发生偏移");
            var gun = f.Create(10, 0x12EB, ActorType.Enemy, default);
            gun.Tether = new(39, f.Boss.InstanceID);
            deck.Update();
            check(module.Arena.Center == ((ArenaBoundsCustom)module.Arena.Bounds).Center
                && module.Arena.InBounds(new(-27, 4)) && module.Arena.InBounds(new(5, -5)),
                "俾斯麦：龙杀者连接后甲壳扩展与绘图中心同步");
            gun.Tether = default;
            deck.Update();
            check(module.Arena.InBounds(new(-27, 4)) && !module.Arena.InBounds(new(5, -5)),
                "俾斯麦：连接解除后收回甲壳且甲板原位保持");
        }
        var world = f.World;
        f.Player.PosRot = new(100, 0, 100, 0);
        var players = new[] { f.Player, f.Create(20, 0, ActorType.Player, new(100, 100)), f.Create(21, 0, ActorType.Player, new(101, 100)), f.Create(22, 0, ActorType.Player, new(100, 101)) };
        for (var i = 1; i < players.Length; ++i)
            world.Execute(new PartyState.OpModify(i, new((ulong)i + 1, players[i].InstanceID, false, "player")));
        var edenBoss = f.Create(40, (uint)BossMod.Shadowbringers.Raid.E12EdensPromise.OID.Boss, ActorType.Enemy, new(100, 100));
        using var eden = new BossMod.Shadowbringers.Raid.E12EdensPromise.E12EdensPromise(world, edenBoss);
        var stacks = (UniformStackSpread)Component(eden, "BossMod.Shadowbringers.Raid.E12EdensPromise.ClassicalSculptureStack");
        f.Create(30, 0x3097, ActorType.Enemy, new(112, 100));
        stacks.OnEventIcon(players[0], 62, players[0].InstanceID);
        stacks.OnEventIcon(players[1], 62, players[1].InstanceID);
        check(stacks.Stacks.Count == 2, "伊甸12：珍贵记忆存在时按各自图标添加两处分摊");
        var textHints = new BossComponent.TextHints();
        stacks.AddHints(0, f.Player, textHints);
        check(!textHints.Any(h => h.Item2), "伊甸12：人数满足的同位分摊允许重叠而不误报危险");
        var hints = new AIHints();
        stacks.AddAIHints(0, f.Player, Role.MT, hints);
        check(!hints.ForbiddenZones.Any(z => z.shapeDistance.Contains(f.Player.Position))
            && hints.ForbiddenZones.Any(z => z.shapeDistance.Contains(new(112, 100))),
            "伊甸12：重叠分摊点可用且珍贵记忆仍被避让");
    }

    static void TwisterAndDash(Action<bool, string> check)
    {
        var f = new Fixture((uint)BossMod.Stormblood.Ultimate.UCOB.OID.Twintania);
        using var module = new UCOB(f.World, f.Boss);
        var twister = (CastTwister)Component(module, "BossMod.Stormblood.Ultimate.UCOB.P3Twister");
        var cast = new ActorCastInfo { Action = ActionID.MakeSpell(BossMod.Stormblood.Ultimate.UCOB.AID.TwistingDive), TotalTime = 3 };
        var start = f.World.CurrentTime;
        twister.OnCastStarted(f.Boss, cast);
        void At(double seconds) { f.World.Frame = new(start.AddSeconds(seconds), 0, 0, 0, 0, 1); twister.Update(); }
        var hints = new AIHints();
        At(1);
        twister.AddAIHints(0, f.Player, Role.MT, hints);
        check(hints.ForcedMovement == null && !hints.ForbidDashes, "P3旋风：长读条前段不提前冻结移动");
        hints.Clear(); At(2.3);
        twister.AddAIHints(0, f.Player, Role.MT, hints);
        check(hints.ForcedMovement == Vector3.Zero && hints.MaxCastTime == 0 && hints.ForceCancelCast && hints.ForbidDashes,
            "P3旋风：按预测前窗口冻结移动、读条与冲刺");
        hints.Clear(); At(3.91);
        twister.AddAIHints(0, f.Player, Role.MT, hints);
        check(twister.ActiveAOEs(0, f.Player).Length == 1 && hints.ForcedMovement == null && hints.ForbidDashes && hints.Voidzones.Count == 1,
            "P3旋风：预测生成后释放走位但圈内禁止冲刺");
        f.Player.PosRot = new(5, 0, 5, 0);
        hints.Clear(); twister.AddAIHints(0, f.Player, Role.MT, hints);
        check(!hints.ForbidDashes && hints.Voidzones.Count == 1, "P3旋风：圈外可冲刺但保留轨迹障碍");
        hints.Clear();
        hints.PathfindMapBounds = new ArenaBoundsCircle(30);
        hints.Voidzones.Add((default, 2));
        check(ActionDefinitions.IsDashDangerous(new(-5, 0), new(5, 0), hints), "冲刺：起终点安全也不能横穿旋风圆");
        check(ActionDefinitions.IsDashDangerous(new(-5, 0), new(-1, 0), hints), "冲刺：进入圆但尚未穿出也须拦截");
        check(!ActionDefinitions.IsDashDangerous(default, new(5, 0), hints), "冲刺：已经在危险圆内时允许向外脱离");
        check(!ActionDefinitions.IsDashDangerous(new(-5, 5), new(5, 5), hints), "冲刺：不相交轨迹正常通过");
        hints.ForbidDashes = true;
        check(ActionDefinitions.IsDashDangerous(new(-5, 5), new(5, 5), hints), "冲刺：机制禁位移优先于几何安全");
        hints.Clear();
        check(hints.Voidzones.Count == 0 && !hints.ForbidDashes, "冲刺：逐帧清理障碍和机制限制");
    }

    static void Timeline(Action<bool, string> check)
    {
        var f = new Fixture((uint)BossMod.Dawntrail.Trial.T04Zelenia.OID.Boss);
        using var module = new T04Zelenia(f.World, f.Boss);
        var tankbuster = new StateMachine.State { ID = 2, Duration = 3, EndHint = StateMachine.StateHint.Tankbuster };
        var raidwide = new StateMachine.State { ID = 1, Duration = 5, EndHint = StateMachine.StateHint.Raidwide, NextStates = [tankbuster] };
        module.StateMachine.Phases.Clear();
        module.StateMachine.Phases.Add(new(raidwide, "离线时间轴"));
        module.StateMachine.Start(f.World.CurrentTime);
        var events = StateTimeline.Upcoming(module, f.World).ToArray();
        check(events.Length == 2 && events[0].at == f.World.FutureTime(5) && events[1].at == f.World.FutureTime(8), "时间轴：累加当前团伤与后继死刑时间");
        var hints = new AIHints();
        hints.AddPredictedDamage(BitMask.Build(0), f.World.FutureTime(5.2f), AIHints.PredictedDamageType.Raidwide);
        check(StateTimeline.Raidwides(module, f.World, hints).Count() == 1, "时间轴：真实伤害预测与相邻估计去重");
        f.Boss.TargetID = f.Player.InstanceID;
        check(StateTimeline.Tankbusters(module, f.World, hints).Single().target == f.Player, "时间轴：死刑回退到首领当前队伍目标");
        f.Boss.TargetID = 0;
        check(!StateTimeline.Tankbusters(module, f.World, hints).Any(), "时间轴：无队伍目标时不猜测死刑受击人");
        raidwide.NextStates = [tankbuster, new()];
        check(StateTimeline.Upcoming(module, f.World).Count() == 1, "时间轴：遇未知分支停止推算");
        f.World.Frame = new(f.World.FutureTime(8.2f), 0, 0, 0, 0, 1);
        module.StateMachine.Update(f.World.CurrentTime);
        check(!StateTimeline.Upcoming(module, f.World).Any(), "时间轴：严重超时的血量/加怪等待不生成虚假倒计时");
    }

    static void CountdownAndPlan(Action<bool, string> check)
    {
        var f = new Fixture();
        f.Player.Class = Class.BLM;
        var config = Service.Config.Get<ActionTweaksConfig>();
        var old = config.CountdownHold;
        var action = ActionID.MakeSpell(ClassShared.AID.Addle);
        var queue = new ActionQueue();
        var hints = new AIHints();
        ActionID Best() => queue.FindBest(f.World, f.Player, f.World.Client.Cooldowns, 0, hints, 0, true).Action;
        try
        {
            config.CountdownHold = true;
            f.World.Client.CountdownRemaining = 10;
            queue.Push(action, f.Boss, 3000, manual: true);
            check(Best() == default, "倒计时：手动排队攻击按命中时刻延后");
            f.World.Client.CountdownRemaining = 0;
            check(Best() == action, "倒计时：归零后释放手动攻击");
            f.World.Client.CountdownRemaining = 10;
            config.CountdownHold = false;
            check(Best() == action, "倒计时：关闭选项后保持原有手动队列行为");
            config.CountdownHold = true;
            queue.Clear(); queue.Push(action, f.Boss, 3000);
            check(Best() == action, "倒计时：选项不延误自动循环自身调度");
        }
        finally { config.CountdownHold = old; }
        var path = Path.Combine(Path.GetTempPath(), "BossModPlan20261015-" + Guid.NewGuid().ToString("N"));
        var plans = new PlanDatabase(path);
        var original = new Plan("原计划", typeof(T04Zelenia)) { Guid = Guid.NewGuid().ToString(), Class = Class.PLD, Level = 100 };
        plans.ModifyPlan(null, original);
        var updated = original.MakeClone(); updated.Name = "修改后";
        plans.ModifyPlan(original.MakeClone(), updated);
        check(plans.GetPlans(typeof(T04Zelenia), Class.PLD).Plans.Single().Name == "修改后", "计划：独立克隆引用通过稳定Guid替换而不发生负索引");
        plans.ModifyPlan(updated, null);
    }

    static void Mitigation(Action<bool, string> check)
    {
        var f = new Fixture();
        f.Player.Class = Class.BLM; f.Player.InCombat = true; f.Boss.InCombat = true;
        var path = Path.Combine(Path.GetTempPath(), "BossModMitigation20261015-" + Guid.NewGuid().ToString("N"));
        var db = new RotationDatabase(new DirectoryInfo(path), new FileInfo(Path.Combine(path, "defaults.json")));
        using var bossmods = new BossModuleManager(f.World);
        var hints = new AIHints();
        using var manager = new RotationModuleManager(db, bossmods, hints);
        Actor? target = f.Boss;
        void Damage(float seconds) { hints.Clear(); hints.AddPredictedDamage(BitMask.Build(0), f.World.FutureTime(seconds), AIHints.PredictedDamageType.Raidwide); }
        var caster = new BossMod.Autorotation.xan.Caster(manager, f.Player);
        var cs = new BossMod.Autorotation.xan.Caster.Strategy { Addle = new(DisabledByDefault.Enabled, new StrategyValueTrack(), 0) };
        Damage(4); caster.Execute(in cs, ref target, 0, false);
        check(hints.ActionsToExecute.Entries.Any(e => e.Action == ActionID.MakeSpell(ClassShared.AID.Addle)), "法系减伤：五秒内团伤且目标无昏乱时排队");
        f.Boss.Statuses[0] = new((uint)ClassShared.SID.Addle, 0, f.World.FutureTime(10), f.Player.InstanceID);
        Damage(4); caster.Execute(in cs, ref target, 0, false);
        check(!hints.ActionsToExecute.Entries.Any(e => e.Action == ActionID.MakeSpell(ClassShared.AID.Addle)), "法系减伤：已有昏乱不重复施加");
        f.Boss.Statuses[0] = default;
        Damage(6); caster.Execute(in cs, ref target, 0, false);
        check(hints.ActionsToExecute.Entries.Count == 0, "法系减伤：远期团伤不提前浪费昏乱");
        cs.Addle = new(DisabledByDefault.Disabled, new StrategyValueTrack(), 0);
        Damage(4); caster.Execute(in cs, ref target, 0, false);
        check(hints.ActionsToExecute.Entries.Count == 0, "法系减伤：默认关闭策略不自动使用");
        f.Player.Class = Class.DRG;
        var melee = new BossMod.Autorotation.xan.MeleeAI(manager, f.Player);
        var ms = new BossMod.Autorotation.xan.MeleeAI.Strategy { Feint = new(DisabledByDefault.Enabled, new StrategyValueTrack(), 0) };
        Damage(4); melee.Execute(in ms, ref target, 0, false);
        check(hints.ActionsToExecute.Entries.Any(e => e.Action == ActionID.MakeSpell(ClassShared.AID.Feint)), "近战减伤：团伤前使用牵制");
        f.Player.Class = Class.MCH;
        var ranged = new BossMod.Autorotation.xan.RangedAI(manager, f.Player);
        var rs = new BossMod.Autorotation.xan.RangedAI.Strategy { PartyMit = new(DisabledByDefault.Enabled, new StrategyValueTrack(), 0) };
        Damage(4); ranged.Execute(in rs, ref target, 0, false);
        check(hints.ActionsToExecute.Entries.Any(e => e.Action == ActionID.MakeSpell(BossMod.MCH.AID.Tactician)), "远敏减伤：团伤前选择本职业团队减伤");
        f.Player.Statuses[0] = new((uint)BossMod.BRD.SID.Troubadour, 0, f.World.FutureTime(30), f.Player.InstanceID);
        Damage(4); ranged.Execute(in rs, ref target, 0, false);
        check(!hints.ActionsToExecute.Entries.Any(e => e.Action == ActionID.MakeSpell(BossMod.MCH.AID.Tactician)), "远敏减伤：其他远敏同类增益生效时不叠用");
    }

    sealed class Fixture
    {
        public readonly WorldState World = new(10000000, "upstream-20261015");
        public readonly Actor Player;
        public readonly Actor Boss;
        public Fixture(uint bossOID = 0xffff9919)
        {
            Array.Fill(World.Client.ClassJobLevels, (short)100);
            World.Frame = new(DateTime.UnixEpoch.AddHours(1), 0, 0, 0, 0, 1);
            Player = Create(1, 0, ActorType.Player, new(3, 0));
            World.Execute(new PartyState.OpModify(0, new(1, 1, false, "player")));
            Boss = Create(2, bossOID, ActorType.Enemy, default);
        }
        public Actor Create(ulong id, uint oid, ActorType type, WPos pos)
        {
            World.Execute(new ActorState.OpCreate(id, oid, (int)id * 2, 0, "fixture", 0, type, type == ActorType.Player ? Class.PLD : Class.None, 100,
                new(pos.X, 0, pos.Z, 0), .5f, new(1000, 1000, 0, 10000, 10000), true, type == ActorType.Player, 0, 0, 0));
            return World.Actors.Find(id)!;
        }
    }
}
