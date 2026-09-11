# 辅助职业离线回归

读取本仓 Release DLL、现有 Dalamud DLL 与本地游戏数据；不启动游戏、不发送技能，不添加外部依赖。

```powershell
$env:DALAMUD_HOME='D:\mod-source\_dalamud_tmp'
dotnet build BossMod/BossMod.csproj -c Release
dotnet run --project Tests/SupportJobs -c Release -- --sqpack 'D:\最终幻想XIV\game\sqpack'
```

覆盖动作表注册、新增输出开关、未装备/友方/倒计时限制、盗窃默认关闭、烟幕续用、火冰雷弱点优先级、侦测、读条与即刻状态、普通职业连段保护、输出职业自疗条件、复活模式及目标过滤、魔跳跃与前踏步危险区保护、旧预设按策略名称恢复。

这些检查验证动作选择与保护条件，不替代游戏内技能效果、实际出手和移动组合验收。测试时间显式设为有效日期，避免默认 DateTime.MinValue 影响弱点过期判断。
