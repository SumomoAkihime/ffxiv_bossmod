namespace BossMod.Dawntrail.Trial.T03QueenEternal;

sealed class LegitimateForce(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(2);
    private static readonly AOEShapeRect rect = new(20f, 40f);
    private readonly Besiegement _aoe = module.FindComponent<Besiegement>()!;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0 || _aoe.AOEs.Count != 0)
        {
            return [];
        }

        if (count > 1 && _aoes[0].Rotation != _aoes[1].Rotation)
        {
            ref var aoe0 = ref _aoes.Ref(0);
            aoe0.Color = Colors.Danger;
            return _aoes;
        }
        return _aoes.Take(1);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.LegitimateForceLL:
                AddAOEs(caster, -90f, -90f);
                break;
            case (uint)AID.LegitimateForceLR:
                AddAOEs(caster, -90f, 90f);
                break;
            case (uint)AID.LegitimateForceRR:
                AddAOEs(caster, 90f, 90f);
                break;
            case (uint)AID.LegitimateForceRL:
                AddAOEs(caster, 90f, -90f);
                break;
        }

        void AddAOEs(Actor caster, float first, float second)
        {
            AddAOE(first);
            AddAOE(second, 3.1f);
            void AddAOE(float offset, float delay = default) => _aoes.Add(new(rect, caster.Position, spell.Rotation + offset.Degrees(), Module.CastFinishAt(spell, delay)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0)
        {
            switch (spell.Action.ID)
            {
                case (uint)AID.LegitimateForceLL:
                case (uint)AID.LegitimateForceLR:
                case (uint)AID.LegitimateForceRR:
                case (uint)AID.LegitimateForceRL:
                case (uint)AID.LegitimateForceR:
                case (uint)AID.LegitimateForceL:
                    _aoes.RemoveAt(0);
                    break;
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) => base.AddAIHints(slot, actor, assignment, hints);
}

