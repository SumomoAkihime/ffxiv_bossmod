namespace BossMod.Dawntrail.Alliance.A23Kamlanaut;

class ShieldBash(BossModule module) : Components.KnockbackFromCastTarget(module, AID.ShieldBash, 30)
{
    private static readonly float _platformSafeRad = MathF.Atan2(5, 40);

    public static Func<WPos, bool> SafetyShape(WPos origin) => p =>
    {
        var d = p - origin;
        var angle = d.ToAngle();
        return !angle.AlmostEqual(180.Degrees(), _platformSafeRad) && !angle.AlmostEqual(60.Degrees(), _platformSafeRad) && !angle.AlmostEqual(-60.Degrees(), _platformSafeRad) || d.LengthSq() >= 100;
    };

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var src in Sources(slot, actor))
            if (!IsImmune(slot, src.Activation))
                hints.AddForbiddenZone(SafetyShape(src.Origin), src.Activation);
    }
}
