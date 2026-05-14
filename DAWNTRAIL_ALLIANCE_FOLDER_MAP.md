# Dawntrail Alliance Folder Mapping (BossMod vs Reborn)

Last updated: 2026-05-14

Rule baseline: **BossMod local canonical folders are authoritative**. Reborn naming-only variants are treated as compatibility/debris unless explicitly needed.

| Reborn folder | BossMod canonical folder | Status |
|---|---|---|
| A10Trash | (none) | Reborn-only trash wave (not canonical in local) |
| A11Prishe | A11Prishe | Same |
| A12Fafnir | A12Fafnir | Same |
| A13ArkAngels | A13ArkAngels | Same |
| A14ShadowLord | A14ShadowLord | Same |
| A20Trash | (none) | Reborn-only trash wave (not canonical in local) |
| A21FaithboundKirin | A21FaithboundKirin | Same |
| A22UltimaOmega | A22OmegaTheOne | Naming drift |
| A23Kam'lanaut | A23Kamlanaut | Naming drift (apostrophe) |
| A24Eald'narche | A24Ealdnarche | Naming drift (apostrophe) |
| A30Shantoto | A31ShantottoTheDemon | Numbering/naming drift |
| A31AlZahbi | A32MedusaSwarmsinger | Numbering/naming drift |
| A32Alexander | A33AlexanderResurrected | Numbering/naming drift |
| A33Awaern | A34Awaern | Numbering drift |
| A34Promathia | A35Promathia | Numbering drift |
| A35ShinryuParadox | A36ShinryuParadox | Numbering drift |
| A36HollowKing | (none) | Reborn-only terminal entry (no local canonical folder) |

## Current duplicate risk points in local tree

- Both canonical + Reborn-style folders currently coexist for:
  - `A22OmegaTheOne` and `A22UltimaOmega`
  - `A23Kamlanaut` and `A23Kam'lanaut`
  - `A24Ealdnarche` and `A24Eald'narche`

## Operational policy for migration

1. Keep canonical gameplay implementation under local folders (`A22OmegaTheOne`, `A23Kamlanaut`, `A24Ealdnarche`, etc.).
2. If Reborn-name paths are needed for bookkeeping, keep them as placeholder-only compatibility files.
3. Do not bulk-delete duty directories in `Dawntrail/Alliance`; delete only explicit untracked placeholder files after `git ls-files --others --exclude-standard`.
