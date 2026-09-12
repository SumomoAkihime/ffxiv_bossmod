using BossMod;
using BossMod.Components;
using Stage27 = BossMod.Global.MaskedCarnivale.Stage27.Stage27;

static class PrePullMigrationTests
{
    public static void Run(Action<bool, string> check)
    {
        var migrated = BossModuleRegistry.RegisteredModules.Values
            .Where(info => info.Category == BossModuleInfo.Category.MaskedCarnivale && info.HasPrePullHints).ToList();
        check(migrated.Count > 0, "青魔斗场战前提示进入生成注册表");
        foreach (var info in migrated)
        {
            var world = World();
            var boss = Create(world, 1, info.PrimaryActorOID);
            using var module = BossModuleRegistry.CreateModule(info, world, boss)!;
            check(module.PrePullHints.Length > 0, $"{info.ModuleType.Name}可读取战前提示");
            module.StateMachine.Start(world.CurrentTime);
            check(module.StateMachine.ActiveState != null, $"{info.ModuleType.Name}迁移后仍可激活状态机");
        }
        BombsRemainCurrent(check);
    }

    private static void BombsRemainCurrent(Action<bool, string> check)
    {
        var world = World();
        var boss = Create(world, 1, (uint)BossMod.Global.MaskedCarnivale.Stage27.OID.Boss);
        var bomb = Create(world, 2, (uint)BossMod.Global.MaskedCarnivale.Stage27.OID.Bomb);
        using var module = new Stage27(world, boss);
        module.StateMachine.Start(world.CurrentTime);
        var component = module.Components.OfType<GenericAOEs>().Single(c => c.GetType().Name == "Explosion");
        component.OnActorModelStateChange(bomb, 0, 1, 0);
        component.Update();
        component.Update();
        check(component.ActiveAOEs(0, boss).Length == 1, "青魔斗场第27关重复更新不累积旧炸弹范围");
        bomb.PosRot = new(4, 0, 5, 0);
        component.Update();
        var aoes = component.ActiveAOEs(0, boss);
        check(aoes.Length == 1 && aoes[0].Origin == bomb.Position, "青魔斗场第27关炸弹移动后只保留当前范围");
        component.OnEventCast(bomb, new(ActionID.MakeSpell(BossMod.Global.MaskedCarnivale.Stage27.AID.Explosion), 0, 0, 0, default, 1, 0, default));
        component.Update();
        check(component.ActiveAOEs(0, boss).Length == 0, "青魔斗场第27关炸弹爆炸后清理范围");
    }

    private static WorldState World()
    {
        var world = new WorldState(10000000, "pre-pull-migration-test");
        world.Frame = new(DateTime.UnixEpoch.AddHours(1), 0, 0, 0, 0, 1);
        return world;
    }

    private static Actor Create(WorldState world, ulong id, uint oid)
    {
        world.Execute(new ActorState.OpCreate(id, oid, (int)id, 0, "fixture", 0, ActorType.Enemy, Class.None, 100, default, .5f, new(1000, 1000, 0, 10000, 10000), true, false, 0, 0, 0));
        return world.Actors.Find(id)!;
    }
}
