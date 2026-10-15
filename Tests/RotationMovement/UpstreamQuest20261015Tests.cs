using System.Numerics;
using BossMod;
using BossMod.QuestBattle;

static class UpstreamQuest20261015Tests
{
    private static readonly (uint CFC, string TypeName, int MinimumObjectives)[] Modules =
    [
        (381, "StrayIntoTheShadows", 2),
        (384, "GrinnersInTheMist", 2),
        (385, "SweetSorrows", 1),
        (386, "CloyingVictory", 9),
        (681, "LegendsOfTheNotSoHiddenTemple", 10),
        (793, "InFromTheCold", 15),
    ];

    public static void Run(WorldState world, Actor player, AIHints hints, Action<bool, string> check)
    {
        var property = typeof(Service).GetProperty("PluginInterface")!;
        var original = property.GetValue(null);
        property.SetValue(null, null); // QuestBattle has an explicit offline/UIDev constructor path.
        try { RunOffline(world, player, hints, check); }
        finally { property.SetValue(null, original); hints.Clear(); }
    }

    private static void RunOffline(WorldState world, Actor player, AIHints hints, Action<bool, string> check)
    {
        using (var deferred = ZoneModuleRegistry.CreateModule(world, 382, BossModuleInfo.Maturity.Contributed))
            check(deferred == null, "StifledScreams remains unregistered until local blocked-target visibility is available");

        foreach (var typeName in new[] { "ChasingShadows", "LurkersInTheGrotto" })
        {
            var type = typeof(QuestBattle).Assembly.GetType($"BossMod.QuestBattle.ARealmReborn.MSQ.{typeName}", true)!;
            check(type.GetMethod(nameof(QuestBattle.AddQuestAIHints))?.DeclaringType == type, $"{typeName} uses the QuestBattle AI hint hook");
        }

        foreach (var (cfc, typeName, minimumObjectives) in Modules)
        {
            using var module = ZoneModuleRegistry.CreateModule(world, cfc, BossModuleInfo.Maturity.Contributed);
            check(module != null && module.GetType().Name == typeName, $"Quest CFC {cfc} registers {typeName}");
            if (module is not QuestBattle quest)
                continue;

            var objectives = quest.DefineObjectives(world);
            check(objectives.Count >= minimumObjectives, $"{typeName} defines ordered task objectives");
            check(objectives.All(o => ReferenceEquals(o.World, world)), $"{typeName} objectives bind to the active world");

            if (typeName == "CloyingVictory")
            {
                var defeatedEnemy = new Actor(0x700100, 0xD8E, 1, 0, "fixture", 0, ActorType.Enemy, Class.None, 1, new(1, 0, 1, 0));
                objectives[1].OnActorKilled?.Invoke(defeatedEnemy);
                check(objectives[1].Completed, "CloyingVictory advances after the expected enemy is killed");
            }
            if (typeName == "StrayIntoTheShadows")
            {
                var hiddenTarget = new Actor(0x700101, 0xD47, 1, 0, "fixture", 0, ActorType.Enemy, Class.None, 1, new(1, 0, 1, 0));
                hints.Clear();
                hints.PotentialTargets.Add(new AIHints.Enemy(hiddenTarget, 1, true));
                quest.AddQuestAIHints(player, hints);
                check(hints.PotentialTargets[0].Priority == AIHints.Enemy.PriorityUndesirable, "StrayIntoTheShadows suppresses combat targets while stealthing");
                check(hints.ActionsToExecute.Entries.Any(a => a.Action == ActionID.MakeSpell(BossMod.NIN.AID.Hide)), "StrayIntoTheShadows queues Hide outside combat");
                hints.Clear();
            }
        }

        using var legends = (QuestBattle?)ZoneModuleRegistry.CreateModule(world, 681, BossModuleInfo.Maturity.Contributed);
        if (legends != null)
        {
            var firstRoute = legends.DefineObjectives(world)[0].Connections;
            check(firstRoute.Count == 1 && firstRoute[0].Position == new Vector3(14.57f, 0.07f, 40f), "Hidden Temple keeps its first patrol-safe route waypoint");
        }
    }
}
