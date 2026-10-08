namespace BossMod.Stormblood.Ultimate.UCOB;

sealed class P5Teraflare(BossModule module) : Components.CastCounter(module, (uint)AID.Teraflare)
{
    public bool DownForTheCountAssigned;

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.DownForTheCount)
        {
            DownForTheCountAssigned = true;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        hints.ScriptedDeath = true;
    }
}

sealed class P5FlamesOfRebirth(BossModule module) : Components.CastCounter(module, (uint)AID.FlamesOfRebirth)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        hints.ScriptedDeath = true;
    }
}
