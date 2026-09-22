using System.Reflection;
using BossMod;
using BossMod.Autorotation;
using BossMod.Autorotation.xan;

static class UpstreamHealing20260922Tests
{
    private const BindingFlags All = BindingFlags.Instance | BindingFlags.NonPublic;

    public static void Run(RotationModuleManager manager, Actor player, AIHints hints, Action<bool, string> check)
    {
        TestCasterLimitBreak(manager, player, hints, check);
        TestBabysitNoHeal(manager, player, check);
        TestPartyCoverage(manager, player, hints, check);
    }

    private static void TestCasterLimitBreak(RotationModuleManager manager, Actor player, AIHints hints, Action<bool, string> check)
    {
        var world = manager.WorldState;
        var oldLimitBreakCur = world.Party.LimitBreakCur;
        var oldLimitBreakMax = world.Party.LimitBreakMax;
        var utility = new ClassBLMUtility(manager, player);
        var executeShared = typeof(RoleCasterUtility).GetMethod("ExecuteShared", All)!;
        var configs = ClassBLMUtility.Definition().Configs;

        try
        {
            foreach (var (level, option, action) in new[]
            {
                (1, GenericUtility.LBOption.LB1Only, ActionID.MakeSpell(BossMod.ClassShared.AID.Skyshard)),
                (2, GenericUtility.LBOption.LB2Only, ActionID.MakeSpell(BossMod.ClassShared.AID.Starstorm))
            })
            {
                world.Execute(new PartyState.OpLimitBreakChange(level * 10000, 10000));
                var strategy = new StrategyValues(configs);
                var lb = (StrategyValueTrack)strategy.Values[(int)RoleCasterUtility.SharedTrack.LB];
                lb.Option = (int)option;
                lb.Target = StrategyTarget.PointAbsolute;
                lb.Offset1 = 8;
                lb.Offset2 = -4;
                hints.Clear();
                executeShared.Invoke(utility, [strategy, ClassBLMUtility.IDLimitBreak3, null]);
                var queued = hints.ActionsToExecute.Entries.SingleOrDefault();
                check(queued.Action == action && queued.Target == null && queued.TargetPos.X == 8 && queued.TargetPos.Z == -4,
                    $"法系LB{level}使用范围动作并按地面目标位置排队");
            }
        }
        finally
        {
            world.Execute(new PartyState.OpLimitBreakChange(oldLimitBreakCur, oldLimitBreakMax));
            hints.Clear();
        }
    }

    private static void TestBabysitNoHeal(RotationModuleManager manager, Actor player, Action<bool, string> check)
    {
        var healer = new HealerAI(manager, player);
        var health = (TrackPartyHealth)typeof(HealerAI).GetField("Health", All)!.GetValue(healer)!;
        var oldNoHeal = health.PartyMemberStates[0].NoHealStatusRemaining;
        var strategy = new HealerAI.Strategy
        {
            Heal = new(HealerAI.HealMode.Babysit, new StrategyValueTrack { Target = StrategyTarget.Self }, ActionQueue.Priority.Medium)
        };

        try
        {
            foreach (var methodName in new[] { "HealSingleNow", "HealSingleSoon" })
            {
                var method = typeof(HealerAI).GetMethod(methodName, All)!;
                var calls = 0;
                Action<Actor, float> heal = (_, _) => ++calls;
                health.PartyMemberStates[0].NoHealStatusRemaining = 2;
                method.Invoke(healer, [strategy, heal]);
                check(calls == 0, $"Babysit {methodName} 跳过禁疗剩余时间超过1.5秒的目标");

                health.PartyMemberStates[0].NoHealStatusRemaining = 1.49f;
                method.Invoke(healer, [strategy, heal]);
                check(calls == 1, $"Babysit {methodName} 在禁疗剩余时间低于1.5秒时恢复治疗");
            }
        }
        finally
        {
            health.PartyMemberStates[0].NoHealStatusRemaining = oldNoHeal;
        }
    }

    private static void TestPartyCoverage(RotationModuleManager manager, Actor player, AIHints hints, Action<bool, string> check)
    {
        var world = manager.WorldState;
        var master = world.Party[1];
        var slots = Enumerable.Range(2, PartyState.MaxPartySize - 2).Where(slot => world.Party[slot] == null).Take(2).ToArray();
        if (master == null || slots.Length != 2)
        {
            check(false, "覆盖算法测试夹具需要至少四个可用队员槽位");
            return;
        }

        var savedPlayerPosition = player.PosRot;
        var savedMasterPosition = master.PosRot;
        var positions = new[]
        {
            new WPos(9.9595f, -9.8391f),
            new WPos(-13.2059f, -4.6480f),
            new WPos(-13.7152f, 2.8097f),
            new WPos(0.6681f, 13.9840f)
        };
        Actor? first = null;
        Actor? second = null;

        try
        {
            player.PosRot = new(positions[0].X, 0, positions[0].Z, 0);
            master.PosRot = new(positions[1].X, 0, positions[1].Z, 0);
            first = CreatePartyActor(world, 9001, 9001, slots[0], positions[2]);
            second = CreatePartyActor(world, 9002, 9002, slots[1], positions[3]);

            var healer = new HealerAI(manager, player);
            var health = (TrackPartyHealth)typeof(HealerAI).GetField("Health", All)!.GetValue(healer)!;
            health.Update(hints);
            var getCoverage = typeof(HealerAI).GetMethod("GetBestPartyCoverage", All)!;
            var center = (System.Numerics.Vector3)getCoverage.Invoke(healer, [15f])!;
            var covered = new[] { player, master, first, second }.All(actor =>
            {
                if (actor == null)
                    return false;
                var dx = actor.Position.X - center.X;
                var dz = actor.Position.Z - center.Z;
                return dx * dx + dz * dz <= 225;
            });
            check(covered, "半径15候选圆心搜索覆盖可共圈的四名队员");
        }
        finally
        {
            player.PosRot = savedPlayerPosition;
            master.PosRot = savedMasterPosition;
            foreach (var (actor, slot) in new[] { (first, slots[0]), (second, slots[1]) })
            {
                if (actor != null)
                {
                    world.Execute(new PartyState.OpModify(slot, PartyState.EmptySlot));
                    world.Execute(new ActorState.OpDestroy(actor.InstanceID));
                }
            }
            hints.Clear();
        }
    }

    private static Actor CreatePartyActor(WorldState world, ulong id, int index, int slot, WPos position)
    {
        world.Execute(new ActorState.OpCreate(id, 0, index, 0, "fixture", 0, ActorType.Player, Class.PLD, 100, new(position.X, 0, position.Z, 0), .5f, new(1000, 1000, 0, 10000, 10000), true, true, 0, 0, 0));
        world.Execute(new PartyState.OpModify(slot, new(0, id, false, $"coverage-{slot}")));
        return world.Actors.Find(id)!;
    }
}