# BossMod Migration Exclusions (Current)

Last updated: 2026-05-15

This file defines known naming-drift buckets that should not be treated as actionable missing-module work.

## Excluded Buckets

1. `BossMod/Modules/Dawntrail/Savage`
- Reason: known `MxxS*` vs `RMxxS*` naming drift family.
- Policy: keep local canonical implementation paths; do not reopen as missing unless confirmed gameplay behavior gaps exist.

2. `BossMod/Modules/Dawntrail/Alliance`
- Reason: known folder-name drift (`A22/A23/A24` variants with punctuation/name differences) and numbering-shift families (`A30+`).
- Policy: use local canonical folders as source of truth; do not port by folder name/prefix only.
- Mapping detail: see `DAWNTRAIL_ALLIANCE_FOLDER_MAP.md`.

## Actionable-Missing Rule

When global Reborn-vs-local path diff contains only the two buckets above, treat actionable missing count as zero for migration planning.
