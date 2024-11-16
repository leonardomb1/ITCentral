namespace ITCentral.Common;
using System.Reflection;
using ITCentral.Data;

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
    public static TimeSpan LogDumpTime {get; private set;}
    public static bool Logging {get; private set;}
    public static int PortNumber {get; private set;}
    public static string ConnectionString {get; private set;} = "";
    public static bool Ssl {get; private set;}
    public static string HostName {get; private set;} = "";
    public static string DbType {get; private set;} = "";
    public static string MasterKey {get; private set;} = "";
    public static IDBCall GenerateCallerInstance()
    {
        return DbType switch
        {
            _ when DbType == "MSSQL" => new SqlServerCall(ConnectionString),
            _ when DbType == "SQLite" => new SqlLiteCall(ConnectionString),
            _ => throw new Exception("Database not supported.")
        };
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
            "   Port, DbType, ConnectionString, SSL, Host, Logging, LogTime, LogPath"
            );
    }

    public static string GenerateSessionId(string seed)
    {
        return Encryption.Sha256($"{seed}{DateTime.Now:yyyy-MM-dd HH:mm}{DateTime.Now.AddMinutes(30):yyyy-MM-dd HH:mm}");
    }

    public static bool ValidateSessionId(string seed, string token)
    {
        string[] parts = token.Split([seed], StringSplitOptions.None);
        if (parts.Length < 2)
        {
            return false;
        }

        string expirationTimeString = parts[1][^16..];
        if (DateTime.TryParseExact(expirationTimeString, "yyyy-MM-dd HH:mm", null, System.Globalization.DateTimeStyles.None, out DateTime expirationTime))
        {
            return DateTime.Now < expirationTime;
        }

        return false;
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
        if (args.Length < 9)
        {
            throw new ArgumentException("Insufficient arguments provided.");
        }

        if (!int.TryParse(args[1], out int port))
        {
            throw new ArgumentException("Port value must be an integer.");
        }

        if (!bool.TryParse(args[4], out bool ssl))
        {
            throw new ArgumentException("SSL value must be a boolean.");
        }

        if (!bool.TryParse(args[6], out bool logging))
        {
            throw new ArgumentException("SSL value must be a boolean.");
        }

        if (!int.TryParse(args[7], out int logTime))
        {
            throw new ArgumentException("Log Time value must be an integer.");
        }

        string db = args[2];
        string conn = args[3];
        string hostName = args[5];
        string logPath = args[8];
        string key = args[9];

        PortNumber = port;
        DbType = db;
        ConnectionString = conn;
        HostName = hostName;
        Ssl = ssl;
        Logging = logging;
        LogFilePath = logPath;
        LogDumpTime = TimeSpan.FromSeconds(logTime);
        MasterKey = key;
    }

    public static void InitializeFromEnv()
    {
        var envs = new Dictionary<string, string>
        {
            { "PORT_NUMBER", nameof(PortNumber) },
            { "DB_TYPE", nameof(DbType) },
            { "CONNECTION_STRING", nameof(ConnectionString) },
            { "SSL_ENABLED", nameof(Ssl) },
            { "HOST_NAME", nameof(HostName) },
            { "LOGGING", nameof(HostName) },
            { "LOG_FILE_PATH", nameof(LogFilePath) },
            { "LOG_DUMP_TIME", nameof(LogDumpTime) },
            { "ENCRYPT_KEY", nameof(MasterKey) },
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