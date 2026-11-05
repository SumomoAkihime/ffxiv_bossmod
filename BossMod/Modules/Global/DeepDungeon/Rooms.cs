using Lumina.Data.Files;
using static FFXIVClientStructs.FFXIV.Client.Game.InstanceContent.InstanceContentDeepDungeon;
using static Lumina.Data.Parsing.Layer.LayerCommon;

namespace BossMod.Global.DeepDungeon;

abstract partial class AutoClear
{
    private record struct RoomBox(int Room, Vector3 Position, Vector3 Scale, Vector3 Rotation)
    {
        public readonly WPos Pos => new(Position.XZ());
        public readonly WDir Size => new(Scale.XZ());

        public readonly bool Contains(WPos p) => p.InRect(Pos, default(Angle), Size.Z, Size.Z, Size.X);
        public readonly bool Contains(Vector3 v) => Contains(new WPos(v.X, v.Z));
    }

    private readonly List<List<RoomBox>> _floorRects = [];

    private void LoadGeometry()
    {
        _floorRects.Clear();

        var tt = Service.LuminaRow<Lumina.Excel.Sheets.TerritoryType>(World.CurrentZone);
        if (tt == null)
        {
            Service.Log($"unable to load level data for zone {World.CurrentZone}");
            return;
        }

        var bg = tt.Value.Bg.ToString();
        var lgb = $"bg/{bg[..bg.LastIndexOf('/')]}/planmap.lgb";

        var levelData = Service.LuminaGameData?.GetFile<LgbFile>(lgb);
        if (levelData == null)
        {
            Service.Log($"unable to load level data for zone {World.CurrentZone}");
            return;
        }

        foreach (var layer in levelData.Layers)
        {
            var roomRanges = layer.InstanceObjects.Where(o => o.AssetType == Lumina.Data.Parsing.Layer.LayerEntryType.EventRange && ((EventRangeInstanceObject)o.Object).ParentData.Priority is > 100 and < 126).ToList();

            if (roomRanges.Count == 0)
                continue;

            var roomsPrio = roomRanges.GroupBy(r => ((EventRangeInstanceObject)r.Object).ParentData.Priority);

            _floorRects.Add([.. Enumerable.Repeat(default(RoomBox), 25)]);
            var fr = _floorRects[^1];

            foreach (var grouping in roomsPrio)
            {
                var prio = grouping.Key;
                // assume largest box is the room collider and any others are hallways
                var box = grouping.MaxBy(r => r.Transform.Scale.X * r.Transform.Scale.Z);

                var roomIndex = prio - 100;
                if (roomIndex is < 0 or > 24)
                    throw new ArgumentException($"got unrecognized priority {prio} for collision box");

                fr[roomIndex] = new(roomIndex, new Vector3(box.Transform.Translation.X, box.Transform.Translation.Y, box.Transform.Translation.Z), new Vector3(box.Transform.Scale.X, box.Transform.Scale.Y, box.Transform.Scale.Z), new Vector3(box.Transform.Rotation.X, box.Transform.Rotation.Y, box.Transform.Rotation.Z));
            }
        }

        // TODO: reverse more layout stuff so we can use a less stupid solution
        // tileset 0 is almost always SW and tileset 1 is NE but palace swaps them for certain floors
        var layerRow = Service.LuminaSheet<Lumina.Excel.Sheets.DeepDungeonLayer>()?.FirstOrNull(l => l.DeepDungeon.RowId == (uint)Palace.DungeonId && l.FloorSet == (byte)(Palace.Floor / 10));
        if (layerRow?.RoomA.RowId > layerRow?.RoomB.RowId)
            _floorRects.Reverse();
    }

    private static WDir ToCardinal(WDir x)
    {
        return MathF.Abs(x.X) < MathF.Abs(x.Z) ? new WDir(0, x.Z) : new WDir(x.X, 0);
    }

    private float DistanceBox2D(WPos p, Vector2 rectMin, Vector2 rectMax)
    {
        var dx = MathF.Max(MathF.Max(rectMin.X - p.X, 0), p.X - rectMax.X);
        var dy = MathF.Max(MathF.Max(rectMin.Y - p.Z, 0), p.Z - rectMax.Y);
        return dx * dx + dy * dy;
    }

    private float DistanceBox2D(WPos p, RoomBox rect) => DistanceBox2D(p, (rect.Position - rect.Scale).XZ(), (rect.Position + rect.Scale).XZ());

    private bool HasRoomGeometry => Palace.Progress.Tileset < _floorRects.Count && _floorRects[Palace.Progress.Tileset].Any(r => r.Size != default);

    private (int Index, RoomBox Room) FindClosestRoom(WPos p)
    {
        if (!HasRoomGeometry)
            return default;
        return _floorRects[Palace.Progress.Tileset].Index().Where(i => i.Item.Size != default && Palace.Rooms[i.Index] != RoomFlags.None)
            .MinBy(r => DistanceBox2D(p, r.Item));
    }

    private bool IsRoomAdjacent(int i, int j)
    {
        if (i is >= 25 or < 0)
            return false;
        var md = Palace.Rooms[i];
        if (md.HasFlag(RoomFlags.ConnectionN) && i - 5 == j)
            return true;
        if (md.HasFlag(RoomFlags.ConnectionS) && i + 5 == j)
            return true;
        if (md.HasFlag(RoomFlags.ConnectionW) && i - 1 == j)
            return true;
        if (md.HasFlag(RoomFlags.ConnectionE) && i + 1 == j)
            return true;
        return false;
    }

    private bool IsInThisRoomOrAdjacent(WPos p)
    {
        if (!HasRoomGeometry)
            return true;
        var (r, _) = FindClosestRoom(p);
        return PlayerRoom == r || IsRoomAdjacent(r, PlayerRoom);
    }

    private bool IsInThisRoomOrAdjacent(Actor a) => IsInThisRoomOrAdjacent(a.Position);

}
