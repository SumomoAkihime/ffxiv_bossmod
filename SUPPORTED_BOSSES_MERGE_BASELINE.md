# Supported Bosses Merge Baseline

Generated from module path comparison between:
- current: ffxiv_bossmod
- reference: BossmodReborn

## Summary
- ffxiv_bossmod files: 1901
- BossmodReborn files: 2711
- Common: 1222
- Only in ffxiv_bossmod: 679
- Only in BossmodReborn: 1489

## Merge Priority Rule
1. Preserve existing ffxiv_bossmod modules by default.
2. When adding from BossmodReborn, prioritize modules with clear radar/mechanic visualization value.
3. Do not import autorotation feature behavior changes from BossmodReborn.

## Top Only-In-Reborn Groups (for phased merge)
- Shadowbringers/Foray: 211
- Dawntrail/Savage: 167
- Dawntrail/Raid: 82
- Shadowbringers/Alliance: 81
- Endwalker/Variant: 79
- Dawntrail/Foray: 63
- Dawntrail/Trial: 52
- Heavensward/Alliance: 48
- Dawntrail/Extreme: 47
- Dawntrail/Alliance: 45
- Stormblood/Alliance: 43
- Stormblood/Foray: 40
- Heavensward/Dungeon: 39
- Stormblood/Trial: 36
- RealmReborn/Alliance: 35
- Stormblood/Extreme: 31
- Shadowbringers/Quest: 29
- Dawntrail/Dungeon: 27
- Stormblood/Quest: 24
- Endwalker/Quest: 23
- Dawntrail/TreasureHunt: 23
- Dawntrail/Quest: 21
- RealmReborn/Dungeon: 21
- Heavensward/DeepDungeon: 21
- Dawntrail/Variant: 20

## Top Only-In-Bossmod Groups (already preserved)
- Dawntrail/Savage: 151
- Shadowbringers/Foray: 94
- Dawntrail/Foray: 48
- Dawntrail/Extreme: 39
- Shadowbringers/Alliance: 39
- Shadowbringers/Quest: 30
- Stormblood/Quest: 24
- Endwalker/Quest: 23
- Dawntrail/Alliance: 22
- Heavensward/DeepDungeon: 20
- Stormblood/Foray: 16
- Dawntrail/Quantum: 15
- Dawntrail/DeepDungeon: 12
- Endwalker/Criterion: 11
- Dawntrail/Variant: 11
- Dawntrail/Dungeon: 9
- Endwalker/DeepDungeon: 9
- Endwalker/Unreal: 9
- Heavensward/Dungeon: 8
- Dawntrail/Unreal: 8
- Dawntrail/Quest: 7
- Stormblood/DeepDungeon: 7
- Heavensward/Quest: 7
- RealmReborn/Dungeon: 6
- RealmReborn/Novice: 5

## Compatibility Probe (2026-05-03, Phase 3)
- Result: direct module import remains blocked by framework API drift.
- Verified blockers:
  - `GenericAOEs` signature mismatch (`ReadOnlySpan`-style modules vs current `IEnumerable` base).
  - Reborn-only helper/component usage and geometry types not present in current fork.
  - state/helper API differences in module utility calls.
- Stable action taken:
  - added lightweight compatibility adapters in `BossMod/Components/CompatReborn.cs`.
  - added `BossModuleInfo.Maturity.AISupport` compatibility alias.
- Scope safety:
  - attempted Dawntrail Alliance `A10Trash` / `A20Trash` import was rolled back after compile validation.
  - repository remains buildable after rollback.

## Compatibility Probe (2026-05-03, Phase 4)
- Pilot succeeded: added `Dawntrail/Extreme/Ex2ZoraalJa/DawnOfAnAgeArenaChange.cs` in current-framework style.
- Implementation notes:
  - replaced Reborn-only `ReadOnlySpan` pattern with current `IEnumerable<AOEInstance>`.
  - replaced Reborn-only square helper usage with `AOEShapeCustom` + `RelPolygonWithHoles`.
- Validation: Release build passes after adding this module.

## Compatibility Probe (2026-05-03, Phase 5)
- Pilot succeeded: added `Dawntrail/Extreme/Ex6GuardianArkveld/Ex6GuardianArkveldConfig.cs`.
- Type: low-risk config-only merge from Reborn.
- Validation: Release build passes after import.

## Compatibility Probe (2026-05-03, Phase 6)
- Pilot succeeded: added `Dawntrail/Extreme/Ex3QueenEternal/Ex3QueenEternalConfig.cs`.
- Type: low-risk config-only merge from Reborn.
- Validation: Release build passes after import.

## Compatibility Probe (2026-05-03, Phase 7)
- Pilot succeeded: added `Dawntrail/Chaotic/Ch01CloudOfDarkness/Ch01CloudOfDarknessConfig.cs`.
- Type: low-risk config-only merge from Reborn.
- Validation: Release build passes after import.
