# BossMod CN Sync Checklist

## Version Rule
- Local version format: `a.b.c.(d*100+n)`.
- Example: upstream `7.5.0.2` -> local base `7.5.0.200`.
- This cycle uses `7.5.0.201`.
- Next releases must increment `n` by 1: `7.5.0.202`, `7.5.0.203`, ...

## This Round Changes
- UI text localization (config/about tabs) in:
  - `BossMod/Config/ConfigUI.cs`
  - `BossMod/Config/AboutTab.cs`
- Version bump:
  - `BossMod/BossMod.csproj`

## Supported Bosses Merge Policy
- Do not copy full BossmodReborn codebase.
- Keep current BossMod functionality as primary source.
- Use BossmodReborn only as reference for:
  - Chinese text
  - Supported-boss data comparison
- Priority: keep modules with radar/mechanic visualization support.
- For conflicting modules by same encounter:
  - keep current `ffxiv_bossmod` module unless Reborn entry clearly adds radar-critical data.

## Data Comparison Baseline (Modules folder)
- ffxiv_bossmod files: 1901
- BossmodReborn files: 2711
- Common: 1222
- Only in ffxiv_bossmod: 679
- Only in BossmodReborn: 1489

## Next Update Procedure
1. Merge upstream `awgil/ffxiv_bossmod` into local fork.
2. Rebuild and verify plugin load.
3. Re-run module diff (path-level and OID-level).
4. Apply CN text patches from tracked files only.
5. Re-apply selected supported-boss merge list (encounter-level), not wholesale file copy.
6. Bump local version using rule above.
7. Rebuild package and update custom repo.

## Files To Track Carefully
- `BossMod/BossModule/BossModuleRegistry.cs`
- `BossMod/Config/ModuleViewer.cs`
- `BossMod/Config/ConfigUI.cs`
- `BossMod/Config/AboutTab.cs`
- `BossMod/BossMod.csproj`

## Note
- BossmodReborn intentionally diverges in feature scope (e.g., autorotation adjustments). Keep this fork aligned with upstream BossMod behavior unless explicitly requested.
