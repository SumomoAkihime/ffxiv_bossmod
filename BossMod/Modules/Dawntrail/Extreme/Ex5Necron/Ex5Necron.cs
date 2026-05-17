namespace BossMod.Dawntrail.Extreme.Ex5Necron;

[ModuleInfo(GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1062, NameID = 14093, PlanLevel = 100)]
public class Ex5Necron(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsRect(18, 15));

// Compatibility wrappers for Reborn state-tracking names.
sealed class Wipe : BossComponent
{
    public bool Wiped;

    public Wipe(BossModule module) : base(module)
    {
        KeepOnPhaseChange = true;
    }

    public override void OnEventDirectorUpdate(uint updateID, uint param1, uint param2, uint param3, uint param4)
    {
        if (updateID == 0x80000029)
            Wiped = true;
    }
}

sealed class Intermission(BossModule module) : BossComponent(module)
{
    public bool Started;

    public override void OnEventDirectorUpdate(uint updateID, uint param1, uint param2, uint param3, uint param4)
    {
        if (param2 == 0 && updateID == 0x8000000C)
            Started = true;
    }
}
