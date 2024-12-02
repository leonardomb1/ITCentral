namespace ITCentral.Common;
using System.Reflection;
using ITCentral.Data;
using YamlDotNet.RepresentationModel;
public static class AppCommon
{
    public const bool Success = true;
    public const string ProgramVersion = "0.0.1";
    public const string ProgramName = "ITCentral";
    public const string MessageInfo = "INFO";
    public const string MessageWarning = "WARN";
    public const string MessageRequest = "REQUEST";
    public const string MessageError = "ERROR";
    public static string LogFilePath {get; private set;} = "";
    public static int LogDumpTime {get; private set;}
    public static int SessionTime {get; private set;}
    public static bool Logging {get; private set;}
    public static int PortNumber {get; private set;}
    public static string ConnectionString {get; private set;} = "";
    public static bool Ssl {get; private set;}
    public static string HostName {get; private set;} = "";
    public static string DbType {get; private set;} = "";
    public static string MasterKey {get; private set;} = "";
    public static string ApiKey {get; private set;} = "";

    public static IDBCall DbConfig()
    {
        return DbType switch
        {
            "SqlServer" => new SqlServerCall(ConnectionString),
            "Sqlite" => new SqlLiteCall(ConnectionString),
            _ => throw new Exception("Unsupported database type")
        };
    }

    private static readonly Dictionary<string, string> keyMap = new()
    {
        { "PORT_NUMBER", nameof(PortNumber) },
        { "DB_TYPE", nameof(DbType) },
        { "CONNECTION_STRING", nameof(ConnectionString) },
        { "SSL_ENABLED", nameof(Ssl) },
        { "HOST_NAME", nameof(HostName) },
        { "ENABLE_LOG_DUMP", nameof(Logging) },
        { "LOG_DUMP_TIME", nameof(LogDumpTime) },
        { "LOG_FILE_PATH", nameof(LogFilePath) },
        { "ENCRYPT_KEY", nameof(MasterKey) },
        { "SESSION_TIME", nameof(SessionTime) },
        { "API_KEY", nameof(ApiKey) },
    };
    public static void ShowHelp() 
    {
        ShowSignature();
        Console.WriteLine(
            $"Usage: {ProgramName} [options]\n" +
            "Options:\n" +
            "   -h --help      Show this help message\n" +
            "   -v --version   Show version information\n" +
            "   -e --environment    [Options]  Use configuration variables\n\n" +
            "   [Options]: \n" +
            "   Port, DbType, ConnectionString, SSL, Host, Logging, LogTime, LogPath"
            );
    }

    public static void ShowVersion() 
    {
        ShowSignature();
        Console.WriteLine($"{ProgramName} version {ProgramVersion}");
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

        for(int i = 0; i < values.Count(); i++) {
            var key = keyMap.ElementAt(i).Value;
            var propertyInfo = typeof(AppCommon).GetProperty(key, BindingFlags.Public | BindingFlags.Static);

            if(propertyInfo != null && propertyInfo.CanWrite)
            {
                var val = values.ElementAt(i);
                var convert = Convert.ChangeType(val, propertyInfo.PropertyType);
                propertyInfo.SetValue(null, convert);
            }
        }  
    }

    public static void InitializeFromEnv()
    {
        Dictionary<string, string?> config = keyMap.ToDictionary(
            env => env.Key,
            env => Environment.GetEnvironmentVariable(env.Key)
        );

        if (config.Any(variable => variable.Value is null)) {
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
    }
}