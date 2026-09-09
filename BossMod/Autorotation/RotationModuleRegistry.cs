namespace BossMod.Autorotation;

// database containing all registered rotation module definitions and builder functions
public static class RotationModuleRegistry
{
    public readonly record struct Entry(RotationModuleDefinition Definition, Func<RotationModuleManager, Actor, RotationModule> Builder);

    public static readonly Dictionary<string, Type> ModulesByName = [];
    public static readonly Dictionary<Type, Entry> Modules = BuildModules();

    private static Dictionary<Type, Entry> BuildModules()
    {
        Dictionary<Type, Entry> res = [];

        GeneratedRegistries.RegisterRotationModules(res, ModulesByName);

        return res;
    }
}
