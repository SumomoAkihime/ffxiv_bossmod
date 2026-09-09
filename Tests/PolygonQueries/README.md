# 多边形查询离线回归

针对几何索引尚未初始化导致的空引用。无需启动游戏、读取游戏数据或安装测试框架；使用本仓构建所需的 .NET SDK、Dalamud 目录内现有的 Lumina DLL 和已生成的 Release DLL。沿用插件项目的 `DALAMUD_HOME`，未设置时使用默认 Dalamud 开发目录。

在仓库根目录执行：

```powershell
dotnet build BossMod/BossMod.csproj -c Release
dotnet run --project Tests/PolygonQueries -c Release
dotnet run --project Tests/PolygonQueries -c Release --no-build -- --distance-first
```

需要对照历史 DLL 时，首个测试命令可附加 `-p:BossModAssembly="历史 DLL 的绝对路径"`。正式验证当前源码时重新运行默认命令，避免误测旧产物。任何用例失败都会返回非零退出码。

覆盖 11 个多边形查询入口、正反距离优先、空形状、带孔形状、变换、扩缩、布尔运算、分离区域、孔中孤岛、并发首次查询、显式重建及 Prishe 两种对角范围的缓存复用。首次查询与预热查询比较之外，还用独立解析几何验证点位和已知距离。

上游几何核心合并后重复运行这两种顺序；不能只跑提前设置场地或绘制尺寸的测试，否则会漏掉首次查询路径。这套检查覆盖初始化问题，不验证副本时间轴、实际技能范围或 DX11 画面。
