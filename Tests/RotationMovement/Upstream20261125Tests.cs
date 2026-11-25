using System.Reflection;
using BossMod;
using BossMod.Pathfinding;

static class Upstream20261125Tests
{
    public static void Run(Action<bool, string> check)
    {
        ReplayRoundtrip(check);
        FixedCenterBounds(check);
        ChainCompatibility(check);
        LegacyPartyFormat(check);
        BackgroundWriteFailure(check);
    }

    static void ReplayRoundtrip(Action<bool, string> check)
    {
        var world = new WorldState(10000000, "sync-20261125");
        world.Frame = new(DateTime.UnixEpoch.AddHours(2), 0, 0, 0, 0, 1);
        const ulong actorID = 0x10000001;
        const ulong contentID = 0x123456789ABCDEF;
        world.Execute(new ActorState.OpCreate(actorID, 0, 0, 0, "测试原名", 0, ActorType.Player, Class.PLD, 100,
            new(0, 0, 0, 0), 0.5f, new(1000, 1000, 0, 10000, 10000), true, true, 0, 0, 0));
        world.Execute(new PartyState.OpModify(0, new(contentID, actorID, false, "测试原名")));
        world.Execute(new PartyState.OpAllianceChange(AllianceLetter.C));
        world.Execute(new ActorState.OpRawRotation(actorID, 1.25f));
        world.Execute(new NetworkState.OpRTT(0.14f));
        var directory = Directory.CreateDirectory(Path.GetFullPath(Path.Combine("temp", "replay-20261125-" + Guid.NewGuid().ToString("N"))));
        var paths = new List<string>();
        try
        {
            foreach (var format in new[] { ReplayLogFormat.TextCondensed, ReplayLogFormat.BinaryUncompressed, ReplayLogFormat.BinaryCompressed })
            {
                foreach (var anonymous in new[] { false, true })
                {
                    string firstPath;
                    using (var first = new ReplayRecorder(world, format, true, directory, "same-name", anonymous))
                    using (var second = new ReplayRecorder(world, format, true, directory, "same-name", anonymous))
                    {
                        firstPath = first.LogPath;
                        paths.Add(firstPath);
                        paths.Add(second.LogPath);
                        check(firstPath != second.LogPath, "同秒并发录制分配独立文件，避免覆盖已存在回放");
                        world.Execute(new PartyState.OpModify(1, PartyState.EmptySlot));
                        world.Execute(new ActorState.OpRename(actorID, "测试更名", 0));
                        world.Execute(new ActorState.OpRawRotation(actorID, null));
                        world.Execute(new ActorState.OpRawRotation(actorID, 1.25f));
                    }
                    float progress = 0;
                    var messages = new List<string>();
                    var savedLog = Service.LogHandlerDebug;
                    Replay replay;
                    try
                    {
                        Service.LogHandlerDebug = messages.Add;
                        replay = ReplayParserLog.Parse(firstPath, ref progress, default);
                    }
                    finally { Service.LogHandlerDebug = savedLog; }
                    check(replay.Ops.Count > 0, "v32回放实际解析到事件：" + format + " " + string.Join("\n", messages));
                    var player = new ReplayPlayer(replay);
                    player.AdvanceTo(DateTime.MaxValue, static () => { });
                    var actor = player.WorldState.Actors.Find(actorID);
                    check(actor != null && Math.Abs(actor.ActualRotation.Rad - 1.25f) < 0.0001f, "v32保留初始原始朝向：" + format);
                    check(Math.Abs(player.WorldState.Network.LastRTT - 0.14f) < 0.0001f, "v32保留初始RTT：" + format);
                    check(player.WorldState.Party.Alliance == AllianceLetter.C, "v32保留初始联盟字母：" + format);
                    check(player.WorldState.Party.Members[0].ContentId != 0
                        && player.WorldState.Party.Members.Skip(1).All(member => member.ContentId == 0), "匿名回放保持空队员ContentID为零：" + format);
                    check(anonymous ? player.WorldState.Party.Members[0].ContentId != contentID && actor!.Name != "测试更名"
                        : player.WorldState.Party.Members[0].ContentId == contentID && actor!.Name == "测试更名", "匿名开关同时控制ContentID与重命名：" + format);
                }
            }
            check(paths.Distinct(StringComparer.OrdinalIgnoreCase).Count() == paths.Count, "重复导出与各格式录制均保留各自文件");
        }
        finally
        {
            foreach (var path in paths)
                File.Delete(path);
            directory.Delete();
        }
    }

    static void FixedCenterBounds(Action<bool, string> check)
    {
        var source = new ArenaBoundsCustom([new Square(new(0, 0), 9), new Square(new(20, 0), 9)]);
        var polygon = source.Shape.Transform(source.Center.ToWDir(), new(0, 1));
        var type = typeof(ArenaBounds).Assembly.GetType("BossMod.Stormblood.Extreme.Ex3Shinryu.ShinryuPolygonBounds", true)!;
        var bounds = (ArenaBounds)Activator.CreateInstance(type, BindingFlags.Public | BindingFlags.Instance, null, [30f, polygon, 0.5f], null)!;
        check(bounds.Contains(new(0, 0)) && bounds.Contains(new(20, 0)) && !bounds.Contains(new(10, 0)) && !bounds.Contains(new(-20, 0)), "极神龙非对称剩余平台保持机制固定中心与平台空隙");
        check(Math.Abs(bounds.IntersectRay(default, new(1, 0)) - 9f) < 0.05f, "极神龙固定中心边界射线使用真实平台边缘");
        var map = new Map();
        bounds.PathfindMap(map, default);
        foreach (var position in new[] { new WPos(0, 0), new WPos(20, 0), new WPos(10, 0) })
        {
            var (x, y) = map.WorldToGrid(position);
            var safe = x >= 0 && y >= 0 && x < map.Width && y < map.Height && map.PixelMaxG[map.GridToIndex(x, y)] > 0;
            check(safe == bounds.Contains(position.ToWDir()), "极神龙边界查询与地图栅格坐标一致：" + position);
        }
        var info = BossModuleRegistry.FindByOID(0x1F5A);
        check(info != null && info.GroupID == 278 && info.NameID == 5640 && info.Maturity == BossModuleInfo.Maturity.Contributed, "极神龙按实际主OID、CFC与NameID注册并达到默认加载门槛");
    }
    static void ChainCompatibility(Action<bool, string> check)
    {
        var world = new WorldState(10000000, "chains-20261125");
        world.Frame = new(DateTime.UnixEpoch.AddHours(3), 0, 0, 0, 0, 1);
        Actor Create(ulong id, uint oid, WPos position, ActorType type)
        {
            world.Execute(new ActorState.OpCreate(id, oid, (int)id, 0, "fixture", 0, type, Class.PLD, 100,
                new(position.X, 0, position.Z, 0), 0.5f, new(1000, 1000, 0, 10000, 10000), true, type == ActorType.Player, 0, 0, 0));
            return world.Actors.Find(id)!;
        }
        var player = Create(1, 0, default, ActorType.Player);
        var partner = Create(2, 0, new(10, 0), ActorType.Player);
        world.Execute(new PartyState.OpModify(0, new(1, 1, false, "player")));
        world.Execute(new PartyState.OpModify(1, new(2, 2, false, "partner")));
        using var module = new BossMod.Stormblood.Extreme.Ex3Shinryu.Ex3Shinryu(world, Create(3, 0x1F5A, default, ActorType.Enemy));
        var legacy = new BossMod.Components.Chains(module, 9, chainLength: 15);
        var fixedDistance = new BossMod.Components.Chains(module, 9, chainLength: 15, activationDelay: 3.6, useInitialDistance: true);
        var tether = new ActorTetherInfo(9, partner.InstanceID);
        legacy.OnTethered(player, tether);
        fixedDistance.OnTethered(player, tether);
        var activation = world.FutureTime(3.6);
        world.Frame.Timestamp = world.FutureTime(1);
        world.Execute(new ActorState.OpMove(player.InstanceID, new(5, 0, 0, 0)));
        var hints = new AIHints();
        legacy.AddAIHints(0, player, default, hints);
        var zone = hints.ForbiddenZones.Single();
        check(zone.shapeDistance.Contains(new(15, 0)) && !zone.shapeDistance.Contains(new(17, 0)), "旧Chains仍使用当前距离加1米的禁区半径");
        check(zone.activation == world.FutureTime(10), "旧Chains仍使用滚动10秒期限");
        hints.Clear();
        fixedDistance.AddAIHints(0, player, default, hints);
        zone = hints.ForbiddenZones.Single();
        check(zone.shapeDistance.Contains(new(34, 0)) && !zone.shapeDistance.Contains(new(36, 0)), "极神龙Chains使用初始10米距离加15米扯断距离");
        check(zone.activation == activation, "极神龙Chains使用连线后固定3.6秒期限");
        hints.Clear();
        fixedDistance.AddAIHints(1, partner, default, hints);
        check(hints.ForbiddenZones.Single().shapeDistance.Contains(new(-19, 0)), "连线双方都保存相同初始距离");
        fixedDistance.OnUntethered(player, tether);
        hints.Clear();
        fixedDistance.AddAIHints(0, player, default, hints);
        fixedDistance.AddAIHints(1, partner, default, hints);
        check(hints.ForbiddenZones.Count == 0, "解线后清理双方约束");
    }

    static void LegacyPartyFormat(Action<bool, string> check)
    {
        var path = Path.GetFullPath(Path.Combine("temp", "party-codec-" + Guid.NewGuid().ToString("N") + ".log"));
        try
        {
            foreach (var version in new[] { 14, 15, 30, 31, 32 })
            {
                var fields = version < 15 ? "" : version < 31 ? "|False|legacy-name" : "|False";
                File.WriteAllText(path, $"1970-01-01T01:00:00.0000000Z|VER |{version}|10000000|test\n1970-01-01T01:00:00.0000000Z|PAR |0|123|10000001{fields}\n", System.Text.Encoding.UTF8);
                float progress = 0;
                var replay = ReplayParserLog.Parse(path, ref progress, default);
                var member = replay.Ops.OfType<PartyState.OpModify>().Single().Member;
                check(member.ContentId == 0x123 && member.InstanceId == 0x10000001
                    && member.Name == (version is >= 15 and < 31 ? "legacy-name" : ""), "队员字段按历史版本读取：v" + version);
            }
        }
        finally { File.Delete(path); }
    }

    static void BackgroundWriteFailure(Action<bool, string> check)
    {
        var type = typeof(ReplayRecorder).Assembly.GetType("BossMod.BackgroundWriteStream", true)!;
        var destination = new FailingStream();
        var stream = (Stream)Activator.CreateInstance(type, [destination, null])!;
        var failed = false;
        try { stream.Write(new byte[65536 * 70]); }
        catch (IOException) { failed = true; }
        try { stream.Dispose(); }
        catch (IOException) { failed = true; }
        check(failed && destination.Disposed, "异步写入失败会关闭队列、上报错误并释放目标流");
    }

    sealed class FailingStream : MemoryStream
    {
        public bool Disposed;
        public override void Write(byte[] buffer, int offset, int count) => throw new IOException("expected write failure");
        protected override void Dispose(bool disposing) { Disposed = true; base.Dispose(disposing); }
    }

}
