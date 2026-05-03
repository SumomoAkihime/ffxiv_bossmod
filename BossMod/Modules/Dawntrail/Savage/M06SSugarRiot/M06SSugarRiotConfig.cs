namespace BossMod.Dawntrail.Savage.M06SSugarRiot;

[ConfigDisplay(Order = 0x110, Parent = typeof(DawntrailConfig))]
public sealed class M06SSugarRiotConfig() : ConfigNode
{
    [PropertyDisplay("Enable custom priority list", tooltip: "If disabled the add phase priority list below will not be used.")]
    public bool EnablePriorityList = true;

    [PropertyDisplay("Add phase priorities for autorotation (from highest to lowest)")]
    public int[] AddsPriorityOrder = [15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0];

    [PropertyDisplay("Time left before spotlights get drawn")]
    [PropertySlider(0.1f, 34, Speed = 1)]
    public float SpotlightTimer = 34;
}
