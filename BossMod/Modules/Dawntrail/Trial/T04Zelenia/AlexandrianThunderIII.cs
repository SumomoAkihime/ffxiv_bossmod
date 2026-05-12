namespace BossMod.Dawntrail.Trial.T04Zelenia;

class AlexandrianThunderIII(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCone Cone = new(16, 30.Degrees());
    private static readonly AOEShapeCircle Circle = new(4);
    private readonly List<AOEInstance> _aoes = [];
    private readonly bool[] _activeTiles = new bool[6];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var visited = new bool[6];
        const float slice = 1f / 60f;

        foreach (var aoe in _aoes)
        {
            if (aoe.Shape == Circle)
            {
                yield return aoe;
            }
            else
            {
                var sliceIdx = (int)MathF.Round((180 - aoe.Rotation.Deg) * slice);
                if (sliceIdx >= 0 && sliceIdx < visited.Length && !visited[sliceIdx])
                {
                    visited[sliceIdx] = true;
                    yield return aoe;
                }
            }
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index is >= 0x04 and <= 0x09)
        {
            switch (state)
            {
                case 0x00400100u:
                    _activeTiles[index - 0x04] = true;
                    break;
                case 0x00040020u:
                    _activeTiles[index - 0x04] = false;
                    break;
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var aid = (AID)spell.Action.ID;
        if (aid is AID.AlexandrianThunderIII1 or AID.AlexandrianThunderIII2)
        {
            Span<bool> visited = stackalloc bool[6];
            var pos = Arena.Center;
            var loc = spell.LocXZ;
            var id = caster.InstanceID;
            var act = Module.CastFinishAt(spell);

            _aoes.Add(new(Circle, loc, default, act));

            for (var i = 0; i < 6; ++i)
            {
                if (!_activeTiles[i] || visited[i])
                    continue;

                var intersects = Intersect.CircleCone(loc, 4, pos, 16, (180 - 60 * i).Degrees().ToDirection(), 30.Degrees());
                if (!intersects)
                    continue;

                var left = i;
                while (true)
                {
                    var prev = (left - 1 + 6) % 6;
                    if (!_activeTiles[prev] || visited[prev])
                        break;
                    left = prev;
                }

                var right = i;
                while (true)
                {
                    var next = (right + 1) % 6;
                    if (!_activeTiles[next] || visited[next])
                        break;
                    right = next;
                }

                var j = left;
                while (true)
                {
                    if (!visited[j])
                    {
                        visited[j] = true;
                        _aoes.Add(new(Cone, pos, (180 - 60 * j).Degrees(), act));
                    }

                    if (j == right)
                        break;
                    j = (j + 1) % 6;
                }
                break;
            }
        }
        else if (aid is AID.AlexandrianThunderIVCircle2 or AID.AlexandrianThunderIVDonut2 && _aoes.Count == 0)
        {
            var act = Module.CastFinishAt(spell);
            for (var i = 0; i < 6; ++i)
                if (_activeTiles[i])
                    _aoes.Add(new(Cone, Arena.Center, (180 - 60 * i).Degrees(), act));
        }
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id != 0x1E46)
            return;

        var rotRounded = (int)actor.Rotation.Deg;
        var index = rotRounded switch
        {
            143 or -143 or 180 => 0,
            83 or 156 or 119 => 1,
            96 or 23 or 59 => 2,
            0 or -36 or 36 => 3,
            -96 or -23 or -60 => 4,
            -120 or -156 or -83 => 5,
            _ => -1
        };
        if (index >= 0)
            _activeTiles[index] = true;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.AlexandrianThunderIII1:
            case AID.AlexandrianThunderIII2:
                _aoes.Clear();
                break;
            case AID.AlexandrianThunderIVCircle2:
            case AID.AlexandrianThunderIVDonut2:
                if (++NumCasts == 2)
                {
                    _aoes.Clear();
                    NumCasts = 0;
                }
                break;
        }
    }
}
