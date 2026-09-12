using BossMod;

static class ModuleManagementTests
{
    public static void Run(BossModuleManager manager, Actor boss, Action<bool, string> check)
    {
        var config = BossModuleManager.Config;
        var oid = boss.OID;
        var moduleName = typeof(EntryProbe).ToString();
        var oldMaturity = config.MinMaturity;
        var oldMaxLoadDistance = config.MaxLoadDistance;
        var oldOIDDisabled = config.DisabledModuleOIDs.Contains(oid);
        var oldIPCDisabled = config.DisabledModules.Contains(moduleName);
        var player = manager.WorldState.Party.Player() ?? throw new InvalidOperationException("测试世界缺少本地玩家");
        var oldPlayerPosRot = player.PosRot;

        try
        {
            config.MinMaturity = BossModuleInfo.Maturity.WIP;
            config.SetModuleEnabled(oid, true);
            config.DisabledModules.Remove(moduleName);
            config.Modified.Fire();
            manager.Update();
            var retained = CheckSingleInstance(manager, boss, true, null, check, "启用后加载现有实体且无重复实例");

            config.SetModuleEnabled(oid, false);
            manager.Update();
            CheckSingleInstance(manager, boss, false, retained, check, "OID 开关立即将现有实体转为等待");

            config.DisabledModules.Add(moduleName);
            config.SetModuleEnabled(oid, true);
            manager.Update();
            CheckSingleInstance(manager, boss, false, retained, check, "恢复 OID 开关不越过 IPC 禁用");

            config.DisabledModules.Remove(moduleName);
            manager.Update();
            CheckSingleInstance(manager, boss, true, retained, check, "IPC 恢复后无需重建实体即可加载");

            config.SetModuleEnabled(oid, false);
            config.DisabledModules.Add(moduleName);
            manager.Update();
            config.DisabledModules.Remove(moduleName);
            manager.Update();
            CheckSingleInstance(manager, boss, false, retained, check, "恢复 IPC 不越过 OID 禁用");

            config.SetModuleEnabled(oid, true);
            manager.Update();
            CheckSingleInstance(manager, boss, true, retained, check, "两路均恢复后重新加载且仍无重复");

            config.MinMaturity = BossModuleInfo.Maturity.Contributed;
            config.Modified.Fire();
            manager.Update();
            CheckSingleInstance(manager, boss, false, retained, check, "成熟度门槛立即将 WIP 模块转为等待");

            config.MinMaturity = BossModuleInfo.Maturity.WIP;
            config.Modified.Fire();
            manager.Update();
            CheckSingleInstance(manager, boss, true, retained, check, "恢复成熟度后加载原实例");

            config.MaxLoadDistance = 1f;
            manager.WorldState.Execute(new ActorState.OpMove(player.InstanceID, new System.Numerics.Vector4(boss.PosRot.X + 80f, boss.PosRot.Y, boss.PosRot.Z, player.PosRot.W)));
            manager.Update();
            CheckSingleInstance(manager, boss, true, retained, check, "加载距离配置低于 100 时仍按下限加载 80 码实体");

            manager.WorldState.Execute(new ActorState.OpMove(player.InstanceID, new System.Numerics.Vector4(boss.PosRot.X + 101f, boss.PosRot.Y, boss.PosRot.Z, player.PosRot.W)));
            manager.Update();
            CheckSingleInstance(manager, boss, false, retained, check, "超过 100 码时转为等待并保留实例");

            manager.WorldState.Execute(new ActorState.OpMove(player.InstanceID, new System.Numerics.Vector4(boss.PosRot.X + 80f, boss.PosRot.Y, boss.PosRot.Z, player.PosRot.W)));
            manager.Update();
            CheckSingleInstance(manager, boss, true, retained, check, "回到 100 码内后恢复原实例");
        }
        finally
        {
            manager.WorldState.Execute(new ActorState.OpMove(player.InstanceID, oldPlayerPosRot));
            config.MaxLoadDistance = oldMaxLoadDistance;
            if (oldIPCDisabled)
            {
                if (!config.DisabledModules.Contains(moduleName))
                    config.DisabledModules.Add(moduleName);
            }
            else
            {
                config.DisabledModules.Remove(moduleName);
            }
            config.SetModuleEnabled(oid, !oldOIDDisabled);
            config.MinMaturity = oldMaturity;
            config.Modified.Fire();
            manager.Update();
        }
    }

    private static BossModule CheckSingleInstance(BossModuleManager manager, Actor boss, bool loaded, BossModule? expected, Action<bool, string> check, string message)
    {
        var loadedModules = manager.LoadedModules.Where(m => m.PrimaryActor.InstanceID == boss.InstanceID).ToList();
        var pendingModules = manager.PendingModules.Where(m => m.PrimaryActor.InstanceID == boss.InstanceID).ToList();
        var instances = loadedModules.Concat(pendingModules).ToList();
        check(instances.Count == 1 && (loaded ? loadedModules.Count == 1 : pendingModules.Count == 1)
            && (expected == null || ReferenceEquals(instances[0], expected)), message);
        return instances[0];
    }
}
