using System.Runtime.Loader;
using System.Text.Json;
using BossMod;
using BossMod.Autorotation;
using BossMod.Autorotation.xan;
using BossMod.Data;
using Dalamud.Bindings.ImGui;

class Program
{
    static int checks;
    static void Check(bool value, string message) { ++checks; if (!value) throw new Exception(message); }
    static int Main(string[] args)
    {
        var dalamud = Environment.GetEnvironmentVariable("DALAMUD_HOME") ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "XIVLauncher/addon/Hooks/dev");
        AssemblyLoadContext.Default.Resolving += (_, name) => File.Exists(Path.Combine(dalamud, name.Name + ".dll")) ? AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.Combine(dalamud, name.Name + ".dll")) : null;
        if (args.Length != 2 || args[0] != "--sqpack") { Console.Error.WriteLine("用法：--sqpack <游戏 game/sqpack 目录>，仅离线读取。"); return 2; }
        try { Run(args[1]); Console.WriteLine($"通过：{checks} 项辅助职业断言。"); return 0; }
        catch (Exception ex) { Console.Error.WriteLine(ex.GetBaseException()); return 1; }
    }

    static void Run(string sqpack)
    {
        using var data = new Lumina.GameData(sqpack, new Lumina.LuminaOptions { DefaultExcelLanguage = Lumina.Data.Language.ChineseSimplified });
        Service.LuminaGameData = data;
        Service.Config.Initialize();
        Service.WindowSystem = new("offline");
        var context = ImGui.CreateContext();
        try
        {
            var world = new WorldState(10000000, "offline");
            world.Frame.Timestamp = new DateTime(2026, 9, 11, 0, 0, 0, DateTimeKind.Utc);
            Actor Create(ulong id, int index, ActorType type, Class cls)
            {
                world.Execute(new ActorState.OpCreate(id, 0, index, 0, "fixture", 0, type, cls, 100, default, .5f, new(1000, 1000, 0, 10000, 10000), true, type == ActorType.Player, 0, 0, 0));
                return world.Actors.Find(id)!;
            }
            var player = Create(1, 0, ActorType.Player, Class.MCH);
            var ally = Create(2, 2, ActorType.Player, Class.PLD);
            var boss = Create(20, 4, ActorType.Enemy, Class.None);
            world.Execute(new PartyState.OpModify(0, new(1, 1, false, "player")));
            world.Execute(new PartyState.OpModify(1, new(2, 2, false, "ally")));
            using var bossmods = new BossModuleManager(world);
            var hints = new AIHints();
            var temp = Path.Combine(Path.GetTempPath(), "BossModSupportJobs");
            var db = new RotationDatabase(new DirectoryInfo(temp), new FileInfo(Path.Combine(temp, "empty-defaults.json")));
            using var manager = new RotationModuleManager(db, bossmods, hints);
            var ai = new PhantomAI(manager, player);
            var definition = PhantomAI.Definition();
            int Index(string track) => definition.Configs.FindIndex(c => c.InternalName == track);
            void Equip(params PhantomID[] actions)
            {
                Array.Clear(world.Client.DutyActions);
                Array.Clear(world.Client.Cooldowns);
                for (var i = 0; i < actions.Length; ++i)
                    world.Client.DutyActions[i] = new(ActionID.MakeSpell(actions[i]), 1, 1);
            }
            void Tick(Actor? target, string? track = null, string? option = null)
            {
                hints.Clear();
                var values = new StrategyValues(definition.Configs);
                if (track != null)
                {
                    var index = Index(track);
                    var config = (StrategyConfigTrack)definition.Configs[index];
                    ((StrategyValueTrack)values.Values[index]).Option = config.Options.FindIndex(o => o.InternalName == option);
                }
                ai.Execute(values, ref target, 0, false);
            }
            bool Has(PhantomID action) => hints.ActionsToExecute.Entries.Any(e => e.Action == ActionID.MakeSpell(action));
            ActionQueue.Entry Entry(PhantomID action) => hints.ActionsToExecute.Entries.Single(e => e.Action == ActionID.MakeSpell(action));

            foreach (var id in Enum.GetValues<PhantomID>().Where(id => id != PhantomID.None))
            {
                Check(ActionDefinitions.Instance.Spell(id) != null, "技能已注册 " + id);
                Check(!string.IsNullOrEmpty(data.GetExcelSheet<Lumina.Excel.Sheets.Action>()!.GetRow((uint)id).Name.ToString()), "本地游戏存在技能 " + id);
            }
            foreach (var (track, actions) in new (string, PhantomID[])[] {
                ("Ninja", [PhantomID.FumaShuriken, PhantomID.LightningScroll, PhantomID.FlameScroll]),
                ("WhiteMage", [PhantomID.OccultHoly]),
                ("BlackMage", [PhantomID.OccultFireIII, PhantomID.OccultBlizzardIII, PhantomID.OccultThunderIII, PhantomID.OccultFlare]),
                ("Summoner", [PhantomID.Hellfire, PhantomID.JudgmentBolt, PhantomID.Thunderstorm, PhantomID.Megaflare]),
                ("BlueMage", [PhantomID.OccultAero, PhantomID.OccultAeroII, PhantomID.OccultAquaBreath]),
                ("RedMage", [PhantomID.OccultFireII, PhantomID.OccultBlizzardII, PhantomID.OccultThunderII]),
                ("Dragoon", [PhantomID.OccultJump, PhantomID.Lance]) })
            {
                Equip(actions); Tick(boss);
                foreach (var id in actions) Check(Has(id), track + " 默认排队 " + id);
                Tick(boss, track, "Disabled"); Check(hints.ActionsToExecute.Entries.Count == 0, track + " 禁用有效");
                Tick(null); Check(hints.ActionsToExecute.Entries.Count == 0, track + " 无目标");
                Tick(ally); Check(hints.ActionsToExecute.Entries.Count == 0, track + " 不攻击队友");
                world.Client.CountdownRemaining = 5; Tick(boss); Check(hints.ActionsToExecute.Entries.Count == 0, track + " 倒计时不抢跑"); world.Client.CountdownRemaining = null;
                Equip(); Tick(boss); Check(hints.ActionsToExecute.Entries.Count == 0, track + " 未装备不排队");
            }

            Equip(PhantomID.Steal); Tick(boss); Check(!Has(PhantomID.Steal), "盗窃默认关闭");
            Tick(boss, "Steal", "Enabled"); Check(Has(PhantomID.Steal), "显式开启盗窃");
            Equip(PhantomID.Smoke); Tick(boss); Check(!Has(PhantomID.Smoke), "非战斗不维持烟幕");
            player.InCombat = true; Tick(boss); Check(Has(PhantomID.Smoke), "战斗补烟幕");
            player.Statuses[0] = new((uint)PhantomSID.Smoke, 0, world.FutureTime(30), player.InstanceID);
            Tick(boss); Check(!Has(PhantomID.Smoke), "烟幕充足不重复");
            player.Statuses[0] = new((uint)PhantomSID.Smoke, 0, world.FutureTime(4), player.InstanceID);
            Tick(boss); Check(Has(PhantomID.Smoke), "烟幕临期续用"); Array.Clear(player.Statuses);

            foreach (var triple in new[] {
                new[] { PhantomID.OccultFireII, PhantomID.OccultBlizzardII, PhantomID.OccultThunderII },
                new[] { PhantomID.OccultFireIII, PhantomID.OccultBlizzardIII, PhantomID.OccultThunderIII } })
            {
                Equip(triple);
                for (var element = 0; element < 3; ++element)
                {
                    boss.Statuses[0] = new((uint)(5322 + element), 0, world.FutureTime(60), player.InstanceID);
                    Tick(boss);
                    Check(Entry(triple[element]).Priority > Entry(triple[(element + 1) % 3]).Priority, "按实际元素弱点选择 " + triple[element]);
                }
            }
            Equip(PhantomID.Hellfire, PhantomID.JudgmentBolt, PhantomID.Thunderstorm);
            boss.Statuses[0] = new(5324, 0, world.FutureTime(60), player.InstanceID); Tick(boss);
            Check(Entry(PhantomID.Thunderstorm).Priority == Entry(PhantomID.JudgmentBolt).Priority && Entry(PhantomID.Thunderstorm).Priority > Entry(PhantomID.Hellfire).Priority, "雷暴与制裁之雷都匹配雷弱点");
            boss.Statuses[0] = new(5325, 0, world.FutureTime(60), player.InstanceID); Tick(boss);
            Check(Entry(PhantomID.Thunderstorm).Priority == Entry(PhantomID.Hellfire).Priority, "雷暴不匹配风弱点");
            Array.Clear(boss.Statuses);
            Equip(PhantomID.OccultLibra, PhantomID.OccultFireII); Tick(boss);
            Check(Has(PhantomID.OccultLibra) && !Has(PhantomID.OccultFireII), "无弱点先侦测");
            boss.Statuses[0] = new(5322, 0, world.FutureTime(60), player.InstanceID); Tick(boss);
            Check(!Has(PhantomID.OccultLibra) && Has(PhantomID.OccultFireII), "已有弱点继续输出"); Array.Clear(boss.Statuses);

            Equip(PhantomID.OccultFireIII, PhantomID.Hellfire); Tick(boss);
            Check(Entry(PhantomID.OccultFireIII).CastTime > 0 && Entry(PhantomID.Hellfire).CastTime == 4, "读条时间传给移动策略");
            player.Statuses[0] = new((uint)BossMod.ClassShared.SID.Swiftcast, 0, world.FutureTime(10), player.InstanceID); Tick(boss);
            Check(Entry(PhantomID.OccultFireIII).CastTime == 0 && Entry(PhantomID.Hellfire).CastTime == 4, "即刻咏唱不作用于召唤技能");
            foreach (var sid in PhantomAI.BreakableComboStatus)
            {
                player.Statuses[0] = new(sid, 0, world.FutureTime(30), player.InstanceID); Tick(boss);
                Check(hints.ActionsToExecute.Entries.Count == 0, "辅助魔法不打断普通职业连段 " + sid);
            }
            Array.Clear(player.Statuses);

            foreach (var (track, id) in new[] { ("WHMSelfHeal", PhantomID.OccultWHMCureII), ("RDMSelfHeal", PhantomID.OccultRDMCureII) })
            {
                Equip(id); player.HPMP = new(500, 1000, 0, 1500, 10000); Tick(boss);
                Check(Has(id) && Entry(id).Target == player, track + " 输出职业低血自疗");
                Tick(boss, track, "Disabled"); Check(!Has(id), track + " 自疗可关闭");
                player.HPMP = new(600, 1000, 0, 1500, 10000); Tick(boss); Check(!Has(id), track + " 血线边界");
                player.HPMP = new(500, 1000, 0, 1499, 10000); Tick(boss); Check(!Has(id), track + " 蓝量不足");
                player.HPMP = new(500, 1000, 0, 1500, 10000); ally.Class = Class.WHM; Tick(boss); Check(!Has(id), track + " 有治疗队友"); ally.Class = Class.PLD;
                player.Class = Class.PLD; Tick(boss); Check(!Has(id), track + " 坦克不触发"); player.Class = Class.MCH;
                player.InCombat = false; Tick(boss); Check(!Has(id), track + " 非战斗不触发"); player.InCombat = true;
            }
            player.HPMP = new(1000, 1000, 0, 10000, 10000);
            Equip(PhantomID.OccultRaise); ally.IsDead = true;
            Tick(boss); Check(!Has(PhantomID.OccultRaise), "复活默认关闭");
            Tick(boss, "WHMRaise", "OutOfCombat"); Check(!Has(PhantomID.OccultRaise), "脱战复活模式不在战斗生效");
            player.InCombat = false; Tick(boss, "WHMRaise", "OutOfCombat");
            Check(Has(PhantomID.OccultRaise) && Entry(PhantomID.OccultRaise).Target == ally && Entry(PhantomID.OccultRaise).CastTime == 0, "魔复活为瞬发并选择阵亡队友");
            ally.Statuses[0] = new(148, 0, world.FutureTime(30), player.InstanceID); Tick(boss, "WHMRaise", "InCombat"); Check(!Has(PhantomID.OccultRaise), "不重复复活"); Array.Clear(ally.Statuses);
            ally.PosRot = new(100, 0, 0, 0); Tick(boss, "WHMRaise", "InCombat"); Check(!Has(PhantomID.OccultRaise), "不复活超距目标"); ally.PosRot = default;
            player.InCombat = true; Tick(boss, "WHMRaise", "InCombat"); Check(Has(PhantomID.OccultRaise), "允许战斗复活"); ally.IsDead = false;

            var cfg = Service.Config.Get<ActionTweaksConfig>(); cfg.DashSafety = true; cfg.DashSafetyExtra = true;
            var step = ActionDefinitions.Instance.Spell(PhantomID.StepForth)!.ForbidExecute!;
            var jump = ActionDefinitions.Instance.Spell(PhantomID.OccultJump)!.ForbidExecute!;
            var dash = new ActionQueue.Entry { TargetPos = new(10, 0, 0) };
            hints.Clear(); Check(!step(world, player, dash, hints), "前踏步安全落点");
            hints.AddForbiddenZone(new AOEShapeCircle(2), new(10, 0), default, DateTime.MaxValue);
            Check(step(world, player, dash, hints), "前踏步禁止进入危险落点");
            hints.Clear(); hints.AddForbiddenZone(new AOEShapeCircle(2), player.Position, default, DateTime.MaxValue);
            Check(jump(world, player, default, hints), "魔跳跃危险区域保护保留");

            const string oldPreset = """{"Name":"旧辅助配置","Modules":{"BossMod.Autorotation.xan.PhantomAI":[{"Track":"Dragoon","Option":"Disabled"},{"Track":"Predict","Option":"Disabled"},{"Track":"Dancer","Option":"Disabled"}]}}""";
            var preset = JsonSerializer.Deserialize<Preset>(oldPreset)!;
            foreach (var setting in preset.Modules.Single().SerializedSettings)
            {
                var config = (StrategyConfigTrack)definition.Configs[setting.Track];
                Check(config.Options[((StrategyValueTrack)setting.Value).Option].InternalName == "Disabled", "旧预设按名称恢复 " + config.InternalName);
            }
            Check(preset.Modules.Single().SerializedSettings.Count == 3, "旧配置条目完整");
        }
        finally { ImGui.DestroyContext(context); }
    }
}
