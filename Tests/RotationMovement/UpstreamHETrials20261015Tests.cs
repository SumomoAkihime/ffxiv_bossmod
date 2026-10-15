using System.Reflection;
using System.Numerics;
using BossMod;
using BossMod.Components;

static class UpstreamHETrials20261015Tests
{
    public static void Run(Action<bool, string> check)
    {
        VerifyUniqueRegistrations(check);
        VerifyRavana(check);
        VerifyBismarck(check);
        VerifyThordan(check);
    }

    private static readonly (uint oid, uint cfc, uint nameID, string type, uint lifecycleAid)[] Cases =
    [
        (0xEB4, 86, 3660, "T01Ravana", 3719),
        (0xEF8, 88, 3649, "T02Bismarck", 4932),
        (0x1073, 90, 3632, "T03Thordan", 4197)
    ];

    private static void VerifyUniqueRegistrations(Action<bool, string> check)
    {
        var modules = typeof(BossModuleRegistry).Assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(BossModule)) && t.GetCustomAttribute<ModuleInfoAttribute>() is not null)
            .Select(t => (type: t, info: BossModuleRegistry.Info.Build(t)))
            .Where(x => x.info is { } info && (Cases.Any(c => info.PrimaryActorOID == c.oid) || info.GroupType == BossModuleInfo.GroupType.CFC && Cases.Any(c => info.GroupID == c.cfc)))
            .ToArray();

        foreach (var item in Cases)
        {
            var sameOID = modules.Where(x => x.info?.PrimaryActorOID == item.oid).ToArray();
            var matches = sameOID.Where(x => x.info is { } info && info.GroupType == BossModuleInfo.GroupType.CFC && info.GroupID == item.cfc && info.NameID == item.nameID).ToArray();
            check(sameOID.Length == 1 && matches.Length == 1 && matches[0].type.Name == item.type
                && matches[0].info!.Maturity == BossModuleInfo.Maturity.Contributed,
                $"{item.type} 以正确身份唯一注册为 Contributed");
        }
    }

    private static void VerifyRavana(Action<bool, string> check)
    {
        using var fixture = new Fixture();
        var module = fixture.Module(Cases[0]);
        using (module)
        {
            module.StateMachine.Start(fixture.World.CurrentTime);
            check(module.Components.Any(c => c.GetType().Name == "PillarsOfHeavenKnockback")
                && module.Components.Any(c => c.GetType().Name == "RavanaSeeing"), "Ravana 状态机激活击退与招架组件");
            VerifySimpleAOELifecycle(module, Cases[0], check);

            var initial = module.StateMachine.ActiveState;
            fixture.World.Execute(new ActorState.OpCastInfo(module.PrimaryActor.InstanceID, Cast(Cases[0].lifecycleAid, module.Arena.Center, 100)));
            module.StateMachine.Update(fixture.World.CurrentTime);
            check(initial != null && module.StateMachine.ActiveState != initial, "Ravana 开场读条触发阶段状态转换");
        }
    }

    private static void VerifyBismarck(Action<bool, string> check)
    {
        using var fixture = new Fixture();
        using var module = fixture.Module(Cases[1]);
        module.StateMachine.Start(fixture.World.CurrentTime);
        check(module.Components.Any(c => c.GetType().Name == "DeckArena")
            && module.Components.Any(c => c.GetType().Name == "SanuwaSeparation")
            && module.Components.Any(c => c.GetType().Name == "Interactables"), "Bismarck 状态机激活动态场地及机制组件");
        VerifySimpleAOELifecycle(module, Cases[1], check);
    }

    private static void VerifyThordan(Action<bool, string> check)
    {
        using var fixture = new Fixture();
        using var module = fixture.Module(Cases[2]);
        module.StateMachine.Start(fixture.World.CurrentTime);
        check(module.Components.OfType<ConcentricAOEs>().SingleOrDefault(c => c.GetType().Name == "HeavyImpact") is { } impact,
            "Thordan 状态机激活重击四段组件");
        VerifySimpleAOELifecycle(module, Cases[2], check);

        if (module.Components.OfType<ConcentricAOEs>().SingleOrDefault(c => c.GetType().Name == "HeavyImpact") is not { } heavy)
            return;
        var boss = module.PrimaryActor;
        heavy.OnCastStarted(boss, Cast(4218, boss.Position, 3));
        heavy.Update();
        var first = heavy.Sequences.SingleOrDefault();
        check(first.NumCastsDone == 0 && heavy.ActiveAOEs(0, boss).Length == 1, "Thordan 重击序列从首段范围开始");
        heavy.OnEventCast(boss, EventCast(4218));
        heavy.Update();
        check(heavy.Sequences.SingleOrDefault().NumCastsDone == 1, "Thordan 首段结算推进到第二段");
    }

    private static void VerifySimpleAOELifecycle(BossModule module, (uint oid, uint cfc, uint nameID, string type, uint lifecycleAid) item, Action<bool, string> check)
    {
        var aoe = module.Components.OfType<SimpleAOEs>().SingleOrDefault(c => c.WatchedAction == item.lifecycleAid);
        check(module.StateMachine.ActiveState != null && aoe != null, $"{item.type} 状态机激活代表 AOE");
        if (aoe == null)
            return;

        var cast = Cast(item.lifecycleAid, module.Arena.Center, 4);
        aoe.OnCastStarted(module.PrimaryActor, cast);
        check(aoe.ActiveAOEs(0, module.PrimaryActor).Length > 0, $"{item.type} 读条开始显示范围");
        aoe.OnCastFinished(module.PrimaryActor, cast);
        check(aoe.ActiveAOEs(0, module.PrimaryActor).Length == 0, $"{item.type} 读条结束清理范围");
    }

    private static ActorCastInfo Cast(uint aid, WPos location, float total) => new()
    {
        Action = new(ActionType.Spell, aid),
        Location = new(location.X, 0, location.Z),
        TotalTime = total
    };

    private static ActorCastEvent EventCast(uint aid) => new(new(ActionType.Spell, aid), 0, 0, 0, Vector3.Zero, 0, 0, default);

    private sealed class Fixture : IDisposable
    {
        public readonly WorldState World = new(10000000, "upstream-he-trials-20261015-test");
        private ulong _nextID = 1;

        public Fixture() => World.Frame = new(DateTime.UnixEpoch.AddHours(1), 0, 0, 0, 0, 1);

        public Actor Create(uint oid, WPos position)
        {
            var id = _nextID++;
            World.Execute(new ActorState.OpCreate(id, oid, (int)id, 0, "fixture", 0, ActorType.Enemy, Class.None, 100,
                new(position.X, 0, position.Z, 0), 0.5f, new(1000, 1000, 0, 10000, 10000), true, false, 0, 0, 0));
            return World.Actors.Find(id)!;
        }

        public BossModule Module((uint oid, uint cfc, uint nameID, string type, uint lifecycleAid) item)
        {
            var boss = Create(item.oid, new(100, 100));
            var module = BossModuleRegistry.CreateModuleForActor(World, boss, BossModuleInfo.Maturity.Contributed)
                ?? throw new InvalidOperationException($"未注册模块 OID {item.oid:X}");
            return module;
        }

        public void Dispose() { }
    }
}