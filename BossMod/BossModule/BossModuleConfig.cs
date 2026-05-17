namespace BossMod;

[ConfigDisplay(Name = "Boss 模块与雷达", Order = 1)]
public class BossModuleConfig : ConfigNode
{
    public enum RadarCloseBehavior
    {
        [PropertyDisplay("打开设置窗口")]
        Prompt,
        [PropertyDisplay("隐藏雷达")]
        DisableRadar,
        [PropertyDisplay("禁用当前模块（并隐藏雷达）")]
        DisableActiveModule,
        [PropertyDisplay("禁用当前模块及同分类全部模块")]
        DisableActiveModuleCategory
    }

    // boss module settings
    [PropertyDisplay("模块自动加载最低成熟度", tooltip: "部分模块会标记为“WIP”，默认不会自动加载，除非你调整这里")]
    public BossModuleInfo.Maturity MinMaturity = BossModuleInfo.Maturity.Contributed;

    [PropertyDisplay("允许模块自动使用技能")]
    public bool AllowAutomaticActions = true;

    [PropertyDisplay("允许模块自动与场景对象交互", since: "0.3.5.6")]
    public bool AllowAutomaticInteract = true;

    [PropertyDisplay("显示测试用雷达与提示窗口", tooltip: "可在非 Boss 战斗中调试雷达和提示窗口布局", separator: true)]
    public bool ShowDemo = false;

    [PropertyDisplay("Allow WIP modules", since: "7.5.0.10", tooltip: "WIP modules are unfinished and may have severe bugs. Enable at your own risk.")]
    public bool AllowIncompleteModules = false;

    [PropertyDisplay("Enable Striking Dummy module during Explorer Mode dungeons", since: "7.5.0.10", separator: true)]
    public bool EnableDummyModule = false;

    // radar window settings
    [PropertyDisplay("启用雷达")]
    public bool Enable = true;

    [PropertyDisplay("关闭按钮行为")]
    public RadarCloseBehavior CloseBehavior = RadarCloseBehavior.Prompt;

    [PropertyDisplay("锁定雷达与提示窗口位置及鼠标交互")]
    public bool Lock = false;

    [PropertyDisplay("雷达窗口背景透明", tooltip: "移除雷达周围黑色底框；跨显示器时可能无效")]
    public bool TrishaMode = false;

    [PropertyDisplay("雷达内场地添加不透明底色")]
    public bool OpaqueArenaBackground = false;

    [PropertyDisplay("在雷达标记上显示描边与阴影")]
    public bool ShowOutlinesAndShadows = false;

    [PropertyDisplay("雷达场地缩放系数", tooltip: "雷达窗口内场地大小缩放")]
    [PropertySlider(0.1f, 10, Speed = 0.1f, Logarithmic = true)]
    public float ArenaScale = 1;

    [PropertyDisplay("雷达元素线条粗细缩放", tooltip: "全局缩放雷达元素描边粗细")]
    [PropertySlider(0.1f, 10, Speed = 0.1f, Logarithmic = true)]
    public float ThicknessScale = 1;

    [PropertyDisplay("雷达随镜头方向旋转")]
    public bool RotateArena = true;

    [PropertyDisplay("旋转时为雷达预留额外边距", tooltip: "启用随镜头旋转后，为避免边缘裁切而增加显示边距")]
    public bool AddSlackForRotations = true;

    [PropertyDisplay("在雷达中显示场地边界")]
    public bool ShowBorder = true;

    [PropertyDisplay("高风险位置时改变场地边界颜色", tooltip: "当你站在可能吃到机制的位置时，将白色边框改为红色")]
    public bool ShowBorderRisk = true;

    [PropertyDisplay("在雷达上显示东南西北方向文字")]
    public bool ShowCardinals = false;

    [PropertyDisplay("方向文字字号")]
    [PropertySlider(0.1f, 100, Speed = 1)]
    public float CardinalsFontSize = 17;

    [PropertyDisplay("在雷达上显示场地标点")]
    public bool ShowWaymarks = false;

    [PropertyDisplay("在雷达上显示标记（攻击/禁锢/无视及形状标）", since: "0.4.10.0")]
    public bool ShowSigns = false;

    [PropertyDisplay("始终显示所有存活队友")]
    public bool ShowIrrelevantPlayers = false;

    [PropertyDisplay("允许在雷达上绘制非队伍玩家", tooltip: "仅影响部分内容类型（如特殊探索野外）", depends: nameof(ShowIrrelevantPlayers))]
    public bool ShowAllPlayers = true;

    [PropertyDisplay("未着色玩家按职业角色上色显示")]
    public bool ColorPlayersBasedOnRole = false;

    [PropertyDisplay("始终显示焦点目标队友", separator: true)]
    public bool ShowFocusTargetPlayer = false;

    // hint window settings
    [PropertyDisplay("在独立窗口显示文字提示", tooltip: "将文字提示与雷达窗口分离，便于单独摆放")]
    public bool HintsInSeparateWindow = false;

    [PropertyDisplay("显示机制顺序与计时提示")]
    public bool ShowMechanicTimers = true;

    [PropertyDisplay("显示全团伤害提示")]
    public bool ShowGlobalHints = true;

    [PropertyDisplay("显示个人提示与警告", separator: true)]
    public bool ShowPlayerHints = true;

    // misc. settings
    [PropertyDisplay("在场景中显示移动引导", tooltip: "使用频率不高，但可在游戏场景中用箭头提示部分机制走位")]
    public bool ShowWorldArrows = false;

    public List<string> DisabledModules = [];
    public List<BossModuleInfo.Category> DisabledCategories = [];
}
