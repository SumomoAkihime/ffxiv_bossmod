namespace BossMod;

[ConfigDisplay(Name = "回放", Order = 0)]
public class ReplayManagementConfig : ConfigNode
{
    [PropertyDisplay("显示回放管理界面")]
    public bool ShowUI = false;

    [PropertyDisplay("在副本/野外模块开始或结束时自动录制回放")]
    public bool AutoRecord = true;

    [PropertyDisplay("在剧情回放（Duty Recorder）中自动录制", tooltip: "需先开启自动录制")]
    public bool AutoARR = true;

    [PropertyDisplay("保留回放数量上限")]
    [PropertySlider(0, 1000)]
    public int MaxReplays = 20;

    [PropertyDisplay("在回放中记录并存储服务端封包")]
    public bool RecordServerPackets = false;

    [PropertyDisplay("将服务端封包输出到 dalamud.log")]
    public bool DumpServerPackets = false;

    [PropertyDisplay("输出到 dalamud.log 时忽略其他玩家封包")]
    public bool DumpServerPacketsPlayerOnly = false;

    [PropertyDisplay("将客户端封包输出到 dalamud.log")]
    public bool DumpClientPackets = false;

    [PropertyDisplay("录制日志格式")]
    public ReplayLogFormat WorldLogFormat = ReplayLogFormat.BinaryCompressed;
}
