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

## Applied Merge (2026-05-03, Phase 2)
- Scope: Dawntrail modules only.
- Method: copied files that exist in `BossmodReborn/BossMod/Modules/Dawntrail` but are missing in `ffxiv_bossmod/BossMod/Modules/Dawntrail`.
- Files added: 608.
- Overwrites: none (add-only merge to minimize regression risk).
- Change list file: `MERGE_MISSING_FROM_REBORN_DAWNTRAIL.txt`.
- Note: This phase intentionally avoids `Autorotation` and other non-Modules feature changes.

## Future Sync SOP (Supported Bosses)
1. Fetch upstream `ffxiv_bossmod` and merge first.
2. Recompute module diff against `BossmodReborn` for target expansion scope.
3. Add-only copy for missing `Modules` files first; avoid overwriting unless there is a confirmed bugfix.
4. Build and smoke-test plugin load in game.
5. Record:
   - source commit refs
   - added/updated module files
   - skipped items and reasons
6. Bump CN version by +2 on the last two digits (example: `7.5.0.203` -> `7.5.0.205`).

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
