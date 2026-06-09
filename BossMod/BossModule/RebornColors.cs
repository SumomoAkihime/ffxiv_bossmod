namespace BossMod;

// Reborn modules use Colors.* for radar rendering. Map it to this fork's
// configurable arena palette so migrated modules keep their color semantics.
public static class Colors
{
    public static uint AOE => ArenaColor.AOE;
    public static uint Danger => ArenaColor.Danger;
    public static uint Safe => ArenaColor.Safe;
    public static uint SafeFromAOE => ArenaColor.SafeFromAOE;
    public static uint Border => ArenaColor.Border;
    public static uint Enemy => ArenaColor.Enemy;
    public static uint PC => ArenaColor.PC;
    public static uint Object => ArenaColor.Object;
    public static uint PlayerInteresting => ArenaColor.PlayerInteresting;
    public static uint PlayerGeneric => ArenaColor.PlayerGeneric;
    public static uint Vulnerable => ArenaColor.Vulnerable;
}
