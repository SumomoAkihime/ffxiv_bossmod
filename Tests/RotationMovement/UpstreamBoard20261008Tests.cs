using BossMod;
using BossMod.Components;
using Administrator = BossMod.Global.CrucibleOfTheUnbroken.FirstMasterBoard.AdministratorPiece.AdministratorPiece;
using Borgny = BossMod.Global.CrucibleOfTheUnbroken.FirstMasterBoard.BorgnyTheVenomous.BorgnyTheVenomous;
using CorpseFlower = BossMod.Global.CrucibleOfTheUnbroken.FirstMasterBoard.CorpseFlowerPiece.CorpseFlowerPiece;
using Gargoyle = BossMod.Global.CrucibleOfTheUnbroken.FirstMasterBoard.GargoylePiece.GargoylePiece;
using Golem = BossMod.Global.CrucibleOfTheUnbroken.FirstMasterBoard.GolemPiece.GolemPiece;
using IceDragon = BossMod.Global.CrucibleOfTheUnbroken.FirstMasterBoard.IceDragonPiece.IceDragonPiece;
using Morbol = BossMod.Global.CrucibleOfTheUnbroken.FirstMasterBoard.MorbolPiece.MorbolPiece;
using Progenitrix = BossMod.Global.CrucibleOfTheUnbroken.FirstMasterBoard.ProgenitrixPiece.ProgenitrixPiece;
using Strix = BossMod.Global.CrucibleOfTheUnbroken.FirstMasterBoard.StrixPiece.StrixPiece;
using Treant = BossMod.Global.CrucibleOfTheUnbroken.FirstMasterBoard.TreantPiece.TreantPiece;
using MantiCore = BossMod.Global.CrucibleOfTheUnbroken.SecondBoard.MantiCorePiece.ManticorePiece;
using Voidmancer = BossMod.Global.CrucibleOfTheUnbroken.SecondBoard.VoidmancerPiece.VoidmancerPiece;
using Wyvern = BossMod.Global.CrucibleOfTheUnbroken.SecondBoard.WyvernPiece.WyvernPiece;
using YoungerTablitaur = BossMod.Global.CrucibleOfTheUnbroken.SecondBoard.YoungerTablitaurPiece.YoungerTablitaurPiece;
using Lakhamu = BossMod.Global.CrucibleOfTheUnbroken.ThirdBoard.LakhamuPiece.LakhamuPiece;
using LakhamuAID = BossMod.Global.CrucibleOfTheUnbroken.ThirdBoard.LakhamuPiece.AID;
using LakhamuOID = BossMod.Global.CrucibleOfTheUnbroken.ThirdBoard.LakhamuPiece.OID;
using LakhamuIcon = BossMod.Global.CrucibleOfTheUnbroken.ThirdBoard.LakhamuPiece.IconID;
using MantiAID = BossMod.Global.CrucibleOfTheUnbroken.SecondBoard.MantiCorePiece.AID;
using MantiOID = BossMod.Global.CrucibleOfTheUnbroken.SecondBoard.MantiCorePiece.OID;
using Gigantis = BossMod.Global.CrucibleOfTheUnbroken.SecondMasterBoard.GigantisPiece.GigantisPiece;
using Boogyman = BossMod.Global.CrucibleOfTheUnbroken.SecondMasterBoard.BoogymanPiece.BoogymanPiece;
using Drake = BossMod.Global.CrucibleOfTheUnbroken.SecondMasterBoard.DrakePiece.DrakePiece;
using Durga = BossMod.Global.CrucibleOfTheUnbroken.SecondMasterBoard.DurgaPiece.DurgaPiece;
using Flauros = BossMod.Global.CrucibleOfTheUnbroken.SecondMasterBoard.FlaurosPiece.FlaurosPiece;
using Zu = BossMod.Global.CrucibleOfTheUnbroken.ThirdBoard.ZuPiece.ZuPiece;
using ZuAID = BossMod.Global.CrucibleOfTheUnbroken.ThirdBoard.ZuPiece.AID;
using ZuOID = BossMod.Global.CrucibleOfTheUnbroken.ThirdBoard.ZuPiece.OID;
using ZuIcon = BossMod.Global.CrucibleOfTheUnbroken.ThirdBoard.ZuPiece.IconID;
using Guttler = BossMod.Global.CrucibleOfTheUnbroken.ThirdBoard.GuttlerTheGutter.GuttlerTheGutter;
using Ymir = BossMod.Global.CrucibleOfTheUnbroken.ThirdBoard.YmirPiece.YmirPiece;
using YmirOID = BossMod.Global.CrucibleOfTheUnbroken.ThirdBoard.YmirPiece.OID;

static class UpstreamBoard20261008Tests
{
    public static void Run(Action<bool, string> check)
    {
        VerifyModuleRegistrations(check);
        VerifyManticoreHeadAndTailLifecycle(check);
        VerifyYoungerLateSpawn(check);
        VerifyLakhamuBaitTimestampReset(check);
        VerifyZuEggObstacleLifecycle(check);
        VerifyCombinedKnockbackShapeDistances(check);
    }

    private static void VerifyModuleRegistrations(Action<bool, string> check)
    {
        VerifyModule<MantiCore>(check, 0x4C53, 1089, 14545);
        VerifyModule<Voidmancer>(check, 0x4C5C, 1089, 14552);
        VerifyModule<Wyvern>(check, 0x4C58, 1089, 14549);
        VerifyModule<YoungerTablitaur>(check, 0x4C60, 1089, 14556);
        VerifyModule<Administrator>(check, 0x4CC4, 1091, 14610);
        VerifyModule<Borgny>(check, 0x4CD8, 1091, 14628);
        VerifyModule<CorpseFlower>(check, 0x4CBD, 1091, 14603);
        VerifyModule<Gargoyle>(check, 0x4CC2, 1091, 14608);
        VerifyModule<Golem>(check, 0x4CCB, 1091, 14617);
        VerifyModule<IceDragon>(check, 0x4CC0, 1091, 14606);
        VerifyModule<Morbol>(check, 0x4CB9, 1091, 14599);
        VerifyModule<Progenitrix>(check, 0x4CD2, 1091, 14623);
        VerifyModule<Strix>(check, 0x4CB6, 1091, 14596);
        VerifyModule<Treant>(check, 0x4CCD, 1091, 14618);
        VerifyModule<Lakhamu>(check, (uint)LakhamuOID.LakhamuPiece, 1090, 14580);
        VerifyModule<Guttler>(check, 0x4CAA, 1090, 14592);
        VerifyModule<Ymir>(check, (uint)YmirOID.YmirPiece, 1090, 14569);
        VerifyModule<Zu>(check, (uint)ZuOID.ZuPiece, 1090, 14572);
        VerifyModule<Gigantis>(check, 0x4D03, 1092, 14670);
        VerifyModule<Boogyman>(check, 0x4CE2, 1092, 14638);
        VerifyModule<Drake>(check, 0x4CF0, 1092, 14651);
        VerifyModule<Durga>(check, 0x4CF6, 1092, 14657);
        VerifyModule<Flauros>(check, 0x4CDB, 1092, 14631);
    }

    private static void VerifyModule<T>(Action<bool, string> check, uint oid, uint groupID, uint nameID) where T : BossModule
    {
        using var fixture = new Fixture();
        var boss = fixture.Create(oid, new(120, -420));
        using var module = BossModuleRegistry.CreateModuleForActor(fixture.World, boss, BossModuleInfo.Maturity.Contributed);
        var info = module?.Info;
        check(module?.GetType() == typeof(T) && info is { Maturity: BossModuleInfo.Maturity.Contributed, Expansion: BossModuleInfo.Expansion.Global, Category: BossModuleInfo.Category.CrucibleOfTheUnbroken, GroupType: BossModuleInfo.GroupType.CFC, GroupID: var actualGroupID, NameID: var actualNameID } && actualGroupID == groupID && actualNameID == nameID,
            $"{typeof(T).Name} 以预期身份和 Contributed 成熟度注册");
        if (module != null)
        {
            module.StateMachine.Start(fixture.World.CurrentTime);
            check(module.StateMachine.ActiveState != null && module.Components.Count > 0, $"{typeof(T).Name} 在附属实体尚未生成时也能激活机制组件");
        }
    }

    private static void VerifyYoungerLateSpawn(Action<bool, string> check)
    {
        using var fixture = new Fixture();
        var boss = fixture.Create(0x4C60, new(120, 0));
        var player = fixture.Create(0, new(125, 0), ActorType.Player);
        fixture.World.Execute(new PartyState.OpModify(0, new(1, player.InstanceID, false, "player")));
        boss.InCombat = true;
        using var module = new YoungerTablitaur(fixture.World, boss);
        module.Update();
        var bait = (BaitAwayIcon)module.Components.Single(c => c.GetType().Name == "TonzeSlash100");
        bait.OnEventIcon(player, 412, player.InstanceID);
        check(module.ElderTablitaur == null && bait.CurrentBaits.Count == 0, "双首领缺另一实体时安全激活，诱导不错误指向当前首领");
        var elder = fixture.Create(0x4C5F, new(115, 0));
        module.Update();
        bait.OnEventIcon(player, 412, player.InstanceID);
        check(module.ElderTablitaur == elder && bait.CurrentBaits.Single().Source == elder, "另一首领晚生成后刷新引用并从正确来源显示诱导");

        var knockback = (GenericKnockback)module.Components.Single(c => c.GetType().Name == "Shockwave");
        var helper1 = fixture.Create(0x233C, boss.Position);
        var helper2 = fixture.Create(0x233C, boss.Position);
        knockback.OnCastStarted(helper1, Cast(48202, boss.Position, 10.5f));
        knockback.OnCastStarted(helper2, Cast(48199, boss.Position, 7));
        var sources = knockback.ActiveKnockbacks(0, player);
        check(sources.Length == 2 && sources[0].ActorID == helper2.InstanceID && sources[0].Activation < sources[1].Activation,
            "双首领击退按结算时间排序，先收到长读条也不反转预测顺序");
    }

    private static void VerifyManticoreHeadAndTailLifecycle(Action<bool, string> check)
    {
        using var fixture = new Fixture();
        var boss = fixture.Create((uint)MantiOID.ManticorePiece, new(120, -420));
        using var module = new MantiCore(fixture.World, boss);
        module.StateMachine.Start(fixture.World.CurrentTime);
        var headsAndTails = module.Components.OfType<GenericAOEs>().SingleOrDefault(c => c.GetType().Name == "HeadsAndTails");
        check(headsAndTails != null, "Manticore 状态机激活头尾组合预警");
        if (headsAndTails == null)
            return;

        var helper = fixture.Create((uint)MantiOID.Helper, boss.Position);
        var cast = Cast((uint)MantiAID.HeadsAndTailsFront, boss.Position, 3.9f);
        headsAndTails.OnCastStarted(helper, cast);
        check(headsAndTails.ActiveAOEs(0, boss).Length == 2, "头尾组合预警同时显示当前与下一击");
        headsAndTails.OnEventCast(helper, Event((uint)MantiAID.HeadsAndTailsFront));
        check(headsAndTails.ActiveAOEs(0, boss).Length == 1, "头尾组合结算后清理当前击并保留下一击");
    }

    private static void VerifyLakhamuBaitTimestampReset(Action<bool, string> check)
    {
        using var fixture = new Fixture();
        var boss = fixture.Create((uint)LakhamuOID.LakhamuPiece, new(120, 0));
        var player = fixture.Create(0, new(120, 0), ActorType.Player);
        using var module = new Lakhamu(fixture.World, boss);
        module.StateMachine.Start(fixture.World.CurrentTime);
        var bait = (BaitAwayIcon)module.Components.Single(c => c.GetType().Name == "EarthShaker");

        bait.OnEventIcon(player, (uint)LakhamuIcon.EarthShake, player.InstanceID);
        bait.OnEventCast(boss, Event((uint)LakhamuAID.EarthShaker));
        fixture.Advance(1.1);
        bait.Update();
        check(bait.CurrentBaits.Count == 0, "地摇首轮伤害延迟后清理诱导");

        bait.OnEventIcon(player, (uint)LakhamuIcon.EarthShake, player.InstanceID);
        bait.Update();
        check(bait.CurrentBaits.Count == 1, "地摇新一轮不会受上一轮时间戳影响");
    }

    private static void VerifyZuEggObstacleLifecycle(Action<bool, string> check)
    {
        using var fixture = new Fixture();
        var boss = fixture.Create((uint)ZuOID.ZuPiece, new(120, -420));
        var player = fixture.Create(0, new(120, -420), ActorType.Player);
        var egg = fixture.Create((uint)ZuOID.PulletPieceEgg, new(124, -420));
        using var module = new Zu(fixture.World, boss);
        module.StateMachine.Start(fixture.World.CurrentTime);
        var bait = module.Components.SingleOrDefault(c => c.GetType().Name == "CrossbreezeBait");
        check(bait != null, "祖鸟状态机激活蛋避让诱导");
        if (bait == null)
            return;

        bait.OnEventIcon(player, (uint)ZuIcon.Crossbreeze, player.InstanceID);
        var hints = new AIHints();
        bait.AddAIHints(0, player, default, hints);
        check(hints.TemporaryObstacles.Count == 1 && hints.TemporaryObstacles[0].Distance(egg.Position) <= 0,
            "十字诱导期间把蛋命中范围作为临时障碍");

        var helper = fixture.Create((uint)ZuOID.Helper, boss.Position);
        var cast = Cast((uint)ZuAID.Crossbreeze, boss.Position, 8.1f);
        bait.OnCastStarted(helper, cast);
        check(((GenericBaitAway)bait).CurrentBaits.Count == 0, "十字实际施法开始后清理诱导目标");
    }
    private static void VerifyCombinedKnockbackShapeDistances(Action<bool, string> check)
    {
        var origin = new WPos(0, 0);
        var player = new WPos(5, 0);
        var dangerCircle = new WPos(8, 0);
        WPos[] circles = [dangerCircle];
        var rects = new (WPos Origin, WDir Direction)[] { (new(8, 0), new(1, 0)) };
        var rectKnockback = new SDKnockbackInAABBRectAwayFromOriginPlusAOERects(default, origin, 5, 20, 20, rects, 5, 1, 1);
        var rectCircleKnockback = new SDKnockbackInAABBRectAwayFromOriginPlusIntersectAOECircles(default, origin, 5, 20, 20, circles, 1, 1);
        var squareCircleKnockback = new SDKnockbackInAABBSquareAwayFromOriginPlusIntersectAOECircles(default, origin, 5, 20, circles, 1, 1);
        var circleKnockback = new SDInCircleAwayFromOriginPlusIntersectAOECircles(origin, 5, [(dangerCircle, 1)], 1);
        var safePosition = new WPos(5, 5);

        check(rectKnockback.Contains(player) && !rectKnockback.Contains(safePosition), "组合击退同时排除场外落点和矩形 AOE 轨迹");
        check(rectCircleKnockback.Contains(player) && !rectCircleKnockback.Contains(safePosition), "矩形场地击退会排除穿过圆形 AOE 的路径");
        check(squareCircleKnockback.Contains(player) && !squareCircleKnockback.Contains(safePosition), "方形场地击退会排除穿过圆形 AOE 的路径");
        check(circleKnockback.Contains(player) && !circleKnockback.Contains(safePosition), "圆形场地击退会排除穿过圆形 AOE 的路径");
    }

    private static ActorCastInfo Cast(uint aid, WPos location, float total) => new()
    {
        Action = new(ActionType.Spell, aid),
        Location = new(location.X, 0, location.Z),
        TotalTime = total
    };

    private static ActorCastEvent Event(uint aid) => new(new(ActionType.Spell, aid), 0, 0, 0, default, 1, 0, default);

    private sealed class Fixture : IDisposable
    {
        public readonly WorldState World = new(10000000, "upstream-board-20261008-test");
        private ulong _nextID = 1;

        public Fixture() => World.Frame = new(DateTime.UnixEpoch.AddHours(1), 0, 0, 0, 0, 1);

        public Actor Create(uint oid, WPos position, ActorType type = ActorType.Enemy)
        {
            var id = _nextID++;
            World.Execute(new ActorState.OpCreate(id, oid, (int)id, 0, "fixture", 0, type, Class.None, 100, new(position.X, 0, position.Z, 0), .5f, new(1000, 1000, 0, 10000, 10000), true, false, 0, 0, 0));
            return World.Actors.Find(id)!;
        }

        public void Advance(double seconds) => World.Frame = new(World.CurrentTime.AddSeconds(seconds), 0, 0, 0, 0, 1);
        public void Dispose() { }
    }
}