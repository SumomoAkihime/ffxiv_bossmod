using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace BossMod;

public sealed class ConfigRoot
{
    public Event Modified = new();
    public readonly Dictionary<Type, ConfigNode> _nodes = [];
    public List<ConfigNode> Nodes => [.. _nodes.Values];
    private readonly record struct TransientOverride(object? Original, object? Value);
    private readonly Dictionary<(ConfigNode node, ConfigFieldMetadata field), TransientOverride> _transient = [];
    private readonly System.Threading.Lock _saveLock = new();

    public void Initialize()
    {
        GeneratedRegistries.RegisterConfigNodes((type, inst) =>
        {
            inst.Modified.Subscribe(() => DropChangedTransients(inst));
            inst.Modified.Subscribe(Modified.Fire);
            _nodes[type] = inst;
        });
    }

    public T Get<T>() where T : ConfigNode => (T)_nodes[typeof(T)];
    public T Get<T>(Type derived) where T : ConfigNode => (T)_nodes[derived];

    public ConfigListener<T> GetAndSubscribe<T>(Action<T> modified) where T : ConfigNode => new(Get<T>(), modified);

    public void LoadFromFile(FileInfo file)
    {
        try
        {
            var data = ConfigConverter.Schema.Load(file);
            using var json = data.document;
            var ser = Serialization.BuildSerializationOptions();

            foreach (var jconfig in data.payload.EnumerateObject())
            {
                var type = GeneratedFactories.FindType(jconfig.Name);
                var node = type != null ? _nodes.GetValueOrDefault(type) : null;
                try
                {
                    node?.Deserialize(jconfig.Value, ser);
                }
                catch (AggregateException exc)
                {
                    Service.Logger.Warning(exc, "An error occurred while deserializing the plugin config. As a result, some settings may have unexpected values.");
                }
            }
        }
        catch (Exception e)
        {
            Service.Log($"Failed to load config from {file.FullName}: {e}");
        }
    }

    public void SaveToFile(FileInfo file)
    {
        using var saveLock = _saveLock.EnterScope();
        try
        {
            var ser = Serialization.BuildSerializationOptions();
            var serializedNodes = new ConcurrentDictionary<Type, string>();

            // IPC setters and serialization share one lock so an override cannot leak into a saved snapshot.
            lock (_transient)
            {
                Parallel.ForEach(_nodes, entry =>
                {
                    using var ms = new MemoryStream();
                    using var tempWriter = new Utf8JsonWriter(ms);
                    entry.Value.Serialize(tempWriter, ser);
                    tempWriter.Flush();
                    serializedNodes[entry.Key] = Encoding.UTF8.GetString(ms.ToArray());
                });

                Dictionary<Type, JsonObject> patched = [];
                foreach (var ((node, field), entry) in _transient)
                {
                    var type = node.GetType();
                    if (!patched.TryGetValue(type, out var json))
                        patched[type] = json = JsonNode.Parse(serializedNodes[type])!.AsObject();
                    // Compare the serialized snapshot, not a later UI value, so an edit during serialization cannot leak the override.
                    if (JsonNode.DeepEquals(json[field.Name], JsonSerializer.SerializeToNode(entry.Value, field.FieldType, ser)))
                        json[field.Name] = JsonSerializer.SerializeToNode(entry.Original, field.FieldType, ser);
                }
                foreach (var (type, json) in patched)
                    serializedNodes[type] = json.ToJsonString();
            }

            using var stream = new FileStream(file.FullName, FileMode.Create, FileAccess.Write, FileShare.None);
            using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });

            writer.WriteStartObject();
            writer.WriteNumber("Version", ConfigConverter.Schema.CurrentVersion);
            writer.WritePropertyName("Payload");
            writer.WriteStartObject();

            foreach (var (t, jsonStr) in serializedNodes)
            {
                writer.WritePropertyName(t.FullName!);
                writer.WriteRawValue(jsonStr);
            }

            writer.WriteEndObject();
            writer.WriteEndObject();
        }
        catch (Exception e)
        {
            Service.Log($"Failed to save config to {file.FullName}: {e}");
        }
    }

    public List<string> ConsoleCommand(ReadOnlySpan<string> args, bool save = true) => ApplyConsoleCommand(args, save, false);

    public List<string> SetTransient(ReadOnlySpan<string> args)
    {
        if (args.Length < 3)
            return ["Usage: Configuration.SetTransient <config-type> <field> <value>"];
        return ApplyConsoleCommand(args, true, true);
    }

    public int ClearTransient()
    {
        HashSet<ConfigNode> modified = [];
        var restored = 0;
        lock (_transient)
        {
            foreach (var ((node, field), entry) in _transient)
            {
                if (Equals(field.Getter(node), entry.Value))
                {
                    field.Setter(node, entry.Original);
                    modified.Add(node);
                    ++restored;
                }
            }
            _transient.Clear();
        }
        foreach (var node in modified)
            node.Modified.Fire();
        return restored;
    }

    internal bool IsTransient(ConfigNode node, ConfigFieldMetadata field)
    {
        lock (_transient)
            return _transient.ContainsKey((node, field));
    }

    // UI changes share the transient gate, including custom property renderers; notify subscribers after leaving it.
    internal bool ApplyUserEdit(ConfigNode node, ConfigFieldMetadata field, Func<bool> edit)
    {
        lock (_transient)
        {
            if (!edit())
                return false;
            _transient.Remove((node, field));
            return true;
        }
    }

    private void DropChangedTransients(ConfigNode node)
    {
        lock (_transient)
        {
            foreach (var (key, entry) in _transient)
            {
                if (key.node == node && !Equals(key.field.Getter(node), entry.Value))
                    _transient.Remove(key);
            }
        }
    }

    private List<string> ApplyConsoleCommand(ReadOnlySpan<string> args, bool save, bool transient)
    {
        List<string> result = [];
        if (args.Length == 0)
        {
            result.Add("Usage: /bmr cfg <config-type> <field> <value>");
            result.Add("Both config-type and field can be shortened. Valid config-types:");
            foreach (var t in _nodes.Keys)
            {
                result.Add($"- {t.Name}");
            }
        }
        else
        {
            List<ConfigNode> matchingNodes = [];
            foreach (var (t, n) in _nodes)
            {
                var arg = args[0];
                if (t.Name.Contains(arg, StringComparison.CurrentCultureIgnoreCase))
                {
                    // check for exact match
                    if (t.Name.Length == arg.Length)
                    {
                        matchingNodes.Clear();
                        matchingNodes.Add(n);
                        break;
                    }
                    matchingNodes.Add(n);
                }
            }
            if (matchingNodes.Count == 0)
            {
                result.Add("Config type not found. Valid types:");
                foreach (var t in _nodes.Keys)
                {
                    result.Add($"- {t.Name}");
                }
            }
            else if (matchingNodes.Count > 1)
            {
                result.Add("Ambiguous config type, pass longer pattern. Matches:");
                foreach (var n in matchingNodes)
                {
                    result.Add($"- {n.GetType().Name}");
                }
            }
            else if (args.Length == 1)
            {
                result.Add("Usage: /bmr cfg <config-type> <field> <value>");
                result.Add($"Valid fields for {matchingNodes[0].GetType().Name}:");
                foreach (var f in GeneratedConfigMetadata.Get(matchingNodes[0]).DisplayFields)
                {
                    if (f.Display != null)
                    {
                        result.Add($"- {f.Name}");
                    }
                }
            }
            else
            {
                List<ConfigFieldMetadata> matchingFields = [];
                foreach (var f in GeneratedConfigMetadata.Get(matchingNodes[0]).DisplayFields)
                {
                    if (f.Display == null)
                    {
                        continue;
                    }

                    var arg = args[1];
                    if (f.Name.Contains(arg, StringComparison.CurrentCultureIgnoreCase))
                    {
                        // check for exact match
                        if (f.Name.Length == arg.Length)
                        {
                            matchingFields.Clear();
                            matchingFields.Add(f);
                            break;
                        }
                        matchingFields.Add(f);
                    }
                }
                if (matchingFields.Count == 0)
                {
                    result.Add($"Field not found {args[1]}, Valid fields:");
                    foreach (var f in GeneratedConfigMetadata.Get(matchingNodes[0]).DisplayFields)
                    {
                        if (f.Display != null)
                        {
                            result.Add($"- {f.Name}");
                        }
                    }
                }
                else if (matchingFields.Count > 1)
                {
                    result.Add("Ambiguous field name, pass longer pattern. Matches:");
                    foreach (var f in matchingFields)
                    {
                        result.Add($"- {f.Name}");
                    }
                }
                /*else if (args.Count == 2)
                {
                    result.Add("Usage: /bmr cfg <config-type> <field> <value>");
                    result.Add($"Type of {matchingNodes[0].GetType().Name}.{matchingFields[0].Name} is {matchingFields[0].FieldType.Name}");
                }*/
                else
                {
                    try
                    {
                        if (args.Length == 2)
                        {
                            result.Add(matchingFields[0].Getter(matchingNodes[0])?.ToString() ?? $"Failed to get value of '{matchingFields[0].Name}'");
                        }
                        else
                        {
                            var val = FromConsoleString(args[2], matchingFields[0].FieldType);
                            if (val == null)
                            {
                                result.Add($"Failed to convert '{args[2]}' to {matchingFields[0].FieldType}");
                            }
                            else
                            {
                                var node = matchingNodes[0];
                                var field = matchingFields[0];
                                lock (_transient)
                                {
                                    var key = (node, field);
                                    var current = field.Getter(node);
                                    var original = _transient.TryGetValue(key, out var existing) && Equals(current, existing.Value) ? existing.Original : current;
                                    // Keep the ledger unchanged if conversion or the field setter fails.
                                    field.Setter(node, val);
                                    if (transient)
                                        _transient[key] = new(original, val);
                                    else
                                        _transient.Remove(key);
                                }
                                if (save)
                                {
                                    matchingNodes[0].Modified.Fire();
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        if (args.Length == 2)
                        {
                            result.Add($"Failed to get value of {matchingNodes[0].GetType().Name}.{matchingFields[0].Name} : {e}");
                        }
                        else
                        {
                            result.Add($"Failed to set {matchingNodes[0].GetType().Name}.{matchingFields[0].Name} to {args[2]}: {e}");
                        }
                    }
                }
            }
        }
        return result;
    }

    private object? FromConsoleString(string str, Type t)
        => t == typeof(bool) ? bool.Parse(str)
        : t == typeof(float) ? float.Parse(str)
        : t == typeof(int) ? int.Parse(str)
        : t.IsAssignableTo(typeof(Enum)) ? Enum.Parse(t, str)
        : null;
}
