using BossMod;
using BossMod.Components;
using Campeador = BossMod.Global.CrucibleOfTheUnbroken.ThirdBoard.CampeadorPiece.CampeadorPiece;
using Cavalier = BossMod.Global.CrucibleOfTheUnbroken.ThirdBoard.CavalierPiece.CavalierPiece;
using Guttler = BossMod.Global.CrucibleOfTheUnbroken.ThirdBoard.GuttlerTheGutter.GuttlerTheGutter;
using Lakhamu = BossMod.Global.CrucibleOfTheUnbroken.ThirdBoard.LakhamuPiece.LakhamuPiece;
using Siren = BossMod.Global.CrucibleOfTheUnbroken.ThirdBoard.SirenPiece.SirenPiece;
using Ymir = BossMod.Global.CrucibleOfTheUnbroken.ThirdBoard.YmirPiece.YmirPiece;
using Zu = BossMod.Global.CrucibleOfTheUnbroken.ThirdBoard.ZuPiece.ZuPiece;

static class ThirdBoardUpstreamTests
{
    public static void Run(Action<bool, string> check)
    {
        VerifyRegistration(check, "骑兵棋子", typeof(Cavalier), 0x4C8E, "MenaceValfodr", new(120, 0));
        VerifyRegistration(check, "尤弥尔棋子", typeof(Ymir), 0x4C93, "Tsunami", new(120, 0));
        VerifyRegistration(check, "祖鸟棋子", typeof(Zu), 0x4C96, "Carve", new(120, -420));
        VerifyRegistration(check, "拉哈姆棋子", typeof(Lakhamu), 0x4C9E, "Landslip", new(120, 0));
        VerifyRegistration(check, "海妖棋子", typeof(Siren), 0x4CA1, "InvitingVerse", new(120, -420));
        VerifyRegistration(check, "坎帕多棋子", typeof(Campeador), 0x4CA5, "Needles", new(120, -420));
        VerifyRegistration(check, "格特勒", typeof(Guttler), 0x4CAA, "BeastlyAura", new(520, -420));
        VerifyEarthShakerLifecycle(check);
        VerifyKnockback(check, 0x4C8E, 48471, "CrushingBlade", new(120, 0));
        VerifyKnockback(check, 0x4C93, 48481, "Tsunami", new(120, 0));
        VerifyKnockback(check, 0x4C9E, 48556, "Landslip", new(120, 0));
        VerifyKnockback(check, 0x4CAA, 48607, "BeastlyAura", new(520, -420));
    }

    private static void VerifyRegistration(Action<bool, string> check, string name, Type expectedType, uint oid, string componentName, WPos position)
    {
        using var fixture = new Fixture();
        var boss = fixture.Create(oid, position);
        using var module = BossModuleRegistry.CreateModuleForActor(fixture.World, boss, BossModuleInfo.Maturity.Contributed);
        check(module?.GetType() == expectedType && module.Info?.ModuleType == expectedType, $"{name}以成熟模块注册并构造");
        if (module == null)
            return;

        module.StateMachine.Start(fixture.World.CurrentTime);
        check(module.StateMachine.ActiveState != null && module.Components.Any(c => c.GetType().Name == componentName), $"{name}状态机启动后激活{componentName}");
    }

    private static void VerifyEarthShakerLifecycle(Action<bool, string> check)
    {
        using var fixture = new Fixture();
        var boss = fixture.Create(0x4C9E, new(120, 0));
        using var module = new Lakhamu(fixture.World, boss);
        module.StateMachine.Start(fixture.World.CurrentTime);
        var earthShaker = (GenericBaitAway)module.Components.Single(c => c.GetType().Name == "EarthShaker");

        earthShaker.OnEventIcon(boss, 40, boss.InstanceID);
        check(earthShaker.CurrentBaits.Count == 1, "地摇第一轮图标创建诱导范围");
        earthShaker.OnEventCast(boss, Event(48558));
        fixture.Advance(1.1);
        earthShaker.Update();
        check(earthShaker.CurrentBaits.Count == 0, "地摇第一轮伤害后延迟清理诱导范围");

        earthShaker.OnEventIcon(boss, 40, boss.InstanceID);
        earthShaker.Update();
        check(earthShaker.CurrentBaits.Count == 1, "地摇第二轮图标不会被上一轮时间戳提前清理");
        earthShaker.OnEventCast(boss, Event(48558));
        fixture.Advance(1.1);
        earthShaker.Update();
        check(earthShaker.CurrentBaits.Count == 0, "地摇第二轮伤害后仍按延迟清理");
    }

    private static ActorCastEvent Event(uint aid) => new(new(ActionType.Spell, aid), 0, 0, 0, default, 1, 0, default);

    private static void VerifyKnockback(Action<bool, string> check, uint oid, uint aid, string componentName, WPos center)
    {
        using var fixture = new Fixture();
        var player = fixture.Create(0, center, ActorType.Player);
        fixture.World.Execute(new PartyState.OpModify(0, new(1, player.InstanceID, false, "player")));
        var boss = fixture.Create(oid, center);
        using var module = BossModuleRegistry.CreateModuleForActor(fixture.World, boss, BossModuleInfo.Maturity.Contributed)!;
        module.StateMachine.Start(fixture.World.CurrentTime);
        var kb = (GenericKnockback)module.Components.Single(c => c.GetType().Name == componentName);
        if (componentName == "Landslip")
            fixture.Create(0x4C9F, center + new WDir(10, 0));
        var cast = new ActorCastInfo { Action = new(ActionType.Spell, aid), Location = new(center.X, 0, center.Z), TotalTime = 4.7f };
        if (componentName == "CrushingBlade")
            kb.OnEventIcon(player, 633, player.InstanceID);
        kb.OnCastStarted(boss, cast);
        var hints = new AIHints();
        kb.AddAIHints(0, player, default, hints);
        check(hints.ForbiddenZones.Count > 0, $"{componentName}在读条时生成击退准备区域");
        if (componentName == "Tsunami")
        {
            var zone = hints.ForbiddenZones.Single();
            check(zone.activation == fixture.World.FutureTime(5), "海啸约束按真实读条结算时间生效");
            check(zone.shapeDistance.Contains(center) && !zone.shapeDistance.Contains(center + new WDir(0, -18)), "海啸排除会被击退出界的起点，保留可安全落地起点");
        }
        var immunity = new ActorStatus(1209, 0, fixture.World.FutureTime(10), player.InstanceID);
        kb.OnStatusGain(player, ref immunity);
        hints.Clear();
        kb.AddAIHints(0, player, default, hints);
        check(hints.ForbiddenZones.Count == 0, $"{componentName}抗击退覆盖结算时释放移动限制");
        immunity.ExpireAt = fixture.World.FutureTime(2);
        kb.OnStatusGain(player, ref immunity);
        hints.Clear();
        kb.AddAIHints(0, player, default, hints);
        check(hints.ForbiddenZones.Count > 0, $"{componentName}抗击退提前过期时仍要求安全站位");
        kb.OnStatusLose(player, ref immunity);
        kb.OnCastFinished(boss, cast);
        hints.Clear();
        kb.AddAIHints(0, player, default, hints);
        check(hints.ForbiddenZones.Count == 0, $"{componentName}结算后释放移动限制");
    }

    private sealed class Fixture : IDisposable
    {
        public readonly WorldState World = new(10000000, "third-board-upstream-test");
        private ulong _nextID = 1;

        public Fixture() => World.Frame = new(DateTime.UnixEpoch.AddHours(1), 0, 0, 0, 0, 1);

        public Actor Create(uint oid, WPos position, ActorType type = ActorType.Enemy)
        {
            var id = _nextID++;
            World.Execute(new ActorState.OpCreate(id, oid, (int)id, 0, "fixture", 0, type, Class.None, 100, new(position.X, 0, position.Z, 0), .5f, new(1000, 1000, 0, 10000, 10000), true, false, 0, 0, 0));
            return World.Actors.Find(id)!;
        }

        public void Advance(double seconds) => World.Frame = new(World.CurrentTime.AddSeconds(seconds), 0, 0, 0, 0, 1);
        public void Dispose() { }
    }
}
