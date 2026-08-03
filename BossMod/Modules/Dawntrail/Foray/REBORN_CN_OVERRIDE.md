# RebornCN 机制覆盖台账

## 同步与发布基线

- 官方 Reborn 基线版本：`7.5.5.21`
- 官方 Reborn 提交：`9d8ff1327e214c9155559b763fcebf2dba33b5ca`
- RebornCN 来源版本：`7.5.5.17`
- RebornCN 来源提交：`70d4bc11e2c49861a06d333e615e6f1d8e508fb8`
- 本仓导入提交：`92bbf7368b43dc050a243ccff278cd24e5052145`
- 首次发布版本：`7.5.5.1701`
- 本地 CE 回退发布版本：`7.5.5.1702`
- `7.5.5.21` 同步后的下一个发布版本：`7.5.5.2101`

版本号继续以官方 Reborn 标签为基线，不因 RebornCN 的本地完成度推进官方版本段。官方 Reborn 后续补全对应模块后，应回归官方同一任务实现；在此之前保留本台账记录的 RebornCN 覆盖。

## 本地备份

替换前的本地 CE 已完整备份到：

`D:\mod-source\backups\ffxiv_bossmod-ce-before-bossmodreborncn-7.5.5.17-20260802`

备份包含 28 个文件、`SHA256SUMS.tsv` 和来源提交记录。

## CE 覆盖

`7.5.5.21` 已采用以下 8 个达到 `Verified` 或 `Contributed` 的官方模块：

| 主 OID | 官方模块 | 成熟度 |
|---|---|---|
| `0x4BB8` | `CE207DoubleTrouble` | `Contributed` |
| `0x4BBE` | `CE213QuarriedAway` | `Verified` |
| `0x4BC1` | `CE206DarkArtistry` | `Verified` |
| `0x4BC5` | `CE203AheadoftheCompetition` | `Verified` |
| `0x4BD3` | `CE209ForbiddenFolios` | `Contributed` |
| `0x4BD9` | `CE208FamiliarTactics` | `Contributed` |
| `0x4C6D` | `CE214TinyTerror` | `Contributed` |
| `0x4C77` | `CE202AcceptNoImitators` | `Contributed` |

以下 7 个任务的官方实现仍为 `Dummy` 或 `WIP`，继续按主 OID 保留现有实现：

| 主 OID | 当前模块 | 来源 | 官方状态 |
|---|---|---|---|
| `0x4BCA` | `CE215ManyMouthsToFeed` | RebornCN | `Dummy` |
| `0x4BE1` | `CE211LostontheWind` | 本仓实测 | `WIP` |
| `0x4C46` | `CE212CursedResurgence` | RebornCN | `Dummy` |
| `0x4C4B` | `CE205GluttonousCursefiend` | RebornCN | `Dummy` |
| `0x4C4F` | `CE201ABeastUnleashed` | 本仓实测 | `Dummy` |
| `0x4D8F` | `CE203AppallingBehavior` | RebornCN | `Dummy` |
| `0x4DFA` | `CE213WebOfTerror` | RebornCN | `Dummy` |

编号和名称不能用于跨仓身份判断，必须以 `GroupID=1093` 和主 `OID` 为准。继续保留剩余 RebornCN 模块使用的公共辅助 `CriticalEngagement/ReplayValidatedCastAOEs.cs`。

首次导入的 RebornCN 源码中，77 处集合容量语法 `[with(n)]` 已按本仓编译器要求等价转换为 `new(n)`；该适配只改变集合初始化语法，不改变机制逻辑。

## FATE 覆盖

`7.5.5.21` 已对齐官方除 `NH101` 外的 19 个 FATE 文件。官方 `NH101` 仍为 `WIP`，继续使用 RebornCN 的 `Contributed` 实现；以下两个官方尚不存在的任务也继续保留 RebornCN：

- `NH110RelicIcewolf`，主 `OID 0x4D5E`
- `NH111RegnantChimera`，主 `OID 0x4C7D`

## 回归官方规则

1. 每次同步官方 Reborn 时，按 `GroupID`、主 `OID` 对照本表，不按 CE 编号或文件名覆盖。
2. 官方对应模块仍为 `Dummy`、`WIP` 或明确不可用时，继续保留本表中的 RebornCN 或本仓实测实现。
3. 官方对应模块达到可用成熟度并确认机制完整后，以官方实现替换本表中的临时实现。
4. 替换后检查 `ReplayValidatedCastAOEs` 是否仍被引用；无引用时再删除。
5. 回归官方后继续采用当时官方标签作为版本基线，本地发布只递增该基线下的两位差异序号。
