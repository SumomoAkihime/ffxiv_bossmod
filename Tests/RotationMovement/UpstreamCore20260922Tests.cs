using System.Reflection;
using System.Runtime.CompilerServices;
using BossMod;

static class UpstreamCore20260922Tests
{
    public static void Run(Action<bool, string> check)
    {
        var world = new WorldState(10000000, "upstream-core-test");
        world.Frame = new(DateTime.UnixEpoch.AddHours(1), 0, 0, 0, 0, 1);
        var cases = new (uint oid, ActorType type, bool canMove)[]
        {
            (0x4DD4, ActorType.Enemy, false),
            (0x4B8E, ActorType.Enemy, false),
            (0x938, ActorType.Part, false),
            (0x938, ActorType.Enemy, true)
        };
        for (var i = 0; i < cases.Length; ++i)
        {
            var c = cases[i];
            world.Execute(new ActorState.OpCreate((ulong)i + 10, c.oid, i * 2 + 2, 0, "fixture", 0, c.type, Class.None, 100,
                default, 1, new(1000, 1000, 0, 10000, 10000), true, false, 0, 0, 0));
        }
        var builder = (AIHintsBuilder)RuntimeHelpers.GetUninitializedObject(typeof(AIHintsBuilder));
        const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
        typeof(AIHintsBuilder).GetField("_ws", flags)!.SetValue(builder, world);
        var hints = new AIHints();
        typeof(AIHintsBuilder).GetMethod("FillEnemies", flags)!.Invoke(builder, [hints, true, AIHints.Enemy.PriorityUndesirable]);
        for (var i = 0; i < cases.Length; ++i)
        {
            var actor = world.Actors.Find((ulong)i + 10)!;
            check(hints.Enemies[actor.CharacterSpawnIndex] is { } enemy && enemy.CanMove == cases[i].canMove, $"不可移动敌人判定保留普通敌人的移动能力：{i}");
        }

        var amex = typeof(AIHints).Assembly.GetType("BossMod.ActionManagerEx", true)!;
        var isGCD = amex.GetMethod("IsGCD", BindingFlags.NonPublic | BindingFlags.Static)!;
        bool Query(ActionID action) => (bool)isGCD.Invoke(null, [action])!;
        check(Query(new(ActionType.Spell, 119)), "普通法术按游戏表识别GCD");
        check(!Query(new(ActionType.Spell, 7561)) && !Query(new(ActionType.Item, 119)), "即刻咏唱与物品不等待GCD才预测读条停止");
    }
}
