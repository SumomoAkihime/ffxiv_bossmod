using System.Reflection;
using BossMod;
using BossMod.Components;
using PasDeSeul = BossMod.Global.CrucibleOfTheUnbroken.FirstBoard.PasDeSeul.PasDeSeul;

static class UpstreamSeptemberTests
{
    public static void Run(Action<bool, string> check)
    {
        var world = new WorldState(10000000, "upstream-september-test");
        world.Frame = new(DateTime.UnixEpoch.AddHours(1), 0, 0, 0, 0, 1);
        world.Client.Inventory[999001] = 3;
        world.Client.Inventory[999002] = 0;
        world.Client.Inventory[999003] = 8;
        world.Client.BozjaHolster[1] = 2;
        world.Client.BozjaHolster[3] = 5;
        var ops = world.Client.CompareToInitial();
        var inventory = ops.OfType<ClientState.OpInventoryChange>().ToArray();
        check(inventory.Any(o => o.ItemId == 999001 && o.Quantity == 3) && inventory.Any(o => o.ItemId == 999003 && o.Quantity == 8), "录制初始快照保留每种物品的数量");
        check(!inventory.Any(o => o.ItemId == 999002), "录制初始快照不写入零数量物品");
        var replay = new WorldState(10000000, "snapshot-restore-test");
        foreach (var op in ops.OfType<ClientState.OpInventoryChange>())
            replay.Execute(op);
        foreach (var op in ops.OfType<ClientState.OpBozjaHolsterChange>())
            replay.Execute(op);
        check(replay.Client.GetInventoryItemQuantity(999001) == 3 && replay.Client.GetInventoryItemQuantity(999003) == 8, "回放从快照恢复不同物品数量");
        check(replay.Client.BozjaHolster[1] == 2 && replay.Client.BozjaHolster[3] == 5, "博兹雅两种背包物品以各自ID恢复");

        ulong nextID = 1;
        Actor Create(uint oid, WPos position)
        {
            var id = nextID++;
            world.Execute(new ActorState.OpCreate(id, oid, (int)id, 0, "fixture", 0, ActorType.Enemy, Class.None, 100, new(position.X, 0, position.Z, 0), .5f, new(1000, 1000, 0, 10000, 10000), true, false, 0, 0, 0));
            return world.Actors.Find(id)!;
        }
        var boss = Create(0x4B90, new(520, -420));
        using var module = new PasDeSeul(world, boss);
        module.StateMachine.Start(world.CurrentTime);
        var heart = module.Components.OfType<GenericAOEs>().Single(c => c.GetType().Name == "HeartShatter");
        var first = Create(0x4B93, new(510, -420));
        check(heart.ActiveAOEs(0, boss).IsEmpty, "单颗心不猜测汇合点");
        var second = Create(0x4B93, new(530, -420));
        check(heart.ActiveAOEs(0, boss).Length == 1 && heart.ActiveAOEs(0, boss)[0].Origin.AlmostEqual(new(520, -420), 0.01f), "双心生成后提前显示中点范围");
        var third = Create(0x4B93, new(510, -400));
        var fourth = Create(0x4B93, new(530, -400));
        heart.OnCastFinished(first, Cast(46937, first.Position, 1));
        check(heart.ActiveAOEs(0, boss).Length == 1 && heart.ActiveAOEs(0, boss)[0].Origin.AlmostEqual(new(520, -400), 0.01f), "第一对心结算保留另一对预测");
        heart.OnCastFinished(second, Cast(46937, second.Position, 1));
        check(heart.ActiveAOEs(0, boss).Length == 1, "同一对心重复结束不清除其他预测");
        var helper = Create(0x233C, new(520, -400));
        var damage = Cast(46938, helper.Position, 1);
        heart.OnCastStarted(helper, damage);
        check(heart.ActiveAOEs(0, boss).Length == 1, "实际伤害读条接管中点范围且不重复绘制");
        heart.OnCastFinished(helper, damage);
        check(heart.ActiveAOEs(0, boss).IsEmpty, "实际伤害结束清除对应范围");
        heart.OnCastStarted(helper, damage);
        check(heart.ActiveAOEs(0, boss).Length == 1, "缺少双心生成事件时仍显示实际伤害读条");
        heart.OnCastFinished(helper, damage);
        var pending = Create(0x4B93, new(500, -420));
        world.Execute(new ActorState.OpDestroy(pending.InstanceID));
        var lone = Create(0x4B93, new(520, -420));
        check(heart.ActiveAOEs(0, boss).IsEmpty, "销毁的单颗心不会与下一轮错误配对");
        Create(0x4B93, new(530, -420));
        world.Execute(new ActorState.OpDestroy(lone.InstanceID));
        check(heart.ActiveAOEs(0, boss).IsEmpty, "双心提前销毁清除其预测");

        var type = typeof(BossModule).Assembly.GetType("BossMod.Global.HallOfTheNovice.NoviceTactical.NA02BlazingSurge", true)!;
        typeof(BossModule).GetMethod(nameof(BossModule.ActivateComponent), BindingFlags.Public | BindingFlags.Instance)!.MakeGenericMethod(type).Invoke(module, null);
        var surge = (GenericAOEs)module.Components.Single(c => c.GetType() == type);
        var late = Create(0x233C, new(520, -420));
        var early = Create(0x233C, new(520, -420));
        var lateCast = Cast(40733, late.Position, 8);
        var earlyCast = Cast(40732, early.Position, 5);
        surge.OnCastStarted(late, lateCast);
        surge.OnCastStarted(early, earlyCast);
        check(surge.ActiveAOEs(0, boss).Length == 1 && surge.ActiveAOEs(0, boss)[0].ActorID == early.InstanceID, "新手训练按实际结算顺序显示半场AOE，不依赖读条事件顺序");
        surge.OnCastFinished(early, earlyCast);
        check(surge.ActiveAOEs(0, boss).Length == 1 && surge.ActiveAOEs(0, boss)[0].ActorID == late.InstanceID, "新手训练第一段结束后显示第二半场");
        surge.OnCastFinished(late, lateCast);
        check(surge.ActiveAOEs(0, boss).IsEmpty, "新手训练连续半场结束无残留");
    }

    private static ActorCastInfo Cast(uint aid, WPos location, float duration) => new()
    {
        Action = new(ActionType.Spell, aid),
        Location = new(location.X, 0, location.Z),
        TotalTime = duration
    };
}
