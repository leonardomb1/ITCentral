using System.Reflection;
using ITCentral.Data;
using ITCentral.Models;
using ITCentral.Router;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Mapping;
using YamlDotNet.RepresentationModel;

namespace ITCentral.Common;

public static class AppCommon
{
    public const bool Success = true;

    public const bool Fail = false;

    public const string ProgramVersion = "0.0.3";

    public const string ProgramName = "ITCentral";

    public static string VersionHeader => $"{ProgramName} - Version: {ProgramVersion}";

    public const string MessageInfo = "INFO";

    public const string MessageWarning = "WARN";

    public const string MessageRequest = "REQUEST";

    public const string MessageError = "ERROR";

    public const int StructuredNormal = 1;

    public const int StructuredDifferentFiles = 2;

    public static int LogDumpTime { get; private set; }

    public static int MaxDegreeParallel { get; set; }

    public static ParallelOptions ParallelRule => new() { MaxDegreeOfParallelism = MaxDegreeParallel };

    public static int ConsumerFetchMax { get; private set; }

    public static int ProducerLineMax { get; private set; }

    public static int SessionTime { get; private set; }

    public static int BulkCopyTimeout { get; private set; }

    public static int ConsumerAttemptMax { get; private set; }

    public static bool Logging { get; private set; }

    public static int PortNumber { get; private set; }

    public static int LdapPort { get; private set; }

    public static string ConnectionString { get; private set; } = "";

    public static bool Ssl { get; private set; }

    public static bool LdapSsl { get; private set; }

    public static string HostName { get; private set; } = "";

    public static string DbType { get; private set; } = "";

    public static string MasterKey { get; private set; } = "";

    public static string ApiKey { get; private set; } = "";

    public static string LdapServer { get; private set; } = "";

    public static string LdapDomain { get; private set; } = "";

    public static string LdapBaseDn { get; private set; } = "";

    public static string LdapGroups { get; private set; } = "";

    public static string LdapGroupDN { get; private set; } = "";

    private static readonly Dictionary<string, string> keyMap = new()
    {
        { "PORT_NUMBER", nameof(PortNumber) },
        { "DB_TYPE", nameof(DbType) },
        { "CONNECTION_STRING", nameof(ConnectionString) },
        { "SSL_ENABLED", nameof(Ssl) },
        { "HOST_NAME", nameof(HostName) },
        { "ENABLE_LOG_DUMP", nameof(Logging) },
        { "LOG_DUMP_TIME", nameof(LogDumpTime) },
        { "ENCRYPT_KEY", nameof(MasterKey) },
        { "SESSION_TIME", nameof(SessionTime) },
        { "API_KEY", nameof(ApiKey) },
        { "MAX_DEGREE_PARALLEL", nameof(MaxDegreeParallel) },
        { "MAX_CONSUMER_FETCH", nameof(ConsumerFetchMax) },
        { "MAX_CONSUMER_ATTEMPT", nameof(ConsumerAttemptMax) },
        { "MAX_PRODUCER_LINECOUNT", nameof(ProducerLineMax) },
        { "LDAP_DOMAIN", nameof(LdapDomain) },
        { "LDAP_SERVER", nameof(LdapServer) },
        { "LDAP_PORT", nameof(LdapPort) },
        { "LDAP_BASEDN", nameof(LdapBaseDn) },
        { "LDAP_GROUPS", nameof(LdapGroups) },
        { "LDAP_GROUPDN", nameof(LdapGroupDN) },
        { "LDAP_SSL", nameof(LdapSsl) },
        { "BULK_TIMEOUT_SEC", nameof(BulkCopyTimeout) },
    };

    public static void ShowHelp()
    {
        ShowSignature();
        Console.WriteLine(
            $"Usage: {VersionHeader} \n" +
            "   [Options]: \n" +
            "   -h --help      Show this help message\n" +
            "   -v --version   Show version information\n" +
            "   -e --environment  Use environment variables for configuration\n" +
            "   -f --file  Use yml file for configuration\n" +
            "   -c --console  Use command-line arguments for configuration"
            );
    }

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

    public static void ShowVersion()
    {
        Console.WriteLine(VersionHeader);
    }

    private static void ShowSignature()
    {
        Console.WriteLine(
            "Developed by Leonardo M. Baptista\n"
        );
    }

    public static void InitializeFromArgs(string[] args)
    {
        var values = args.Skip(1);

        for (int i = 0; i < values.Count(); i++)
        {
            var key = keyMap.ElementAt(i).Value;
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
        Dictionary<string, string?> config = keyMap.ToDictionary(
            env => env.Key,
            env => Environment.GetEnvironmentVariable(env.Key)
        );

        if (config.Any(variable => variable.Value is null))
        {
            throw new Exception("Environment variable not configured!");
        }

        foreach (var env in keyMap)
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

        foreach (var env in keyMap)
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