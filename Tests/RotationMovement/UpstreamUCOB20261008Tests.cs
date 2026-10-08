using System.Reflection;
using BossMod;
using BossMod.Components;
using BossMod.Stormblood.Ultimate.UCOB;
using Role = BossMod.PartyRolesConfig.Assignment;

static class UpstreamUCOB20261008Tests
{
    private const BindingFlags All = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
    private static Type Internal(string name) => typeof(UCOB).Assembly.GetType("BossMod.Stormblood.Ultimate.UCOB." + name, true)!;
    private static BossComponent Component(UCOB module, string name) => (BossComponent)Activator.CreateInstance(Internal(name), All, null, [module], null)!;
    private static FieldInfo Field(object target, string name)
    {
        for (var type = target.GetType(); type != null; type = type.BaseType)
            if (type.GetField(name, All) is { } field)
                return field;
        throw new MissingFieldException(target.GetType().FullName, name);
    }

    public static void Run(Action<bool, string> check)
    {
        VerifyTwisterLifecycle(check);
        VerifyHatchWait(check);
        VerifyLiquidHellLifecycle(check);
        VerifyBlackfireMovement(check);
        VerifyBahamutLifecycle(check);
        VerifyScriptedDeathLifecycle(check);
        VerifyPhaseTransitions(check);
    }

    private static void VerifyTwisterLifecycle(Action<bool, string> check)
    {
        var (world, module, player) = Fixture();
        using (module)
        {
            foreach (var name in new[] { "Twister", "P1Twister" })
            {
                player.PosRot = new(10f, player.PosRot.Y, 0f, player.PosRot.W);
                var twister = (CastTwister)Component(module, name);
                var start = world.CurrentTime;
                twister.OnCastStarted(module.PrimaryActor, new ActorCastInfo { Action = ActionID.MakeSpell(AID.Twister), TotalTime = 2f });
                world.Frame = new(start.AddSeconds(1.94d), 0, 0, 0, 0, 1);
                twister.Update();
                var hints = new AIHints();
                twister.AddAIHints(0, player, Role.MT, hints);
                check(twister.PredictionTime == 0.65d && twister.ActiveAOEs(0, player).Length == 0 && hints.ForcedMovement == new System.Numerics.Vector3(0f)
                    && hints.ForceCancelCast && hints.MaxCastTime == 0f, name + "：计入NPC延迟后1.94秒仍停止移动并取消读条");

                world.Frame = new(start.AddSeconds(1.96d), 0, 0, 0, 0, 1);
                twister.Update();
                hints.Clear();
                twister.AddAIHints(0, player, Role.MT, hints);
                var aoes = twister.ActiveAOEs(0, player);
                check(aoes.Length == 1 && Math.Abs((aoes[0].Activation - start).TotalSeconds - 2.6d) < 0.0001d, name + "：1.95秒记录位置并保留2.6秒绝对生效时间");
                check(hints.ForbiddenZones.Count == 1 && hints.ForbiddenZones[0].shapeDistance.Distance(player.Position) < 0f
                    && hints.ForcedMovement == null && !hints.ForceCancelCast && !hints.GoalZonesEnabled, name + "：预测后释放移动并关闭圈内站位奖励");
                player.PosRot = new(15f, player.PosRot.Y, 5f, player.PosRot.W);
                hints.Clear();
                twister.AddAIHints(0, player, Role.MT, hints);
                check(hints.GoalZonesEnabled && hints.ForbiddenZones[0].shapeDistance.Distance(player.Position) > 0f, name + "：离开预测圈后恢复站位奖励");

                var actual = Create(world, 30, (uint)OID.VoidzoneTwister, ActorType.EventObj, new(2f, 3f));
                twister.OnActorCreated(actual);
                world.Frame = new(start.AddSeconds(2.7d), 0, 0, 0, 0, 1);
                hints.Clear();
                twister.AddAIHints(0, player, Role.MT, hints);
                check(twister.ActiveAOEs(0, player).Length == 1 && hints.ForbiddenZones.Count == 1
                    && hints.ForbiddenZones[0].shapeDistance.Distance(actual.Position) < 0f, name + "：生成实体后用真实位置替换预测");
                actual.EventState = 7;
                hints.Clear();
                twister.AddAIHints(0, player, Role.MT, hints);
                check(hints.ForbiddenZones.Count == 0 && hints.ForcedMovement == null && hints.GoalZonesEnabled, name + "：实际旋风结束后清除危险与停步");
                world.Execute(new ActorState.OpDestroy(actual.InstanceID));
            }
        }
    }

    private static void VerifyHatchWait(Action<bool, string> check)
    {
        var (world, module, player) = Fixture();
        using (module)
        {
            var hatch = Component(module, "Hatch");
            var link = Create(world, 31, (uint)OID.Neurolink, ActorType.EventObj, new(0f, -8f));
            hatch.OnActorCreated(Create(world, 32, (uint)OID.Oviform, ActorType.Enemy, new(15f, 0f)));
            Field(hatch, "_targets").SetValue(hatch, BitMask.Build(0));
            ((Actor?[])Field(hatch, "_assignedLinks").GetValue(hatch)!)[0] = link;
            Field(hatch, "Twister").SetValue(hatch, true);
            hatch.Update();
            check((bool)Field(hatch, "Twister").GetValue(hatch)!, "孵化：缺少旋风预测时保持等待");
            var twister = (GenericTwister)Activate(module, "P1Twister");
            twister.AddPredicted(0.3d);
            hatch.Update();
            check(!(bool)Field(hatch, "Twister").GetValue(hatch)!, "孵化：P1旋风预测出现后即解除等待");
            var hints = new AIHints();
            hatch.AddAIHints(0, player, Role.MT, hints);
            check(hints.ForbiddenZones.Count == 1 && hints.ForbiddenZones[0].shapeDistance.Distance(link.Position) > 0f,
                "孵化：预测后目标可进入拘束器而非保持环扇区域");
            Field(hatch, "_targets").SetValue(hatch, default(BitMask));
            hints.Clear();
            hatch.AddAIHints(0, player, Role.MT, hints);
            check(hints.ForbiddenZones.Any(z => z.shapeDistance.Distance(link.Position + new WDir(8f, 0f)) < 0f),
                "孵化：单拘束器且有球时非目标避开拘束器12米范围");
            hints.Clear();
            hatch.AddAIHints(-1, player, Role.MT, hints);
            check(hints.ForbiddenZones.Count == 0 && hints.GoalZones.Count == 0, "孵化：无效队伍槽不生成默认站位区域");
        }
    }

    private static void VerifyLiquidHellLifecycle(Action<bool, string> check)
    {
        var (world, module, player) = Fixture();
        using (module)
        {
            var liquid = Component(module, "LiquidHell");
            liquid.OnEventCast(module.PrimaryActor, Event(AID.LiquidHell, new(1f, 2f)));
            var hints = new AIHints();
            liquid.AddAIHints(0, player, Role.MT, hints);
            check(hints.ForbiddenZones.Count == 1 && hints.ForbiddenZones[0].activation == world.FutureTime(1.3d), "火圈：动作后按生成时刻标记预测危险区");
            var fire = Create(world, 33, (uint)OID.VoidzoneLiquidHell, ActorType.EventObj, new(1f, 2f));
            liquid.Update();
            hints.Clear();
            liquid.AddAIHints(0, player, Role.MT, hints);
            check(hints.ForbiddenZones.Count == 1 && hints.TemporaryObstacles.Count == 1, "火圈：实体替换预测且圈外同时有危险区与障碍");
            player.PosRot = new(fire.PosRot.X, player.PosRot.Y, fire.PosRot.Z, player.PosRot.W);
            hints.Clear();
            liquid.AddAIHints(0, player, Role.MT, hints);
            check(hints.ForbiddenZones.Count == 1 && hints.TemporaryObstacles.Count == 0, "火圈：圈内玩家可寻路离开且立即避险");
            fire.EventState = 7;
            hints.Clear();
            liquid.AddAIHints(0, player, Role.MT, hints);
            check(hints.ForbiddenZones.Count == 0 && hints.TemporaryObstacles.Count == 0, "火圈：结束后同时清除危险区与障碍");
        }
    }

    private static void VerifyBlackfireMovement(Action<bool, string> check)
    {
        var (_, module, player) = Fixture();
        using (module)
        {
            Activate(module, "P3BahamutPositioning");
            Activate(module, "P3BlackfireTrio");
            var liquid = Component(module, "P3BlackfireLiquidHell");
            var predictions = (List<(WPos pos, DateTime time)>)Field(liquid, "_predictedByEvent").GetValue(liquid)!;
            for (var i = 0; i < 5; ++i)
                predictions.Add((new(100f + i, 100f), DateTime.MaxValue));
            player.PosRot = new(0f, player.PosRot.Y, 0f, player.PosRot.W);
            var hints = new AIHints();
            liquid.AddAIHints(0, player, Role.MT, hints);
            check(hints.ForbiddenZones.Count == 1 && hints.ForbiddenZones[0].shapeDistance.Distance(new(-8f, 0f)) > 0f
                && hints.ForbiddenZones[0].shapeDistance.Distance(new(8f, 0f)) < 0f, "黑火三连：辅助职业可使用整个指定半平面避开火圈");
            player.Class = Class.BLM;
            hints.Clear();
            liquid.AddAIHints(0, player, Role.R1, hints);
            check(hints.ForbiddenZones[0].shapeDistance.Distance(new(8f, 0f)) > 0f, "黑火三连：输出职业使用另一半平面");
            var stack = Component(module, "P3MegaflareStack");
            stack.OnEventIcon(player, (uint)IconID.MegaflareStack, player.InstanceID);
            hints.Clear();
            stack.AddAIHints(0, player, Role.R1, hints);
            var south = module.Center + new WDir(0f, -8f);
            check(hints.ForbiddenZones.Count == 1 && hints.ForbiddenZones[0].shapeDistance.Distance(south + new WDir(5f, 0f)) > 0f,
                "黑火三连：首发Hypernova前分摊站位保留7.5米空间");
            stack.OnEventCast(module.PrimaryActor, Event(AID.Hypernova));
            hints.Clear();
            stack.AddAIHints(0, player, Role.R1, hints);
            check(hints.ForbiddenZones[0].shapeDistance.Distance(south + new WDir(5f, 0f)) < 0f, "黑火三连：首发Hypernova后收缩到2.5米分摊范围");
        }
    }

    private static void VerifyBahamutLifecycle(Action<bool, string> check)
    {
        var (world, module, player) = Fixture();
        using (module)
        {
            var positioning = Component(module, "P3BahamutPositioning");
            var hug = Component(module, "P3HugBahamut");
            var preposition = Component(module, "P5Preposition");
            var hints = new AIHints();
            positioning.AddAIHints(0, player, Role.MT, hints);
            hug.AddAIHints(0, player, Role.MT, hints);
            preposition.AddAIHints(0, player, Role.MT, hints);
            check(hints.GoalZones.Count == 0, "巴哈站位：实体尚未生成时不产生默认目标且不抛异常");
            var bahamut = Create(world, 34, (uint)OID.BahamutPrime, ActorType.Enemy, new(3f, 4f));
            typeof(UCOB).GetMethod("UpdateModule", All)!.Invoke(module, null);
            Field(positioning, "DesiredPosition").SetValue(positioning, new WPos(7f, 8f));
            var enemy = new AIHints.Enemy(bahamut, 1, true);
            hints.Enemies[bahamut.CharacterSpawnIndex] = enemy;
            positioning.AddAIHints(0, player, Role.MT, hints);
            hug.AddAIHints(0, player, Role.MT, hints);
            check(enemy.DesiredPosition == new WPos(7f, 8f) && hints.GoalZones.Count == 1 && hints.GoalZones[0](bahamut.Position) == 0.5f,
                "巴哈站位：实体晚生成后动态恢复坦克目标与贴身奖励");
            bahamut.IsTargetable = false;
            hints.Clear();
            preposition.AddAIHints(0, player, Role.MT, hints);
            check(hints.GoalZones.Count == 1 && hints.GoalZones[0](module.Center) > 0f, "P5预站位：不可选中期间向场中集合");
            bahamut.IsTargetable = true;
            hints.Clear();
            preposition.AddAIHints(0, player, Role.MT, hints);
            check(hints.GoalZones.Count == 0, "P5预站位：首领可选中后退出预站位");
        }
    }

    private static void VerifyScriptedDeathLifecycle(Action<bool, string> check)
    {
        var (_, module, player) = Fixture();
        using (module)
        {
            foreach (var name in new[] { "P5Teraflare", "P5FlamesOfRebirth" })
            {
                var component = Activate(module, name);
                var hints = new AIHints();
                component.AddAIHints(0, player, Role.MT, hints);
                check(hints.ScriptedDeath, name + "：剧情死亡阶段发出保留预设标记");
                typeof(BossModule).GetMethod("DeactivateComponent", All)!.MakeGenericMethod(Internal(name)).Invoke(module, null);
                hints.Clear();
                check(!hints.ScriptedDeath && !module.Components.Contains(component), name + "：组件结束与下一帧清理移除剧情死亡标记");
            }
        }
    }

    private static void VerifyPhaseTransitions(Action<bool, string> check)
    {
        var (world, module, _) = Fixture();
        using (module)
        {
            var naelPhase = module.StateMachine.Phases[3];
            var addsPhase = module.StateMachine.Phases[4];
            module.PrimaryActor.IsDestroyed = true;
            check(!naelPhase.Update!() && !addsPhase.Update!(), "绝巴哈阶段：主实体销毁且Nael未生成时不提前结束P2或P3");
            var nael = Create(world, 35, (uint)OID.NaelDeusDarnus, ActorType.Enemy, new(0f, 5f));
            typeof(UCOB).GetMethod("UpdateModule", All)!.Invoke(module, null);
            nael.IsTargetable = true;
            check(!naelPhase.Update!() && !addsPhase.Update!(), "绝巴哈阶段：主实体销毁但Nael尚可选中时保持当前阶段");
            nael.IsTargetable = false;
            nael.HPMP.CurHP = 2u;
            check(!naelPhase.Update!(), "绝巴哈阶段：Nael不可选中但HP高于1时不结束P2");
            nael.HPMP.CurHP = 1u;
            check(naelPhase.Update!(), "绝巴哈阶段：Nael不可选中且HP不高于1时完成P2");
            Activate(module, "P2BlockTransition");
            check(!naelPhase.Update!(), "绝巴哈阶段：Nael完成条件仍尊重P2转换阻挡组件");
            module.PrimaryActor.IsDead = true;
            check(!addsPhase.Update!(), "绝巴哈阶段：主实体死亡但Nael存活时不结束P3");
            nael.IsDead = true;
            check(addsPhase.Update!(), "绝巴哈阶段：主实体和动态Nael均死亡后完成P3");
        }
    }

    private static BossComponent Activate(UCOB module, string name)
    {
        typeof(BossModule).GetMethod("ActivateComponent", All)!.MakeGenericMethod(Internal(name)).Invoke(module, null);
        return module.Components.Single(c => c.GetType() == Internal(name));
    }

    private static ActorCastEvent Event(AID action, WPos target = default) => new(ActionID.MakeSpell(action), 0, 0, 0, target.ToVec3(), 1, 0, default);

    private static Actor Create(WorldState world, ulong id, uint oid, ActorType type, WPos position)
    {
        world.Execute(new ActorState.OpCreate(id, oid, (int)id, 0, "fixture", 0, type, type == ActorType.Player ? Class.PLD : Class.None, 100,
            new(position.X, 0f, position.Z, 0f), 0.5f, default, true, type == ActorType.Player, 0, 0, 0));
        return world.Actors.Find(id)!;
    }

    private static (WorldState world, UCOB module, Actor player) Fixture()
    {
        var world = new WorldState(10000000, "ucob-20261008");
        world.Frame = new(DateTime.UnixEpoch.AddHours(1d), 0, 0, 0, 0, 1);
        var player = Create(world, 1, 0, ActorType.Player, new(10f, 0f));
        world.Execute(new PartyState.OpModify(0, new(1, player.InstanceID, false, "player")));
        var twintania = Create(world, 20, (uint)OID.Twintania, ActorType.Enemy, default);
        return (world, new UCOB(world, twintania), player);
    }
}
