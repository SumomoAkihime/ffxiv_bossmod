# BossMod Upstream Sync Playbook

## Current Strategy
- First pass: low-risk compatibility layer to keep compile green and preserve installability.
- Second pass: behavior parity for high-risk mechanics (state machines, arena transformations, complex helper APIs).

## Versioning Rule
- Use four-part AssemblyVersion.
- Current local rule: increment by +1 on last part each release.

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
