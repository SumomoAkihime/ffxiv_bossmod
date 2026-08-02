# RebornCN 机制覆盖台账

## 发布基线

- 官方 Reborn 基线版本：`7.5.5.17`
- 官方 Reborn 提交：`9ddb06075d6a1b82ef9e4bb23972d60e1824d420`
- RebornCN 来源版本：`7.5.5.17`
- RebornCN 来源提交：`70d4bc11e2c49861a06d333e615e6f1d8e508fb8`
- 本仓导入提交：`92bbf7368b43dc050a243ccff278cd24e5052145`
- 首次发布版本：`7.5.5.1701`
- 本地 CE 回退发布版本：`7.5.5.1702`

版本号继续以官方 Reborn 标签为基线，不因 RebornCN 的本地完成度推进官方版本段。官方 Reborn 后续补全对应模块后，应回归官方同一任务实现；在此之前保留本台账记录的 RebornCN 覆盖。

## 本地备份

替换前的本地 CE 已完整备份到：

`D:\mod-source\backups\ffxiv_bossmod-ce-before-bossmodreborncn-7.5.5.17-20260802`

备份包含 28 个文件、`SHA256SUMS.tsv` 和来源提交记录。

## CE 覆盖

以下 10 个任务继续使用 RebornCN，导入时均标记为 `Contributed`。编号和名称不能用于跨仓身份判断，必须以 `GroupID=1093` 和主 `OID` 为准。

| 主 OID | RebornCN 模块 |
|---|---|
| `0x4BB8` | `CE211DoubledTrouble` |
| `0x4BC1` | `CE209WhatGoesAround` |
| `0x4BCA` | `CE215ManyMouthsToFeed` |
| `0x4BD3` | `CE214ForbiddenFolios` |
| `0x4BD9` | `CE201FamiliarTactics` |
| `0x4C46` | `CE212CursedResurgence` |
| `0x4C4B` | `CE205GluttonousCursefiend` |
| `0x4C77` | `CE210AcceptNoImitators` |
| `0x4D8F` | `CE203AppallingBehavior` |
| `0x4DFA` | `CE213WebOfTerror` |

以下 5 个任务在 `7.5.5.1702` 恢复本仓实测实现，文件取自替换前提交 `628cf5fe6eb8778972a6f38530aed5bd3296ad95`：

| 主 OID | 本仓模块 |
|---|---|
| `0x4BBE` | `CE213QuarriedAway` |
| `0x4BC5` | `CE203AheadoftheCompetition` |
| `0x4BE1` | `CE211LostontheWind` |
| `0x4C4F` | `CE201ABeastUnleashed` |
| `0x4C6D` | `CE214TinyTerror` |

继续保留 RebornCN 公共辅助 `CriticalEngagement/ReplayValidatedCastAOEs.cs`。`CE206RebelliousFamiliar` 被本仓同 OID 实现替换后，已删除不再使用的 `SDKnockbackInAABBSquareTowardsOrigin`。

首次导入的 RebornCN 源码中，77 处集合容量语法 `[with(n)]` 已按本仓编译器要求等价转换为 `new(n)`；该适配只改变集合初始化语法，不改变机制逻辑。

## FATE 覆盖

已对齐 RebornCN 的整个 `Dawntrail/Foray/FATE` 目录。北岛 `NH101` 至 `NH111` 均为 `Contributed`，其中本仓新增：

- `NH110RelicIcewolf`，主 `OID 0x4D5E`
- `NH111RegnantChimera`，主 `OID 0x4C7D`

## 回归官方规则

1. 每次同步官方 Reborn 时，按 `GroupID`、主 `OID` 对照本表，不按 CE 编号或文件名覆盖。
2. 官方对应模块仍为 `Dummy`、`WIP` 或明确不可用时，继续保留本表中的 RebornCN 或本仓实测实现。
3. 官方对应模块达到可用成熟度并确认机制完整后，以官方实现替换本表中的临时实现。
4. 替换后检查 `ReplayValidatedCastAOEs` 是否仍被引用；无引用时再删除。
5. 回归官方后继续采用当时官方标签作为版本基线，本地发布只递增该基线下的两位差异序号。
