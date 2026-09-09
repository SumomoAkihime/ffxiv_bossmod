using System.Collections;
using System.Reflection;
using BossMod;

var checks = 0;
var failures = 0;
// AIHints 只需要此默认配置；不加载用户配置、其他模块或游戏数据。
var configType = typeof(AIHints).Assembly.GetType("BossMod.AI.AIConfig", throwOnError: true)!;
Service.Config._nodes[configType] = (ConfigNode)Activator.CreateInstance(configType)!;
var initialize = typeof(RelSimplifiedComplexPolygon).GetMethod("VerifyPolygonIndexExistance", BindingFlags.Instance | BindingFlags.NonPublic)!;
var rebuild = typeof(RelSimplifiedComplexPolygon).GetMethod("InitPolygonIndex", BindingFlags.Instance | BindingFlags.NonPublic)!;
var indexField = typeof(RelSimplifiedComplexPolygon).GetField("_polyIndex", BindingFlags.Instance | BindingFlags.NonPublic)!;

// 每个查询都使用独立的新多边形，防止前一个检查掩盖初始化遗漏。
(string Name, Func<RelSimplifiedComplexPolygon, object> Query)[] queries =
[
    ("包含", p => p.Contains(new(6, 0))),
    ("射线", p => p.Raycast(new(6, 0), new(1, 0))),
    ("圆", p => p.PolygonCircleIntersection(new(6, 0), 2)),
    ("环", p => p.PolygonDonutIntersection(new(6, 0), 1, 3)),
    ("矩形", p => p.PolygonRectIntersection(new(6, 0), new(0, 1), 2, 3)),
    ("轴对齐矩形", p => p.PolygonAABBIntersection(new(6, 0), 2, 3)),
    ("定向矩形", p => p.PolygonDirectionalRectIntersection(new(6, 0), new(0, 1), 3, 1, 2)),
    ("胶囊", p => p.PolygonCapsuleIntersection(new(6, 0), new(0, 1), 3, 2)),
    ("最近边界", p => p.ClosestPointOnBoundary(new(6, 0))),
    ("可见区域", p => p.Visibility(new(6, 0))),
    ("击退方向", p =>
    {
        var hints = new AIHints();
        p.AddForbiddenDirections(new(25, 0), default, hints, DateTime.UnixEpoch, 40);
        return hints.ForbiddenDirections.ToArray();
    }),
    ("距离优先", p => new SDPolygonWithHolesBase(default, p).Distance(new(6, 0))),
    ("反转距离优先", p => new SDPolygonWithHolesBase(default, p).DistanceInverted(new(6, 0)))
];

(string Name, Func<RelSimplifiedComplexPolygon> Create, Func<WDir, bool> Contains)[] cases =
[
    ("空", () => new(), _ => false),
    ("方形", () => Square(10), p => InSquare(p, 10)),
    ("带孔", Ring, p => InSquare(p, 10) && !InSquare(p, 3)),
    ("变换", () => Square(10).Transform(new(5, 3), new(1, 0)), p => InSquare(p - new WDir(5, 3), 10)),
    ("扩张", () => Square(10).Offset(1), p => InSquare(p, 11)),
    ("收缩", () => Square(10).Offset(-1), p => InSquare(p, 9)),
    ("差集", () => new PolygonClipper().Difference(new(Square(10)), new(Square(3))), p => InSquare(p, 10) && !InSquare(p, 3)),
    ("分离区域", () => new([new RelPolygonWithHoles(Vertices(4, -10)), new RelPolygonWithHoles(Vertices(4, 10))]), p => InSquare(p - new WDir(-10, 0), 4) || InSquare(p - new WDir(10, 0), 4)),
    ("孔中孤岛", () => new PolygonClipper().Union(new(Ring()), new(Square(1))), p => InSquare(p, 10) && (!InSquare(p, 3) || InSquare(p, 1)))
];

foreach (var (name, create, contains) in cases)
{
    foreach (var (queryName, query) in queries)
        Run($"{name}/{queryName}/首次与预热一致", () =>
        {
            var cold = create();
            var warm = create();
            initialize.Invoke(warm, null);
            var expected = query(warm);
            Equal(query(cold), expected);
            var index = indexField.GetValue(cold);
            Equal(query(cold), expected);
            Require(index != null && ReferenceEquals(index, indexField.GetValue(cold)), "查询未复用索引");
        });

    Run($"{name}/独立几何判定", () =>
    {
        var polygon = create();
        for (var z = -30; z <= 30; ++z)
            for (var x = -30; x <= 30; ++x)
            {
                // 避开边界，期望值只用解析矩形计算，不依赖被测索引。
                var point = new WDir(x * .5f + .13f, z * .5f + .17f);
                Equal(polygon.Contains(point), contains(point));
            }
    });
}

Run("已知距离与射线", () =>
{
    Equal(Square(10).Raycast(default, new(1, 0)), 10f);
    Equal(new SDPolygonWithHolesBase(default, Square(10)).Distance(new(13, 0)), 3f);
    // 距离类仅搜索当前空间网格的边，选取与左边界同格的内部点。
    Equal(new SDPolygonWithHolesBase(default, Square(10)).DistanceInverted(new(-7, 0)), 3f);
    Equal(Square(10).ClosestPointOnBoundary(new(13, 0)), new WDir(10, 0));
});

Run("并发首次查询与回收", () =>
{
    for (var round = 0; round < 32; ++round)
    {
        var polygon = Ring();
        var indexes = new object?[8];
        using var ready = new CountdownEvent(indexes.Length);
        using var start = new ManualResetEventSlim();
        var tasks = Enumerable.Range(0, indexes.Length).Select(slot => Task.Factory.StartNew(() =>
        {
            ready.Signal();
            start.Wait();
            var result = polygon.Contains(new(6, 0));
            indexes[slot] = indexField.GetValue(polygon);
            return result;
        }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default)).ToArray();
        ready.Wait();
        start.Set();
        Task.WaitAll(tasks);
        Require(tasks.All(t => t.Result), "并发包含结果错误");
        Require(indexes[0] != null && indexes.All(i => ReferenceEquals(i, indexes[0])), "并发查询发布了不同索引");
        GC.Collect();
        GC.WaitForPendingFinalizers();
        Equal(polygon.Contains(new(6, 0)), true);
    }
});

Run("显式重建", () =>
{
    var polygon = Square(10);
    Equal(polygon.Contains(default), true);
    polygon.Parts.Clear();
    polygon.Parts.Add(new(Vertices(4, 20)));
    rebuild.Invoke(polygon, null);
    Equal(polygon.Contains(default), false);
    Equal(polygon.Contains(new(20, 0)), true);
});

// Prishe 两种对角布局，交换运行参数可在独立进程中验证距离先于范围检查。
foreach (var diagonal in new[] { false, true })
    Run($"Prishe 对角={diagonal}", () =>
    {
        var origin = new WPos(800, 400);
        BossMod.Square[] squares = diagonal
            ? [new(new(795, 395), 10), new(new(805, 405), 10)]
            : [new(new(795, 405), 10), new(new(805, 395), 10)];
        for (var repeat = 0; repeat < 2; ++repeat)
        {
            var shape = new AOEShapeCustom(squares, invertForbiddenZone: true);
            if (args.Contains("--distance-first"))
                Require(shape.Distance(origin, default).Distance(origin) > 0, "距离优先失败");
            Equal(shape.Check(origin, origin, default), true);
            Equal(shape.Check(new(830, 400), origin, default), false);
            Require(shape.Distance(origin, default).Distance(origin) > 0, "反转距离错误");
            Equal(shape.Distance(origin, default).Distance(new(830, 400)), 0f);
        }
    });

Console.WriteLine($"断言：{checks}，失败用例：{failures}");
return failures == 0 ? 0 : 1;

void Run(string name, Action test)
{
    try
    {
        test();
        Console.WriteLine($"通过：{name}");
    }
    catch (Exception ex)
    {
        ++failures;
        Console.WriteLine($"失败：{name}：{ex.GetBaseException().Message}");
    }
}

void Require(bool condition, string message)
{
    ++checks;
    if (!condition)
        throw new InvalidOperationException(message);
}

void Equal(object actual, object expected) => Require(StructuralComparisons.StructuralEqualityComparer.Equals(actual, expected), $"实际 {actual}，预期 {expected}");

static List<WDir> Vertices(float halfSize, float x = 0) =>
    [new(x - halfSize, -halfSize), new(x + halfSize, -halfSize), new(x + halfSize, halfSize), new(x - halfSize, halfSize)];

static RelSimplifiedComplexPolygon Square(float halfSize) => new(Vertices(halfSize));

static RelSimplifiedComplexPolygon Ring()
{
    var part = new RelPolygonWithHoles(Vertices(10));
    var hole = Vertices(3);
    hole.Reverse();
    part.AddHole(hole);
    return new([part]);
}

static bool InSquare(WDir point, float halfSize) => MathF.Abs(point.X) < halfSize && MathF.Abs(point.Z) < halfSize;
