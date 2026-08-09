# Reborn 7.5.5.35 普通魔之塔与蜃景幻界对照记录

## 同步范围

- 上游基线：`FFXIV-CombatReborn/BossmodReborn` `7.5.5.29 -> 7.5.5.35`，共 17 个提交、16 个变动文件。
- 本轮只同步已完成的成熟度调整和有明确材质证据的场地参数，不整文件覆盖本地实测机制。

## 已同步

- `CE202AcceptNoImitators`、`CE207DoubleTrouble`、`CE210ImbalancedDiet` 提升为 `Verified`。
- `FATE/AllureOfTheOccult`、`FATE/ScaleModel` 提升为 `Verified`。
- 普通魔之塔 Boss3 场地半径由 25 调整为上游的 24；机制继续使用本地 `Contributed` 实现。
- 普通与高难魔之塔 Boss4 共用场地改为材质 `0x00007004` 的实际顶点：初始三平台、元素控制六平台、中央内六边形空洞；寻路边界使用 `Offset -1`。

## 本地已包含

- 普通魔之塔 Boss1 的连续冰焰月环外径已经是 60，不重复应用上游同项修正。
- `CE211LostontheWind` 本地已为 `Contributed`，不产生变更。

## 暂不合并

- `CE201ABeastUnleashed` 上游仍为 `WIP`，本地为 `Contributed`。
- 上游 `CE212ManyMouthstoFeed` 与本地 `CE215ManyMouthsToFeed` 同为 `GroupID 1093 / 主 OID 0x4BCA`；上游仍为 `WIP`，不新增重复模块。
- 普通魔之塔 Boss1 的 `Buffet` 强制目标、Boss3/Boss4 的新增机制仍随上游模块标记为 `WIP`；本轮不覆盖本地完整实现，后续只在确认具体行为缺口后逐项对照。

## 实测重点

- Boss3 半径 24 是否准确贴合即死带。
- Boss4 中央空洞、三/六平台外缘是否与实景一致，以及 `Offset -1` 下自动移动是否仍有贴边倾向。
