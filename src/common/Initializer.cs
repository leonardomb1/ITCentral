using System.Reflection;
using ITCentral.Data;
using ITCentral.Models;
using ITCentral.Router;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Mapping;
using YamlDotNet.RepresentationModel;

namespace ITCentral.Common;

public static class Initializer
{
    public static void InitializeDb()
    {
        DataConnection.DefaultSettings = new CallProvider();

        using var repository = new CallBase();

        var entityTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t =>
            {
                return typeof(IModel)
                    .IsAssignableFrom(t) &&
                    t.IsClass &&
                    t.GetCustomAttribute<TableAttribute>() != null;
            })
            .ToList();

        var dependencies = new Dictionary<Type, List<Type>>();

        foreach (var type in entityTypes)
        {
            var foreignKeys = type.GetProperties()
                .Where(p => entityTypes.Contains(p.PropertyType))
                .Select(p => p.PropertyType)
                .Distinct()
                .ToList();

            dependencies[type] = foreignKeys;
        }

        var sortedEntities = TopologicalSort(entityTypes, dependencies);

        foreach (var entityType in sortedEntities)
        {
            var tableAttribute = entityType.GetCustomAttribute<TableAttribute>();
            string tableName = tableAttribute?.Name ?? entityType.Name;

            if (!repository.Exists(tableName))
            {
                Log.Out($"Creating table for model class: {entityType.Name}");

                var createTableMethod = typeof(DataExtensions)
                    .GetMethod(nameof(DataExtensions.CreateTable))?
                    .MakeGenericMethod(entityType);

                createTableMethod?.Invoke(null,
                [
                    repository,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    TableOptions.None
                ]);

                Log.Out($"Table created for: {entityType.Name}");
            }
            else
            {
                Log.Out($"Table already exists for: {entityType.Name}");
            }
        }
    }


    public static void InitializeFromArgs(string[] args)
    {
        var values = args.Skip(1);

        for (int i = 0; i < values.Count(); i++)
        {
            var key = AppCommon.keyMap.ElementAt(i).Value;
            var propertyInfo = typeof(AppCommon).GetProperty(key, BindingFlags.Public | BindingFlags.Static);

            if (propertyInfo != null && propertyInfo.CanWrite)
            {
                var val = values.ElementAt(i);
                var convert = Convert.ChangeType(val, propertyInfo.PropertyType);
                propertyInfo.SetValue(null, convert);
            }
        }

        InitializeDb();
        Server server = new();
        server.Run();
    }

    public static void InitializeFromEnv()
    {
        Dictionary<string, string?> config = AppCommon.keyMap.ToDictionary(
            env => env.Key,
            env => Environment.GetEnvironmentVariable(env.Key)
        );

        if (config.Any(variable => variable.Value is null))
        {
            throw new Exception("Environment variable not configured!");
        }

        foreach (var env in AppCommon.keyMap)
        {
            var propertyInfo = typeof(AppCommon).GetProperty(env.Value, BindingFlags.Public | BindingFlags.Static);
            if (propertyInfo != null && propertyInfo.CanWrite)
            {
                var value = Convert.ChangeType(config[env.Key], propertyInfo.PropertyType);
                propertyInfo.SetValue(null, value);
            }
        }

        InitializeDb();
        Server server = new();
        server.Run();
    }

    public static void InitializeFromYaml(string yamlFilePath)
    {
        if (!File.Exists(yamlFilePath))
        {
            throw new FileNotFoundException($"Configuration file not found: {yamlFilePath}");
        }

        var yamlContent = File.ReadAllText(yamlFilePath);
        var yaml = new YamlStream();
        yaml.Load(new StringReader(yamlContent));

        var root = (YamlMappingNode)yaml.Documents[0].RootNode;

        foreach (var env in AppCommon.keyMap)
        {
            var key = env.Key;
            var propertyName = env.Value;
            var propertyInfo = typeof(AppCommon).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Static);

            if (propertyInfo != null && propertyInfo.CanWrite)
            {
                if (root.Children.TryGetValue(new YamlScalarNode(key), out var valueNode))
                {
                    var value = valueNode.ToString();
                    var convertedValue = Convert.ChangeType(value, propertyInfo.PropertyType);
                    propertyInfo.SetValue(null, convertedValue);
                }
            }
        }


        InitializeDb();
        Server server = new();
        server.Run();
    }

    private static List<Type> TopologicalSort(List<Type> types, Dictionary<Type, List<Type>> dependencies)
    {
        var sorted = new List<Type>();
        var visited = new HashSet<Type>();

        void Visit(Type type)
        {
            if (visited.Contains(type))
                return;

            visited.Add(type);

            if (dependencies.TryGetValue(type, out var dependentTypes))
            {
                foreach (var depType in dependentTypes)
                    Visit(depType);
            }

            sorted.Add(type);
        }

        foreach (var type in types)
            Visit(type);

        return sorted;
    }
}