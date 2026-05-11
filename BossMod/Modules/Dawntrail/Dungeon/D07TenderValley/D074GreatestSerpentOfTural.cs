namespace BossMod.Dawntrail.Dungeon.D07TenderValley.D074GreatestSerpentOfTural;

public enum OID : uint { Boss = 0x4164, Helper = 0x233C }
public enum AID : uint
{
    DubiousTulidisaster = 36748,
    MisplacedMystery = 36750,
    ExaltedWobble = 36749,
    ScreesOfFury = 36757,
    MightyBlorp1 = 39983,
    MightyBlorp2 = 39982,
    MightyBlorp3 = 39981,
    GreatestFlood = 36756,
    GreatTorrentAOE = 36754,
    GreatTorrentSpread = 36755,
    GreatestLabyrinth = 36745
}
public enum IconID : uint { MightyBlorp1 = 62, MightyBlorp2 = 542, MightyBlorp3 = 543, GreatTorrent = 139 }

class DubiousTulidisaster(BossModule module) : Components.RaidwideCast(module, AID.DubiousTulidisaster);
class GreatestLabyrinthRaidwide(BossModule module) : Components.RaidwideCast(module, AID.GreatestLabyrinth);
class GreatestFloodRaidwide(BossModule module) : Components.RaidwideCast(module, AID.GreatestFlood);
class ScreesOfFury(BossModule module) : Components.SingleTargetCast(module, AID.ScreesOfFury);
class GreatestFlood(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.GreatestFlood, 15);
class ExaltedWobble(BossModule module) : Components.StandardAOEs(module, AID.ExaltedWobble, new AOEShapeCircle(9));
class MisplacedMystery(BossModule module) : Components.StandardAOEs(module, AID.MisplacedMystery, new AOEShapeRect(52, 2.5f));
class GreatTorrent(BossModule module) : Components.StandardAOEs(module, AID.GreatTorrentAOE, new AOEShapeCircle(6));
class GreatTorrentSpread(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.GreatTorrent, AID.GreatTorrentSpread, 6, 5.1f);
class MightyBlorp1(BossModule module) : Components.StackWithIcon(module, (uint)IconID.MightyBlorp1, AID.MightyBlorp1, 6, 4.6f, 4, 4);
class MightyBlorp2(BossModule module) : Components.StackWithIcon(module, (uint)IconID.MightyBlorp2, AID.MightyBlorp2, 5, 4.6f, 4, 4);
class MightyBlorp3(BossModule module) : Components.StackWithIcon(module, (uint)IconID.MightyBlorp3, AID.MightyBlorp3, 4, 4.6f, 4, 4);

class D074GreatestSerpentOfTuralStates : StateMachineBuilder
{
    public D074GreatestSerpentOfTuralStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DubiousTulidisaster>()
            .ActivateOnEnter<GreatestLabyrinthRaidwide>()
            .ActivateOnEnter<GreatestFloodRaidwide>()
            .ActivateOnEnter<ScreesOfFury>()
            .ActivateOnEnter<MightyBlorp1>()
            .ActivateOnEnter<MightyBlorp2>()
            .ActivateOnEnter<MightyBlorp3>()
            .ActivateOnEnter<GreatestFlood>()
            .ActivateOnEnter<ExaltedWobble>()
            .ActivateOnEnter<MisplacedMystery>()
            .ActivateOnEnter<GreatTorrent>()
            .ActivateOnEnter<GreatTorrentSpread>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "The Combat Reborn Team, CN compatibility port", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 834, NameID = 12709)]
public class D074GreatestSerpentOfTural(WorldState ws, Actor primary) : BossModule(ws, primary, new(-130, -554), new ArenaBoundsSquare(14.5f));
