using System.Reflection;
using BossMod;
using BossMod.Components;
using BossMod.Stormblood.Ultimate.UCOB;
using Role = BossMod.PartyRolesConfig.Assignment;

static class UCOBUpstreamTests
{
    const BindingFlags All = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

    static Type Internal(string name) => typeof(UCOB).Assembly.GetType("BossMod.Stormblood.Ultimate.UCOB." + name, true)!;
    static BossComponent Component(UCOB module, string name) => (BossComponent)Activator.CreateInstance(Internal(name), All, null, [module], null)!;
    static void Set(object target, string field, object? value) => target.GetType().GetField(field, All)!.SetValue(target, value);

    static (WorldState world, UCOB module, Actor player) Fixture()
    {
        var world = new WorldState(10000000, "offline");
        Actor Create(ulong id, int index, uint oid, ActorType type, WPos position)
        {
            world.Execute(new ActorState.OpCreate(id, oid, index, 0, "fixture", 0, type, type == ActorType.Player ? Class.PLD : Class.None, 100, new(position.X, 0, position.Z, 0), .5f, default, true, type == ActorType.Player, 0, 0, 0));
            return world.Actors.Find(id)!;
        }

        var player = Create(1, 0, 0, ActorType.Player, new(5f, 0f));
        world.Execute(new PartyState.OpModify(0, new(1, 1, false, "player")));
        var twintania = Create(20, 20, (uint)OID.Twintania, ActorType.Enemy, default);
        return (world, new UCOB(world, twintania), player);
    }

    static ActorCastEvent Event(AID action) => new(ActionID.MakeSpell(action), 0, 0, 0, default, 1, 0, default);

    public static void Run(Action<bool, string> check)
    {
        var (world, module, player) = Fixture();
        using (module)
        {
            var quote = (UniformStackSpread)Component(module, "QuoteMeteorStream");
            quote.AddSpread(player, world.FutureTime(5f));
            Set(quote, "Fixed", true);
            var hints = new AIHints();
            quote.AddAIHints(0, player, Role.MT, hints);
            check(hints.ForbiddenZones.Count == 0, "绝巴哈固定散开缺巴哈实体时退回通用分散，不生成假目标");

            var preposition = Component(module, "P3HeavensfallPreposition");
            hints.Clear();
            preposition.AddAIHints(0, player, Role.MT, hints);
            check(hints.ForbiddenZones.Count == 0, "绝巴哈天堂陨落预站位缺巴哈实体时不猜测目标");

            var trio = Component(module, "P3HeavensfallTrio");
            Set(trio, "_divesActive", true);
            hints.Clear();
            trio.AddAIHints(99, player, Role.MT, hints);
            check(hints.ForbiddenZones.Count == 0, "绝巴哈天堂陨落未完成初始化不产生默认安全点");

            var heavensfall = (GenericKnockback)Component(module, "P3Heavensfall");
            var activation = world.FutureTime(8f);
            Set(heavensfall, "Activation", activation);
            hints.Clear();
            heavensfall.AddAIHints(0, player, Role.MT, hints);
            check(hints.ForbiddenZones.Count == 0 && heavensfall.ActiveKnockbacks(0, player)[0].Activation == activation,
                "绝巴哈天堂陨落保留击退结算时间，站位约束交由塔组件统一处理");

            var towers = (GenericTowers)Component(module, "P3HeavensfallTowers");
            var towerActivation = world.FutureTime(10f);
            towers.Towers.Add(new(new WPos(0f, 5f), 3f, activation: towerActivation));
            hints.Clear();
            towers.AddAIHints(0, player, Role.MT, hints);
            check(hints.ForbiddenZones.Count == 1 && hints.ForbiddenZones[0].activation == towerActivation.AddSeconds(-2.5d), "绝巴哈塔击退前保留预站位约束");
            towers.OnEventCast(module.PrimaryActor, Event(AID.Heavensfall));
            hints.Clear();
            towers.AddAIHints(0, player, Role.MT, hints);
            check(hints.ForbiddenZones.Count == 1 && hints.ForbiddenZones[0].activation == towerActivation, "绝巴哈塔击退后切换到普通踩塔约束");

            var enemy = new AIHints.Enemy(module.PrimaryActor, 1, true) { DesiredPosition = new(10f, 0f), DesiredRotation = default };
            module.PrimaryActor.TargetID = player.InstanceID;
            hints.Clear();
            hints.Enemies[module.PrimaryActor.CharacterSpawnIndex] = enemy;
            hints.PotentialTargets.Add(enemy);
            module.CalculateAIHints(0, player, Role.MT, hints);
            check(enemy.TankDistance == 0f && hints.GoalZones.Count == 3, "绝巴哈坦克目标同时保留拉怪、精确朝向和通用目标");
            hints.Clear();
            enemy.CanMove = false;
            hints.Enemies[module.PrimaryActor.CharacterSpawnIndex] = enemy;
            hints.PotentialTargets.Add(enemy);
            module.CalculateAIHints(0, player, Role.MT, hints);
            check(hints.GoalZones.Count == 2, "绝巴哈不可移动首领不输出拉怪目标");
        }
    }
}
