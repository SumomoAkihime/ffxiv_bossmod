namespace BossMod.RealmReborn.Trial.T09WhorleaterH;

sealed class GrandFall(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GrandFall, 8f);
sealed class Hydroshot(BossModule module) : Components.VoidzoneAtCastTarget(module, 5f, (uint)AID.Hydroshot, GetVoidzones)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.HydroshotZone);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.EventState != 7)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }
}
sealed class Dreadstorm(BossModule module) : Components.VoidzoneAtCastTarget(module, 5f, (uint)AID.Dreadstorm, GetVoidzones)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.DreadstormZone);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.EventState != 7)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }
}

sealed class T09WhorleaterHStates : StateMachineBuilder
{
    public T09WhorleaterHStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<GrandFall>()
            .ActivateOnEnter<Hydroshot>()
            .ActivateOnEnter<Dreadstorm>()
            .ActivateOnEnter<BodySlamKB>()
            .ActivateOnEnter<BodySlamAOE>()
            .ActivateOnEnter<SpinningDive>()
            .ActivateOnEnter<SpinningDiveKB>()
            .ActivateOnEnter<Hints>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "taurenkey, Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 72, NameID = 2505)]
public sealed class T09WhorleaterH(WorldState ws, Actor primary) : BossModule(ws, primary, default, new ArenaBoundsRect(14.5f, 19.5f))
{
    private static readonly uint[] adds = [(uint)OID.Tail, (uint)OID.WavespineSahagin, (uint)OID.WavespineSahagin, (uint)OID.WavetoothSahagin];
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(this, adds);
        Arena.Actors(Enemies((uint)OID.Spume), Colors.Vulnerable);
        Arena.Actors(Enemies((uint)OID.Converter), Colors.Object);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var tankMimicry = actor.FindStatus(SID.MimicryTank) != null;
        var magic = actor.Class.GetClassCategory() is ClassCategory.Caster or ClassCategory.Healer || actor.Class is Class.BLU && !tankMimicry;
        var physRanged = actor.Class.GetClassCategory() is ClassCategory.PhysRanged;
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.WavetoothSahagin => 4,
                OID.Spume => 3,
                OID.WavespineSahagin => 2,
                OID.Boss or OID.Tail => 1,
                _ => 0
            };
            if (physRanged && (OID)e.Actor.OID == OID.Boss && e.Actor.FindStatus(SID.VeilOfTheWhorl) != null
                || magic && (OID)e.Actor.OID == OID.Tail && e.Actor.FindStatus(SID.MantleOfTheWhorl) != null)
                e.Priority = AIHints.Enemy.PriorityForbidden;
        }

        if (Enemies(OID.Converter).FirstOrDefault(c => c.IsTargetable) is { } converter
            && Raid.WithoutSlot().Where(p => !p.IsDead).MinBy(converter.DistanceToHitbox) == actor)
            hints.InteractWithTarget = converter;
    }
}
