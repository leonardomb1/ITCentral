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

    public static readonly Dictionary<string, string> keyMap = new()
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

    public static List<Type> TopologicalSort(List<Type> types, Dictionary<Type, List<Type>> dependencies)
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