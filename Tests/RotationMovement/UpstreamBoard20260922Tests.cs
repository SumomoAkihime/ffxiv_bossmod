using BossMod;
using BossMod.Components;
using BoneAID = BossMod.Global.CrucibleOfTheUnbroken.FirstBoard.BoneBishop.AID;
using BoneOID = BossMod.Global.CrucibleOfTheUnbroken.FirstBoard.BoneBishop.OID;
using BoneSID = BossMod.Global.CrucibleOfTheUnbroken.FirstBoard.BoneBishop.SID;
using Drake = BossMod.Global.CrucibleOfTheUnbroken.SecondMasterBoard.DrakePiece.DrakePiece;
using DrakeAID = BossMod.Global.CrucibleOfTheUnbroken.SecondMasterBoard.DrakePiece.AID;
using DrakeOID = BossMod.Global.CrucibleOfTheUnbroken.SecondMasterBoard.DrakePiece.OID;
using Guttler = BossMod.Global.CrucibleOfTheUnbroken.ThirdBoard.GuttlerTheGutter.GuttlerTheGutter;
using GuttlerAID = BossMod.Global.CrucibleOfTheUnbroken.ThirdBoard.GuttlerTheGutter.AID;

static class UpstreamBoard20260922Tests
{
    public static void Run(Action<bool, string> check)
    {
        VerifyDrakeRegistrationAndLifecycle(check);
        VerifyBoneBishopBlackEruptionLifecycle(check);
        VerifyGuttlerBeastlyFlareRadius(check);
    }

    private static void VerifyDrakeRegistrationAndLifecycle(Action<bool, string> check)
    {
        using var fixture = new Fixture();
        var drake = fixture.Create((uint)DrakeOID.DrakePiece, new(520, 0));
        using var module = BossModuleRegistry.CreateModuleForActor(fixture.World, drake, BossModuleInfo.Maturity.Contributed);
        check(module?.GetType() == typeof(Drake) && module.Info is { ModuleType: var type, Maturity: BossModuleInfo.Maturity.Contributed, Expansion: BossModuleInfo.Expansion.Global, Category: BossModuleInfo.Category.CrucibleOfTheUnbroken } && type == typeof(Drake), "Drake 以 Contributed 成熟度和棋盘分类注册");
        if (module == null)
            return;

        module.StateMachine.Start(fixture.World.CurrentTime);
        var cyclone = module.Components.OfType<GenericAOEs>().SingleOrDefault(c => c.GetType().Name == "BurningCyclone");
        check(module.StateMachine.ActiveState != null && cyclone != null, "Drake 状态机激活旋风机制");
        if (cyclone == null)
            return;

        var cast = Cast((uint)DrakeAID.BurningCyclone, new(520, 0), 4);
        var helper = fixture.Create((uint)DrakeOID.Helper, new(520, 0));
        cyclone.OnCastStarted(helper, cast);
        check(cyclone.ActiveAOEs(0, drake).Length == 1, "旋风读条时显示范围");
        cyclone.OnCastFinished(helper, cast);
        check(cyclone.ActiveAOEs(0, drake).Length == 0, "旋风读条结束后清理范围");

        cyclone.OnEventCast(helper, Event((uint)DrakeAID.Teleport, new(520, 5)));
        check(cyclone.ActiveAOEs(0, drake).Length == 1, "首领传送后重新显示旋风范围");
        cyclone.OnActorDeath(drake);
        check(cyclone.ActiveAOEs(0, drake).Length == 0, "首领死亡后清理旋风范围");

        fixture.World.Execute(new ActorState.OpDead(drake.InstanceID, true));
        module.StateMachine.Update(fixture.World.CurrentTime);
        check(module.StateMachine.ActivePhaseIndex == 0, "Drake 死亡但 Abaddon 尚未出现时模块保持活动");
        var abaddon = fixture.Create((uint)DrakeOID.AbaddonPiece, new(520, 0));
        module.Update();
        fixture.World.Execute(new ActorState.OpDead(abaddon.InstanceID, true));
        module.StateMachine.Update(fixture.World.CurrentTime);
        check(module.StateMachine.ActivePhaseIndex != 0, "Abaddon 出现且全部首领结束后模块退出");
    }

    private static void VerifyBoneBishopBlackEruptionLifecycle(Action<bool, string> check)
    {
        using var fixture = new Fixture();
        var boss = fixture.Create((uint)BoneOID.BoneBishop, new(120, -420));
        using var module = BossModuleRegistry.CreateModuleForActor(fixture.World, boss, BossModuleInfo.Maturity.Contributed);
        check(module?.GetType().Name == "BoneBishop", "骸骨主教以成熟模块注册并构造");
        if (module == null)
            return;

        module.StateMachine.Start(fixture.World.CurrentTime);
        var guard = module.Components.SingleOrDefault(c => c.GetType().Name == "ForwardGuard");
        check(guard != null, "骸骨主教激活格挡 AI 提示");
        if (guard != null)
        {
            var parry = new ActorStatus((uint)BoneSID.DirectionalParry, 0, fixture.World.FutureTime(5), boss.InstanceID);
            guard.OnStatusGain(boss, ref parry);
            var hints = new AIHints();
            guard.AddAIHints(0, boss, default, hints);
            check(hints.ForbiddenZones.Count == 0, "骑士尚未生成时格挡 AI 提示安全跳过");
        }
        fixture.Create((uint)BoneOID.BoneKnight, new(120, -420));
        var eruption = module.Components.OfType<GenericAOEs>().SingleOrDefault(c => c.GetType().Name == "BlackEruption");
        check(eruption != null && module.Components.Any(c => c.GetType().Name == "Ossify"), "骸骨主教激活地裂与打断提示");
        if (eruption == null)
            return;

        eruption.OnEventCast(boss, Event((uint)BoneAID.BlackEruption1, boss.Position));
        var predicted = eruption.ActiveAOEs(0, boss);
        check(predicted.Length == 4, "黑暗爆发只显示下一组四个预测范围");
        if (predicted.Length != 4)
            return;

        var first = predicted[0].Origin;
        var corrected = predicted[1].Origin + new WDir(0.25f, 0);
        var helper = fixture.Create((uint)BoneOID.Helper, corrected);
        var cast = Cast((uint)BoneAID.BlackEruption2, corrected, 1.5f);
        eruption.OnCastStarted(helper, cast);
        var updated = eruption.ActiveAOEs(0, boss);
        check(updated.Length == 4 && updated[1].Origin.AlmostEqual(corrected, 0.01f), "黑暗爆发按实际落点校正对应预测范围");
        check(updated.Length == 4 && updated[1].ShapeDistance is { } shapeDistance
            && MathF.Abs(shapeDistance.Distance(corrected + new WDir(5.1f, 0)) - 0.1f) < 0.01f, "黑暗爆发同步校正 AI 禁区圆心");
        eruption.OnCastFinished(helper, cast);
        var remaining = eruption.ActiveAOEs(0, boss);
        check(remaining.Length == 4 && remaining[0].Origin.AlmostEqual(first, 0.01f), "黑暗爆发按实际落点清理而保留其他范围");
        eruption.OnActorDeath(boss);
        check(eruption.ActiveAOEs(0, boss).Length == 0, "首领死亡后清理所有黑暗爆发范围");
    }

    private static void VerifyGuttlerBeastlyFlareRadius(Action<bool, string> check)
    {
        using var fixture = new Fixture();
        var boss = fixture.Create(0x4CAA, new(520, -420));
        using var module = BossModuleRegistry.CreateModuleForActor(fixture.World, boss, BossModuleInfo.Maturity.Contributed);
        check(module?.GetType() == typeof(Guttler), "格特勒以成熟模块注册并构造");
        if (module == null)
            return;

        module.StateMachine.Start(fixture.World.CurrentTime);
        var flare = module.Components.OfType<GenericAOEs>().SingleOrDefault(c => c.GetType().Name == "BeastlyFlare");
        if (flare == null)
        {
            check(false, "格特勒状态机激活 Beastly Flare");
            return;
        }

        var cast = Cast((uint)GuttlerAID.BeastlyFlare, module.Arena.Center, 8);
        flare.OnCastStarted(boss, cast);
        var aoes = flare.ActiveAOEs(0, boss);
        check(aoes.Length == 1 && aoes[0].Shape is AOEShapeCircle circle && MathF.Abs(circle.Radius - 30f) < 0.001f, "Beastly Flare 使用半径 30 的危险范围");
        flare.OnCastFinished(boss, cast);
        check(flare.ActiveAOEs(0, boss).Length == 0, "Beastly Flare 结算后清理范围");
    }

    private static ActorCastInfo Cast(uint aid, WPos location, float total) => new()
    {
        Action = new(ActionType.Spell, aid),
        Location = new(location.X, 0, location.Z),
        TotalTime = total
    };

    private static ActorCastEvent Event(uint aid, WPos target) => new(new(ActionType.Spell, aid), 0, 0, 0, new(target.X, 0, target.Z), 1, 0, default);

    private sealed class Fixture : IDisposable
    {
        public readonly WorldState World = new(10000000, "upstream-board-20260922-test");
        private ulong _nextID = 1;

        public Fixture() => World.Frame = new(DateTime.UnixEpoch.AddHours(1), 0, 0, 0, 0, 1);

        public Actor Create(uint oid, WPos position)
        {
            var id = _nextID++;
            World.Execute(new ActorState.OpCreate(id, oid, (int)id, 0, "fixture", 0, ActorType.Enemy, Class.None, 100, new(position.X, 0, position.Z, 0), 0.5f, new(1000, 1000, 0, 10000, 10000), true, false, 0, 0, 0));
            return World.Actors.Find(id)!;
        }

        public void Dispose() { }
    }
}
