# BossMod Upstream Sync Playbook

## Current Strategy
- First pass: low-risk compatibility layer to keep compile green and preserve installability.
- Second pass: behavior parity for high-risk mechanics (state machines, arena transformations, complex helper APIs).

## Versioning Rule
- Use four-part AssemblyVersion.
- After each upstream merge, reset local version base from upstream version.
- Formula: if upstream is `a.b.c.d`, first local release is `a.b.c.(d*100+1)`, then increment by `+1` each local release.
- Example: upstream `7.5.0.5` -> local `7.5.0.501`, `7.5.0.502`, `7.5.0.503`...

## Files Added As Compatibility (Not Full Behavior Parity)
- Ex5/Ex6/Ex7/Ex4 alias and scaffold files under `BossMod/Modules/Dawntrail/Extreme/*`.
- Ex3 scaffold file: `Ex3CompatibilityScaffolds.cs`.

## Remaining High-Risk Port Scope
- Ex3QueenEternal full module set (states/enums/mechanics).
- Ex4 advanced behavior files where direct aliasing is not equivalent.

## Safe Release Checklist
1. `dotnet build -c Release BossMod/BossMod.csproj` succeeds with 0 errors.
2. Copy package to `release/BossMod/latest.zip`.
3. Regenerate root repo index with `tools/Build-DalamudRepo.ps1 -BaseUrl ...`.
4. Verify `plugin-repo/repo.json` BossMod AssemblyVersion matches csproj.
5. Restore unrelated plugin artifacts (for example `WrathComboCN/latest.zip`) before commit.

## Conflict/Drift Checks
- Recompute normalized missing list before each port batch.
- Treat `EX_MISSING_AFTER_PHASE4.txt` as advisory only; verify class/file coverage in current fork.
- If a Reborn class references missing framework APIs, fallback to scaffold + TODO marker instead of forcing a risky bulk port.

## Latest Batch (2026-05-10)
- Reborn source window reviewed: `30308ae43..69bc2b2ea` (up to tag `7.5.0.28`).
- Scope kept to module-facing radar/mechanic compatibility, excluding autorotation/framework/network/data/submodule updates.
- Applied compatibility downgrade strategy:
  - `Dawntrail/Extreme/Ex8Enuo`: added alias coverage for newly introduced Reborn mechanic names instead of framework-level rewrites.
  - `Dawntrail/Dungeon/D13Clyteum`: preserved existing local behavior model and continued parameter/icon-driven alignment.
- Additional low-risk behavior parity applied on current fork APIs:
  - `RealmReborn/Trial/T08ThornmarchH/PomMeteor.cs`: switched tower offset initialization to helper generation, matching Reborn's static-init-safe path.
  - `Endwalker/Extreme/Ex4Barbariccia/WindingGale.cs`: normalized donut-sector parameters and action-id matching to the newer implementation style without changing local API shape.
  - `Dawntrail/Extreme/Ex8Enuo`: widened intermission radar bounds and added explicit `NaughtHuntsJumps` tether-line visualization while keeping the existing local `EndlessChase`/state-machine model.
- Deferred after review because they still require non-trivial framework/module scaffolding in this fork:
  - `Dawntrail/Trial/T08Enuo`
  - `Dawntrail/Raid/M07NBruteAbombinator`
  - `Dawntrail/Advanced/Ad01TheMerchantsTale/Ad011PariofPlenty`
  - `Dawntrail/Savage/M12SP1Lindwurm` (local equivalent remains `RM12S1TheLindwurm`)
- Skeleton-first completion batch:
  - Added minimal compile-safe modules for `Ad011PariofPlenty`, `M07NBruteAbombinator`, and `T08Enuo`.
  - Strategy was intentionally downgraded to clean local skeletons plus stable radar-visible mechanics only, instead of direct multi-file Reborn parity.
  - This fork now has these module entrypoints present and publishable; deeper mechanic parity remains future follow-up work.
- Follow-up parity batch on top of the skeletons:
  - `T08Enuo`: restored `NaughtHunts` chasing AOE visibility using the fork's native `StandardChasingAOEs`.
  - `M07NBruteAbombinator`: added `NeoBombarianSpecial` knockback, `BrutalSmash` shared tankbuster, and `AbominableBlink` flare visibility using local stable components.
- Additional fast parity batch:
  - `T08Enuo`: added `NaughtHuntsJumps` tether-line visualization for player pass links.
  - `M07NBruteAbombinator`: added `Sporesplosion` and `Explosion` AOE visibility components.
- Release target for this batch: `7.5.0.315` (sequential local versioning per current branch policy).
- Additional downgrade batch (7.5.0.316):
  - Attempted direct multi-file Reborn port for `Ad011PariofPlenty` (`Enums/States/FireFlight/LongNights`) but hit broad API/analyzer incompatibility on this fork (GenericAOEs signature drift and style-as-errors).
  - Applied compatibility fallback instead: kept local minimal module model and added low-risk radar support `FalseFlameDisplay` (`OID.FalseFlame` + `AddsPointless` state activation) to improve mechanic visibility without framework changes.
- Additional low-risk batch (7.5.0.317):
  - `M07NBruteAbombinator`: added LOS mechanic visibility for `QuarrySwamp` using local `CastLineOfSightAOE` API (`OID.BloomingAbomination` blockers + state activation).
  - Deferred `ArenaChanges.cs` direct port: Reborn version depends on shared arena-bound helpers not present in this fork; keep current local arena model to avoid framework-side drift.
- Additional low-risk batch (7.5.0.318):
  - `Ad011PariofPlenty`: added Long-Nights fireflight visibility subset on local APIs (`LeftRightFireflight` + `WheelOfFireflight` icon-driven cone sequence), including required AID/Icon mappings and state activation.
  - Kept migration scoped to radar/telegraph visibility only; no framework-side behavior ports.
- Additional low-risk batch (7.5.0.319):
  - `M07NBruteAbombinator`: added `BloomingAbomination` adds visibility and `ItCameFromTheDirt` AOE telegraph support (radius 6), plus state activation.
  - Kept scope to independent radar/telegraph components; no arena-framework synchronization changes.
- Additional low-risk batch (7.5.0.320):
  - `Ad011PariofPlenty`: added non-invasive battlefield entity visibility for `FieryBauble` and `FlyingCarpet` via `AddsPointless` components and state activation.
  - No timeline/state sequencing behavior changed.
- Additional low-risk batch (7.5.0.321):
  - `M07NBruteAbombinator`: added `BrutishSwing` circle/donut telegraphs (`AID.BrutishSwingCircle2`, `AID.BrutishSwingDonut`) with local `SimpleAOEs` components and state activation.
- Accelerated low-risk batch (7.5.0.322):
  - `M07NBruteAbombinator`: merged multiple independent telegraph components in one pass:
    - `BrutishSwingCone1`, `BrutishSwingCone2`
    - `BrutishSwingDonutSegment1`, `BrutishSwingDonutSegment2`
    - `RevengeOfTheVines` raidwide cast
  - Scope kept to local `SimpleAOEs`/`RaidwideCast` components and state activation only.
- Accelerated low-risk batch (7.5.0.323):
  - `M07NBruteAbombinator`: added interrupt hint coverage (`CrossingCrosswindsHint`, `WindingWildwindsHint`), enemy draw visibility for boss/adds, and AI target priority preferring `BloomingAbomination`.
  - Adapted draw calls to this fork's arena API signature (`Arena.Actor(..., color)` / `Arena.Actors(..., color)`).
- Accelerated low-risk batch (7.5.0.324):
  - `Ad011PariofPlenty`: added independent mechanic visibility components:
    - `ParisCurse` (`RaidwideCast`)
    - `FirePowder` + `HighFirePowder` (grouped 15y circle AOEs)
  - Added required AID mappings and state activations only.
- Accelerated low-risk batch (7.5.0.325):
  - `Ad011PariofPlenty`: added `SparkPuddle` persistent voidzone tracking (`OID.SparkPuddle` eventobj + `PersistentVoidzoneAtCastTarget` on `AID.BurningPillar`).
- Accelerated low-risk batch (7.5.0.326):
  - `Ad011PariofPlenty`: merged additional independent line telegraphs via grouped components:
    - `FireflightLines` for `Left/RightFireflight` + `FactAndFiction` variants
    - `DoubleFableFlightLines` for `Left/RightFableflight1`
  - Added required AID mappings and state activations only.
- Ad011 completion pass (unreleased working batch):
  - Added `FellSpark` tether warning and `CurseOfCompanionshipSolitude` status-driven stack/spread hints.
  - Added compatibility aliases for `FireFlight` / `FireFlightFactOrFiction` / `DoubleFableFlight` mapped to local grouped telegraph components.
  - Build check passed on local fork API; release intentionally deferred per workflow change (publish only after full instance completion or explicit instruction).
- M07 follow-up pass (unreleased working batch):
  - Added compatibility class-name coverage for `BrutishSwingCone` and `BrutishSwingDonutSegment` using local grouped AOE components.
  - Build passed; release deferred by workflow policy.
- Ad011 behavior-alignment pass (unreleased working batch):
  - Tuned `CharmedChains` to parity-like parameters (`chainLength: 22`, `activationDelay: 5`).
  - Switched `FireOfVictory` to `BaitAwayCast` style targeting (4y, center-at-target, ends-on-cast-event) consistent with validated variant implementation.
  - Build check passed; release intentionally deferred.
- Ad011 minor parity pass (unreleased working batch):
  - Added `SID.DarkResistanceDown` enum mapping to support further FellSpark logic alignment.
  - Build check passed; no release performed.
- Ad011 FellSpark logic pass (unreleased working batch):
  - Replaced basic `TankbusterTether` with status/timing-aware `BossComponent` logic:
    - tracks `DarkResistanceDown` remaining duration by slot
    - tracks tether target slot via `TetherID.FellSpark`
    - provides pass/take tether hints based on upcoming resolve timing
    - draws boss->target guidance line with safe/danger color based on debuff window
  - Build check passed; release deferred.
- M07 compatibility-name closure (unreleased working batch):
  - Added low-risk compatibility aliases for Reborn split naming:
    - `ArenaChanges` (no-op compatibility shim)
    - `BrutalImpactRevengeOfTheVines1NeoBombarianSpecial` -> local `BrutalImpact`
    - `BrutishSwingCircle2` -> local `BrutishSwingCircle`
  - Kept behavior unchanged and verified build success.
- Ad011 state-name closure (unreleased working batch):
  - Added compatibility activations to match Reborn state names:
    - `FireFlight`
    - `FireFlightFactOrFiction`
    - `DoubleFableFlight`
  - No behavior expansion; these map to existing local grouped implementations.
- Ad011 completion milestone (release batch):
  - `Ad011PariofPlenty` now considered migration-complete on this fork's supported API surface.
  - Included: line telegraphs, long-nights wheel sequence, stacks/spreads, raidwides, puddle tracking, chain timing, FellSpark pass/take tether logic, and compatibility-name activations.
  - Excluded by design: direct Reborn GenericAOEs/advanced helper internals that require framework/API extensions not present in this fork.
- M07 completion milestone (release batch):
  - `M07NBruteAbombinator` now considered migration-complete on this fork's supported API surface.
  - Included: core telegraphs, raidwides, interrupts, adds visibility, tankbuster/flare/towers, LOS handling, targeting hints, and compatibility-name shims.
  - Added broad `OID/AID` data parity constants from Reborn enums (including visual/driver actions) without changing local behavior model.
  - Excluded by design: direct Reborn arena-shared-bounds framework hooks not present in this fork.
- T08 completion pass (7.5.0.329):
  - Added `T08EnuoCompatibilityAliases.cs` to map Reborn/Ex8 naming onto the local T08 implementation without framework changes.
  - Implemented aliases with top-level fully-qualified `using` directives to satisfy this fork's IDE0065 style-as-error rule.
  - Release build verified with 0 errors.
- RM12S1 equivalence-sync pass (7.5.0.330):
  - Added `RM12S1CompatibilityAliases.cs` to map Reborn `M12SP1Lindwurm` naming to local `RM12S1TheLindwurm` components.
  - Kept scope to naming compatibility only (no framework/state-machine rewrites, no parallel module import).
  - Release build verified with 0 errors.
- M08N completion bootstrap pass (7.5.0.331):
  - Local `M08NHowlingBlade` directory was present but empty; replaced with a compile-safe minimal module skeleton.
  - Added foundational radar/visibility mechanics on fork-stable APIs: `ExtraplanarPursuit`, `RavenousSaber`, `GreatDivide`, `TargetedQuake`, `TrackingTremors`.
  - Deferred advanced behaviors (`ArenaChanges`, WolvesReign/Towerfall dynamic sequencing) to future parity passes due current API-shape drift.
  - Release build verified with 0 errors.
- M09 completion bootstrap pass (7.5.0.332):
  - Added compile-safe `M09NVampFatale` module skeleton with core visibility mechanics (raidwides, cone/rect/circle AOEs, spread/stack, towers).
  - Intentionally deferred higher-risk dynamic voidzone/arena behavior details for follow-up parity work on this fork API.
  - Release build target: 0-error verification before publish.
- UI crash hotfix pass (7.5.0.333):
  - Fixed a crash path in supported-boss list rendering (`ModuleViewer` -> `UIMisc.IconButton` -> `ImRaii.PushFont`) by guarding icon-font push and adding safe fallback rendering when icon font handle is invalid.
  - Scope is UI-only stability fix; no combat-mechanic behavior changes.
  - Release build verified with 0 errors.
- M08 behavior visibility pass (7.5.0.334):
  - Expanded `M08NHowlingBlade` from minimal skeleton to practical mechanic visibility set on current fork APIs.
  - Added/activated: `Heavensearth1/2`, `Gust`, `WolvesReignRect1/2`, `WolvesReignCircle`, `WolvesReignCone`, `RoaringWindShadowchase`, `GrowlingWindWealofStone`, `FangedCharge`, `TerrestrialTitans`, `Towerfall`.
  - Kept implementation on compile-safe local signatures; no framework-level refactors.
  - Release build verified with 0 errors.
- M10 completion bootstrap pass (7.5.0.335):
  - Added compile-safe `M10NDaringDevils` module from previously empty local directory.
  - Included core mechanic visibility set on fork-stable APIs: tankbusters (`HotImpact`, `DeepImpact`), raidwides (`DiversDare`, `XtremeSpectacular`), key AOEs (`CutbackBlaze`, `AlleyOopInferno`, `DeepVarial`, `SickestTakeOff`, `SteamBurst`, `InsaneAir` snaps), stack/spread (`PyrotationStack`), and knockback (`SickSwellKB`).
  - Deferred high-risk persistence/stateful helpers to later parity passes.
- M11 completion pass (7.5.0.336):
  - Filled previously empty local `M11NTheTyrant` directory with a compile-safe, high-coverage module implementation.
  - Added broad mechanic visibility set (Crown/Smashdowns, Void Stardust + Cometite, Assault Evolved set, Raw Steel TB + spreads, Charybdistopia, LOS mechanics, knockback/towers, avalanche/wall/fury meteor patterns).
  - Kept implementation on current fork API signatures; no framework changes.
- M12 completion pass (7.5.0.337):
  - Filled previously empty local `M12NLindwurm` with compile-safe high-coverage mechanic visibility.
  - Added core casts/aoes/raidwides/towers for: TheFixer, Serpentine/Ravenous patterns, Burst/Visceral, Splattershed, BringDown/Split/Venomous, GrandEntrance, MindlessFlesh, Constrictor/CruelCoil, Grotesquerie set, Maelstrom adds.
  - Implemented using fork-stable component signatures only.
- M01 completion pass (7.5.0.338):
  - Filled empty local `M01NBIackCat` module with compile-safe core mechanic visibility.
  - Added mechanics: BloodyScratch, BiscuitMaker, Clawful, GrimalkinGale, Overshadow line-stack, Shockwave knockback, OneTwoPaw, BlackCatCrossing, Mouser telegraph/hit, PredaceousPounce circles/charges.
  - Implemented on current fork signatures with no framework changes.
- M02 completion pass (7.5.0.339):
  - Filled empty local `M02NHoneyBLovely` module with compile-safe core visibility coverage.
  - Added key mechanics: CallMeHoney, TemptingTwist, HoneyBeeline, HoneyedBreeze TB bait, HoneyBLive raidwide, Heartsore/Heartsick, Loveseeker, BlowKiss, HoneyBFinale, Venom stack/spread, BlindingLove, HeartStruck, Fracture towers, Splinter.
  - Kept implementation on local stable component signatures.

- 2026-05-11: M03NBruteBomber completed (core mechanics port: towers+knockback, rotating cones, lariat combo, lit fuse AOEs); bumped to 7.5.0.340.

- 2026-05-11: M04NWickedThunder completed (arena changes, sidewise spark/burst, wicked cannon series, hypercannon lane, witch hunt); bumped to 7.5.0.341.

- 2026-05-11: M05NDancingGreen completed (core mechanics visibility with compatibility downgrade); bumped to 7.5.0.342.
