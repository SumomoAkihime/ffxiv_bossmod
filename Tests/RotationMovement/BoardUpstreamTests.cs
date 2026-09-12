using BossMod;
using ArchDemon = BossMod.Global.CrucibleOfTheUnbroken.FirstBoard.ArchDemonPiece.ArchDemonPiece;
using Banemite = BossMod.Global.CrucibleOfTheUnbroken.FirstBoard.BanemitePiece.BanemitePiece;
using BoneBishop = BossMod.Global.CrucibleOfTheUnbroken.FirstBoard.BoneBishop.BoneBishop;
using Ogre = BossMod.Global.CrucibleOfTheUnbroken.FirstBoard.OgrePiece.OgrePiece;
using PasDeSeul = BossMod.Global.CrucibleOfTheUnbroken.FirstBoard.PasDeSeul.PasDeSeul;
using Piscodemon = BossMod.Global.CrucibleOfTheUnbroken.FirstBoard.PiscodemonPiece.PiscodemonPiece;

static class BoardUpstreamTests
{
    public static void Run(Action<bool, string> check)
    {
        VerifyModuleAndAOE(check, "大恶魔棋子", typeof(ArchDemon), 0x4B88, 46876, "BossMod.Global.CrucibleOfTheUnbroken.FirstBoard.ArchDemonPiece.AbyssalCharge", new(520, 0));
        VerifyModuleAndAOE(check, "灾祸蛛棋子", typeof(Banemite), 0x4B8B, 46908, "BossMod.Global.CrucibleOfTheUnbroken.FirstBoard.BanemitePiece.VenomWeb", new(120, -420));
        VerifyModuleAndAOE(check, "骸骨主教", typeof(BoneBishop), 0x4B87, 46868, "BossMod.Global.CrucibleOfTheUnbroken.FirstBoard.BoneBishop.DeathSpiral", new(120, -420));
        VerifyModuleAndAOE(check, "食人魔棋子", typeof(Ogre), 0x4B8D, 46916, "BossMod.Global.CrucibleOfTheUnbroken.FirstBoard.OgrePiece.Magma", new(120, -420));
        VerifyModuleAndAOE(check, "独舞", typeof(PasDeSeul), 0x4B90, 46926, "BossMod.Global.CrucibleOfTheUnbroken.FirstBoard.PasDeSeul.BloodRainCircle", new(520, -420));
        VerifyModuleAndAOE(check, "鱼魔棋子", typeof(Piscodemon), 0x4B8A, 46892, "BossMod.Global.CrucibleOfTheUnbroken.FirstBoard.PiscodemonPiece.VoidThunderIII", new(120, 0));
        VerifyPasDeSeulBoundsCenter(check);
    }

    private static void VerifyModuleAndAOE(Action<bool, string> check, string name, Type expectedType, uint oid, uint aid, string componentName, WPos position)
    {
        using var fixture = new Fixture();
        var boss = fixture.Create(oid, position);
        using var module = BossModuleRegistry.CreateModuleForActor(fixture.World, boss, BossModuleInfo.Maturity.Contributed);
        check(module?.GetType() == expectedType && module.Info?.ModuleType == expectedType, $"{name}以实际主实体注册并构造");
        if (module == null)
            return;

        module.StateMachine.Start(fixture.World.CurrentTime);
        var component = module.Components.OfType<BossMod.Components.GenericAOEs>().SingleOrDefault(c => c.GetType().FullName == componentName);
        check(module.StateMachine.ActiveState != null && component != null, $"{name}状态机启动后激活代表 AOE 组件");
        if (component == null)
            return;

        var cast = Cast(aid, module.Arena.Center, default, 3);
        component.OnCastStarted(boss, cast);
        check(component.ActiveAOEs(0, boss).Length != 0, $"{name}代表 AOE 在读条时出现");
        component.OnCastFinished(boss, cast);
        check(component.ActiveAOEs(0, boss).Length == 0, $"{name}代表 AOE 在读条结束后清理");
    }

    private static void VerifyPasDeSeulBoundsCenter(Action<bool, string> check)
    {
        using var fixture = new Fixture();
        var boss = fixture.Create(0x4B90, new(520, -420));
        using var module = BossModuleRegistry.CreateModuleForActor(fixture.World, boss, BossModuleInfo.Maturity.Contributed);
        check(module != null, "独舞可由注册表构造以核对自定义边界");
        if (module == null)
            return;

        var bounds = (ArenaBoundsCustom)module.Bounds;
        check(module.Arena.Center.AlmostEqual(bounds.Center, 0.01f), $"独舞场地中心不一致：Arena.Center={module.Arena.Center}，Bounds.Center={bounds.Center}");
    }

    private static ActorCastInfo Cast(uint aid, WPos location, Angle rotation, float total) => new()
    {
        Action = new(ActionType.Spell, aid),
        Location = new(location.X, 0, location.Z),
        Rotation = rotation,
        TotalTime = total
    };

    private sealed class Fixture : IDisposable
    {
        public readonly WorldState World = new(10000000, "board-upstream-test");
        private ulong _nextID = 1;

        public Fixture() => World.Frame = new(DateTime.UnixEpoch.AddHours(1), 0, 0, 0, 0, 1);

        public Actor Create(uint oid, WPos position)
        {
            var id = _nextID++;
            World.Execute(new ActorState.OpCreate(id, oid, (int)id, 0, "fixture", 0, ActorType.Enemy, Class.None, 100, new(position.X, 0, position.Z, 0), .5f, new(1000, 1000, 0, 10000, 10000), true, false, 0, 0, 0));
            return World.Actors.Find(id)!;
        }

        public void Dispose() { }
    }
}
