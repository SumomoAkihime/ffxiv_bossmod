namespace BossMod.Shadowbringers.Alliance.A12Hobbes;

class FireResistanceTest(BossModule module) : Components.GenericAOEs(module)
{
    private int _platform; // 0-2
    enum Pattern
    {
        None,
        Inside,
        Outside,
        CW,
        CCW
    }
    private Pattern _pattern;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        WPos center = _platform switch
        {
            0 => new(-831, -225),
            1 => new(-805, -270),
            2 => new(-779, -225),
            _ => default
        };
        List<AOEInstance> aoes = [];
        switch (_pattern)
        {
            case Pattern.CW:
                aoes.Add(new(new AOEShapeRect(22, 21, 2), center, Angle.FromDirection(Arena.Center - center) + 85.Degrees()));
                break;
            case Pattern.CCW:
                aoes.Add(new(new AOEShapeRect(22, 21, 2), center, Angle.FromDirection(Arena.Center - center) - 85.Degrees()));
                break;
            case Pattern.Inside:
                var dir = (Arena.Center - center).Normalized() * 15;
                aoes.Add(new(new AOEShapeCone(70, 15.Degrees()), Arena.Center + dir, Angle.FromDirection(center - Arena.Center)));
                break;
            case Pattern.Outside:
                var platformEdge = (Arena.Center - center).Normalized() * 20 + center;
                var angle = Angle.FromDirection(center - Arena.Center);
                aoes.Add(new(new AOEShapeCone(70, 30.Degrees()), platformEdge, angle + 55.Degrees()));
                aoes.Add(new(new AOEShapeCone(70, 30.Degrees()), platformEdge, angle - 55.Degrees()));
                break;
        }
        return aoes.ToArray();
    }

    public override void OnMapEffect(byte index, uint state)
    {
        var plat = index switch
        {
            7 => 0,
            9 => 1,
            10 => 2,
            _ => -1
        };
        if (plat >= 0)
        {
            _platform = plat;
            switch (state)
            {
                case 0x00100010:
                    _pattern = Pattern.Inside;
                    break;
                case 0x00800080:
                    _pattern = Pattern.Outside;
                    break;
                case 0x04000400:
                    _pattern = Pattern.CCW;
                    break;
                case 0x20002000:
                    _pattern = Pattern.CW;
                    break;
                case 0x00400004:
                case 0x02000004:
                case 0x10000004:
                case 0x80000004:
                    // mapeffect is generally close enough to the actual cast that we can just use it as an indicator for when the voidzone disappears
                    _pattern = Pattern.None;
                    _platform = 0;
                    break;
            }
        }
    }
}
