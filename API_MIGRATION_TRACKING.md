# BossMod Reborn API Migration Tracking

## Current Baseline

- Last Reborn sync checked: `49e17ab75d0959d1286586812f057a5cfbad4dd3`
- Reborn component types after recount: `118`
- Local component types after batch 4: `114`
- Missing component API types after batch 4: `15`
- Batch sizing rule: missing `10-24` => migrate `2-4` APIs per batch
- Recount note: parser now includes indented public nested component types, which is more reliable than the earlier top-level-only count.

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

## Batch 3 - Reborn compatibility adapters

Implemented:

- `SimpleExaflare`
- `StretchTetherDuo`
- `StretchTetherSingle`
- `StatusStackSpread`

Notes:

- `SimpleExaflare` maps Reborn's first/rest cast pattern onto this fork's existing `Exaflare.Line` model and supports cast-finish or event-cast progression.
- `StretchTetherDuo` and `StretchTetherSingle` provide tether visibility, stretch-distance hints, and source/target bait tracking. Reborn's deeper immunity-specific behavior is accepted at the API surface but downgraded to this fork's existing hint/visibility behavior.
- `StatusStackSpread` maps stack/spread statuses onto this fork's `UniformStackSpread` lists using status expiration for activation timing.

Verification:

- `dotnet build -c Release BossMod\BossMod.csproj`: passed with `0` warnings and `0` errors.

## Batch 4 - Reborn compatibility adapters

Implemented:

- `CastHints`
- `Dispel`
- `ThinIce`
- `InterceptTether`

Notes:

- `CastHints` adds a multi-AID cast-hint aggregator on top of this fork's existing `CastHint`.
- `Dispel` is mapped to the local `DispelHint` implementation (target-name hint + AI dispel flagging) to keep behavior aligned with existing local systems.
- `ThinIce` is implemented as a status-driven directional knockback compatibility base using this fork's `GenericKnockback`.
- `InterceptTether` provides tether tracking, line visibility, and "grab tether" hinting; excluded-allies filtering is retained as a compatibility parameter.

Verification:

- `dotnet build -c Release BossMod\BossMod.csproj`: passed with `0` warnings and `0` errors.

## Remaining Missing APIs

- `ActionDrivenForcedMarch`
- `BaitAwayChargeTether`
- `CastGazes`
- `CastLineOfSightAOEComplex`
- `CastTowersOpenWorld`
- `DonutStack`
- `GenericBaitProximity`
- `InterceptTetherAOE`
- `InterceptTetherStatus`
- `InverseWildCharge`
- `SimpleKnockbackGroups`
- `SingleTargetCasts`
- `SingleTargetDelayableCasts`
- `SingleTargetEventDelay`
- `VoidzoneAtCastTargetGroup`

## Next Batch Candidates

Current missing count is `15`, so the next batch should target `2-4` APIs.

Recommended next batch:

- `CastGazes`
- `BaitAwayChargeTether`
- `InterceptTetherAOE`
- `InterceptTetherStatus`
