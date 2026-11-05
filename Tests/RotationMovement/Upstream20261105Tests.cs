using System.Reflection;
using System.Text.Json;
using BossMod;
using BossMod.Autorotation;
using BossMod.Autorotation.kage;
using BossMod.Components;
using UWU = BossMod.Stormblood.Ultimate.UWU;

static class Upstream20261105Tests
{
    const BindingFlags All = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    public static void Run(RotationModuleManager manager, Actor player, AIHints hints, Action<bool, string> check)
    {
        WhmUtility(manager, player, hints, check);
        AssignmentValidation(check);
        QueuePositionLock(check);
        RsrTargeting(check);
        NaStrategy(check);
        WorldData(check);
        LegacyPreset(check);
        MultiHit(check);
        DeepDungeon(check);
        UltimateSafety(check);
        GrandOctetLateActors(check);
    }

    static void WhmUtility(RotationModuleManager manager, Actor player, AIHints hints, Action<bool, string> check)
    {
        var saved = (player.Class, player.Level, player.HPMP, player.IsTargetable);
        try
        {
            player.Class = Class.WHM;
            player.Level = 100;
            player.HPMP = new(1000, 1000, 0, 10000, 10000);
            var definition = BossMod.Autorotation.Utility.ClassWHMUtilityEx.Definition();
            var strategy = new StrategyValues(definition.Configs);
            var heal = (StrategyValueTrack)strategy.Values[definition.Configs.FindIndex(c => c.InternalName == "Heal")];
            heal.Option = (int)BossMod.Autorotation.Utility.ClassWHMUtilityEx.HealStrategy.FullHeal;
            heal.Target = StrategyTarget.Self;
            heal.ExpireIn = 5;
            RotationModule utility = new BossMod.Autorotation.Utility.ClassWHMUtilityEx(manager, player);
            Actor? target = null;
            hints.Clear();
            utility.Execute(strategy, ref target, 0, false);
            check(hints.ActionsToExecute.Entries.Count == 0, "额外白魔治疗计划遇到满血目标正常返回");
            player.HPMP = new(100, 1000, 0, 10000, 10000);
            player.IsTargetable = false;
            utility.Execute(strategy, ref target, 0, false);
            check(hints.ActionsToExecute.Entries.Count == 0, "额外白魔治疗计划遇到空有效目标正常返回");
            player.IsTargetable = true;
            utility.Execute(strategy, ref target, 0, false);
            check(hints.ActionsToExecute.Entries.Count > 0, "额外白魔治疗计划仍为受伤目标生成治疗动作");
        }
        finally { (player.Class, player.Level, player.HPMP, player.IsTargetable) = saved; hints.Clear(); }
    }

    static void QueuePositionLock(Action<bool, string> check)
    {
        using var f = new Fixture();
        var action = ActionID.MakeSpell(BossMod.ClassShared.AID.Sprint);
        var queue = new ActionQueue();
        queue.Push(action, f.Player, 2000, forced: true, positionLock: 2);
        queue.Push(action, f.Player, 1000, forced: true);
        var hints = new AIHints { MaxCastTime = 2 };
        var best = queue.FindBest(f.World, f.Player, f.World.Client.Cooldowns, 0, hints, 0, true);
        check(best.Priority == 1000, "位置锁定动作超过有效移动窗口时选择可执行的低优先级动作");
        hints.MaxCastTime = 3;
        best = queue.FindBest(f.World, f.Player, f.World.Client.Cooldowns, 0, hints, 0, true);
        check(best.Priority == 2000 && best.PositionLock == 2, "窗口充足时正常执行带位置锁定的动作");
    }

    static void RsrTargeting(Action<bool, string> check)
    {
        var property = typeof(Service).GetProperty("PluginInterface")!;
        var original = property.GetValue(null);
        var config = Service.Config.Get<ActionTweaksConfig>();
        var smart = config.SmartTargets;
        var loaded = true;
        var reads = 0;
        var pluginsType = property.PropertyType.GetProperty("InstalledPlugins")!.PropertyType;
        var pluginType = pluginsType.GetGenericArguments().Single();
        var plugin = IpcProxy.Create(pluginType, (m, _) => m.Name switch
        {
            "get_IsLoaded" => loaded,
            "get_Name" or "get_InternalName" => "Rotation Solver Reborn",
            _ => m.ReturnType.IsValueType ? Activator.CreateInstance(m.ReturnType) : null
        });
        var plugins = Array.CreateInstance(pluginType, 1);
        plugins.SetValue(plugin, 0);
        object NewInterface() => IpcProxy.Create(property.PropertyType, (m, _) =>
        {
            if (m.Name == "get_InstalledPlugins") { ++reads; return plugins; }
            return m.ReturnType.IsValueType ? Activator.CreateInstance(m.ReturnType) : null;
        });
        var detector = typeof(ActionTweaksConfig).GetMethod("IsRSREnabled", BindingFlags.Static | BindingFlags.NonPublic)!;
        bool Detect() => (bool)detector.Invoke(null, null)!;
        try
        {
            property.SetValue(null, NewInterface());
            check(Detect() && Detect() && reads == 1, "RSR 已加载检测在一秒窗口内复用缓存");
            using var f = new Fixture();
            var ally = f.Create(0, new(102, 100), ActorType.Player);
            var hints = new AIHints();
            var queue = new ManualActionQueueTweak(f.World, hints);
            var definition = new ActionDefinition(new(ActionType.Spell, 0xff3000))
            {
                AllowedTargets = ActionTargets.Self | ActionTargets.Party,
                SmartTarget = (_, _, _, _) => f.Player
            };
            var resolve = typeof(ManualActionQueueTweak).GetMethod("ResolveTarget", All)!;
            Actor? Target()
            {
                object?[] args = [definition, f.Player, ally.InstanceID, (Func<(ulong, System.Numerics.Vector3?)>)(() => default), (Func<ulong>)(() => 0), true, null, null];
                check((bool)resolve.Invoke(queue, args)!, "手动队列接受合法友方目标");
                return (Actor?)args[6];
            }
            config.SmartTargets = true;
            check(Target() == ally, "RSR 存在时不覆盖用户指定的友方目标");
            loaded = false;
            property.SetValue(null, NewInterface());
            check(!Detect() && reads == 2, "插件接口替换后立即刷新 RSR 检测");
            check(Target() == f.Player, "RSR 未加载时保留本地智能选目标委托");
        }
        finally { property.SetValue(null, original); config.SmartTargets = smart; }
    }

    static void NaStrategy(Action<bool, string> check)
    {
        var north = BossMod.Dawntrail.Ultimate.DMU.P1GravitasData.Side.NORTH;
        var south = BossMod.Dawntrail.Ultimate.DMU.P1GravitasData.Side.SOUTH;
        var na = BossMod.Dawntrail.Ultimate.DMU.DMUConfig.P1GravenImage2Strategy.GravenImage2KefkaBin;
        check(BossMod.Dawntrail.Ultimate.DMU.P1GravitasData.TryGetPuddleSpot(na, north, out var n) && n == new WPos(100, 88)
            && BossMod.Dawntrail.Ultimate.DMU.P1GravitasData.TryGetPuddleSpot(na, south, out var t) && t == new WPos(100, 112), "妖星乱舞 NA 南北毒圈采用固定上游坐标");
        check(!BossMod.Dawntrail.Ultimate.DMU.P1GravitasData.TryGetPuddleSpot(na, BossMod.Dawntrail.Ultimate.DMU.P1GravitasData.Side.NONE, out _), "妖星乱舞未知侧向不生成虚构站位");
    }

    static void AssignmentValidation(Action<bool, string> check)
    {
        var config = new UWU.UWUConfig();
        check(config.BeyondLimits.Validate() && config.P1Friction.Validate(), "绝神兵默认三人分工有效");
        foreach (var invalid in new int[][] { [], [-1], [0, 1, 2, 3, -1, -1, -1, -1], [0, 1, 8, -1, -1, -1, -1, -1], [0, 0, 2, -1, -1, -1, -1, -1], [0, 1, 2, -2, -1, -1, -1, -1] })
            check(!new UWU.GroupAssignment3 { Assignments = invalid }.Validate(), "绝神兵拒绝损坏或越界分工 " + string.Join(',', invalid));
    }

    static void WorldData(Action<bool, string> check)
    {
        using var f = new Fixture();
        f.World.Execute(new ActorState.OpRawRotation(f.Player.InstanceID, 1.25f));
        check(f.Player.ActualRotation.Rad == 1.25f && f.World.Actors.CompareToInitial().OfType<ActorState.OpRawRotation>().Any(), "原始朝向采集事件进入状态快照");
        f.World.Execute(new ActorState.OpRawRotation(f.Player.InstanceID, null));
        check(f.Player.ActualRotation == f.Player.Rotation, "缺原始朝向时回退插值朝向");
        f.World.Execute(new NetworkState.OpRTT(0.15f));
        check(f.World.Network.LastRTT == 0.15f && f.World.Network.CompareToInitial().OfType<NetworkState.OpRTT>().Single().RTT == 0.15f, "网络提前量进入可重放状态快照");
    }

    static void LegacyPreset(Action<bool, string> check)
    {
        const string oldPreset = """{"Name":"legacy-blm","Modules":{"BossMod.Autorotation.kage.KageBLM":[{"Track":"Burst","Option":"Force"},{"Track":"Movement","Option":"Delay"},{"Track":"LeyLines","Option":"ASAP"}]}}""";
        foreach (var name in new[] { "BLM", "DNC", "MCH", "PLD", "SAM", "SGE", "VPR", "WHM" })
        {
            var input = "{\"Name\":\"legacy\",\"Modules\":{\"BossMod.Autorotation.kage.Kage" + name + "\":[{\"Track\":\"Burst\",\"Option\":\"Delay\"}]}}";
            var legacy = JsonSerializer.Deserialize<Preset>(input, Serialization.BuildSerializationOptions())!;
            check(legacy.Modules.Single().SerializedSettings.Count == 1, "Kage " + name + " 保留旧合并爆发控制");
        }
        var preset = JsonSerializer.Deserialize<Preset>(oldPreset, Serialization.BuildSerializationOptions())!;
        var module = preset.Modules.Single();
        check(module.SerializedSettings.Count == 3, "黑魔旧合并爆发、移动及黑魔纹控制导入无丢项");
        var serialized = JsonSerializer.Serialize(preset, Serialization.BuildSerializationOptions());
        check(serialized.Contains("Burst") && serialized.Contains("Movement") && !serialized.Contains("ASAP"), "旧黑魔预设回写新选项名称并保留合并控制");
        var clone = JsonSerializer.Deserialize<Preset>(serialized, Serialization.BuildSerializationOptions())!;
        check(clone.Modules.Single().SerializedSettings.Count == 3, "黑魔预设二次导入保留全部设置");
    }

    static void MultiHit(Action<bool, string> check)
    {
        using var f = new Fixture();
        using var module = f.Module();
        var multi = new RaidwideMultiHit(module, 0xff2000, [0xff2001], 0, 1, 3);
        var cast = new ActorCastInfo { Action = new(ActionType.Spell, 0xff2000), TotalTime = 4 };
        multi.OnCastStarted(module.PrimaryActor, cast);
        var hints = new AIHints();
        multi.AddAIHints(0, f.Player, default, hints);
        check(hints.ForbiddenZones.Count == 0 && hints.PredictedDamage.Single().Hits == 3, "多段团伤仅记录预测与提示");
        var hit = f.Event(0xff2001);
        multi.OnEventCast(module.PrimaryActor, hit);
        multi.OnEventCast(module.PrimaryActor, hit);
        check(multi.Remaining == 2 && multi.NumCasts == 1, "同帧重复团伤事件只推进一次");
        f.Advance(1);
        multi.OnEventCast(module.PrimaryActor, hit);
        f.Advance(1);
        multi.OnEventCast(module.PrimaryActor, hit);
        hints.Clear();
        multi.AddAIHints(0, f.Player, default, hints);
        check(multi.Remaining == 0 && hints.PredictedDamage.Count == 0, "多段团伤结算后释放预测");
        multi.OnCastStarted(module.PrimaryActor, cast);
        f.Advance(20);
        multi.Update();
        check(multi.Remaining == 0, "缺失团伤结算按最后预期时间清理");
    }

    static void DeepDungeon(Action<bool, string> check)
    {
        using var f = new Fixture();
        f.World.DeepDungeon.DungeonId = DeepDungeonState.DungeonType.POTD;
        f.World.DeepDungeon.Progress = new(1, 0, 99, 99, 99, 0, 0, 0, false);
        using var deep = new DeepProbe(f.World);
        var baseType = typeof(BossMod.Global.DeepDungeon.AutoClear);
        baseType.GetField("BetweenFloors", All)!.SetValue(deep, false);
        var config = Service.Config.Get<BossMod.Global.DeepDungeon.AutoDDConfig>();
        var saved = (config.Enable, config.AutoMoveTreasure, config.GoldCoffer, config.SilverCoffer, config.BronzeCoffer, config.AutoPassage, config.FullClear, config.MaxPull, config.AutoClear, config.AutoPoms);
        try
        {
            config.Enable = true;
            config.AutoMoveTreasure = true;
            config.GoldCoffer = false;
            config.SilverCoffer = false;
            config.BronzeCoffer = false;
            config.AutoPassage = false;
            config.FullClear = false;
            config.MaxPull = 0;
            config.AutoClear = BossMod.Global.DeepDungeon.AutoDDConfig.ClearBehavior.None;
            var gold = f.Create(0x1EA13E, new(120, 100), ActorType.EventObj);
            var hints = new AIHints();
            deep.CalculateAIHints(0, f.Player, hints);
            check(hints.InteractWithTarget == null, "深层迷宫保留单独关闭金箱的设置");
            config.GoldCoffer = true;
            hints.Clear();
            deep.CalculateAIHints(0, f.Player, hints);
            check(hints.InteractWithTarget == gold && hints.GoalZones.Count > 0, "深层迷宫缺失房间几何时仍可选择合法宝箱");
            f.Player.InCombat = true;
            hints.Clear();
            deep.CalculateAIHints(0, f.Player, hints);
            check(hints.InteractWithTarget == null && hints.GoalZones.Count == 0, "深层迷宫 MaxPull 为零时战斗中暂停导航");
            f.Player.InCombat = false;
            var autoUse = baseType.GetMethod("CanAutoUse", All)!;
            var strength = Enum.Parse(autoUse.GetParameters()[0].ParameterType, "Strength");
            config.AutoPoms.Set(Convert.ToInt32(strength));
            check((bool)autoUse.Invoke(deep, [strength, f.Player])!, "可用道具仍允许按配置自动使用");
            f.World.Execute(new ActorState.OpStatus(f.Player.InstanceID, 1, new(1094, 0, f.World.FutureTime(20), 0)));
            check(!(bool)autoUse.Invoke(deep, [strength, f.Player])!, "道具禁止楼层不尝试消耗魔陶器");
        }
        finally
        {
            (config.Enable, config.AutoMoveTreasure, config.GoldCoffer, config.SilverCoffer, config.BronzeCoffer, config.AutoPassage, config.FullClear, config.MaxPull, config.AutoClear, config.AutoPoms) = saved;
        }
    }

    sealed class DeepProbe(WorldState world) : BossMod.Global.DeepDungeon.AutoClear(world, 60);

    static void UltimateSafety(Action<bool, string> check)
    {
        using var f = new Fixture();
        using var module = f.Module();
        var other = f.Create(0, new(112, 100), ActorType.Player);
        f.World.Execute(new PartyState.OpModify(1, new(2, other.InstanceID, false, "other")));
        var strict = new UnassignedSpread(module);
        strict.AddSpread(other, f.World.FutureTime(5));
        var hints = new AIHints();
        strict.AddAIHints(0, f.Player, default, hints);
        check(hints.ForbiddenZones.Any(z => z.shapeDistance.Contains(other.Position)), "未分配固定站位仍保留通用分散避让");

        var beyond = f.Activate<GenericAOEs>(module, "BeyondLimits");
        beyond.OnActorCreated(f.Create((uint)UWU.OID.BeyondLimits, new(104, 100)));
        hints.Clear();
        beyond.AddAIHints(0, f.Player, default, hints);
        check(hints.ForbiddenZones.Any(z => z.shapeDistance.Contains(new(104, 100))), "超越之力未知分工不默认让零号玩家吃圈");
        var soakerField = beyond.GetType().GetField("_soaker", All)!;
        soakerField.SetValue(beyond, 0);
        hints.Clear();
        beyond.AddAIHints(0, f.Player, default, hints);
        check(hints.ForbiddenZones.Single().shapeDistance.Contains(new(110, 100)) && !hints.ForbiddenZones.Single().shapeDistance.Contains(new(104, 100))
            && !beyond.ActiveAOEs(0, f.Player)[0].Risky, "超越之力分工者的绘图和进圈约束一致");
        var consumed = new ActorStatus((uint)UWU.SID.BeyondLimits, 0, f.World.FutureTime(30), 0);
        beyond.OnStatusGain(f.Player, ref consumed);
        check(beyond.ActiveAOEs(0, f.Player).Length == 0, "超越之力获得状态后清除圈和分工");

        var mesohigh = f.Activate<BossComponent>(module, "P4Mesohigh");
        mesohigh.GetType().GetField("Order", All)!.SetValue(mesohigh, 2);
        f.World.Execute(new ActorState.OpTether(module.PrimaryActor.InstanceID, new((uint)UWU.TetherID.Mesohigh, other.InstanceID)));
        f.World.Execute(new ActorState.OpStatus(f.Player.InstanceID, 0, new((uint)UWU.SID.SearingWind, 0, f.World.FutureTime(30), 0)));
        var wind = f.Activate<StrictUniformStackSpread>(module, "P4SearingWind");
        wind.AddSpread(f.Player, f.World.FutureTime(6));
        hints.Clear();
        var independent = new SDCircle(new(90, 90), 5);
        hints.AddForbiddenZone(independent);
        wind.AddAIHints(0, f.Player, default, hints);
        mesohigh.AddAIHints(0, f.Player, default, hints);
        check(hints.ForbiddenZones.Any(z => ReferenceEquals(z.shapeDistance, independent)) && hints.ForbiddenZones.Count > 1, "绝神兵接线保留其他组件禁区并追加拦截约束");
        check(wind.GetSpreadDestination(0, f.Player) == null, "接线时只撤销灼热风自己的固定站位");
        wind.Spreads.Clear();
        wind.OnEventCast(module.PrimaryActor, f.Event((uint)UWU.AID.SearingWind));
        check(wind.Spreads.Count == 0, "灼热风空目标收到结算不抛异常");

        var crush = f.Activate<GenericAOEs>(module, "P3Geocrush2");
        var titan = f.Create((uint)UWU.OID.Titan, module.Arena.Center);
        f.World.Execute(new ActorState.OpRawRotation(titan.InstanceID, MathF.PI * 0.5f));
        crush.OnActorPlayActionTimelineEvent(titan, 0x1E39);
        f.Advance(0.5f);
        crush.Update();
        check(crush.ActiveAOEs(0, f.Player).ToArray().Any(a => a.Origin.AlmostEqual(new(114, 100), 0.01f)), "大地粉碎用真实原始朝向预测到十四米边界");
    }

    static void GrandOctetLateActors(Action<bool, string> check)
    {
        using var f = new Fixture();
        var twin = f.Create((uint)BossMod.Stormblood.Ultimate.UCOB.OID.Twintania, new(20, 0));
        using var module = new BossMod.Stormblood.Ultimate.UCOB.UCOB(f.World, twin);
        var type = typeof(BossModule).Assembly.GetType("BossMod.Stormblood.Ultimate.UCOB.P3GrandOctet", true)!;
        var octet = (BossComponent)Activator.CreateInstance(type, All, null, [module], null)!;
        var nael = f.Create((uint)BossMod.Stormblood.Ultimate.UCOB.OID.NaelDeusDarnus, new(0, -20));
        var baha = f.Create((uint)BossMod.Stormblood.Ultimate.UCOB.OID.BahamutPrime, new(0, 20));
        octet.OnActorPlayActionTimelineEvent(nael, 0x1E43);
        octet.OnActorPlayActionTimelineEvent(twin, 0x1E44);
        octet.OnActorPlayActionTimelineEvent(baha, 0x1E43);
        var casters = (List<Actor>)type.GetField("Casters", All)!.GetValue(octet)!;
        check(casters.Count == 0, "八连俯冲缺小龙时不先初始化不完整顺序");
        var dragons = new[] { BossMod.Stormblood.Ultimate.UCOB.OID.Firehorn, BossMod.Stormblood.Ultimate.UCOB.OID.Iceclaw, BossMod.Stormblood.Ultimate.UCOB.OID.Thunderwing,
            BossMod.Stormblood.Ultimate.UCOB.OID.TailOfDarkness, BossMod.Stormblood.Ultimate.UCOB.OID.FangOfLight };
        Actor? last = null;
        for (var i = 0; i < dragons.Length; i++)
        {
            last = f.Create((uint)dragons[i], new WPos(0, 0) + (i * 45).Degrees().ToDirection() * 20);
            octet.OnActorCreated(last);
        }
        check(casters.Count == 8, "八连俯冲最后小龙迟到后补齐八个来源");
        octet.OnActorCreated(last!);
        octet.OnActorPlayActionTimelineEvent(baha, 0x1E43);
        check(casters.Count == 8, "八连俯冲重复生成与动作事件不重复插入来源");
    }

    sealed class UnassignedSpread(BossModule module) : StrictUniformStackSpread(module, 0, 4);

    sealed class Fixture : IDisposable
    {
        public readonly WorldState World = new(10000000, "sync-20261105");
        public readonly Actor Player;
        ulong _id = 1;
        public Fixture()
        {
            World.Frame = new(DateTime.UnixEpoch.AddHours(1), 0, 0, 0, 0, 1);
            Player = Create(0, new(100, 100), ActorType.Player);
            World.Execute(new PartyState.OpModify(0, new(1, Player.InstanceID, false, "player")));
        }
        public Actor Create(uint oid, WPos pos, ActorType type = ActorType.Enemy)
        {
            var id = _id++;
            World.Execute(new ActorState.OpCreate(id, oid, (int)id * 2, 0, "fixture", 0, type, type == ActorType.Player ? Class.PLD : Class.None, 100,
                new(pos.X, 0, pos.Z, 0), 0.5f, new(1000, 1000, 0, 10000, 10000), true, type == ActorType.Player, 0, 0, 0));
            return World.Actors.Find(id)!;
        }
        public UWU.UWU Module() => new(World, Create((uint)UWU.OID.Garuda, new(100, 100)));
        public T Activate<T>(BossModule module, string name) where T : BossComponent
        {
            var type = typeof(BossModule).Assembly.GetType("BossMod.Stormblood.Ultimate.UWU." + name, true)!;
            typeof(BossModule).GetMethod(nameof(BossModule.ActivateComponent))!.MakeGenericMethod(type).Invoke(module, null);
            return (T)module.Components.Single(c => c.GetType() == type);
        }
        public ActorCastEvent Event(uint aid) => new(new(ActionType.Spell, aid), Player.InstanceID, 0, 0, default, 0, 0, default);
        public void Advance(float seconds) => World.Frame = new(World.FutureTime(seconds), 0, 0, 0, 0, 1);
        public void Dispose() { }
    }
}
