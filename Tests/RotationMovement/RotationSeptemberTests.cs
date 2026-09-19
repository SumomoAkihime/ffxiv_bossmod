using BossMod.Autorotation;
using BossMod.Autorotation.xan;
using BossMod;
using System.Reflection;

static class RotationSeptemberTests
{
    public static void Run(RotationModuleManager manager, Actor player, AIHints hints, Action<bool, string> check)
    {
        check((int)MNK.PBStrategy.Automatic == 0 && (int)MNK.PBStrategy.DowntimeLunar == 5 && (int)MNK.PBStrategy.ForceNoShift == 6, "武僧绝技策略追加选项不改变旧预设序列化值");

        var world = new WorldState(10000000, "party-health-test");
        world.Frame = new(DateTime.UnixEpoch.AddHours(1), 0, 0, 0, 0, 1);
        Actor Create(ulong id, ActorType type, uint hp) => new(id, 0, (int)id, 0, "fixture", 0, type, type == ActorType.Player ? Class.PLD : Class.None, 100, default, hpmp: new(hp, 1000, 0, 10000, 10000), ally: true);
        var partyPlayer = Create(1, ActorType.Player, 1000);
        var npc = Create(2, ActorType.EventNpc, 100);
        world.Actors.Actors.Add(partyPlayer.InstanceID, partyPlayer);
        world.Actors.Actors.Add(npc.InstanceID, npc);
        world.Execute(new PartyState.OpModify(0, new(1, partyPlayer.InstanceID, false, "partyPlayer")));
        world.Execute(new PartyState.OpModify(24, new(0, npc.InstanceID, false, "npc")));

        var health = new TrackPartyHealth(world);
        health.Update(new AIHints());
        check(health.TrackedMembers.Any(m => m.Item2 == npc) && health.PartyHealth.Count == 1 && health.BestSTHealTarget?.Target == npc, "队员群体统计排除NPC，但更低血NPC仍可作为单体治疗目标");

        var originalClass = player.Class;
        var originalLevel = player.Level;
        try
        {
            player.Class = Class.MNK;
            player.Level = 100;
            var mnk = new MNK(manager, player);
            var chakras = typeof(MNK).GetField("BeastChakra")!;
            chakras.SetValue(mnk, Array.CreateInstance(chakras.FieldType.GetElementType()!, 3));
            var queuePB = typeof(MNK).GetMethod("QueuePB", BindingFlags.Instance | BindingFlags.NonPublic)!;
            var strategy = new MNK.Strategy { PB = new(MNK.PBStrategy.ForceNoShift, new StrategyValueTrack(), 1) };
            bool Queued(float formShiftLeft)
            {
                mnk.FormShiftLeft = formShiftLeft;
                hints.Clear();
                queuePB.Invoke(mnk, [strategy, null]);
                return hints.ActionsToExecute.Entries.Any(e => e.Action == ActionID.MakeSpell(BossMod.MNK.AID.PerfectBalance));
            }
            check(Queued(0) && !Queued(0.1f), "武僧ForceNoShift仅在无无相状态时将绝技加入动作队列");
        }
        finally
        {
            player.Class = originalClass;
            player.Level = originalLevel;
        }
    }
}
