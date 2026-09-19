using BossMod;
using BossMod.Components;

static class NewRadarSeptemberTests
{
    public static void Run(Action<bool, string> check)
    {
        VerifyRegistration(check);
        VerifySimpleAOELifecycle(check);
        VerifyFlowingLance(check);
        VerifyPliantPetals(check);
    }

    private static void VerifyRegistration(Action<bool, string> check)
    {
        (uint oid, Type type, uint groupID, uint nameID)[] modules =
        [
            (0x2499, typeof(BossMod.Shadowbringers.Raid.E01EdenPrime.E01EdenPrime), 653, 8345),
            (0x290C, typeof(BossMod.Shadowbringers.Raid.E02Voidwalker.E02Voidwalker), 684, 8382),
            (0x296B, typeof(BossMod.Shadowbringers.Raid.E03Leviathan.E03Leviathan), 682, 8486),
            (0x40BA, typeof(BossMod.Endwalker.Variant.V03Aloalo.V031Quaqua.V031Quaqua), 961, 12527),
            (0x4091, typeof(BossMod.Endwalker.Variant.V03Aloalo.V032Ketuduke.V032Ketuduke), 961, 12605),
            (0x4039, typeof(BossMod.Endwalker.Variant.V03Aloalo.V033TheLala.V033TheLala), 961, 12639),
            (0x4050, typeof(BossMod.Endwalker.Variant.V03Aloalo.V034Statice.V034Statice), 961, 12506),
            (0x3FD6, typeof(BossMod.Endwalker.Variant.V03Aloalo.V035Loquloqui.V035Loquloqui), 961, 12636)
        ];

        foreach (var (oid, type, groupID, nameID) in modules)
        {
            using var fixture = new Fixture("new-radar-registration");
            var boss = fixture.Create(oid, default);
            using var module = BossModuleRegistry.CreateModuleForActor(fixture.World, boss, BossModuleInfo.Maturity.Contributed);
            check(module?.GetType() == type && module.Info?.GroupID == groupID && module.Info.NameID == nameID && module.Info.Maturity == BossModuleInfo.Maturity.Contributed,
                $"新增雷达 {type.Name} 按 CFC/NameID 以 Contributed 注册");
            if (module != null)
            {
                module.StateMachine.Start(fixture.World.CurrentTime);
                check(module.StateMachine.ActiveState != null && module.Components.Count > 0, $"新增雷达 {type.Name} 可启动状态机并创建机制组件");
            }
        }
    }

    private static void VerifySimpleAOELifecycle(Action<bool, string> check)
    {
        using var fixture = new Fixture("eden-prime-lifecycle");
        var boss = fixture.Create(0x2499, new(100, 100));
        using var module = new BossMod.Shadowbringers.Raid.E01EdenPrime.E01EdenPrime(fixture.World, boss);
        module.StateMachine.Start(fixture.World.CurrentTime);
        var beam = (GenericAOEs)module.Components.Single(c => c.GetType().Name == "PureBeam");
        var helper = fixture.Create(0x233C, new(90, 100));
        var cast = Cast(15774, helper.Position, 2.7f);

        beam.OnCastStarted(helper, cast);
        check(beam.ActiveAOEs(0, boss).Length == 1, "E01 纯净射线读条时出现范围");
        beam.OnCastFinished(helper, cast);
        check(beam.ActiveAOEs(0, boss).IsEmpty, "E01 纯净射线读条结束后清理范围");
    }

    private static void VerifyFlowingLance(Action<bool, string> check)
    {
        using var fixture = new Fixture("quaqua-flowing-lance");
        var boss = fixture.Create(0x40BA, new(0, 0));
        using var module = new BossMod.Endwalker.Variant.V03Aloalo.V031Quaqua.V031Quaqua(fixture.World, boss);
        module.StateMachine.Start(fixture.World.CurrentTime);
        var lance = (GenericAOEs)module.Components.Single(c => c.GetType().Name == "FlowingLance");
        var helper = fixture.Create(0x233C, new(5, 0));
        var cast = Cast(35745, helper.Position, 7.7f);

        lance.OnCastStarted(helper, cast);
        var initial = lance.ActiveAOEs(0, boss).ToArray();
        check(initial.Length == 2 && initial[0].Color == Colors.Danger && initial[1].Color == Colors.AOE, "阿萝阿萝旋转长枪显示当前与下一段");
        lance.OnEventCast(helper, Event(35745));
        var advanced = lance.ActiveAOEs(0, boss).ToArray();
        check(advanced.Length == 2 && advanced[0].Rotation != initial[0].Rotation && lance.NumCasts == 1, "阿萝阿萝旋转长枪按结算推进方向");
        for (var i = 1; i < 7; ++i)
            lance.OnEventCast(helper, Event(35746));
        check(lance.ActiveAOEs(0, boss).IsEmpty && lance.NumCasts == 7, "阿萝阿萝旋转长枪末段结算后清理");
    }

    private static void VerifyPliantPetals(Action<bool, string> check)
    {
        using var fixture = new Fixture("loquloqui-petals");
        var boss = fixture.Create(0x3FD6, new(950, -860));
        using var module = new BossMod.Endwalker.Variant.V03Aloalo.V035Loquloqui.V035Loquloqui(fixture.World, boss);
        module.StateMachine.Start(fixture.World.CurrentTime);
        var petals = (GenericAOEs)module.Components.Single(c => c.GetType().Name == "PliantPetals");
        var helper = fixture.Create(0x233C, new(955, -860));
        var cast = Cast(34758, helper.Position, 1.7f);

        petals.OnCastStarted(helper, cast);
        check(petals.ActiveAOEs(0, boss).Length == 1, "阿萝阿萝柔韧花瓣读条时出现落点");
        petals.OnEventCast(helper, Event(34758));
        check(petals.ActiveAOEs(0, boss).IsEmpty, "阿萝阿萝柔韧花瓣结算后清理落点");
    }

    private static ActorCastInfo Cast(uint aid, WPos location, float totalTime) => new()
    {
        Action = new(ActionType.Spell, aid),
        Location = new(location.X, 0, location.Z),
        TotalTime = totalTime
    };

    private static ActorCastEvent Event(uint aid) => new(new(ActionType.Spell, aid), 0, 0, 0, default, 1, 0, default);

    private sealed class Fixture : IDisposable
    {
        public readonly WorldState World;
        private ulong _nextID = 1;

        public Fixture(string name)
        {
            World = new(10000000, name);
            World.Frame = new(DateTime.UnixEpoch.AddHours(1), 0, 0, 0, 0, 1);
        }

        public Actor Create(uint oid, WPos position)
        {
            var id = _nextID++;
            World.Execute(new ActorState.OpCreate(id, oid, (int)id, 0, "fixture", 0, ActorType.Enemy, Class.None, 100,
                new(position.X, 0, position.Z, 0), .5f, new(1000, 1000, 0, 10000, 10000), true, false, 0, 0, 0));
            return World.Actors.Find(id)!;
        }

        public void Dispose() { }
    }
}
