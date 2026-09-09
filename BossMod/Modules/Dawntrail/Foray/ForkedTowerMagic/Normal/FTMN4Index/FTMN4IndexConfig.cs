namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Normal.FTMN4Index;

[ConfigDisplay(Order = 0x174, Parent = typeof(DawntrailConfig))]
public sealed class FTMN4IndexConfig : ConfigNode
{
    [PropertyDisplay("炸弹出现时强制 AI 选中最近的炸弹")]
    public bool ForceAddTargeting = false;

    [PropertyDisplay("没有炸弹且没有当前目标时强制 AI 选中首领")]
    public bool ForceBossTargeting = false;
}
