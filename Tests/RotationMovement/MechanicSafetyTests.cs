using System.Reflection;
using BossMod;
using BossMod.Components;
using BossMod.Pathfinding;

static class MechanicSafetyTests
{
    const BindingFlags All = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
    const string CE = "BossMod.Dawntrail.Foray.CriticalEngagement.";
    const string Tower = "BossMod.Dawntrail.Foray.ForkedTowerMagic.Extreme.";

    static Type Internal(string name) => typeof(AIHints).Assembly.GetType(name, true)!;
    static ShapeDistance Shape(string name, params object?[] args) =>
        (ShapeDistance)Activator.CreateInstance(Internal(name), All, null, args, null)!;

    public static void Run(BossModule module, Actor player, Action<bool, string> check)
    {
        var origin = new WPos(0, 0);
        ShapeDistance[] none = [];
        var chainName = CE + "CE201ABeastUnleashed.StarvingDread+UnsafeKnockbackPositions";
        var chain = Shape(chainName, origin, 19f, new WDir(0, 1), new WPos(0, 20), none, none);
        check(!chain.Contains(new(0, 1)), "两段击退均在场内的准备点可用");
        check(chain.Contains(new(1, -1)), "首段留在场内、第二段出场的准备点被禁止");
        var firstOnly = Shape(chainName, origin, 19f, new WDir(0, 1), null, none, none);
        check(!firstOnly.Contains(new(1, -1)), "第二段抗击退时不施加不存在的位移");
        var danger = new AOEShapeCircle(2f).Distance(new(0, -14), default);
        var compound = Shape(chainName, origin, 19f, new WDir(0, 1), new WPos(0, 20), none, new[] { danger });
        check(compound.Contains(new(0, 1)), "留在场内但落入同期 AOE 的位置被禁止");

        var mergeName = CE + "CE214TinyTerror.FlareHolyMerge+MergeSafety";
        var merge = Shape(mergeName, origin, new (WPos, bool, bool)[] { (origin, false, false), (new(0, 20), false, false) }, new[] { none, none });
        check(!merge.Contains(new(0, 1)) && merge.Contains(new(1, 1)), "小小法师第二击从第一击落点继续投影");
        var flareAfter = Shape(mergeName, origin, new (WPos, bool, bool)[] { (origin, false, false), (new(0, -10), true, false) }, new[] { none, none });
        check(!flareAfter.Contains(new(0, 1)), "击退前位于未来核爆内、击退后离开时可用");
        var immune = Shape(mergeName, origin, new (WPos, bool, bool)[] { (origin, false, true), (new(0, -10), true, false) }, new[] { none, none });
        check(immune.Contains(new(0, 1)), "抗击退时未来核爆必须检查原地");

        var square = new AOEShapeCustom([new Square(origin, 20f)]).GetCombinedPolygon(origin);
        var shockName = Tower + "FTME4Index.Shockwave+ShockwaveSafety";
        var shock = Shape(shockName, origin, square, new WPos[] { new(-15, 0), new(15, 0) }, none);
        check(!shock.Contains(new(-14, 0)) && !shock.Contains(new(14, 0)), "每个候选点选择自己的最近击退源");
        var hole = new AOEShapeCustom([new Square(origin, 20f)], [new Circle(origin, 1f)]).GetCombinedPolygon(origin);
        var crossing = Shape(shockName, origin, hole, new WPos[] { new(0, 20) }, none);
        check(crossing.Contains(new(0, 6)), "起点和终点都在平台内、途中跨过中央空洞仍被禁止");

        var soilType = Internal(Tower + "FTME3Necrophobia.FertileSoil");
        var soil = (BossComponent)Activator.CreateInstance(soilType, module)!;
        var head = new Actor(998, 0, 0, 0, "屏障头测试", 0, ActorType.Enemy, Class.None, 100, new(100, 0, 780, 0));
        var prediction = Activator.CreateInstance(soilType.GetNestedType("Prediction", BindingFlags.NonPublic)!, All, null,
            [head, new DateTime(2026, 9, 11), (ushort)1117], null)!;
        var order = (System.Collections.IList)soilType.GetField("_order", All)!.GetValue(soil)!;
        order.Add(prediction);
        var savedStatuses = player.Statuses.ToArray();
        try
        {
            Array.Clear(player.Statuses);
            player.Statuses[0] = new(5136, 0, DateTime.MaxValue, 0);
            var soilHints = new AIHints();
            soil.AddAIHints(0, player, default, soilHints);
            var forbidden = soilHints.ForbiddenZones.Single().shapeDistance;
            check(!forbidden.Contains(new(90, 790)) && forbidden.Contains(new(110, 790)), "实际蓝色播撒状态选择红色通道");
            player.Statuses[0] = new(5137, 0, DateTime.MaxValue, 0);
            soilHints.Clear();
            soil.AddAIHints(0, player, default, soilHints);
            forbidden = soilHints.ForbiddenZones.Single().shapeDistance;
            check(forbidden.Contains(new(90, 790)) && !forbidden.Contains(new(110, 790)), "状态翻色立即改变约束，不依赖已结算次数");
            var elements = (List<BossMod.Shape>)soilType.GetField("_elementDangers", All)!.GetValue(soil)!;
            elements.Add(new Circle(new(110, 790), 3f));
            soilHints.Clear();
            soil.AddAIHints(0, player, default, soilHints);
            check(soilHints.ForbiddenZones.Single().shapeDistance.Contains(new(110, 790)), "正确通道仍要扣除同期元素危险范围");
        }
        finally
        {
            savedStatuses.CopyTo(player.Statuses, 0);
        }

        // Exercise the product navigation implementation rather than only mirroring its geometry.
        var now = new DateTime(2026, 9, 11, 0, 0, 0, DateTimeKind.Utc);
        var probe = new Actor(999, 0, 0, 0, "安全测试", 0, ActorType.Player, Class.PLD, 100, new(1, 0, -1, 0));
        var hints = new AIHints { PathfindMapCenter = origin, PathfindMapBounds = new ArenaBoundsSquare(19f), GoalZonesEnabled = false };
        hints.AddForbiddenZone(chain, now.AddSeconds(10));
        hints.GoalZones.Add(p => p.InCircle(new(0, -15), 2f) ? 1000f : 0f);
        hints.Normalize();
        var decision = NavigationDecision.Build(new(), now, hints, probe);
        check(decision.Destination is WPos destination && !chain.Contains(destination), "真实寻路选择两段安全准备点，输出目标不覆盖安全约束");

        // Actual component lifecycle: the first hit clears only its own stage, even with duplicate helpers.
        var component = (GenericKnockback)Activator.CreateInstance(Internal(CE + "CE201ABeastUnleashed.StarvingDread"), module)!;
        var type = component.GetType();
        type.GetField("_firstDirection", All)!.SetValue(component, new Angle(0));
        type.GetField("_firstActivation", All)!.SetValue(component, now.AddSeconds(5));
        type.GetField("_second", All)!.SetValue(component, new GenericKnockback.Knockback(new(0, 20), 30f, now.AddSeconds(8.5)));
        ActorCastEvent Hit(uint aid) => new(new(ActionType.Spell, aid), 0, 0, 0, default, 1, 0, default);
        component.OnEventCast(player, Hit(49507));
        component.OnEventCast(player, Hit(49507));
        check(component.ActiveKnockbacks(0, player).Length == 1, "重复首段伤害不提前清掉第二段");
        var remaining = new AIHints();
        component.AddAIHints(0, player, default, remaining);
        check(remaining.ForbiddenZones.Count == 1 && remaining.ForbiddenZones[0].activation == module.WorldState.CurrentTime && !remaining.GoalZonesEnabled,
            "首段后第二段约束立即生效，并暂停输出位置评分");
        component.OnEventCast(player, Hit(49506));
        remaining.Clear();
        component.AddAIHints(0, player, default, remaining);
        check(remaining.ForbiddenZones.Count == 0 && remaining.GoalZonesEnabled, "连段结束释放约束，恢复输出位置评分");
    }
}
