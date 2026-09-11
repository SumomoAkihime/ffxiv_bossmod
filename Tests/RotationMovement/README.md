# 循环、移动与外部联动离线回归

使用已生成的本仓 Release DLL、Dalamud 目录中的现有 DLL/cimgui 和游戏 sqpack；不启动游戏、不发送动作、不连接其他插件，也不新增 NuGet 依赖。测试创建独立 ImGui 上下文以支持预设修饰键读取；路径由环境变量及参数传入。

在源码仓根目录执行（DALAMUD_HOME 可使用现有开发目录）：

```powershell
$env:DALAMUD_HOME='D:\mod-source\_dalamud_tmp'
dotnet build BossMod/BossMod.csproj -c Release
dotnet run --project Tests/RotationMovement -c Release -- --sqpack 'D:\最终幻想XIV\game\sqpack'
```

覆盖职业输出与移动同时运行、独立开关、总停止、预设改名、重复移动模块的确定顺序、手动跟随约束、热病/禁止移动/击退、旧 AI 暂停与销毁后异步结果失效、NormalMovement 停用后的旧路径丢弃、副本入场、视线开关/绘图距离及绝龙诗默认语义、队伍排序兼容。

这些检查验证共享行为，不证明每个副本的范围、时间轴或实机画面正确。代码变更涉及几何时仍运行 `Tests/PolygonQueries`；正式验收还需实际同时启用职业循环与自动移动、切换预设以及外部插件启停场景。

可选追加 `--autoduty <预设文件所在目录>`，其中应包含官方 `AutoDuty.json` 和 `AutoDuty_Passive.json`。检查会通过真实 IPC 委托导入两份预设、核对所有模块与策略未丢失，并执行临时移动策略切换；所有预设写入独立临时测试目录。测试本身不联网下载文件。

IPC 检查使用本地 Dalamud 接口代理捕获真实注册委托，覆盖 Configuration 签名、模块禁用/恢复、追加/移除单个预设、多预设、临时覆盖及移动状态查询。代理验证插件端契约，不替代实际 AutoDuty/vnavmesh 跨插件实机验收。

# 击退与复合机制回归

`MechanicSafetyTests` 直接检查产品的落点约束及真实寻路：两段击退、抗击退、击退后核爆、按候选点选择击退源、穿越场地空洞、重复伤害事件及结束后释放约束。应引用本次源码构建的 BossMod.dll；旧发布 DLL 不含新增类型，不能用于验收。

2026-09-11 发布 7.5.5.5206 时已引用当前 Release DLL 执行；循环/移动、IPC 与新增机制安全用例共 109 项断言通过。离线回归不替代实机时序与未知随机组合验证。
