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
