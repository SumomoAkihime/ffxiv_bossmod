namespace BossMod.Dawntrail.Savage.M11STheTyrant;

// Reborn-name compatibility layer.
// Important: these are type aliases via inheritance only; no state machine wiring is added here.
// This avoids duplicate mechanic activation / duplicate AID listeners.
public class M11STheTyrant(WorldState ws, Actor primary) : global::BossMod.Dawntrail.Savage.RM11STheTyrant.M11STheTyrant(ws, primary);

class ArenaChanges(BossModule module) : global::BossMod.Dawntrail.Savage.RM11STheTyrant.FlatlinerArena(module);
class UltimateTrophyWeapons(BossModule module) : global::BossMod.Dawntrail.Savage.RM11STheTyrant.UltimateTrophyWeaponsAOE(module);
class CometTethers(BossModule module) : global::BossMod.Dawntrail.Savage.RM11STheTyrant.ForegoneFatality(module);
class FireBreath(BossModule module) : global::BossMod.Dawntrail.Savage.RM11STheTyrant.CosmicKiss(module);
class MajesticMeteorStorm(BossModule module) : global::BossMod.Dawntrail.Savage.RM11STheTyrant.MajesticMeteor(module);
class MeteorainComets(BossModule module) : global::BossMod.Dawntrail.Savage.RM11STheTyrant.Comet(module);
class ExplosionTowers(BossModule module) : global::BossMod.Dawntrail.Savage.RM11STheTyrant.ExplosionTower(module);
class ExplosionTowerKnockback(BossModule module) : global::BossMod.Dawntrail.Savage.RM11STheTyrant.ExplosionKnockback(module);
class ExplosionTowerHints(BossModule module) : global::BossMod.Dawntrail.Savage.RM11STheTyrant.FireBreathMeteowrathHints(module);
class Shockwave(BossModule module) : global::BossMod.Dawntrail.Savage.RM11STheTyrant.ShockwaveCounter(module);
class ArcadionAvalanche(BossModule module) : global::BossMod.Dawntrail.Savage.RM11STheTyrant.ArcadionAvalancheRect(module);
class ArcadionAvalancheSmash(BossModule module) : global::BossMod.Dawntrail.Savage.RM11STheTyrant.ArcadionAvalancheBoss(module);
class FearsomeFireball(BossModule module) : global::BossMod.Dawntrail.Savage.RM11STheTyrant.FearsomeFireball1(module);
class GreatWallOfFire(BossModule module) : global::BossMod.Dawntrail.Savage.RM11STheTyrant.Firewall(module);
class GreatWallOfFireExplosion(BossModule module) : global::BossMod.Dawntrail.Savage.RM11STheTyrant.FirewallExplosion(module);
class FlatlinerKB(BossModule module) : global::BossMod.Dawntrail.Savage.RM11STheTyrant.Flatliner(module);
class CrushingComet(BossModule module) : global::BossMod.Dawntrail.Savage.RM11STheTyrant.Comets(module);
class WeightyImpactTowers(BossModule module) : global::BossMod.Dawntrail.Savage.RM11STheTyrant.ImpactKiss(module);
class HeartBreakerTower(BossModule module) : global::BossMod.Dawntrail.Savage.RM11STheTyrant.HeartbreakKick(module);
class AtomicImpactVoidZones(BossModule module) : global::BossMod.Dawntrail.Savage.RM11STheTyrant.AtomicImpactPuddle(module);
class DanceOfDomination(BossModule module) : global::BossMod.Dawntrail.Savage.RM11STheTyrant.DanceOfDomination1(module);
class MaelstromVoidZones(BossModule module) : global::BossMod.Dawntrail.Savage.RM11STheTyrant.Maelstrom(module);
class MaelstromGustCones(BossModule module) : global::BossMod.Dawntrail.Savage.RM11STheTyrant.PowerfulGust(module);
class RawSteelTrophyAxe(BossModule module) : global::BossMod.Dawntrail.Savage.RM11STheTyrant.RawSteelTrophyAxe(module);
class RawSteelTrophyScythe(BossModule module) : global::BossMod.Dawntrail.Savage.RM11STheTyrant.RawSteelTrophyScythe(module);

