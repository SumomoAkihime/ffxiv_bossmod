# Reborn 7.5.5.29 普通魔之塔对照记录

## 处理结论

- 对照基线：`FFXIV-CombatReborn/BossmodReborn` `7.5.5.29`。
- 上游 `FTMN1TwoHeadedAevis`、`FTMN2SwordDancer` 均为 `WIP`。
- 本地两个模块为实测后的 `Contributed`，本轮不覆盖源码，只记录可复核候选。
- 后续取得对应回放并确认行为缺口后，再逐项移植，不能整目录替换。

## FTMN1 候选

- `Buffet`：分组和连线处理。
- `StormsBreathCast 47613`：击退预读条信号。
- `HissingReprise`：`SID 5403/5404`、图标和连线驱动的状态实现。
- `HypothermalCombustionShock`：低温燃烧冲击组合。
- `Archaeofury 47747/47748`：分散提示。

## FTMN2 候选

- `RushSurgesword`：冲锋与剑气组合。
- `Cyclosword`：旋剑组合。
- `SwordDance`：剑舞顺序处理。
- `Steelsbreath`：圆形场地内击退 AI 约束。
- 多组 `TurnInner/TurnOuter` 动作 ID 的统一处理。

## 明确保留的本地行为

- 场地中心 `(600,704)`；后续实机确认最外圈飞刀外径 `24` 刚好贴合场地边缘，现采用与上游一致的半径 `24`。
- 本地旋剑钢铁/月环半径、五连击退、独立钢铁提示、剑舞与安全区实现均不被覆盖。
- 高难魔之塔目录不属于本次同步范围。
