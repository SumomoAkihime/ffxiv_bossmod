using System.Collections;
using System.Reflection;
using BossMod;
using BossMod.Components;
using BossMod.Stormblood.Ultimate.UCOB;
using Role = BossMod.PartyRolesConfig.Assignment;

static class UCOBSeptemberTests
{
    private const BindingFlags All = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

    private static Type Internal(string name) => typeof(UCOB).Assembly.GetType("BossMod.Stormblood.Ultimate.UCOB." + name, true)!;
    private static BossComponent Component(UCOB module, string name) => (BossComponent)Activator.CreateInstance(Internal(name), All, null, [module], null)!;
    private static FieldInfo Field(object target, string name) => target.GetType().GetField(name, All)!;
    private static void Set(object target, string name, object? value) => Field(target, name).SetValue(target, value);

    public static void Run(Action<bool, string> check)
    {
        VerifyTwisterTiming(check);
        VerifyDashLock(check);
        VerifyHatchReset(check);
        VerifyHeavensfallCleanup(check);
        VerifyGrandOctetPreposition(check);
        VerifyGrandOctetTowerAssignment(check);
        VerifyGrandOctetLateFinishCleanup(check);
    }

    private static void VerifyTwisterTiming(Action<bool, string> check)
    {
        var (world, module, player) = Fixture();
        using (module)
        {
            var config = Service.Config.Get<UCOBConfig>();
            var oldForceJump = config.TwisterForceJump;
            try
            {
                config.TwisterForceJump = true;
                var twister = (GenericTwister)Component(module, "Twister");
                var cast = new ActorCastInfo { Action = ActionID.MakeSpell(AID.Twister), TotalTime = 2f };
                var startedAt = world.CurrentTime;
                twister.OnCastStarted(module.PrimaryActor, cast);

                world.Frame = new(startedAt.AddSeconds(2.2d), 0, 0, 0, 0, 1);
                twister.Update();
                var aoes = twister.ActiveAOEs(0, player);
                var hints = new AIHints();
                twister.AddAIHints(0, player, Role.MT, hints);
                check(aoes.Length == 1 && Math.Abs((aoes[0].Activation - startedAt).TotalSeconds - 2.6d) < 0.0001d,
                    "绝巴哈旋风按读条结束加0.3秒记录真实生成时刻");
                check(hints.WantJump, "绝巴哈旋风生成前0.5秒请求跳跃刷新位置");
            }
            finally
            {
                config.TwisterForceJump = oldForceJump;
            }
        }
    }

    private static void VerifyDashLock(Action<bool, string> check)
    {
        var (world, module, player) = Fixture();
        using (module)
        {
            var chain = (UniformStackSpread)Component(module, "P2BahamutsFavorChainLightning");
            chain.AddSpread(player, world.FutureTime(5d));
            var hints = new AIHints();
            chain.AddAIHints(0, player, Role.MT, hints);
            check(hints.ForbidDashes && !hints.GoalZonesEnabled, "绝巴哈雷点名同时关闭站位奖励和位移技能");
        }
    }

    private static void VerifyHatchReset(Action<bool, string> check)
    {
        var (_, module, _) = Fixture();
        using (module)
        {
            var hatch = Component(module, "Hatch");
            Set(hatch, "_tenstrikeUntargeted", new BitMask(3ul));
            var assigned = (Actor?[])Field(hatch, "_assignedLinks").GetValue(hatch)!;
            assigned[0] = module.PrimaryActor;
            var intercepts = (IList)Field(hatch, "_intercepts").GetValue(hatch)!;
            intercepts.Add(null);

            hatch.GetType().GetMethod("Reset", All)!.Invoke(hatch, null);
            var remainingMask = (BitMask)Field(hatch, "_tenstrikeUntargeted").GetValue(hatch)!;
            check(!remainingMask.Any() && intercepts.Count == 0 && assigned.All(a => a == null), "绝巴哈鸟笼重置清除十连击分工与拦截状态");
        }
    }

    private static void VerifyHeavensfallCleanup(Action<bool, string> check)
    {
        var (_, module, player) = Fixture();
        using (module)
        {
            var trio = Component(module, "P3HeavensfallTrio");
            Set(trio, "_nael", module.PrimaryActor);
            Set(trio, "_twin", module.PrimaryActor);
            Set(trio, "_baha", module.PrimaryActor);
            Set(trio, "_divesActive", true);
            ((WPos[])Field(trio, "_safeSpots").GetValue(trio)!)[0] = new(5f, 5f);

            var hints = new AIHints();
            trio.AddAIHints(0, player, Role.MT, hints);
            check(hints.ForbiddenZones.Count == 1, "绝巴哈天地崩坏俯冲开始后显示个人安全点");

            trio.OnEventCast(module.PrimaryActor, Event(AID.TwistingDive));
            hints.Clear();
            trio.AddAIHints(0, player, Role.MT, hints);
            check(hints.ForbiddenZones.Count == 0, "绝巴哈天地崩坏俯冲结算后清除个人安全点");
        }
    }

    private static void VerifyGrandOctetPreposition(Action<bool, string> check)
    {
        var (_, module, player) = Fixture();
        using (module)
        {
            var octet = Component(module, "P3GrandOctet");
            var hints = new AIHints();
            octet.AddAIHints(0, player, Role.MT, hints);
            check(hints.ForbiddenZones.Count == 1, "绝巴哈八连俯冲分配前先在场中集合");
        }
    }

    private static void VerifyGrandOctetTowerAssignment(Action<bool, string> check)
    {
        var (_, module, _) = Fixture(8);
        using (module)
        {
            var octet = Activate(module, "P3GrandOctet");
            Set(octet, "<Twintania>k__BackingField", module.PrimaryActor);
            ((int[])Field(octet, "BaitOrder").GetValue(octet)!)[7] = 8;

            var towerComponent = (GenericTowers)Activate(module, "P3GrandOctetTower");
            for (var i = 1; i <= 4; ++i)
            {
                towerComponent.Towers.Add(new(new WPos(i, 0f), 3f));
            }
            Set(towerComponent, "_stackTargets", new BitMask(0x0ful));
            towerComponent.GetType().GetMethod("AssignTowers", All)!.Invoke(towerComponent, null);

            var allowed = towerComponent.Towers
                .Select(t => Enumerable.Range(0, 8).Single(slot => !t.ForbiddenSoakers[slot]))
                .ToArray();
            check(allowed[0] == 7 && allowed.Distinct().Count() == 4, "绝巴哈八连俯冲塔为四名非分摊玩家唯一分工，并优先安排双塔诱导者");
        }
    }

    private static void VerifyGrandOctetLateFinishCleanup(Action<bool, string> check)
    {
        var (_, module, _) = Fixture();
        using (module)
        {
            var octet = Component(module, "P3GrandOctet");
            var casters = (List<Actor>)Field(octet, "Casters").GetValue(octet)!;
            for (var i = 0; i < 8; ++i)
            {
                casters.Add(module.PrimaryActor);
            }

            var aoes = (List<GenericAOEs.AOEInstance>)Field(octet, "AOEs").GetValue(octet)!;
            aoes.Add(new(new AOEShapeCircle(1f), default, actorID: module.PrimaryActor.InstanceID));
            aoes.Add(new(new AOEShapeCircle(1f), default, actorID: 999ul));
            var cast = new ActorCastInfo { Action = ActionID.MakeSpell(AID.Cauterize1) };

            octet.OnCastFinished(module.PrimaryActor, cast);
            octet.OnCastFinished(module.PrimaryActor, cast);
            check(aoes.Count == 1 && aoes[0].ActorID == 999ul, "绝巴哈八连俯冲迟到结束事件按当前AOE数量清理且不越界");
        }
    }

    private static ActorCastEvent Event(AID action) => new(ActionID.MakeSpell(action), 0, 0, 0, default, 1, 0, default);

    private static BossComponent Activate(UCOB module, string name)
    {
        var type = Internal(name);
        typeof(BossModule).GetMethod("ActivateComponent", All)!.MakeGenericMethod(type).Invoke(module, null);
        return module.Components.Single(c => c.GetType() == type);
    }

    private static (WorldState world, UCOB module, Actor player) Fixture(int playerCount = 1)
    {
        var world = new WorldState(10000000, "ucob-september");
        world.Frame = new(DateTime.UnixEpoch.AddHours(1d), 0, 0, 0, 0, 1);

        Actor Create(ulong id, int index, uint oid, ActorType type, WPos position)
        {
            world.Execute(new ActorState.OpCreate(id, oid, index, 0, "fixture", 0, type, type == ActorType.Player ? Class.PLD : Class.None, 100,
                new(position.X, 0f, position.Z, 0f), 0.5f, default, true, type == ActorType.Player, 0, 0, 0));
            return world.Actors.Find(id)!;
        }

        Actor? player = null;
        for (var slot = 0; slot < playerCount; ++slot)
        {
            var partyMember = Create((ulong)(slot + 1), slot * 2, 0, ActorType.Player, new(5f + slot, 0f));
            world.Execute(new PartyState.OpModify(slot, new((ulong)(slot + 1), partyMember.InstanceID, false, "player" + slot)));
            player ??= partyMember;
        }
        var twintania = Create(20, 20, (uint)OID.Twintania, ActorType.Enemy, default);
        return (world, new UCOB(world, twintania), player!);
    }
}
