# BossMod Reborn API Migration Tracking

## Current Baseline

- Last Reborn sync checked: `49e17ab75d0959d1286586812f057a5cfbad4dd3`
- Reborn component types: `120`
- Local component types after batch 2: `109`
- Missing component API types after batch 2: `24`
- Batch sizing rule: missing `10-24` => migrate `2-4` APIs per batch

## Batch 1 - Reborn compatibility adapters

Implemented:

- `Voidzone`
- `VoidzoneAtCastTarget`
- `GenericKnockback`
- `RaidwideCasts`
- `LineStack`
- `CleansableDebuff`

Supporting compatibility added:

- `GenericBaitStack`
- Reborn-style `GenericBaitAway.Bait` optional fields: `Forbidden`, `MaxCasts`

Notes:

- `GenericKnockback.DirBackward`, `SafeWall`, `ActorID`, and `IgnoreImmunes` are accepted for source compatibility, but wall/actor-specific behavior is downgraded to this fork's existing knockback behavior.
- `LineStack` is a visibility-first adapter. It tracks marker/cast/icon targets and clears baits on resolve, but does not yet enforce Reborn's full min/max stack semantics.
- `CleansableDebuff` queues Esuna/Warden's Paean where this fork's action queue supports it.

Verification:

- `dotnet build -c Release BossMod\BossMod.csproj`: passed with `0` warnings and `0` errors.

## Remaining Missing APIs

- `ActionDrivenForcedMarch`
- `BaitAwayChargeTether`
- `CastGazes`
- `CastHints`
- `CastLineOfSightAOEComplex`
- `CastTowersOpenWorld`
- `Dispel`
- `DonutStack`
- `GenericBaitProximity`
- `ImmuneKind`
- `InterceptTether`
- `InterceptTetherAOE`
- `InterceptTetherStatus`
- `InverseWildCharge`
- `SimpleExaflare`
- `SimpleKnockbackGroups`
- `SingleTargetCasts`
- `SingleTargetDelayableCasts`
- `SingleTargetEventDelay`
- `StatusStackSpread`
- `StretchTetherDuo`
- `StretchTetherSingle`
- `ThinIce`
- `VoidzoneAtCastTargetGroup`

## Batch 2 - Reborn compatibility adapters

Implemented:

- `SimpleAOEGroupsByTimewindow`
- `SimpleChargeAOEGroups`
- `RaidwideCastsDelay`
- `TemporaryMisdirection`
- `GenericTowersOpenWorld`

Notes:

- `SimpleAOEGroupsByTimewindow` limits displayed AOEs to the first activation window and preserves Reborn's delayed-risk option in a simplified form.
- `SimpleChargeAOEGroups` provides line AOE visibility for grouped charge casts, including optional extra front length.
- `GenericTowersOpenWorld` maps Reborn tower objects onto this fork's `GenericTowers`; open-world allowed-soaker logic is preserved where possible through forbidden masks.

Verification:

- `dotnet build -c Release BossMod\BossMod.csproj`: passed with `0` warnings and `0` errors.

## Next Batch Candidates

Current missing count is `24`, so the next batch should target `2-4` APIs.

Recommended next batch:

- `StretchTetherDuo`
- `StretchTetherSingle`
- `StatusStackSpread`
- `SimpleExaflare`
