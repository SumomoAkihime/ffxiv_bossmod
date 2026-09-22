using System.Reflection;
using BossMod;
using BossMod.Components;
using BossMod.Stormblood.Ultimate.UCOB;
using Role = BossMod.PartyRolesConfig.Assignment;

static class UpstreamUCOB20260922Tests
{
    private const BindingFlags All = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

    private static Type Internal(string name) => typeof(UCOB).Assembly.GetType("BossMod.Stormblood.Ultimate.UCOB." + name, true)!;
    private static BossComponent Component(UCOB module, string name) => (BossComponent)Activator.CreateInstance(Internal(name), All, null, [module], null)!;
    private static FieldInfo Field(object target, string name) => target.GetType().GetField(name, All)!;

    public static void Run(Action<bool, string> check)
    {
        VerifyTwisterCastLock(check);
        VerifyDalamudDiveCleanupAndReturn(check);
        VerifyHeavensfallPendingKnockback(check);
        VerifyMornAfahHints(check);
        VerifyAkhMornSharing(check);
        VerifyLiquidHellObstacle(check);
        VerifyCenterGathering(check);
    }

    private static void VerifyTwisterCastLock(Action<bool, string> check)
    {
        var (world, module, players, _, _, _) = Fixture();
        using (module)
        {
            var twister = Component(module, "Twister");
            var startedAt = world.CurrentTime;
            twister.OnCastStarted(module.PrimaryActor, new ActorCastInfo { Action = ActionID.MakeSpell(AID.Twister), TotalTime = 2f });
            world.Frame = new(startedAt.AddSeconds(1.4d), 0, 0, 0, 0, 1);

            var hints = new AIHints();
            twister.AddAIHints(0, players[0], Role.MT, hints);
            check(hints.MaxCastTime == 0f, "绝巴哈旋风预测前0.5秒禁止继续读条");

            world.Frame = new(startedAt.AddSeconds(1.9d), 0, 0, 0, 0, 1);
            twister.Update();
            hints.Clear();
            twister.AddAIHints(0, players[0], Role.MT, hints);
            check(hints.MaxCastTime == 0f, "绝巴哈旋风已记录玩家位置且实体未生成时继续禁止读条");
        }
    }

    private static void VerifyDalamudDiveCleanupAndReturn(Action<bool, string> check)
    {
        var (_, module, players, nael, _, _) = Fixture(withNael: true);
        using (module)
        {
            module.PrimaryActor.TargetID = players[0].InstanceID;
            var dive = (GenericBaitAway)Component(module, "P2HeavensfallDalamudDive");
            dive.GetType().GetMethod("Show", All)!.Invoke(dive, null);
            check(dive.CurrentBaits.Count == 1, "绝巴哈P2俯冲显示当前坦克诱导");

            dive.OnEventCast(nael!, Event(AID.DalamudDive));
            var hints = new AIHints();
            dive.AddAIHints(0, players[0], Role.MT, hints);
            check(dive.CurrentBaits.Count == 0 && hints.GoalZones.Count == 1 && hints.GoalZones[0](nael!.Position) > hints.GoalZones[0](players[0].Position),
                "绝巴哈P2俯冲命中后清除诱导并向未选中的Nael回位");
        }
    }

    private static void VerifyHeavensfallPendingKnockback(Action<bool, string> check)
    {
        var (world, module, players, _, _, _) = Fixture();
        using (module)
        {
            var towers = (GenericTowers)Component(module, "P3HeavensfallTowers");
            var activation = world.FutureTime(10d);
            towers.Towers.Add(new(new WPos(0f, 5f), 3f, activation: activation));
            towers.OnEventCast(module.PrimaryActor, Event(AID.Heavensfall));
            players[0].PendingKnockbacks.Add(default);

            var hints = new AIHints();
            towers.AddAIHints(0, players[0], Role.MT, hints);
            check(hints.ForbiddenZones.Count == 1 && hints.ForbiddenZones[0].activation == activation.AddSeconds(-2.5d),
                "绝巴哈天地崩坏效果待结算时保持击退前塔位");

            players[0].PendingKnockbacks.Clear();
            hints.Clear();
            towers.AddAIHints(0, players[0], Role.MT, hints);
            check(hints.ForbiddenZones.Count == 1 && hints.ForbiddenZones[0].activation == activation,
                "绝巴哈天地崩坏效果结算后切换普通踩塔约束");
        }
    }

    private static void VerifyMornAfahHints(Action<bool, string> check)
    {
        var (_, module, players, _, bahamut, _) = Fixture(2, withBahamut: true);
        using (module)
        {
            var mornAfah = Component(module, "P5MornAfah");
            mornAfah.OnCastStarted(bahamut!, new ActorCastInfo
            {
                Action = ActionID.MakeSpell(AID.MornAfah),
                TargetID = players[0].InstanceID,
                TotalTime = 6f
            });

            var hints = new AIHints();
            mornAfah.AddAIHints(0, players[0], Role.MT, hints);
            check(hints.ForbiddenZones.Count == 1 && hints.ForbiddenZones[0].shapeDistance.Distance(bahamut!.Position) > 0f,
                "绝巴哈P5无尽顿悟要求贴近巴哈姆特集合");
            check(hints.PredictedDamage.Count == 1 && hints.PredictedDamage[0].Players[0] && hints.PredictedDamage[0].Players[1],
                "绝巴哈P5无尽顿悟登记全队预伤害");
        }
    }

    private static void VerifyAkhMornSharing(Action<bool, string> check)
    {
        var (_, module, players, _, bahamut, _) = Fixture(2, withBahamut: true);
        using (module)
        {
            var akhMorn = (GenericSharedTankbuster)Component(module, "P5AhkMorn");
            akhMorn.OnCastStarted(bahamut!, new ActorCastInfo
            {
                Action = ActionID.MakeSpell(AID.AkhMorn),
                TargetID = players[0].InstanceID,
                TotalTime = 4f
            });

            Field(akhMorn, "Shared").SetValue(akhMorn, false);
            var hints = new AIHints();
            akhMorn.AddAIHints(1, players[1], Role.OT, hints);
            check(!akhMorn.IsShared && hints.ForbiddenZones.Count == 1 && hints.ForbiddenZones[0].shapeDistance.Distance(players[0].Position) < 0f,
                "绝巴哈P5单吃轮让副坦避开死刑范围");

            Field(akhMorn, "Shared").SetValue(akhMorn, true);
            hints.Clear();
            akhMorn.AddAIHints(1, players[1], Role.OT, hints);
            check(akhMorn.IsShared && hints.ForbiddenZones.Count == 1 && hints.ForbiddenZones[0].shapeDistance.Distance(players[0].Position) > 0f,
                "绝巴哈P5分摊轮要求副坦进入死刑范围");
        }
    }

    private static void VerifyLiquidHellObstacle(Action<bool, string> check)
    {
        var (_, module, players, _, _, _) = Fixture(withLiquidHell: true);
        using (module)
        {
            var liquidHell = Component(module, "LiquidHell");
            var hints = new AIHints();
            liquidHell.AddAIHints(0, players[0], Role.MT, hints);
            check(hints.ForbiddenZones.Count == 0 && hints.TemporaryObstacles.Count == 1,
                "绝巴哈火圈对圈外玩家作为寻路障碍而非定时危险区");
        }
    }

    private static void VerifyCenterGathering(Action<bool, string> check)
    {
        var (_, module, players, _, _, _) = Fixture();
        using (module)
        {
            var quote = Component(module, "Quote");
            Field(quote, "Source").SetValue(quote, module.PrimaryActor);
            var pending = (List<uint>)Field(quote, "PendingMechanics").GetValue(quote)!;
            pending.Add((uint)AID.IronChariot);
            pending.Add((uint)AID.ThermionicBeam);
            var hints = new AIHints();
            quote.AddAIHints(0, players[0], Role.MT, hints);
            check(hints.GoalZones.Count == 1 && hints.GoalZones[0](module.PrimaryActor.Position) > hints.GoalZones[0](players[0].Position),
                "绝巴哈月环接分摊时提前向台词来源集合");

            var blackfire = Component(module, "P3BlackfireTrio");
            blackfire.OnEventCast(module.PrimaryActor, Event(AID.Hypernova));
            blackfire.OnEventCast(module.PrimaryActor, Event(AID.Hypernova));
            hints.Clear();
            blackfire.AddAIHints(0, players[0], Role.MT, hints);
            check(hints.GoalZones.Count == 1 && hints.GoalZones[0](module.Center) > hints.GoalZones[0](players[0].Position),
                "绝巴哈黑火三连第二个黑球后向场中集合");
        }
    }

    private static ActorCastEvent Event(AID action) => new(ActionID.MakeSpell(action), 0, 0, 0, default, 1, 0, default);

    private static (WorldState world, UCOB module, Actor[] players, Actor? nael, Actor? bahamut, Actor? liquidHell) Fixture(
        int playerCount = 1, bool withNael = false, bool withBahamut = false, bool withLiquidHell = false)
    {
        var world = new WorldState(10000000, "ucob-20260922");
        world.Frame = new(DateTime.UnixEpoch.AddHours(1d), 0, 0, 0, 0, 1);

        Actor Create(ulong id, int index, uint oid, ActorType type, WPos position)
        {
            world.Execute(new ActorState.OpCreate(id, oid, index, 0, "fixture", 0, type, type == ActorType.Player ? Class.PLD : Class.None, 100,
                new(position.X, 0f, position.Z, 0f), 0.5f, default, true, type == ActorType.Player, 0, 0, 0));
            return world.Actors.Find(id)!;
        }

        var players = new Actor[playerCount];
        for (var slot = 0; slot < playerCount; ++slot)
        {
            players[slot] = Create((ulong)(slot + 1), slot * 2, 0, ActorType.Player, new(10f + slot * 2f, 0f));
            world.Execute(new PartyState.OpModify(slot, new((ulong)(slot + 1), players[slot].InstanceID, false, "player" + slot)));
        }

        var twintania = Create(20, 20, (uint)OID.Twintania, ActorType.Enemy, new(0f, -5f));
        var nael = withNael ? Create(21, 21, (uint)OID.NaelDeusDarnus, ActorType.Enemy, new(0f, 5f)) : null;
        var bahamut = withBahamut ? Create(22, 22, (uint)OID.BahamutPrime, ActorType.Enemy, default) : null;
        var liquidHell = withLiquidHell ? Create(23, 23, (uint)OID.VoidzoneLiquidHell, ActorType.EventObj, default) : null;
        if (nael != null)
        {
            nael.IsTargetable = false;
        }
        return (world, new UCOB(world, twintania), players, nael, bahamut, liquidHell);
    }
}
