using BossMod;
using BossMod.Components;

static class UpstreamEden20261015Tests
{
    public static void Run(Action<bool, string> check)
    {
        VerifyModuleRegistrationsAndAOELifecycle(check);
        VerifyShivaContinuousAOEs(check);
    }

    private static void VerifyModuleRegistrationsAndAOELifecycle(Action<bool, string> check)
    {
        (uint oid, uint cfc, uint nameID, uint aid, string type)[] cases =
        [
            (0x2D2D, 726, 9298, 19541, "E07TheIdolOfDarkness"),
            (0x2D73, 728, 9353, 19939, "E08Shiva"),
            (0x3056, 749, 9764, 23295, "E09CloudOfDarkness"),
            (0x3062, 747, 9769, 22215, "E10Shadowkeeper"),
            (0x3070, 751, 9707, 22065, "E11Fatebreaker"),
            (0x308D, 758, 9813, 22637, "E12EdensPromise")
        ];

        foreach (var item in cases)
        {
            using var fixture = new Fixture();
            var boss = fixture.Create(item.oid, new(100, 100));
            using var module = BossModuleRegistry.CreateModuleForActor(fixture.World, boss, BossModuleInfo.Maturity.Contributed);
            check(module?.GetType().Name == item.type && module.Info is { GroupType: BossModuleInfo.GroupType.CFC } info
                && info.GroupID == item.cfc && info.NameID == item.nameID && info.PrimaryActorOID == item.oid
                && info.Maturity == BossModuleInfo.Maturity.Contributed, $"{item.type} 身份唯一且以 Contributed 注册");
            if (module == null)
                continue;

            module.StateMachine.Start(fixture.World.CurrentTime);
            var aoe = module.Components.OfType<SimpleAOEs>().SingleOrDefault(c => c.WatchedAction == item.aid);
            check(module.StateMachine.ActiveState != null && aoe != null, $"{item.type} 状态机激活代表 AOE 组件");
            if (aoe == null)
                continue;

            var cast = Cast(item.aid, module.Arena.Center, 4);
            aoe.OnCastStarted(boss, cast);
            check(aoe.ActiveAOEs(0, boss).Length > 0, $"{item.type} 读条开始显示范围");
            aoe.OnCastFinished(boss, cast);
            check(aoe.ActiveAOEs(0, boss).Length == 0, $"{item.type} 读条结算后清理范围");
        }
    }

    private static void VerifyShivaContinuousAOEs(Action<bool, string> check)
    {
        using var fixture = new Fixture();
        using var module = fixture.Module(0x2D73);
        module.StateMachine.Start(fixture.World.CurrentTime);
        var holy = module.Components.OfType<GenericAOEs>().Single(c => c.GetType().Name == "Holy");
        var cast = Cast(19949, module.Arena.Center, 8);
        holy.OnCastStarted(module.PrimaryActor, cast);
        check(holy.ActiveAOEs(0, module.PrimaryActor).Length == 8, "E08 神圣连续陨石按八个方向完整显示");
        holy.OnCastFinished(module.PrimaryActor, cast);
        check(holy.ActiveAOEs(0, module.PrimaryActor).Length == 0, "E08 神圣连续陨石读条结束后全部清理");
    }

    private static ActorCastInfo Cast(uint aid, WPos location, float total) => new()
    {
        Action = new(ActionType.Spell, aid),
        Location = new(location.X, 0, location.Z),
        TotalTime = total
    };

    private sealed class Fixture : IDisposable
    {
        public readonly WorldState World = new(10000000, "upstream-eden-20261015-test");
        private ulong _nextID = 1;

        public Fixture() => World.Frame = new(DateTime.UnixEpoch.AddHours(1), 0, 0, 0, 0, 1);

        public Actor Create(uint oid, WPos position)
        {
            var id = _nextID++;
            World.Execute(new ActorState.OpCreate(id, oid, (int)id, 0, "fixture", 0, ActorType.Enemy, Class.None, 100,
                new(position.X, 0, position.Z, 0), 0.5f, new(1000, 1000, 0, 10000, 10000), true, false, 0, 0, 0));
            return World.Actors.Find(id)!;
        }

        public BossModule Module(uint oid) => BossModuleRegistry.CreateModuleForActor(World, Create(oid, new(100, 100)), BossModuleInfo.Maturity.Contributed)
            ?? throw new InvalidOperationException($"未注册模块 OID {oid:X}");

        public void Dispose() { }
    }
}
