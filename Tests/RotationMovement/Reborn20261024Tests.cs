using System.Reflection;
using System.Text;
using System.Text.Json;
using BossMod;
using BossMod.Components;
using Dmu = BossMod.Dawntrail.Ultimate.DMU;
using Shinryu = BossMod.Dawntrail.Unreal.Un2Shinryu;
using Shadow = BossMod.Endwalker.VariantCriterion.C01ASS.C013Shadowcaster;

static class Reborn20261024Tests
{
    public static void Run(Action<bool, string> check)
    {
        VerifyConfigMigration(check);
        VerifyDMU(check);
        VerifyShinryu(check);
        VerifyCastShadow(check);
    }

    private static void VerifyConfigMigration(Action<bool, string> check)
    {
        var options = Serialization.BuildSerializationOptions();
        Dmu.DMUConfig Load(string text)
        {
            using var document = JsonDocument.Parse(text);
            var config = new Dmu.DMUConfig();
            config.Deserialize(document.RootElement, options);
            return config;
        }
        var defaults = new Dmu.DMUConfig();
        check(defaults.P1TeleTrouncing == Dmu.DMUConfig.P1TeleTrouncingStrategy.Freaky_Arrow
            && defaults.P1GravenImage2 == Dmu.DMUConfig.P1GravenImage2Strategy.GravenImage2LPDUUptime,
            "DMU 新用户采用 Reborn 新默认策略");
        var old = Load("""{"P1GravenImage2":"GravenImage2Uptime","P1TeleTrouncing":"Modified_Xolo","P1GravenImage3Static":false,"P2Forsaken":"Kroxy_Rinon_Melee_Flex"}""");
        check(old.P1GravenImage2 == Dmu.DMUConfig.P1GravenImage2Strategy.GravenImage2LPDUUptime
            && old.P1TeleTrouncing == Dmu.DMUConfig.P1TeleTrouncingStrategy.Modified_Xolo
            && !old.P1TeleTrouncingStaticSpots && old.P2Forsaken == Dmu.DMUConfig.P2ForsakenStrategy.Kroxy_Rinon_Melee_Flex,
            "DMU 迁移旧枚举名和静态站位设置，保留 P2 策略");
        var normal = Load("""{"P1GravenImage2":"GravenImage2Normal"}""");
        check(normal.P1GravenImage2 == Dmu.DMUConfig.P1GravenImage2Strategy.GravenImage2None
            && normal.LegacyGravenImage2NeedsSelection, "DMU 不把无等价旧 Normal 静默转为另一策略");
        var numeric = Load("""{"P1GravenImage2":1,"P1TeleTrouncing":0}""");
        check(numeric.P1GravenImage2 == Dmu.DMUConfig.P1GravenImage2Strategy.GravenImage2LPDUUptime
            && numeric.P1TeleTrouncing == Dmu.DMUConfig.P1TeleTrouncingStrategy.Modified_Xolo, "DMU 兼容旧数值枚举的语义");
        var current = Load("""{"StrategySchemaVersion":1,"P1GravenImage2":0,"P1TeleTrouncing":1}""");
        check(!current.LegacyGravenImage2NeedsSelection
            && current.P1TeleTrouncing == Dmu.DMUConfig.P1TeleTrouncingStrategy.Modified_Xolo,
            "DMU 已升级配置不重复执行旧值迁移");
        using var memory = new MemoryStream();
        using (var writer = new Utf8JsonWriter(memory))
            normal.Serialize(writer, options);
        var restored = Load(Encoding.UTF8.GetString(memory.ToArray()));
        check(restored.StrategySchemaVersion == 1 && restored.LegacyGravenImage2NeedsSelection
            && restored.P1GravenImage2 == Dmu.DMUConfig.P1GravenImage2Strategy.GravenImage2None,
            "DMU 保存和再次加载后保留迁移标记");
        var original = Service.Config._nodes[typeof(Dmu.DMUConfig)];
        try
        {
            Service.Config._nodes[typeof(Dmu.DMUConfig)] = normal;
            using var f = new Fixture();
            using var m = f.Module(0x4C30, 7131);
            check(m.PrePullHints.Any(h => h.Contains("Normal")), "DMU 战前明确提示旧 Normal 需要重选");
            normal.P1GravenImage2 = Dmu.DMUConfig.P1GravenImage2Strategy.GravenImage2LPDUUptime;
            check(!m.PrePullHints.Any(h => h.Contains("Normal")), "DMU 选择有效新策略后解除迁移提示");
        }
        finally { Service.Config._nodes[typeof(Dmu.DMUConfig)] = original; }
    }

    private static void VerifyDMU(Action<bool, string> check)
    {
        using var f = new Fixture();
        using var m = f.Module(0x4C30, 7131);
        check(m is Dmu.DMU && m.Info is { GroupID: 1094, NameID: 7131, Maturity: BossModuleInfo.Maturity.Contributed }
            && m.Arena.Center == new WPos(100, 100) && m.Arena.Bounds is ArenaBoundsCircle { Radius: 20 },
            "DMU 按确认的上游身份、成熟度和圆形边界加载");
        m.StateMachine.Start(f.World.CurrentTime);
        check(m.StateMachine.Phases.Count == 5
            && m.Components.Any(c => c.GetType().Name == "RevoltingRuinIIIFirst")
            && m.Components.Any(c => c.GetType().Name == "RevoltingRuinIIISecond"), "DMU P1 实际激活两次死刑组件并保留五阶段图");
        var pulse = f.Activate<GenericKnockback>(m, "BossMod.Dawntrail.Ultimate.DMU.PulseWave");
        var statue = f.Create((uint)Dmu.OID.GravenImage, new(100, 120));
        var tether = new ActorTetherInfo((uint)Dmu.TetherID.GravenImageTether, f.Player.InstanceID);
        pulse.OnTethered(statue, in tether);
        var knockbacks = pulse.ActiveKnockbacks(0, f.Player);
        check(knockbacks.Length == 1 && knockbacks[0].Distance == 13 && knockbacks[0].IgnoreImmunes,
            "DMU P1 连线波动按上游距离预测且不误用抗击退");
        pulse.OnUntethered(statue, in tether);
        check(pulse.ActiveKnockbacks(0, f.Player).Length == 0 && pulse.NumCasts == 1, "DMU P1 连线解除后清掉该玩家击退来源");
        var lightning = f.Activate<SimpleAOEGroups>(m, "BossMod.Dawntrail.Ultimate.DMU.LightningSafeSpots");
        f.Activate<GenericGaze>(m, "BossMod.Dawntrail.Ultimate.DMU.CursedShriek");
        f.StartCast(statue, (uint)Dmu.AID.ThrummingThunderIII, statue.Position, 5);
        check(lightning.Casters.Count == 1 && lightning.Casters[0].ShapeDistance != null,
            "DMU P4 共用雷安全组件提供新版 Casters 与距离缓存");
        f.FinishCast(statue);
        check(lightning.Casters.Count == 0, "DMU 共用雷组件读条结束清理缓存");
        m.PrimaryActor.IsTargetable = false;
        m.Update();
        check(m.StateMachine.ActivePhaseIndex == 1
            && !m.Components.Any(c => c.GetType().Name == "RevoltingRuinIIIFirst"), "DMU P1 转场进入 P2 并移除 P1 组件");
        var p2 = f.Create((uint)Dmu.OID.BossP2, m.Arena.Center);
        m.Update();
        check(((Dmu.DMU)m).BossP2() == p2, "DMU P2 实体晚生成时可以正常发现");
    }

    private static void VerifyShinryu(Action<bool, string> check)
    {
        using var f = new Fixture();
        using var m = f.Module(0x4DDE, 5640);
        check(m is Shinryu.Un2Shinryu && m.Info is { GroupID: 1118, NameID: 5640, Maturity: BossModuleInfo.Maturity.Contributed },
            "幻神龙以独立 CFC/OID 注册，不覆盖普通神龙");
        m.PrimaryActor.PosRot = new(35, -380, 0, 0);
        m.StateMachine.Start(f.World.CurrentTime);
        f.StartCast(m.PrimaryActor, (uint)Shinryu.AID.EarthenFuryVisual, m.Arena.Center, 10);
        m.Update();
        check(m.Components.Count >= 40 && m.Arena.Center == default && m.Arena.Bounds.Radius == 30,
            "幻神龙 P1 激活整套组件并使用上层平台边界");
        var tail = m.Components.Single(c => c.GetType().Name == "TailMarker");
        tail.OnEventIcon(f.Player, (uint)Shinryu.IconID.TailMarker, f.Player.InstanceID);
        var hints = new AIHints();
        tail.AddAIHints(0, f.Player, default, hints);
        check(hints.GoalZones.Count == 1, "幻神龙尾巴标记能根据当前首领方向提供引导");
        f.Create((uint)Shinryu.OID.Tail, new(-20, 20));
        hints.Clear();
        tail.AddAIHints(0, f.Player, default, hints);
        check(hints.GoalZones.Count == 0, "幻神龙尾巴实际生成后移除旧标记引导");
        var icicle = f.Create((uint)Shinryu.OID.Icicle, new(10, 10));
        var impact = Component<GenericAOEs>(m, "IcicleImpact");
        f.Event(icicle, (uint)Shinryu.AID.IcicleImpact, icicle.Position);
        check(impact.ActiveAOEs(0, f.Player).Length == 1, "幻神龙冰柱落地事件生成范围");
        f.StartCast(icicle, (uint)Shinryu.AID.Spikesicle, icicle.Position, 5);
        f.FinishCast(icicle);
        check(impact.ActiveAOEs(0, f.Player).Length == 0, "幻神龙冰柱冲刺结算清理落地范围");
        m.PrimaryActor.PosRot = new(0, 620, -70, 0);
        f.StartCast(m.PrimaryActor, (uint)Shinryu.AID.DarkMatter, new(0, -70), 5);
        m.Update();
        check(m.StateMachine.ActivePhaseIndex == 1 && m.Arena.Center == new WPos(0, -70) && m.Arena.Bounds.Radius == 30,
            "幻神龙 DarkMatter 触发 P2 和下层场地中心切换");
        f.Event(m.PrimaryActor, (uint)Shinryu.AID.Shatter, m.Arena.Center);
        m.Update();
        check(m.Arena.Center == default && m.Arena.Bounds.Radius == 40, "幻神龙 Shatter 触发 P3 扩场与中心恢复");
        var cocoon = Component<UniformStackSpread>(m, "CocoonSpread");
        cocoon.OnEventIcon(f.Player, (uint)Shinryu.IconID.CocoonSpread, f.Player.InstanceID);
        check(cocoon.Spreads.Count == 1, "幻神龙陨石茧标记生成分散预警");
        f.Advance(9);
        cocoon.Update();
        check(cocoon.Spreads.Count == 0, "幻神龙茧标记即使没有后续读条也能按时间清理");
        var ray = Component<UniformStackSpread>(m, "AtomicRay");
        ray.AddStack(f.Player, f.World.FutureTime(1));
        f.Advance(3);
        ray.Update();
        check(ray.Stacks.Count == 0, "幻神龙未匹配结算的双人分摊不会永久残留");
        var wave = Component<GenericKnockback>(m, "TidalWave");
        f.StartCast(m.PrimaryActor, (uint)Shinryu.AID.TidalWave, m.Arena.Center, 5);
        var text = new BossComponent.TextHints();
        wave.AddHints(0, f.Player, text);
        check(text.Any(h => h.Item1.StartsWith("Arm's length at 2s")), "幻神龙击退在窗口前显示抗击退倒计时");
        f.Advance(4);
        text.Clear();
        wave.AddHints(0, f.Player, text);
        check(text.Any(h => h.Item1 == "ARM'S LENGTH NOW" && h.Item2), "幻神龙进入窗口后明确提示抗击退");
        f.FinishCast(m.PrimaryActor);
        text.Clear();
        wave.AddHints(0, f.Player, text);
        check(text.Count == 0, "幻神龙击退读条清理后移除抗击退提示");
    }

    private static void VerifyCastShadow(Action<bool, string> check)
    {
        using var f = new Fixture();
        using var m = f.Module((uint)Shadow.OID.NBoss, 11393);
        var shadow = f.Activate<GenericAOEs>(m, "BossMod.Endwalker.VariantCriterion.C01ASS.C013Shadowcaster.CastShadow");
        var helpers = Enumerable.Range(0, 12).Select(i => f.Create(0x233C, m.Arena.Center + new WDir(i, 0))).ToArray();
        foreach (var helper in helpers)
            f.StartCast(helper, (uint)Shadow.AID.NCastShadowAOE1, helper.Position, 5);
        check(shadow.ActiveAOEs(0, f.Player).Length == 6, "CastShadow 初始仅显示前六个范围");
        for (var i = 0; i < 7; ++i)
            f.FinishCast(helpers[i]);
        check(shadow.ActiveAOEs(0, f.Player).Length == 5, "CastShadow 剩余不足六个时不越界且全部显示");
        for (var i = 7; i < helpers.Length; ++i)
            f.FinishCast(helpers[i]);
        check(shadow.ActiveAOEs(0, f.Player).Length == 0, "CastShadow 全部结算返回空范围");
    }

    private static T Component<T>(BossModule module, string name) where T : BossComponent
        => (T)module.Components.Single(c => c.GetType().Name == name);

    private sealed class Fixture : IDisposable
    {
        public readonly WorldState World = new(10000000, "reborn-7.5.7.2-test");
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

        public BossModule Module(uint oid, uint nameID)
        {
            var boss = Create(oid, default, nameID: nameID);
            var module = BossModuleRegistry.CreateModuleForActor(World, boss, BossModuleInfo.Maturity.Contributed)
                ?? throw new InvalidOperationException($"未注册主 OID {oid:X}");
            boss.PosRot = new(module.Arena.Center.X, 0, module.Arena.Center.Z, 0);
            Player.PosRot = boss.PosRot;
            return module;
        }

        public T Activate<T>(BossModule module, string typeName) where T : BossComponent
        {
            var type = typeof(BossModule).Assembly.GetType(typeName, true)!;
            if (!module.Components.Any(c => c.GetType() == type))
                typeof(BossModule).GetMethod(nameof(BossModule.ActivateComponent))!.MakeGenericMethod(type).Invoke(module, null);
            return (T)module.Components.Single(c => c.GetType() == type);
        }

        public void StartCast(Actor caster, uint aid, WPos location, float duration) => World.Execute(new ActorState.OpCastInfo(caster.InstanceID,
            new ActorCastInfo { Action = new(ActionType.Spell, aid), TargetID = caster.InstanceID, Location = new(location.X, 0, location.Z), TotalTime = duration }));
        public void FinishCast(Actor caster) => World.Execute(new ActorState.OpCastInfo(caster.InstanceID, null));
        public void Event(Actor caster, uint aid, WPos position) => World.Execute(new ActorState.OpCastEvent(caster.InstanceID,
            new(new(ActionType.Spell, aid), Player.InstanceID, 0, 0, new(position.X, 0, position.Z), 0, 0, default)));
        public void Advance(float seconds) => World.Frame = new(World.FutureTime(seconds), 0, 0, 0, 0, 1);
        public void Dispose() { }
    }
}
