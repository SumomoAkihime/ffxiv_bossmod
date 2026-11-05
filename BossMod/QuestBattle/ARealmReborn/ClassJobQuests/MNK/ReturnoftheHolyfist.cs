namespace BossMod.QuestBattle.ARealmReborn.ClassJobQuests.MNK;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 323)]
internal class ReturnOfTheHolyfist(WorldState ws) : QuestBattle(ws)
{
    public override void AddQuestAIHints(Actor player, AIHints hints)
    {
        hints.PrioritizeAll();
        foreach (var target in hints.PotentialTargets)
            if (target.Actor.Level == 20 && target.Actor.OID == 0x36F)
                target.ForcePriority(0);
    }
}


[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 322)]
internal class ReturnOfTheHolyfistFirst(WorldState ws) : ReturnOfTheHolyfist(ws);
