using BossMod;
using BossMod.Components;
using Inferno = BossMod.Stormblood.Dungeon.D05CastrumAbania.D053Inferno.D053Inferno;

static class CastrumUpstreamTests
{
    public static void Run(Action<bool, string> check)
    {
        var world = new WorldState(10000000, "castrum-upstream-test");
        world.Frame = new(DateTime.UnixEpoch.AddHours(1), 0, 0, 0, 0, 1);
        var center = new WPos(282.5f, -27.25f);
        ulong nextID = 1;
        Actor Create(uint oid, WPos position)
        {
            var id = nextID++;
            world.Execute(new ActorState.OpCreate(id, oid, (int)id, 0, "fixture", 0, ActorType.Enemy, Class.None, 100, new(position.X, 0, position.Z, 0), .5f, new(1000, 1000, 0, 10000, 10000), true, false, 0, 0, 0));
            return world.Actors.Find(id)!;
        }
        var boss = Create(0x1AAE, center);
        using var module = new Inferno(world, boss);
        module.StateMachine.Start(world.CurrentTime);
        var wave = module.Components.OfType<SimpleAOEs>().Single(c => c.GetType().Name == "KetuWave");
        var cutter = module.Components.OfType<SimpleAOEs>().Single(c => c.GetType().Name == "KetuCutter");
        var knockbacks = module.Components.OfType<GenericKnockback>().ToArray();
        check(knockbacks.Length == 2, "堡垒两种距离的彗星击退均激活");
        var coneHelper = Create(0x18D6, center);
        var otherConeHelper = Create(0x18D6, center);
        var circleHelper = Create(0x18D6, center + new WDir(10, 0));
        var cone = Cast(7975, center);
        var circle = Cast(7976, circleHelper.Position);
        cutter.OnCastStarted(coneHelper, cone);
        foreach (var kb in knockbacks)
        {
            check(kb.DestinationUnsafe(0, boss, center + new WDir(0, 8)), "只有扇形时正确判定击退危险落点且不越界");
            check(!kb.DestinationUnsafe(0, boss, center + new WDir(-12, 0)), "只有扇形时保留安全落点");
        }
        wave.OnCastStarted(circleHelper, circle);
        cutter.OnCastStarted(otherConeHelper, cone);
        foreach (var kb in knockbacks)
        {
            check(kb.DestinationUnsafe(0, boss, center + new WDir(0, 8)), "扇形多于圆形时检查正确列表且不越界");
            check(kb.DestinationUnsafe(0, boss, circleHelper.Position), "复合机制仍将圆形内落点判为危险");
            check(!kb.DestinationUnsafe(0, boss, center + new WDir(-12, 0)), "圆形扇形同时存在时保留共同安全落点");
        }
        cutter.OnCastFinished(coneHelper, cone);
        cutter.OnCastFinished(otherConeHelper, cone);
        foreach (var kb in knockbacks)
            check(!kb.DestinationUnsafe(0, boss, center + new WDir(0, 8)), "扇形结束后释放原危险落点");
        wave.OnCastFinished(circleHelper, circle);
        foreach (var kb in knockbacks)
            check(!kb.DestinationUnsafe(0, boss, circleHelper.Position), "圆形结束后清除落点限制");
    }

    private static ActorCastInfo Cast(uint aid, WPos location) => new()
    {
        Action = new(ActionType.Spell, aid),
        Location = new(location.X, 0, location.Z),
        TotalTime = 4
    };
}
