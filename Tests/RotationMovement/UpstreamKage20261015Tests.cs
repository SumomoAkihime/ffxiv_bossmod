using BossMod;
using BossMod.Autorotation;
using BossMod.Autorotation.kage;

static class UpstreamKage20261015Tests
{
    private static readonly (string Name, Class Class)[] Modules =
    [
        ("BLM", Class.BLM),
        ("DNC", Class.DNC),
        ("MCH", Class.MCH),
        ("PLD", Class.PLD),
        ("SAM", Class.SAM),
        ("SGE", Class.SGE),
        ("VPR", Class.VPR),
        ("WHM", Class.WHM),
    ];

    public static void Run(RotationModuleManager manager, Actor player, Actor target, AIHints hints, Action<bool, string> check)
    {
        var oldClass = player.Class;
        var oldLevel = player.Level;
        var oldCombat = player.InCombat;
        var world = manager.WorldState;
        var oldGCD = world.Client.Cooldowns[ActionDefinitions.GCDGroup];
        try
        {
            foreach (var (name, classID) in Modules)
            {
                var type = typeof(KageRotation<>).Assembly.GetType($"BossMod.Autorotation.kage.Kage{name}", true)!;
                var definition = (RotationModuleDefinition)type.GetMethod("Definition")!.Invoke(null, null)!;
                check(definition.Quality == RotationModuleQuality.Ok && definition.MaxLevel == 100 && definition.Configs.Count > 0, $"Kage {name} definition");

                var strategy = new StrategyValues(definition.Configs);
                var module = (RotationModule)Activator.CreateInstance(type, manager, player)!;

                player.Class = classID;
                player.Level = 1;
                hints.Clear();
                RunOnce(null, $"Kage {name} level 1 no target");

                player.Level = 100;
                hints.Clear();
                AddTarget(target, 1);
                RunOnce(target, $"Kage {name} single target");

                hints.Clear();
                AddTarget(target, 1);
                var secondary = new Actor(0x700001, 0, 6, 0, "fixture", 0, ActorType.Enemy, Class.None, 100, new(target.Position.X + 1, 0, target.Position.Z, 0), 1, new(1000, 1000, 0, 10000, 10000), true);
                AddTarget(secondary, 2);
                RunOnce(target, $"Kage {name} multi target");

                if (name == "BLM")
                {
                    hints.Clear();
                    AddTarget(target, 1);
                    player.InCombat = true;
                    world.Client.Cooldowns[ActionDefinitions.GCDGroup] = new(0, 2.5f);
                    var burst = definition.Configs.FindIndex(c => c.InternalName == "Burst");
                    ((StrategyValueTrack)strategy.Values[burst]).Option = (int)OffensiveStrategy.Force;
                    RunOnce(target, "Kage BLM forced burst with empty gauges");
                    check(hints.ActionsToExecute.Entries.Any(e => e.Action == ActionID.MakeSpell(BossMod.BLM.AID.LeyLines)), "Kage BLM forced burst uses Ley Lines at zero resource");
                }

                void AddTarget(Actor actor, int priority)
                {
                    var enemy = new AIHints.Enemy(actor, priority, true);
                    hints.Enemies[actor.CharacterSpawnIndex] = enemy;
                    hints.PotentialTargets.Add(enemy);
                }

                void RunOnce(Actor? primary, string label)
                {
                    module.Execute(strategy, ref primary, 0f, false);
                    check(true, label);
                }
            }
        }
        finally
        {
            player.Class = oldClass;
            player.Level = oldLevel;
            player.InCombat = oldCombat;
            world.Client.Cooldowns[ActionDefinitions.GCDGroup] = oldGCD;
            hints.Clear();
        }

    }
}
