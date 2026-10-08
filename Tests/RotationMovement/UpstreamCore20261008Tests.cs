using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BossMod;
using BossMod.Autorotation;
using BossMod.Components;
using XanRDM = BossMod.Autorotation.xan.RDM;

static class UpstreamCore20261008Tests
{
    const BindingFlags All = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

    public static void Run(RotationModuleManager manager, Actor player, Actor boss, AIHints hints, Action<bool, string> check)
    {
        Alliance(check);
        BaitGeometry(check);
        TargetImport(check);
        PullGoals(check);
        RDM(manager, player, boss, hints, check);
        ScriptedDeath(check);
        InstanceLoading(check);
    }

    static unsafe void Alliance(Action<bool, string> check)
    {
        var sync = typeof(AIHints).Assembly.GetType("BossMod.WorldStateGameSync", true)!;
        var read = sync.GetMethod("ReadAllianceLetter", All)!;
        var group = read.GetParameters()[0].ParameterType.GetElementType()!;
        check(group.StructLayoutAttribute!.Size == 0x7FF0 && Marshal.OffsetOf(group, "PartyId").ToInt32() == 0x7FC8
            && Marshal.OffsetOf(group, "AllianceFlags").ToInt32() == 0x7FE1, "联盟读取与现有SDK的Group结构布局一致");
        byte* buffer = stackalloc byte[0x7FF0];
        new Span<byte>(buffer, 0x7FF0).Clear();
        foreach (byte flags in new byte[] { 0, 1, 3 })
        foreach (byte index in new byte[] { 0, 1, 2, 3, 4, 5, 6, 255 })
        {
            buffer[0x7FE1] = flags;
            buffer[0x7FC0] = index;
            var expected = flags == 0 || index >= (flags == 3 ? 6 : 3) ? AllianceLetter.None : (AllianceLetter)index;
            var actual = (AllianceLetter)read.Invoke(null, [Pointer.Box(buffer, group.MakePointerType())])!;
            check(actual == expected, $"联盟读取区分普通、小队及非法字母：{flags}/{index}");
        }

        var f = new Fixture();
        f.World.Execute(new PartyState.OpAllianceChange(AllianceLetter.C));
        check(f.World.Party.CompareToInitial().OfType<PartyState.OpAllianceChange>().Single().Alliance == AllianceLetter.C, "录制初始快照保存当前联盟字母");
        var directory = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "BossModAlliance-" + Guid.NewGuid().ToString("N")));
        foreach (var format in new[] { ReplayLogFormat.TextCondensed, ReplayLogFormat.BinaryUncompressed })
        {
            string path;
            using (var recorder = new ReplayRecorder(f.World, format, false, directory, format.ToString()))
            {
                path = recorder.LogPath;
                f.World.Execute(new PartyState.OpAllianceChange(AllianceLetter.B));
                f.World.Execute(new PartyState.OpAllianceChange(AllianceLetter.None));
            }
            float progress = 0;
            var replay = ReplayParserLog.Parse(path, ref progress, default);
            var letters = replay.Ops.OfType<PartyState.OpAllianceChange>().Select(o => o.Alliance).ToArray();
            check(letters.SequenceEqual(new[] { AllianceLetter.B, AllianceLetter.None }), "ALGI事件往返保留队伍变化与离队：" + format);
            File.Delete(path);
        }
        directory.Delete();
    }

    static void BaitGeometry(Action<bool, string> check)
    {
        var f = new Fixture();
        var other = f.Create(3, 0, ActorType.Player, new(0, 2));
        f.World.Execute(new PartyState.OpModify(1, new(3, 3, false, "other")));
        using var module = new EntryProbe(f.World, f.Boss);
        var bait = new GenericBaitAway(module);
        bait.CurrentBaits.Add(new(f.Boss, f.Player, new AOEShapeRect(20, 6), f.World.FutureTime(3)));
        var hints = new AIHints();
        bait.AddAIHints(0, f.Player, default, hints);
        check(hints.ForbiddenZones.Count == 1 && new WPos[] { new(0, 10), new(10, 0), new(0, -10) }
            .All(p => float.IsFinite(hints.ForbiddenZones[0].shapeDistance.Distance(p))), "矩形诱导队员距施法源小于半宽时不产生NaN禁区");

        var cleave = new Cleave(module, 0, new AOEShapeRect(12, 6)) { NextExpected = f.World.FutureTime(4) };
        f.Boss.TargetID = f.Player.InstanceID;
        hints.Clear();
        cleave.AddAIHints(0, f.Player, default, hints);
        check(hints.ForbiddenZones.Count == 1 && hints.ForbiddenZones[0].activation == cleave.NextExpected
            && float.IsFinite(hints.ForbiddenZones[0].shapeDistance.Distance(new(0, 8)))
            && hints.ForbiddenZones[0].shapeDistance.Distance(new(0, 20)) > 0, "近距离顺劈禁区保留激活时刻和实际长度");

        other.PosRot = new(0, 0, 10, 0);
        bait.CurrentBaits.Clear();
        bait.CurrentBaits.Add(new(f.Boss, f.Player, new AOEShapeCone(20, 20.Degrees(), 90.Degrees()), f.World.FutureTime(2)));
        hints.Clear();
        bait.AddAIHints(0, f.Player, default, hints);
        check(hints.ForbiddenZones[0].shapeDistance.Contains(new(-10, 0)) && !hints.ForbiddenZones[0].shapeDistance.Contains(new(0, 10)), "带方向偏移的扇形诱导按实际攻击方向限制站位");

        other.IsDead = true;
        var pet = f.Create(4, 0, ActorType.Pet, new(0, 1));
        pet.HitboxRadius = 2;
        f.World.Execute(new PartyState.OpModify(24, new(0, 4, false, "pet")));
        hints.Clear();
        bait.AddAIHints(0, f.Player, default, hints);
        check(hints.ForbiddenZones.Count == 0, "诱导默认忽略宠物判定");
        bait.AllowPetTargets = true;
        bait.AddAIHints(0, f.Player, default, hints);
        check(hints.ForbiddenZones.Count == 1 && hints.ForbiddenZones[0].shapeDistance.Contains(new(0, -10)), "显式宠物诱导考虑覆盖源头的宠物判定圈");

        var hidden = f.Create(5, 0xffff2300, ActorType.Enemy, default);
        hidden.IsTargetable = false;
        var adds = new Adds(module, hidden.OID);
        var visible = new Adds(module, hidden.OID, allowUntargetable: true);
        var multi = new AddsMulti(module, [hidden.OID], allowUntargetable: true);
        check(adds.ActiveActorsCount == 0 && visible.ActiveActorsCount == 1 && multi.ActiveActors.Count == 1, "可选不可选中小怪计数保持默认过滤");
        hidden.IsDead = true;
        check(visible.ActiveActorsCount == 0 && multi.ActiveActorsCount == 0, "不可选中选项仍排除死亡小怪");
    }

    static void TargetImport(Action<bool, string> check)
    {
        var f = new Fixture();
        using var module = new EntryProbe(f.World, f.Boss);
        var tree = new StateMachineTree(module.StateMachine);
        var id = tree.Nodes.Keys.First();
        Plan MakePlan(float time) => new("导入", typeof(EntryProbe))
        {
            Class = Class.PLD,
            Targeting = [new(new StrategyValueTrack { Target = StrategyTarget.Automatic }) { StateID = id, TimeSinceActivation = time, WindowLength = 5 }]
        };
        var columns = new CooldownPlannerColumns(MakePlan(1), new Timeline(), tree, [0], false, [], f.World.CurrentTime);
        columns.Plan = MakePlan(2);
        columns.SyncCreateImport();
        columns.SyncCreateImport();
        check(columns.Plan.Targeting.Count == 1 && columns.Plan.Targeting[0].TimeSinceActivation == 2, "计划重新导入与重复刷新不因旧列清理丢失目标覆盖");
        var targetColumn = (ColumnPlannerTrackTarget)typeof(CooldownPlannerColumns).GetField("_colTarget", All)!.GetValue(columns)!;
        check(targetColumn.Elements.Count == 1 && targetColumn.Elements[0].Window.Delay == 2, "目标覆盖保存数据与重复导入后的显示列一致");
    }

    static void PullGoals(Action<bool, string> check)
    {
        var f = new Fixture();
        f.Boss.HitboxRadius = 2;
        f.Player.HitboxRadius = 0.5f;
        f.Player.TargetID = f.Boss.InstanceID;
        var hints = new AIHints();
        hints.Enemies[f.Boss.CharacterSpawnIndex] = new AIHints.Enemy(f.Boss, 0, true) { TankDistance = 0 };
        var goal = new WPos(0, 10);
        var shortened = hints.PullTargetToLocation(f.Boss, goal, f.Player, 0.2f, greed: true);
        var full = hints.PullTargetToLocation(f.Boss, goal, f.Player, 0.2f, greed: false);
        check(shortened(new(0, 5.4f)) > 0 && full(new(0, 12.5f)) > 0 && full(new(0, 5.4f)) == 0, "拉怪只在允许攻击贪刀时收短距离并预留0.1码");
        f.Player.TargetID = 0;
        check(hints.PullTargetToLocation(f.Boss, goal, f.Player, 0.2f)(new(0, 12.5f)) > 0, "当前没有选中被拉敌人时完成移动目标");
        check(new AIHints.Enemy(new(10, 0x4C95, 2, 0, "", 0, ActorType.Enemy, Class.None, 1, default), 0, false).TankDistance == 30
            && new AIHints.Enemy(new(11, 0x1FDF, 4, 0, "", 0, ActorType.Enemy, Class.None, 1, default), 0, false).TankDistance == 0, "棋盘拉怪距离与绝巴哈零距离规则同时保留");
    }

    static void RDM(RotationModuleManager manager, Actor player, Actor boss, AIHints hints, Action<bool, string> check)
    {
        var world = manager.WorldState;
        var oldClass = player.Class;
        var oldLevel = player.Level;
        var oldCombo = world.Client.ComboState;
        var oldCD = world.Client.Cooldowns[ActionDefinitions.GCDGroup];
        var oldUnlock = ActionDefinitions.Instance.UnlockCheck;
        var def = ActionDefinitions.Instance[ActionID.MakeSpell(BossMod.RDM.AID.CorpsACorps)]!;
        var oldCast = def.CastTime;
        try
        {
            player.Class = Class.RDM;
            player.Level = 100;
            ActionDefinitions.Instance.UnlockCheck = _ => true;
            var rdm = new XanRDM(manager, player);
            var enemy = new AIHints.Enemy(boss, 0, false);
            typeof(XanRDM).GetField("BestAOETarget", All)!.SetValue(rdm, enemy);
            typeof(XanRDM).GetField("BestLineTarget", All)!.SetValue(rdm, enemy);
            rdm.NumLineTargets = 2;
            void Queue(BossMod.RDM.AID last)
            {
                hints.Clear();
                world.Client.ComboState = new(action: (uint)last, remaining: 15);
                typeof(XanRDM).GetMethod("DoGCD", All)!.Invoke(rdm, [new XanRDM.Strategy(), enemy, 50]);
            }
            foreach (var finisher in new[] { BossMod.RDM.AID.Verflare, BossMod.RDM.AID.Verholy })
            {
                Queue(finisher);
                check(hints.ActionsToExecute.Entries.Any(e => e.Action == ActionID.MakeSpell(BossMod.RDM.AID.Scorch))
                    && !hints.ActionsToExecute.Entries.Any(e => e.Action == ActionID.MakeSpell(BossMod.RDM.AID.Resolution)), "赤魔终结技后先排焦热：" + finisher);
            }
            Queue(BossMod.RDM.AID.Scorch);
            check(hints.ActionsToExecute.Entries.Any(e => e.Action == ActionID.MakeSpell(BossMod.RDM.AID.Resolution) && e.FacingAngle != null), "焦热后多目标决断带直线方向");
            rdm.Stacks = 3;
            rdm.BlackMana = 80;
            rdm.WhiteMana = 20;
            player.Level = 69;
            Queue(BossMod.RDM.AID.None);
            check(hints.ActionsToExecute.Entries.Any(e => e.Action == ActionID.MakeSpell(BossMod.RDM.AID.Verflare)), "69级尚未解锁赤神圣时使用赤核爆");
            player.Level = 67;
            check(!(bool)typeof(XanRDM).GetProperty("InRangedCombo", All)!.GetValue(rdm)!, "赤核爆未解锁时三层不误判远程连段");

            player.Level = 100;
            world.Client.Cooldowns[ActionDefinitions.GCDGroup] = new(0, 1);
            def.CastTime = 1.5f;
            check(!rdm.CanWeave(BossMod.RDM.AID.CorpsACorps), "有读条的非GCD完整占用时间超出插入窗口时不排入");

            var definitions = (Dictionary<ActionID, ActionDefinition>)typeof(ActionDefinitions).GetField("_definitions", All)!.GetValue(ActionDefinitions.Instance)!;
            var high = new ActionID(ActionType.Spell, 0xffff2301);
            var low = new ActionID(ActionType.Spell, 0xffff2302);
            definitions.Add(high, new(high) { Range = 0 });
            definitions.Add(low, new(low) { Range = 0, CastTime = 0.8f, CastAnimLock = 0.1f });
            try
            {
                var queue = new ActionQueue();
                queue.Push(high, player, 5000, delay: 1);
                queue.Push(low, player, 2000);
                hints.Clear();
                check(queue.FindBest(world, player, world.Client.Cooldowns, 0, hints, 0.2f, false).Action == high,
                    "动作队列将延迟计入非瞬发占用，避免拖延更高优先级动作");
            }
            finally { definitions.Remove(high); definitions.Remove(low); }
        }
        finally
        {
            def.CastTime = oldCast;
            ActionDefinitions.Instance.UnlockCheck = oldUnlock;
            player.Class = oldClass;
            player.Level = oldLevel;
            world.Client.ComboState = oldCombo;
            world.Client.Cooldowns[ActionDefinitions.GCDGroup] = oldCD;
            hints.Clear();
        }
    }

    static void ScriptedDeath(Action<bool, string> check)
    {
        var f = new Fixture();
        using var modules = new BossModuleManager(f.World);
        var hints = new AIHints();
        var temp = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "BossModDeath-" + Guid.NewGuid().ToString("N")));
        using var manager = new RotationModuleManager(new RotationDatabase(temp, new FileInfo(Path.Combine(temp.FullName, "empty.json"))), modules, hints);
        var output = new Preset("职业");
        output.AddModule(typeof(OutputProbe), new("probe", "", "", "", RotationModuleQuality.Good, new(~0ul), 100), (m, a) => new OutputProbe(m, a));
        var movement = new Preset("移动");
        movement.AddModule(typeof(BossMod.Autorotation.MiscAI.NormalMovement), BossMod.Autorotation.MiscAI.NormalMovement.Definition(), (m, a) => new BossMod.Autorotation.MiscAI.NormalMovement(m, a));
        var old = manager.Config.ClearPresetOnDeath;
        try
        {
            manager.Config.ClearPresetOnDeath = true;
            manager.Activate(output);
            manager.Activate(movement);
            manager.Update(0, false, false);
            f.Player.InCombat = f.Player.IsDead = true;
            hints.ScriptedDeath = true;
            typeof(RotationModuleManager).GetMethod("OnDeadChanged", All)!.Invoke(manager, [f.Player]);
            check(!manager.IsForceDisabled && manager.Presets.Count == 2, "剧情必死时保留职业与移动双预设");
            hints.Clear();
            typeof(RotationModuleManager).GetMethod("OnDeadChanged", All)!.Invoke(manager, [f.Player]);
            check(!hints.ScriptedDeath && manager.IsForceDisabled, "必死标记每帧清零，普通死亡仍执行总停止保护");
        }
        finally { manager.Config.ClearPresetOnDeath = old; }
    }

    static void InstanceLoading(Action<bool, string> check)
    {
        var f = new Fixture();
        var config = BossModuleManager.Config;
        var old = config.MinMaturity;
        try
        {
            config.MinMaturity = BossModuleInfo.Maturity.WIP;
            using var manager = new BossModuleManager(f.World);
            manager.Update();
            var module = manager.LoadedModules.Single(m => m.PrimaryActor == f.Boss);
            module.StateMachine.Start(f.World.CurrentTime);
            f.World.CurrentCFCID = 995;
            f.Player.PosRot = new(1000, 0, 0, 0);
            manager.Update();
            check(manager.LoadedModules.Contains(module) && !manager.PendingModules.Contains(module), "副本中远距监狱平台不将正在战斗模块转为等待");
            f.World.CurrentCFCID = 0;
            manager.Update();
            check(manager.PendingModules.Contains(module), "开放区域仍保留远距模块卸载规则");
        }
        finally { config.MinMaturity = old; }
    }

    sealed class Fixture
    {
        public readonly WorldState World = new(10000000, "core-20261008");
        public readonly Actor Player;
        public readonly Actor Boss;

        public Fixture()
        {
            World.Frame = new(DateTime.UnixEpoch.AddHours(1), 0, 0, 0, 0, 1);
            Player = Create(1, 0, ActorType.Player, new(5, 0));
            World.Execute(new PartyState.OpModify(0, new(1, 1, false, "player")));
            Boss = Create(2, 0xffff2101, ActorType.Enemy, default);
        }

        public Actor Create(ulong id, uint oid, ActorType type, WPos position)
        {
            World.Execute(new ActorState.OpCreate(id, oid, (int)id * 2, 0, "fixture", 0, type, type == ActorType.Player ? Class.PLD : Class.None, 100,
                new(position.X, 0, position.Z, 0), 0.5f, new(1000, 1000, 0, 10000, 10000), true, type is ActorType.Player or ActorType.Pet, 0, 0, 0));
            return World.Actors.Find(id)!;
        }
    }
}
