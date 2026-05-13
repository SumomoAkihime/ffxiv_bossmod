# BossMod Reborn API Migration Tracking

## Current Baseline

- Last Reborn sync checked: `49e17ab75d0959d1286586812f057a5cfbad4dd3`
- Reborn component types: `120`
- Local component types after batch 1: `104`
- Missing component API types after batch 1: `29`
- Batch sizing rule: missing `25-39` => migrate `4-6` APIs per batch

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
- `GenericTowersOpenWorld`
- `ImmuneKind`
- `InterceptTether`
- `InterceptTetherAOE`
- `InterceptTetherStatus`
- `InverseWildCharge`
- `RaidwideCastsDelay`
- `SimpleAOEGroupsByTimewindow`
- `SimpleChargeAOEGroups`
- `SimpleExaflare`
- `SimpleKnockbackGroups`
- `SingleTargetCasts`
- `SingleTargetDelayableCasts`
- `SingleTargetEventDelay`
- `StatusStackSpread`
- `StretchTetherDuo`
- `StretchTetherSingle`
- `TemporaryMisdirection`
- `ThinIce`
- `VoidzoneAtCastTargetGroup`

## Next Batch Candidates

Current missing count is `29`, so the next batch should again target `4-6` APIs.

Recommended next batch:

- `SimpleAOEGroupsByTimewindow`
- `TemporaryMisdirection`
- `StretchTetherDuo`
- `StretchTetherSingle`
- `GenericTowersOpenWorld`
- `RaidwideCastsDelay`
