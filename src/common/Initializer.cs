using System.Reflection;
using ITCentral.Data;
using ITCentral.Router;
using LinqToDB;
using LinqToDB.Data;
using YamlDotNet.RepresentationModel;

namespace ITCentral.Common;

public static class Initializer
{
    public static void InitializeDb()
    {
        DataConnection.DefaultSettings = new CallProvider();
        using var db = new EntityCreate();
        db.Database.EnsureCreated();
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
}