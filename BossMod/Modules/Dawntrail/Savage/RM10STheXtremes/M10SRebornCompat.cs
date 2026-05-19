namespace BossMod.Dawntrail.Savage.RM10STheXtremes;

// Reborn-name compatibility aliases.
// Keep these as thin wrappers only; state machine activation stays on canonical RM10 classes.
class ArenaChanges(BossModule module) : BubbleBounds(module);
class DiversDareRed(BossModule module) : DiversDare1(module);
class DiversDareBlue(BossModule module) : DiversDare2(module);
class XtremeSpectacular(BossModule module) : XtremeSpectacularProximity(module);
class XtremeSpectacularLast(BossModule module) : XtremeSpectacularRaidwideLast(module);
class Firesnaking(BossModule module) : FiresnakingRaidwide(module);
class XtremeFiresnaking(BossModule module) : XtremeFiresnakingRaidwide(module);
class DeepImpactKnockback(BossModule module) : DeepImpactKB(module);
class SickSwell1(BossModule module) : SickSwell(module);
class Pyrorotation(BossModule module) : Pyrotation(module);
class PyrorotationPuddle(BossModule module) : PyrotationPuddle(module);

// No-op placeholders for Reborn-only symbol names that have no 1:1 canonical RM10 component.
// Intentional to avoid introducing duplicated AID watchers or parallel mechanic activations.
class AlleyOopInferno(BossModule module) : BossComponent(module);
class AlleyOopWater(BossModule module) : BossComponent(module);
class AlleyOopWaterAfter(BossModule module) : BossComponent(module);
class AwesomeSplashSlab(BossModule module) : BossComponent(module);
class AwesomeSplashSlabAerial(BossModule module) : BossComponent(module);
class BlastingSnapPuddle(BossModule module) : BossComponent(module);
class BlueTether(BossModule module) : BossComponent(module);
class RedTether(BossModule module) : BossComponent(module);
class RedBlueTether(BossModule module) : BossComponent(module);
class CutbackBlaze(BossModule module) : BossComponent(module);
class DeepImpact(BossModule module) : BossComponent(module);
class HotImpact2(BossModule module) : BossComponent(module);
class InsaneAirTest(BossModule module) : BossComponent(module);
class Watersnaking(BossModule module) : BossComponent(module);
class XtremeWatersnaking(BossModule module) : BossComponent(module);
