namespace ITCentral.Common;
using System.Reflection;

public static class AppCommon
{
    public const bool Success = true;
    public const byte Exists = 1;
    public const string ProgramVersion = "0.0.1";
    public const string ProgramName = "ITCentral";
    private static int port;
    private static string conStr = "";
    private static bool isSsl;
    private static string host = "";
    public static int PortNumber
    {
        get => port;
        set => port = value;
    }
    public static string ConnectionString
    {
        get => conStr;
        set => conStr = value;
    }
    public static bool Ssl
    {
        get => isSsl;
        set => isSsl = value;
    }
    public static string HostName
    {
        get => host;
        set => host = value;
    }
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
            "   Port, ConnectionString, SSL, Host"
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

    public static void Initialize()
    {
        var envs = new Dictionary<string, string>
        {
            { "PORT_NUMBER", nameof(ConnectionString) },
            { "CONNECTION_STRING", nameof(ConnectionString) },
            { "SSL_ENABLED", nameof(ConnectionString) },
            { "HOST_NAME", nameof(ConnectionString) },
        };

        Dictionary<string, string?> config = envs.ToDictionary(
            env => env.Key,
            env => Environment.GetEnvironmentVariable(env.Key)
        );

        if (config.Any(variable => variable.Value is null)) {
            throw new Exception("Environment variable not configured!");
        }

        foreach (var env in envs)
        {
            var propertyInfo = typeof(AppCommon).GetProperty(env.Value, BindingFlags.Public | BindingFlags.Static);
            if (propertyInfo != null && propertyInfo.CanWrite)
            {
                propertyInfo.SetValue(null, config[env.Key]);
            }
        }
    }
}